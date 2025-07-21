# HalfEdge

![Screenshot](https://github.com/user-attachments/assets/541aa20a-aacb-4f33-bfed-f408ece4b73d)

**HalfEdge** is an experimental attempt to develop an optimized Unity C# package
with the help of coding agents like [Claude Code] and [Gemini CLI].

[Claude Code]: https://www.anthropic.com/claude-code
[Gemini CLI]: https://blog.google/technology/developers/introducing-gemini-cli-open-source-ai-agent/

For this experiment, I chose to implement a Half-Edge data structure library,
inspired by wblut’s [HE_Mesh]. I chose it as a subject because it's practical,
strictly unit-testable, and offers plenty of room for optimization — ideal
traits for this kind of experiment.

[HE_Mesh]: https://github.com/wblut/HE_Mesh

## How It Goes

First, I wrote a [design document] (with ChatGPT, of course), set up a Unity
project with a unit test environment, and asked Claude Code to implement the
library with unit tests. It generated several components fluently — impressive
at first glance.

[design document]: /Design.md

However, issues quickly became apparent. It produced many inverted faces, the
Chamfer Edges modifier generated broken edges, and so on. These defects were
obvious to the human eye but difficult to catch with automated unit tests. You
can ask the agent to write specific unit tests, and it sometimes succeeds — but
other times, it adds meaningless ones.

![Screenshot](https://github.com/user-attachments/assets/b1634522-0eba-42f4-8637-f60647f2914d)

After resolving a number of minor bugs, the initial implementation
(HalfEdgeMesh) was done. I then tried to optimize it, but the changes required
were too extensive for an incremental approach. Also, longer tasks tend to fail
more easily — especially with more affordable models like Sonnet. So I pivoted
to building a new version from scratch: HalfEdgeMesh2.

In HalfEdgeMesh2, I adopted an "optimize-first" strategy. I started by writing a
[new design document] and then implemented only the core modules based on it. I
had the agent write them in an already optimized form. This included zero GC
allocation, Burst compiler support, custom collection classes, and the Unity
Jobs System. With some human supervision, we successfully produced a suite of
optimized, zero-GC C# components for Unity. Yay.

[new design document]: /HalfEdgeMesh2_Design.md

![Screenshot](https://github.com/user-attachments/assets/0240bce3-bc77-42ae-ac0d-cf503a6b06a2)

Next, I started porting over the generator and modifier components. But that's
when my motivation dropped. These parts brought new issues and still needed my
input to optimize properly. While it's easier than writing everything myself, I
now clearly saw the time and effort involved — and it’s definitely not "free".  
So I paused the project there.

## Lessons Learned

It’s possible to build a well-optimized Unity package using coding agents. You
can achieve zero GC allocation, Burst optimization, parallel execution, and
solid unit test coverage. But it still requires proper human supervision. It’s
far easier with the agent’s help — but it’s not fully automatic.
