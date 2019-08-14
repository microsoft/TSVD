// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;

namespace TSVDRuntime
{
    /// <summary>
    /// Implements a <see cref="ITrapController"/> that injects delay at random interceptionPoints.
    /// </summary>
    public class RandomizedTrapController : ITrapController
    {
        /// <summary>
        /// Null object instance for static methods.
        /// </summary>
        private readonly object nullObject = new object();

        /// <summary>
        /// The dictionary maintains the current active trapped objects on InterceptionPoints.
        /// </summary>
        private Dictionary<string, Dictionary<Guid, Trap>> trapPoints;

        /// <summary>
        /// This hashtable keeps active trapped objects that are for write operations.
        /// </summary>
        private HashSet<string> writeTrapPoints = new HashSet<string>();

        /// <summary>
        /// Random number generator.
        /// </summary>
        private Random random;

        /// <summary>
        /// Bug logger.
        /// </summary>
        private FileLogger bugLogger;

        /// <summary>
        /// A global variable indicating if a trap is active.
        /// </summary>
        private bool isTrapActive;

        /// <summary>
        /// Gets the controller configuration.
        /// </summary>
        private TSVDRuntimeConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizedTrapController"/> class.
        /// </summary>
        public RandomizedTrapController()
        {
        }

        /// <summary>
        /// Gets or sets the random seed.
        /// </summary>
        public int RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets delay probability.
        /// </summary>
        public double DelayProbability { get; set; }

        /// <summary>
        /// Gets or sets maximum delay time to inject per InterceptionPoint in milliseconds.
        /// </summary>
        public int MaxDelayPerInterceptionPoint { get; set; }

        /// <inheritdoc/>
        public void Initialize(TSVDRuntimeConfiguration configuration)
        {
            this.configuration = configuration;
            this.trapPoints = new Dictionary<string, Dictionary<Guid, Trap>>();
            this.writeTrapPoints = new HashSet<string>();

            if (this.configuration.BugLogPath != null)
            {
                this.bugLogger = new FileLogger(LogHelper.GetFilePathWithExecutionId(this.configuration.BugLogPath));
            }

            this.isTrapActive = false;

            int seed = this.RandomSeed != 0 ? this.RandomSeed : Guid.NewGuid().GetHashCode();
            this.random = new Random(seed);
            ControllerHelper.Debug(string.Format("RandomSeed: {0}", seed));
        }

        /// <inheritdoc/>
        public bool ParametersValid()
        {
            return this.TrapsParamSpecified();
        }

        /// <inheritdoc/>
        public void InterceptionPointStart(InterceptionPoint interceptionPoint, object instance = null, object[] parameters = null)
        {
            /*
            if (!this.isTrapActive && bugFound)
                throw new Exception("ThreadSafety bugs detected by Torch");
            */
            bool newTrapsAllowed = true;
            bool newBugFound = false;

            if (this.configuration == null || this.configuration.ThreadSafetyGroups == null)
            {
                return;
            }

            ThreadSafetyInterceptionPoint threadSafetyInterceptionPoint = ControllerHelper.GetThreadSafetyInterceptionPoint(this.configuration.ThreadSafetyGroups, interceptionPoint);
            if (threadSafetyInterceptionPoint == null)
            {
                return;
            }

            if (!threadSafetyInterceptionPoint.ThreadSafetyGroup.IsStatic && instance == null)
            {
                return;
            }

            string groupName = threadSafetyInterceptionPoint.ThreadSafetyGroup.Name;
            Dictionary<Guid, Trap> trapObjects = null;
            lock (this.trapPoints)
            {
                if (!this.trapPoints.ContainsKey(groupName))
                {
                    this.trapPoints.Add(groupName, new Dictionary<Guid, Trap>());
                    if (threadSafetyInterceptionPoint.IsWriteAPI)
                    {
                        this.writeTrapPoints.Add(groupName);
                    }
                }

                trapObjects = this.trapPoints[groupName];
            }

            Semaphore semaphore = null;
            int sleepDuration = 0;
            Guid keyInstance = ObjectId.GetRefId(null);
            lock (trapObjects)
            {
                if (!threadSafetyInterceptionPoint.ThreadSafetyGroup.IsStatic)
                {
                    keyInstance = ObjectId.GetRefId(instance);
                }

                // Check if a trap on this object is live
                Trap trap = null;
                trapObjects.TryGetValue(keyInstance, out trap);

                // A bug is found if there is a trap on this object and
                // either this access or trapped access is a write
                if (trap != null &&
                    (threadSafetyInterceptionPoint.IsWriteAPI ||
                    trap.ThreadSafetyInterceptionPoint.IsWriteAPI))
                {
                    // Get stack track when a bug is found.
                    interceptionPoint.StackTrace = Environment.StackTrace;
                    ControllerHelper.WriteBug(trap, interceptionPoint);
                    newBugFound = true;

                    try
                    {
                        trap.Semaphore.Release();
                    }
                    catch (SemaphoreFullException)
                    {
                    }
                }
                else if (newTrapsAllowed)
                {
                    // prepare to set up a trap
                    if (!this.isTrapActive)
                    {
                        if (threadSafetyInterceptionPoint.IsWriteAPI && this.random.NextDouble() <= this.DelayProbability)
                        {
                            // Get stack trace only if setting up a trap.
                            interceptionPoint.StackTrace = Environment.StackTrace;

                            semaphore = new Semaphore(0, 1);
                            sleepDuration = this.random.Next(this.MaxDelayPerInterceptionPoint);

                            trap = new Trap
                            {
                                ObjectId = keyInstance,
                                Semaphore = semaphore,
                                InterceptionPoint = interceptionPoint,
                                ThreadSafetyInterceptionPoint = threadSafetyInterceptionPoint,
                                Delay = sleepDuration,
                            };

                            ControllerHelper.WriteTrap(trap);
                            trapObjects.Add(keyInstance, trap);
                            this.isTrapActive = true;
                        }
                    }
                }
            }

            if (semaphore != null)
            {
                semaphore.WaitOne(sleepDuration);

                lock (trapObjects)
                {
                    trapObjects.Remove(keyInstance);
                }

                this.isTrapActive = false;
            }

            if (this.configuration.ThrowExceptionOnRace && newBugFound)
            {
                throw new Exception("ThreadSafety bugs detected by Torch");
            }

            return;
        }

        /// <summary>
        /// Check if traps context are specified.
        /// </summary>
        /// <returns>True, if traps params are specified; false otherwise.</returns>
        private bool TrapsParamSpecified()
        {
            return this.DelayProbability >= 0 && this.DelayProbability <= 1;
        }
    }
}
