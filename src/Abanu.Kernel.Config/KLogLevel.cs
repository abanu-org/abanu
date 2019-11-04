// This file is part of Abanu, an Operating System written in C#. Web: https://www.abanu.org
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.

namespace Abanu.Kernel.Core
{
    public enum KLogLevel : sbyte
    {

        /// <summary>
        /// Default (or last) loglevel
        /// </summary>
        Default = -1,

        /// <summary>
        /// System is unusable
        /// </summary>
        Emergency = 0,

        /// <summary>
        /// Action must be taken immediately
        /// </summary>
        Alert = 1,

        /// <summary>
        /// Critical conditions
        /// </summary>
        Critical = 2,

        /// <summary>
        /// Error conditions
        /// </summary>
        Error = 3,

        /// <summary>
        /// Warning conditions
        /// </summary>
        Warning = 4,

        /// <summary>
        /// Normal but significant condition
        /// </summary>
        Notice = 5,

        /// <summary>
        /// Informational
        /// </summary>
        Info = 6,

        /// <summary>
        /// Debug-level messages
        /// </summary>
        Debug = 7,

        /// <summary>
        /// Tracing level
        /// </summary>
        Trace = 8,
    }

}
