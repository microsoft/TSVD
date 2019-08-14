// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace TSVDRuntime
{
    /// <summary>
    /// Trap delay injection algorithm modes.
    /// </summary>
    public enum TrapAlgorithm
    {
        /// <summary>
        /// No delay injection mode specified
        /// </summary>
        None,

        /// <summary>
        /// Delay injected in a randomized way
        /// </summary>
        Randomized,

        /// <summary>
        /// Delay injected based on dynamically learned happens-before relationship
        /// </summary>
        LearnedHB,
    }
}