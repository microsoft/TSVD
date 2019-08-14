// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

namespace TSVDRuntime
{
    /// <summary>
    /// Thread safety InterceptionPoint.
    /// </summary>
    public class ThreadSafetyInterceptionPoint
    {
        /// <summary>
        /// Gets or sets thread safety group the InterceptionPoint belongs to.
        /// </summary>
        public ThreadSafetyGroup ThreadSafetyGroup { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the API is a write API.
        /// </summary>
        public bool IsWriteAPI { get; set; }
    }
}
