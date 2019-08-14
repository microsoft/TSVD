// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace TSVDRuntime
{
    /// <summary>
    /// Class TrapPlans.
    /// </summary>
    public class TrapPlans
    {
        /// <summary>
        /// The wild card plans.
        /// </summary>
        private readonly List<TrapPlan> wildCardPlans = new List<TrapPlan>();

        /// <summary>
        /// The nonwild card plans.
        /// </summary>
        private readonly Dictionary<string, TrapPlan> nonwildCardPlans = new Dictionary<string, TrapPlan>();

        /// <summary>
        /// Gets or sets the plans.
        /// </summary>
        /// <value>The plans.</value>
        [XmlElement(ElementName = "TrapPlan")]
        public List<TrapPlan> Plans { get; set; } = new List<TrapPlan>();

        /// <summary>
        /// Counts this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int Count()
        {
            return this.Plans.Count;
        }

        /// <summary>
        /// Loads a plan into the lookup dictionary.
        /// </summary>
        /// <param name="plan">A <see cref="TrapPlan" />.</param>
        public void AddPlan(TrapPlan plan)
        {
            if (plan != null)
            {
                if (plan.API.Contains("*") || plan.Method.Contains("*"))
                {
                    this.wildCardPlans.Add(plan);
                }
                else
                {
                    string planId = plan.GetID();
                    if (this.nonwildCardPlans.ContainsKey(planId))
                    {
                        if (plan.HitCounts != null)
                        {
                            this.nonwildCardPlans[planId].HitCounts.AddRange(plan.HitCounts);
                        }
                    }
                    else
                    {
                        this.nonwildCardPlans[planId] = plan;
                    }
                }
            }
        }

        /// <summary>
        /// Loads a list of plans into the lookup directory.
        /// </summary>
        /// <param name="plans">A list of <see cref="TrapPlan" />.</param>
        public void AddPlans(List<TrapPlan> plans)
        {
            if (plans == null)
            {
                return;
            }

            foreach (var plan in plans)
            {
                this.AddPlan(plan);
            }
        }

        /// <summary>
        /// Finds a matching <see cref="TrapPlan" />.
        /// </summary>
        /// <param name="interceptionPoint">CUrrent InterceptionPoint.</param>
        /// <param name="globalHitcountWindow">Global hitcount window.</param>
        /// <param name="localHitcountWindow">Local hitcount window.</param>
        /// <returns>A matching <see cref="TrapPlan" />.</returns>
        public TrapPlan FindMatchingPlan(InterceptionPoint interceptionPoint, int globalHitcountWindow = 20, int localHitcountWindow = 5)
        {
            if (interceptionPoint == null || interceptionPoint.API == null)
            {
                return null;
            }

            var planId = TrapPlan.GetID(interceptionPoint.API, interceptionPoint.Method, interceptionPoint.ILOffset);
            if (this.nonwildCardPlans.ContainsKey(planId))
            {
                var plan = this.nonwildCardPlans[planId];
                bool isMatch = (plan.ThreadId == null || string.Equals(plan.ThreadId, interceptionPoint.ThreadId))
                    && (plan.HitCounts == null || plan.HitCounts.Count(x => x.IsMatch(interceptionPoint.LocalHitcount, localHitcountWindow, interceptionPoint.GLobalHitCount, globalHitcountWindow)) > 0);

                return isMatch ? plan : null;
            }

            foreach (var plan in this.wildCardPlans)
            {
                if (!SignatureMatchUtils.IsWildcardMatch(interceptionPoint.API, plan.API))
                {
                    continue;
                }

                if (plan.Method != null && !SignatureMatchUtils.IsWildcardMatch(interceptionPoint.Method, plan.Method))
                {
                    continue;
                }

                if (plan.ILOffset >= 0 && plan.ILOffset != interceptionPoint.ILOffset)
                {
                    continue;
                }

                if (plan.ThreadId != null && !string.Equals(plan.ThreadId, interceptionPoint.ThreadId))
                {
                    continue;
                }

                if (plan.HitCounts != null && plan.HitCounts.Count(x => x.IsMatch(interceptionPoint.LocalHitcount, localHitcountWindow, interceptionPoint.GLobalHitCount, globalHitcountWindow)) > 0)
                {
                    continue;
                }

                return plan;
            }

            return null;
        }
    }
}
