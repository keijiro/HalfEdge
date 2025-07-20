using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace HalfEdgeMesh2
{
    [BurstCompile]
    unsafe struct EdgeHashMap : IDisposable
    {
        [NativeDisableUnsafePtrRestriction] long* keys;
        [NativeDisableUnsafePtrRestriction] int* values;
        [NativeDisableUnsafePtrRestriction] int* states; // 0=empty, 1=occupied

        int capacity;
        Allocator allocator;

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
        static uint Hash(long key) => (uint)(key ^ (key >> 32)) * 2654435761u;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(long key, int value)
        {
            var index = (int)(Hash(key) & (capacity - 1));
            var startIndex = index;

            while (states[index] != 0)
            {
                index = (index + 1) & (capacity - 1);
                if (index == startIndex) // Full table detected
                {
                    var count = GetCount();
                    throw new InvalidOperationException($"EdgeHashMap is full: {count}/{capacity} entries (Load Factor: {(float)count/capacity:P1})");
                }
            }

            keys[index] = key;
            values[index] = value;
            states[index] = 1;
        }

        int GetCount()
        {
            var count = 0;
            for (var i = 0; i < capacity; i++)
                if (states[i] != 0) count++;
            return count;
        }

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

        public int this[long key]
        {
            get
            {
                TryGetValue(key, out var value);
                return value; // MeshBuilder usage guarantees key exists
            }
        }

        public NativeArray<long> GetKeyArray(Allocator allocator)
        {
            // Count occupied slots first
            var count = 0;
            for (var i = 0; i < capacity; i++)
                if (states[i] != 0) count++;

            var result = new NativeArray<long>(count, allocator);
            var index = 0;
            for (var i = 0; i < capacity; i++)
                if (states[i] != 0) result[index++] = keys[i];

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdd(long key, int value)
        {
            var index = (int)(Hash(key) & (capacity - 1));
            var startIndex = index;

            while (true)
            {
                // Check if slot is empty (non-atomic version for testing)
                if (states[index] == 0)
                {
                    // Set state first
                    states[index] = 1;
                    // Then set key and value
                    keys[index] = key;
                    values[index] = value;
                    return true;
                }

                // Slot is occupied, check if it's the same key
                if (keys[index] == key)
                    return false; // Key already exists

                // Move to next slot
                index = (index + 1) & (capacity - 1);
                if (index == startIndex)
                    return false; // Table is full
            }
        }

        public void Clear() => UnsafeUtility.MemClear(states, capacity * sizeof(int));

        public EdgeHashMapReadOnly AsReadOnly()
        {
            return new EdgeHashMapReadOnly
            {
                keys = keys,
                values = values,
                states = states,
                capacity = capacity
            };
        }
    }

    [BurstCompile]
    unsafe struct EdgeHashMapReadOnly
    {
        [NativeDisableUnsafePtrRestriction] [ReadOnly] public long* keys;
        [NativeDisableUnsafePtrRestriction] [ReadOnly] public int* values;
        [NativeDisableUnsafePtrRestriction] [ReadOnly] public int* states;
        public int capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static uint Hash(long key) => (uint)(key ^ (key >> 32)) * 2654435761u;

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
                if (index == startIndex)
                    break;
            }

            value = 0;
            return false;
        }

        public int this[long key]
        {
            get
            {
                TryGetValue(key, out var value);
                return value;
            }
        }
    }
}