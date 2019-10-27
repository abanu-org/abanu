// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the GNU 2.0 license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel.Core.Scheduling
{
    public enum ThreadStatus
    {
        Empty = 0,
        Creating = 1,
        ScheduleForStart = 2,
        Running = 3,
        Waiting = 4,
        Terminated = 5,
    }
}
