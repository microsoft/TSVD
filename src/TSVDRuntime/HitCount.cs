// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.ComponentModel;

namespace TSVDRuntime
{
    /// <summary>
    /// Class HitCount.
    /// </summary>
    public class HitCount
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HitCount"/> class.
        /// </summary>
        public HitCount()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HitCount"/> class.
        /// </summary>
        /// <param name="localHitCount">The local hit count.</param>
        /// <param name="globalHitCount">The global hit count.</param>
        public HitCount(int localHitCount, int globalHitCount)
        {
            this.LocalHitCount = localHitCount;
            this.GlobalHitCount = globalHitCount;
        }

        /// <summary>
        /// Gets or sets a value indicating how many times all InterceptionPoints have been executed.
        /// </summary>
        /// <value>The global hit count.</value>
        [DefaultValue(-1)]
        public int GlobalHitCount { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating how many times the current InterceptionPoint has been executed.
        /// </summary>
        /// <value>The local hit count.</value>
        [DefaultValue(-1)]
        public int LocalHitCount { get; set; } = -1;

        /// <summary>
        /// Determines whether the specified local hit count is match.
        /// </summary>
        /// <param name="localHitCount">The local hit count.</param>
        /// <param name="localWindow">The local window.</param>
        /// <param name="globalHitCount">The global hit count.</param>
        /// <param name="globalWindow">The global window.</param>
        /// <returns><see langword="true"/> if the specified local hit count is match; otherwise, <see langword="false"/>.</returns>
        public bool IsMatch(int localHitCount, int localWindow, int globalHitCount, int globalWindow)
        {
            return (this.LocalHitCount < 0 || Math.Abs(localHitCount - this.LocalHitCount) <= localWindow)
                && (this.GlobalHitCount < 0 || Math.Abs(globalHitCount - this.GlobalHitCount) <= globalWindow);
        }
    }
}
