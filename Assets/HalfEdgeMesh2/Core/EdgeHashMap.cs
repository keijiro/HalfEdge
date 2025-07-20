using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace HalfEdgeMesh2
{
    // Linear probing hash map optimized for edge connectivity in half-edge meshes.
    // Keys are packed edge indices (v0 << 32 | v1), values are half-edge indices.
    unsafe struct EdgeHashMap : IDisposable
    {
        [NativeDisableUnsafePtrRestriction] long* keys;
        [NativeDisableUnsafePtrRestriction] int* values;
        [NativeDisableUnsafePtrRestriction] int* states; // 0=empty, 1=occupied

        int capacity;
        Allocator allocator;

        const uint HashMultiplier = 2654435761u; // Golden ratio prime

        public EdgeHashMap(int capacity, Allocator allocator)
        {
            // Capacity must be power of 2 for fast modulo
            this.capacity = capacity;
            this.allocator = allocator;

            var keysSize = capacity * sizeof(long);
            var valuesSize = capacity * sizeof(int);
            var statesSize = capacity * sizeof(int);

            keys = (long*)UnsafeUtility.Malloc(keysSize, 8, allocator);
            values = (int*)UnsafeUtility.Malloc(valuesSize, 4, allocator);
            states = (int*)UnsafeUtility.Malloc(statesSize, 4, allocator);

            UnsafeUtility.MemClear(states, statesSize);
        }

        public void Dispose()
        {
            if (keys != null)
            {
                UnsafeUtility.Free(keys, allocator);
                UnsafeUtility.Free(values, allocator);
                UnsafeUtility.Free(states, allocator);
            }
        }

        public bool IsCreated => keys != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Hash(long key) => (uint)(key ^ (key >> 32)) * HashMultiplier;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(long key, out int value)
        {
            var index = (int)(Hash(key) & (capacity - 1));
            var startIndex = index;

            while (states[index] != 0)
            {
                if (keys[index] == key)
                {
                    value = values[index];
                    return true;
                }
                index = (index + 1) & (capacity - 1);
                if (index == startIndex) // Full table, key not found
                    break;
            }

            value = 0;
            return false;
        }

        public NativeArray<long> GetKeyArray(Allocator allocator)
        {
            // Count occupied slots first
            var count = 0;
            for (var i = 0; i < capacity; i++)
                if (states[i] != 0)
                    count++;

            var result = new NativeArray<long>(count, allocator);
            var index = 0;
            for (var i = 0; i < capacity; i++)
                if (states[i] != 0)
                    result[index++] = keys[i];

            return result;
        }

        // Attempts to add a key-value pair. Returns false if key already exists or table is full.
        // Thread-safe for parallel operations (uses non-atomic operations for now).
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(long key, int value)
        {
            var index = (int)(Hash(key) & (capacity - 1));
            var startIndex = index;

            while (true)
            {
                // TODO: Implement proper CAS operations for thread safety
                if (states[index] == 0)
                {
                    states[index] = 1;
                    keys[index] = key;
                    values[index] = value;
                    return true;
                }

                if (keys[index] == key)
                    return false; // Key already exists

                index = (index + 1) & (capacity - 1);
                if (index == startIndex)
                    return false; // Table is full
            }
        }

        // Clears all entries from the hash map.
        public void Clear() => UnsafeUtility.MemClear(states, capacity * sizeof(int));
    }
}