// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Threading;

namespace TSVDRuntime
{
    /// <summary>
    /// Trap information.
    /// </summary>
    public class Trap
    {
        /// <summary>
        /// Gets or sets the object instance.
        /// </summary>
        public Guid ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the semaphore uesd by the trap.
        /// </summary>
        public Semaphore Semaphore { get; set; }

        /// <summary>
        /// Gets or sets the InterceptionPoint details of the trap.
        /// </summary>
        public InterceptionPoint InterceptionPoint { get; set; }

        /// <summary>
        /// Gets or sets the thread-safety details of the trapped torch-point.
        ///
        /// This can be computed from the InterceptionPoint itself but is relatively
        /// expensive to do so and is therefore stored with each trap.
        /// </summary>
        public ThreadSafetyInterceptionPoint ThreadSafetyInterceptionPoint { get; set; }

        /// <summary>
        /// Gets or sets the trap delay.
        /// </summary>
        public int Delay { get; set; }
    }
}