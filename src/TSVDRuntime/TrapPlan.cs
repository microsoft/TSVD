// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TSVDRuntime
{
    /// <summary>
    /// Information of a trap point.
    /// </summary>
    public class TrapPlan
    {
        /// <summary>
        /// The current delay probability.
        /// </summary>
        private double currentDelayProbability = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="TrapPlan"/> class.
        /// </summary>
        public TrapPlan()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrapPlan"/> class.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="method">The method.</param>
        /// <param name="ilOffset">The il offset.</param>
        public TrapPlan(string api, string method = null, int ilOffset = -1)
        {
            this.API = api;
            this.Method = method;
            this.ILOffset = ilOffset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrapPlan"/> class.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="method">The method.</param>
        /// <param name="ilOffset">The il offset.</param>
        /// <param name="localHitCount">The local hit count.</param>
        /// <param name="globalHitCount">The global hit count.</param>
        public TrapPlan(string api, string method, int ilOffset, int localHitCount, int globalHitCount)
        {
            this.API = api;
            this.Method = method;
            this.ILOffset = ilOffset;
            this.HitCounts = new List<HitCount>() { new HitCount(localHitCount, globalHitCount) };
        }

        /// <summary>
        /// Gets or sets API name of the trap point.
        /// </summary>
        /// <value>The API.</value>
        public string API { get; set; }

        /// <summary>
        /// Gets or sets delay probability.
        /// </summary>
        /// <value>The delay probability.</value>
        [DefaultValue(1)]
        public double DelayProbability { get; set; } = 1;

        /// <summary>
        /// Gets or sets the factor by which delay probability for this plan should decay after each executed trap.
        /// </summary>
        /// <value>The delay probability decay factor.</value>
        [DefaultValue(0)]
        public double DelayProbabilityDecayFactor { get; set; } = 0;

        /// <summary>
        /// Gets or sets delay time to inject per InterceptionPoint in milliseconds.
        /// </summary>
        /// <value>The fixed delay ms.</value>
        [DefaultValue(-1)]
        public int FixedDelayMs { get; set; } = -1;

        /// <summary>
        /// Gets or sets the hit counts.
        /// </summary>
        /// <value>The hit counts.</value>
        public List<HitCount> HitCounts { get; set; }

        /// <summary>
        /// Gets or sets IL offset of the trap point.
        /// </summary>
        /// <value>The il offset.</value>
        [DefaultValue(-1)]
        public int ILOffset { get; set; } = -1;

        /// <summary>
        /// Gets or sets source line number of the trap point.
        /// </summary>
        /// <value>The line number.</value>
        [DefaultValue(-1)]
        public int LineNumber { get; set; } = -1;

        /// <summary>
        /// Gets or sets method name of the trap point.
        /// </summary>
        /// <value>The method.</value>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets maximum delay time to inject per InterceptionPoint in milliseconds.
        /// </summary>
        /// <value>The random delay ms.</value>
        [DefaultValue(-1)]
        public int RandomDelayMs { get; set; } = -1;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="TrapPlan"/> is repeat.
        /// </summary>
        /// <value><see langword="true"/> if repeat; otherwise, <see langword="false"/>.</value>
        public bool Repeat { get; set; }

        /// <summary>
        /// Gets or sets thread id of the trap point.
        /// </summary>
        /// <value>The thread identifier.</value>
        public string ThreadId { get; set; }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <param name="api">The API.</param>
        /// <param name="method">The method.</param>
        /// <param name="ilOffset">The il offset.</param>
        /// <returns>System.String.</returns>
        public static string GetID(string api, string method, int ilOffset)
        {
            return api + ";" + method + ";" + ilOffset;
        }

        /// <summary>
        /// Adds the hit count.
        /// </summary>
        /// <param name="local">The local.</param>
        /// <param name="global">The global.</param>
        public void AddHitCount(int local, int global = -1)
        {
            var hitCount = new HitCount(local, global);
            if (this.HitCounts == null)
            {
                this.HitCounts = new List<HitCount>();
            }

            this.HitCounts.Add(hitCount);
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetID()
        {
            return TrapPlan.GetID(this.API, this.Method, this.ILOffset);
        }

        /// <summary>
        /// Returns whether a trap delay should be injected, given a source of random values, using this trap's current delay probability.
        /// </summary>
        /// <param name="random">Pseud-random number generator.</param>
        /// <returns>Whether a trap delay should be injected.</returns>
        public bool ShouldInjectDelay(Random random)
        {
            if (this.currentDelayProbability < 0)
            {
                this.currentDelayProbability = this.DelayProbability;
            }

            if (random.NextDouble() < this.currentDelayProbability)
            {
                this.currentDelayProbability *= 1 - this.DelayProbabilityDecayFactor;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
