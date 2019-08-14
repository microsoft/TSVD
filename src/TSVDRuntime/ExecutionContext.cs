// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace TSVDRuntime
{
    /// <summary>
    /// Class to store the current execution context.
    /// </summary>
    public static class ExecutionContext
    {
        /// <summary>
        /// Initializes static members of the <see cref="ExecutionContext"/> class.
        /// </summary>
        static ExecutionContext()
        {
            ExecutionId = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Gets execution id.
        /// </summary>
        public static string ExecutionId { get; }
    }
}
