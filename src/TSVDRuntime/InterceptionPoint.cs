// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;

namespace TSVDRuntime
{
    /// <summary>
    /// InterceptionPoint runtime information.
    /// </summary>
    public class InterceptionPoint
    {
        /// <summary>
        /// Gets or sets InterceptionPoint id.
        /// </summary>
        public int GLobalHitCount { get; set; }

        /// <summary>
        /// Gets or sets call signature.
        /// </summary>
        public string API { get; set; }

        /// <summary>
        /// Gets or sets method signature.
        /// </summary>
        public string Method { get; set; }

        /// <summary>
        /// Gets or sets IL offset.
        /// </summary>
        public int ILOffset { get; set; }

        /// <summary>
        /// Gets or sets stack trace.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the assigned managed thread id.
        /// </summary>
        public int ThreadId { get; set; }

        /// <summary>
        /// Gets or sets the shortname of a InterceptionPoint.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets a indexID.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the InterceptionPoint is hit.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the active ThdNum of when the InterceptionPoint is hit.
        /// </summary>
        public int ActiveThdNum { get; set; }

        /// <summary>
        /// Gets or sets the touching OBJID for one InterceptionPoint.
        /// </summary>
        public Guid ObjID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether touching LockState for one InterceptionPoint.
        /// </summary>
        public bool LockState { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether InterceptionPoint is trapped or not.
        /// </summary>
        public bool Trapped { get; set; } = false;

        /// <summary>
        /// Gets or sets get or sets the concurrent opeartions number if the InterceptionPoint is trapped.
        /// </summary>
        public int ConcurrentOPs { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the InterceptionPoint is a write operation or not.
        /// </summary>
        public bool IsWrite { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the InterceptionPoint is danger trapped.
        /// </summary>
        public bool IsDanger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this InterceptionPoint is in the plan list.
        /// </summary>
        public bool IsInPlan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the internalID for this InterceptionPoint.
        /// </summary>
        public int LocalHitcount { get; set; }

        /// <summary>
        /// Gets or sets the delay credit.
        /// </summary>
        /// <value>The delay credit.</value>
        public int DelayCredit { get; set; }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>System.String.</returns>
        public string Tostring()
        {
            return this.API + "|" + this.Method + "|" + this.ILOffset.ToString() + "|" + this.GLobalHitCount.ToString() + "|" + this.LocalHitcount;
        }
    }
}
