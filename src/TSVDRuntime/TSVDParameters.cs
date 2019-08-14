// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace TSVDRuntime
{
    /// <summary>
    /// Parameters used by TSVD algorithm.
    /// </summary>
    public class TSVDParameters
    {
        /// <summary>
        /// The blacklist.
        /// </summary>
        private readonly HashSet<string> blacklist = new HashSet<string>();

        /// <summary>
        /// The dangerous tp pairs.
        /// </summary>
        private readonly HashSet<string> dangerousTPPairs = new HashSet<string>();

        /// <summary>
        /// The queue that tracks the last 100 InterceptionPoints.
        /// </summary>
        private readonly Queue<InterceptionPoint> globalTPHistory = new Queue<InterceptionPoint>();

        /// <summary>
        /// The hit time.
        /// </summary>
        private readonly ConcurrentDictionary<string, int> hitTime = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// The known bugs.
        /// </summary>
        private readonly ConcurrentDictionary<string, byte> knownBugs = new ConcurrentDictionary<string, byte>();

        /// <summary>
        /// The Dictionary that tracks the last access of obj.
        /// </summary>
        private readonly Dictionary<Guid, Queue<InterceptionPoint>> lastInterceptionPointForOBJ = new Dictionary<Guid, Queue<InterceptionPoint>>();

        /// <summary>
        /// The per thread blocked tp.
        /// </summary>
        private readonly Dictionary<int, InterceptionPoint> perThreadBlockedTP = new Dictionary<int, InterceptionPoint>();

        /// <summary>
        /// The per thread last tp.
        /// </summary>
        private readonly ConcurrentDictionary<int, InterceptionPoint> perThreadLastTP = new ConcurrentDictionary<int, InterceptionPoint>();

        /// <summary>
        /// The per thread tp count.
        /// </summary>
        private readonly Dictionary<int, int> perThreadTPCount = new Dictionary<int, int>();

        /// <summary>
        /// The per tp hit count.
        /// </summary>
        private readonly ConcurrentDictionary<string, int> perTPHitCount = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// The plan buffer.
        /// </summary>
        private readonly List<string> planBuffer = new List<string>();

        /// <summary>
        /// The plan distance.
        /// </summary>
        private readonly int planDistance = 10000000;

        /// <summary>
        /// The thread safety InterceptionPoint cache.
        /// </summary>
        private readonly ConcurrentDictionary<string, ThreadSafetyInterceptionPoint> threadSafetyInterceptionPointCache = new ConcurrentDictionary<string, ThreadSafetyInterceptionPoint>();

        /// <summary>
        /// The dictionary maintains the current active trapped objects on InterceptionPoints.
        /// </summary>
        private readonly Dictionary<string, Dictionary<Guid, HashSet<Trap>>> trapPoints = new Dictionary<string, Dictionary<Guid, HashSet<Trap>>>();

        /// <summary>
        /// The known plans.
        /// </summary>
        private readonly HashSet<string> knownPlans = new HashSet<string>();

        /// <summary>
        /// Gets the controller configuration.
        /// </summary>
        private TSVDRuntimeConfiguration configuration;

        /// <summary>
        /// A global variable indicating if a trap is active.
        /// </summary>
        private bool isTrapActive;

        /// <summary>
        /// The last flushed.
        /// </summary>
        private DateTime lastFlushed = DateTime.Now;

        /// <summary>
        /// The last run bug logger.
        /// </summary>
        private FileLogger lastRunBugLogger;

        /// <summary>
        /// The last run plan logger.
        /// </summary>
        private FileLogger lastRunPlanLogger;

        /// <summary>
        /// Random number generator.
        /// </summary>
        private Random random;

        /// <summary>
        /// Gets the delay probability at dangerous InterceptionPoint.
        /// </summary>
        [DefaultValue(0.99)]
        public double DelayProbabilityAtDangerousInterceptionPoint => 0.99;

        /// <summary>
        /// Gets or sets the last run bug file.
        /// </summary>
        [DefaultValue("TSVD-allbugs.tsvdlog")]
        public string LastRunBugFile { get; set; } = "TSVD-allbugs.tsvdlog";

        /// <summary>
        /// Gets or sets the last run plan file.
        /// </summary>
        [DefaultValue("TSVD-preplans.tsvdlog")]
        public string LastRunPlanFile { get; set; } = "TSVD-preplans.tsvdlog";

        /// <summary>
        /// Gets the last trap window.
        /// </summary>
        [DefaultValue(5)]
        public int LastTrapWindow => 5;

        /// <summary>
        /// Gets or sets decaying factor of the DelayProbability.
        /// </summary>
        /// <value>The decay factor.</value>
        [DefaultValue(0.1)]
        public double DecayFactor { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets the delay per dangerous InterceptionPoint.
        /// </summary>
        /// <value>The delay per dangerous InterceptionPoint.</value>
        [DefaultValue(100)]
        public int DelayPerDangerousInterceptionPoint { get; set; } = 100;

        /// <summary>
        /// Gets or sets probability of setting a trap.
        /// </summary>
        /// <value>The delay probability.</value>
        [DefaultValue(1)]
        public double DelayProbability { get; set; } = 1;

        /// <summary>
        /// Gets or sets a value indicating whether a value indicating whether dangerous pairs will be detected.
        /// </summary>
        /// <value><see langword="true" /> if [detect dangerous pairs]; otherwise, <see langword="false" />.</value>
        [DefaultValue(true)]
        public bool DetectDangerousPairs { get; set; } = true;

        /// <summary>
        /// Gets or sets the matchable global ID window size.
        /// </summary>
        /// <value>The global hitcount window.</value>
        [DefaultValue(20)]
        public int GlobalHitcountWindow { get; set; } = 20;

        /// <summary>
        /// Gets or sets the size of the history window.
        /// </summary>
        /// <value>The size of the history window.</value>
        [DefaultValue(32)]
        public int HistoryWindowSize { get; set; } = 32;

        /// <summary>
        /// Gets or sets the infer limit.
        /// </summary>
        /// <value>The infer limit.</value>
        [DefaultValue(0.5)]
        public double InferLimit { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the size of the infer.
        /// </summary>
        /// <value>The size of the infer.</value>
        [DefaultValue(5)]
        public int InferSize { get; set; } = 5;

        /// <summary>
        /// Gets or sets the last tp window.
        /// </summary>
        /// <value>The last tp window.</value>
        [DefaultValue(5)]
        public int LastTPWindow { get; set; } = 5;

        /// <summary>
        /// Gets or sets the matchable internal ID window size.
        /// </summary>
        /// <value>The local hitcount window.</value>
        [DefaultValue(5)]
        public int LocalHitcountWindow { get; set; } = 5;

        /// <summary>
        /// Gets or sets log directory.
        /// </summary>
        /// <value>The log directory.</value>
        public string LogDirectory { get; set; } = null;

        /// <summary>
        /// Gets or sets maximum delay per InterceptionPoint.
        /// </summary>
        /// <value>The maximum delay per InterceptionPoint.</value>
        [DefaultValue(100)]
        public int MaxDelayPerInterceptionPoint { get; set; } = 100;

        /// <summary>
        /// Gets or sets the random seed.
        /// </summary>
        /// <value>The random seed.</value>
        public int RandomSeed { get; set; }

        /// <summary>
        /// Gets or sets traps plans.
        /// </summary>
        /// <value>The trap plans.</value>
        public TrapPlans TrapPlans { get; set; } = new TrapPlans();

        /// <summary>
        /// Initialize the controller.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public void Initialize(TSVDRuntimeConfiguration configuration)
        {
            this.configuration = configuration;
            if (this.LogDirectory != null && Directory.Exists(this.LogDirectory))
            {
                ControllerHelper.SetLoggingDirectory(this.LogDirectory);
                this.LastRunPlanFile = Path.Combine(this.LogDirectory, Path.GetFileName(this.LastRunPlanFile));
                this.LastRunBugFile = Path.Combine(this.LogDirectory, Path.GetFileName(this.LastRunBugFile));
            }

            this.isTrapActive = false;

            int seed = this.RandomSeed != 0 ? this.RandomSeed : Guid.NewGuid().GetHashCode();
            this.random = new Random(seed);
            ControllerHelper.Debug(string.Format("RandomSeed: {0}", seed));

            if (this.LastRunPlanFile != null)
            {
                if (File.Exists(this.LastRunPlanFile))
                {
                    this.LoadPlansFromLastRun(this.LastRunPlanFile);
                    File.Delete(this.LastRunPlanFile);
                }

                this.lastRunPlanLogger = new FileLogger(this.LastRunPlanFile);
            }

            if (this.LastRunBugFile != null)
            {
                if (File.Exists(this.LastRunBugFile))
                {
                    this.LoadKnownBuggyInterceptionPoints(this.LastRunBugFile);
                }

                this.lastRunBugLogger = new FileLogger(this.LastRunBugFile);
            }
        }

        /// <summary>
        /// Indicates if all required context are correctly specified.
        /// </summary>
        /// <returns>True if context are valid.</returns>
        /// </summary>
        public bool ParametersValid()
        {
            return true;
        }

        /// <summary>
        /// Converts to rchpointstart.
        /// </summary>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="parameters">The parameters.</param>
        /// </summary>
        public void InterceptionPointStart(InterceptionPoint interceptionPoint, object instance = null, object[] parameters = null)
        {
            /*
            I add more field variables to help the analysis. They are initialized in the constructor.
            when hitting a new InterceptionPoint here are the procedures in the implementation:
            1.assign the hit-seq for InterceptionPoint
            2.assign the global id for InterceptionPoint. It is duplicated with InterceptionPoint.id.
            3.update the InterceptionPoints history to infer the concurrent threads.
            4.find the dangerous pair for this InterceptionPoint
            5.find the the opeartion set that happens-before this InterceptionPoint
            6.check if find a bug
            7.construct the trap
            8.execute the trap
            9.locate the threads that are blocked by this trap
            */
            var newBugFound = false;

            if (this.configuration == null || this.configuration.ThreadSafetyGroups == null)
            {
                return;
            }

            this.UpdateTPHitCounts(interceptionPoint);

            TrapPlan trapPlan = this.TrapPlans.FindMatchingPlan(interceptionPoint, this.GlobalHitcountWindow, this.LocalHitcountWindow);
            interceptionPoint.IsInPlan = trapPlan != null;

            this.UpdateTPHistory(interceptionPoint);

            // This block is used for monitoring the lock.Now we can just skip the lock operations. this.Updatelockinformation(InterceptionPoint);
            if (this.LockRelatedOP(interceptionPoint))
            {
                return;
            }

            // Locate the APIs bypass the lock operation
            ThreadSafetyInterceptionPoint threadSafetyInterceptionPoint = this.GetThreadSafetyInterceptionPoint(interceptionPoint);
            if (threadSafetyInterceptionPoint == null ||
                (!threadSafetyInterceptionPoint.ThreadSafetyGroup.IsStatic && instance == null))
            {
                return;
            }

            // update the write/read InterceptionPoint
            interceptionPoint.IsWrite = threadSafetyInterceptionPoint.IsWriteAPI;

            interceptionPoint.ObjID = threadSafetyInterceptionPoint.ThreadSafetyGroup.IsStatic ? ObjectId.GetRefId(null) : ObjectId.GetRefId(instance);

            // this block will looking for the near-miss pair and learn the dependency.
            if ((interceptionPoint.IsInPlan == false) && (this.DetectDangerousPairs == true))
            {
                this.FindRacingTP(interceptionPoint);
                this.RemoveDependentInterceptionPoints(interceptionPoint);
            }

            var trapObjects = this.GetTrapObjects(threadSafetyInterceptionPoint);

            Trap trap = this.CheckForBugsAndStartTrap(interceptionPoint, threadSafetyInterceptionPoint, trapObjects, trapPlan, instance, out newBugFound);
            if (trap == null)
            {
                return;
            }

            this.FinishTrap(trapObjects, trap, interceptionPoint);

            if (this.configuration.ThrowExceptionOnRace & newBugFound)
            {
                throw new Exception("ThreadSafety bugs detected by TSVD");
            }

            return;
        }

        /// <summary>
        /// Adds the new plan.
        /// </summary>
        /// <param name="interceptionPointInfo">The InterceptionPoint information.</param>
        /// <param name="frequency">The frequency.</param>
        private void AddNewPlan(string interceptionPointInfo, int frequency)
        {
            TrapPlan trapPlan = new TrapPlan();
            string[] tokens = interceptionPointInfo.Split('|');
            trapPlan.API = tokens[0];
            trapPlan.Method = tokens[1];
            trapPlan.ILOffset = int.Parse(tokens[2]);
            trapPlan.AddHitCount(int.Parse(tokens[4]), int.Parse(tokens[3]));
            trapPlan.FixedDelayMs = this.DelayPerDangerousInterceptionPoint;
            trapPlan.Repeat = frequency > 1;
            lock (this.TrapPlans)
            {
                this.TrapPlans.AddPlan(trapPlan);
            }
        }

        /// <summary>
        /// Checks for bugs and start trap.
        /// </summary>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        /// <param name="threadSafetyInterceptionPoint">The thread safety InterceptionPoint.</param>
        /// <param name="trapObjects">The trap objects.</param>
        /// <param name="trapPlan">The trap plan.</param>
        /// <param name="instance">The instance.</param>
        /// <param name="bugFound">If set to <see langword="true" /> then a bug has been found.</param>
        /// <returns>Trap.</returns>
        private Trap CheckForBugsAndStartTrap(InterceptionPoint interceptionPoint, ThreadSafetyInterceptionPoint threadSafetyInterceptionPoint, Dictionary<Guid, HashSet<Trap>> trapObjects, TrapPlan trapPlan, object instance, out bool bugFound)
        {
            int danger = -1;
            Semaphore semaphore = null;
            double prob = 1.0;
            int sleepDuration = 0;
            Guid objectId = interceptionPoint.ObjID;
            bool continueAfterBug = false;
            bugFound = false;
            Trap newTrap = null;
            lock (trapObjects)
            {
                // Check if a thread-safety bug is found
                if (trapObjects.ContainsKey(objectId))
                {
                    foreach (Trap trap in trapObjects[objectId])
                    {
                        ThreadSafetyInterceptionPoint runningTP = this.GetThreadSafetyInterceptionPoint(trap.InterceptionPoint);
                        if (threadSafetyInterceptionPoint.IsWriteAPI || runningTP.IsWriteAPI)
                        {
                            bugFound = true;
                            interceptionPoint.StackTrace = Environment.StackTrace;

                            // DebugLog.Log("HitBug " + InterceptionPoint.Tostring() + " " + runningt.InterceptionPoint.Tostring());
                            ControllerHelper.WriteBug(trap, interceptionPoint);
                            this.LogBugForNextRun(interceptionPoint, trap.InterceptionPoint);

                            // For some InterceptionPoints, keep execution even hitting a bug. It may expose more bugs.
                            if ((trapPlan != null) && (!trapPlan.Repeat))
                            {
                                continueAfterBug = true;
                            }
                        }
                    }
                }

                if (bugFound && (!continueAfterBug))
                {
                    // DebugLog.Log("HIt Bug and exit " + InterceptionPoint.Tostring());
                    return null;
                }

                // else
                {
                    // check if the InterceptionPoint match with dangerous list
                    danger = this.IsInsideDangerList(interceptionPoint.Location);

                    if ((danger < 0) && (!interceptionPoint.IsInPlan))
                    {
                        // exit when not matching and not in the plan
                        return null;
                    }

                    // prepare to set up a trap if (threadSafetyInterceptionPoint.IsWriteAPI && !this.isTrapActive)
                    if ((!this.isTrapActive || (danger >= 0) || interceptionPoint.IsInPlan) && ((trapPlan != null) || threadSafetyInterceptionPoint.IsWriteAPI))
                    {
                        prob = trapPlan != null ? trapPlan.DelayProbability : this.DelayProbability;

                        if (danger >= 0)
                        {
                            prob = this.DelayProbabilityAtDangerousInterceptionPoint - (this.DecayFactor * danger);
                        }

                        if (this.random.NextDouble() <= prob)
                        {
                            // Get stack trace only if setting up a trap.
                            interceptionPoint.StackTrace = Environment.StackTrace;

                            semaphore = new Semaphore(0, 1);
                            sleepDuration = trapPlan == null ?
                                this.random.Next(this.MaxDelayPerInterceptionPoint)
                                : trapPlan.FixedDelayMs;
                            if (danger >= 0)
                            {
                                sleepDuration = this.DelayPerDangerousInterceptionPoint;
                            }

                            newTrap = new Trap
                            {
                                ObjectId = objectId,
                                Semaphore = semaphore,
                                InterceptionPoint = interceptionPoint,
                                Delay = sleepDuration,
                            };

                            ControllerHelper.WriteTrap(newTrap);

                            if (!trapObjects.ContainsKey(objectId))
                            {
                                trapObjects.Add(objectId, new HashSet<Trap>());
                            }

                            trapObjects[objectId].Add(newTrap);

                            this.isTrapActive = true;
                        }
                    }
                }
            }

            return newTrap;
        }

        /// <summary>
        /// Finds the racing tp.
        /// </summary>
        /// <param name="tp">The tp.</param>
        /// <returns>System.String.</returns>
        private string FindRacingTP(InterceptionPoint tp)
        {
            List<InterceptionPoint> list = new List<InterceptionPoint>();

            // this block goes through the last-k tps that access the same object to find the dangerous list.co
            lock (this.lastInterceptionPointForOBJ)
            {
                if (this.lastInterceptionPointForOBJ.ContainsKey(tp.ObjID))
                {
                    Queue<InterceptionPoint> q = this.lastInterceptionPointForOBJ[tp.ObjID];
                    foreach (var tp2 in q)
                    {
                        // from different thread and at least one is write
                        if ((tp2.ThreadId != tp.ThreadId) && (tp.IsWrite || tp2.IsWrite))
                        {
                            // close enough from timing prespective
                            if (tp2.Timestamp.AddMilliseconds(this.planDistance) > tp.Timestamp)
                            {
                                // the next condition is always true as we ignore the lock. Every LockState is false.
                                if ((tp2.LockState == false) || (tp.LockState == false))
                                {
                                    if ((tp2.ActiveThdNum > 1) || (tp.ActiveThdNum > 1))
                                    {
                                        list.Add(tp2);
                                    }
                                }
                            }
                        }
                    }

                    // update the last access for this object
                    q.Enqueue(tp);
                    if (q.Count > this.LastTPWindow)
                    {
                        q.Dequeue();
                    }
                }
                else
                {
                    this.lastInterceptionPointForOBJ[tp.ObjID] = new Queue<InterceptionPoint>();
                    this.lastInterceptionPointForOBJ[tp.ObjID].Enqueue(tp);
                }
            }

            // update the dangerous pair list
            string s = string.Empty;
            HashSet<string> temp = new HashSet<string>();
            foreach (InterceptionPoint tp2 in list)
            {
                string st = tp.Tostring() + " ! " + tp2.Tostring();
                string st2 = tp2.Tostring() + " ! " + tp.Tostring();
                Tuple<string, string> tuple = new Tuple<string, string>(tp.Tostring(), tp2.Tostring());
                string shortst = this.GetPairID(st, st2);

                string pairname = this.GetPairID(tp.Location, tp2.Location);
                if (this.blacklist.Contains(pairname))
                {
                    // the blacklist contains the bugs found in previous ROUNDS. DebugLog.Log("Skip blacked pair " + shortst);
                    continue;
                }

                bool flag = false;
                lock (this.dangerousTPPairs)
                {
                    if (this.hitTime.ContainsKey(tp.Location) && this.hitTime.ContainsKey(tp2.Location) && (this.hitTime[tp.Location] + this.hitTime[tp2.Location] >= 10) && this.dangerousTPPairs.Contains(shortst))
                    {
                        continue;
                    }

                    // the hittime is how many time this dangerous pair has caused trap. It is used for decay.
                    this.hitTime[tp.Location] = 0;
                    this.hitTime[tp2.Location] = 0;
                    this.dangerousTPPairs.Add(shortst);
                    flag = true;
                }

                if (flag)
                {
                    // DebugLog.Log("AddDangerList : " + st);
                    this.LogPlanForNextRun(st);
                }
            }

            return s;
        }

        /// <summary>
        /// Finishes the trap.
        /// </summary>
        /// <param name="trapObjects">The trap objects.</param>
        /// <param name="trap">The trap.</param>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        private void FinishTrap(Dictionary<Guid, HashSet<Trap>> trapObjects, Trap trap, InterceptionPoint interceptionPoint)
        {
            // execute the sleep
            if (trap != null)
            {
                // DebugLog.Log("Start Sleeping  for tp " + InterceptionPoint.Tostring());
                trap.Semaphore.WaitOne(trap.Delay);
                interceptionPoint.DelayCredit = this.InferSize;

                // figure out the which thread is blocked by this trap.
                {
                    lock (this.perThreadLastTP)
                    {
                        foreach (var x in this.perThreadLastTP.Keys)
                        {
                            InterceptionPoint tp2 = this.perThreadLastTP[x];
                            if (interceptionPoint.Timestamp.AddMilliseconds(this.DelayPerDangerousInterceptionPoint * this.InferLimit) > tp2.Timestamp)
                            {
                                this.perThreadBlockedTP[x] = interceptionPoint;

                                // DebugLog.Log(x + " is blocked by " + InterceptionPoint.Tostring());
                            }
                        }
                    }
                }

                interceptionPoint.Trapped = true;

                lock (trapObjects)
                {
                    trapObjects.Remove(trap.ObjectId);
                }

                this.isTrapActive = false;
            }
        }

        /// <summary>
        /// Gets the pair identifier.
        /// </summary>
        /// <param name="s1">The s1.</param>
        /// <param name="s2">The s2.</param>
        /// <returns>System.String.</returns>
        private string GetPairID(string s1, string s2)
        {
            return s1.CompareTo(s2) > 0 ? s1 + " " + s2 : s2 + " " + s1;
        }

        /// <summary>
        /// Get thread safety group details of the InterceptionPoint.
        /// </summary>
        /// <param name="interceptionPoint">InterceptionPoint.</param>
        /// <returns>Thread safety InterceptionPoint.</returns>
        private ThreadSafetyInterceptionPoint GetThreadSafetyInterceptionPoint(InterceptionPoint interceptionPoint)
        {
            if (this.threadSafetyInterceptionPointCache.ContainsKey(interceptionPoint.API))
            {
                return this.threadSafetyInterceptionPointCache[interceptionPoint.API];
            }

            foreach (ThreadSafetyGroup threadSafetyGroup in this.configuration.ThreadSafetyGroups)
            {
                foreach (string writeAPI in threadSafetyGroup.WriteAPIs)
                {
                    if (SignatureMatchUtils.IsWildcardMatch(interceptionPoint.API, writeAPI))
                    {
                        var tsInterceptionPoint = new ThreadSafetyInterceptionPoint()
                        {
                            ThreadSafetyGroup = threadSafetyGroup,
                            IsWriteAPI = true,
                        };
                        this.threadSafetyInterceptionPointCache[interceptionPoint.API] = tsInterceptionPoint;
                        return tsInterceptionPoint;
                    }
                }

                foreach (string readAPI in threadSafetyGroup.ReadAPIs)
                {
                    if (SignatureMatchUtils.IsWildcardMatch(interceptionPoint.API, readAPI))
                    {
                        var tsInterceptionPoint = new ThreadSafetyInterceptionPoint()
                        {
                            ThreadSafetyGroup = threadSafetyGroup,
                            IsWriteAPI = false,
                        };
                        this.threadSafetyInterceptionPointCache[interceptionPoint.API] = tsInterceptionPoint;
                        return tsInterceptionPoint;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the trap objects.
        /// </summary>
        /// <param name="threadSafetyInterceptionPoint">The thread safety InterceptionPoint.</param>
        /// <returns>Dictionary&lt;Guid, HashSet&lt;Trap&gt;&gt;.</returns>
        private Dictionary<Guid, HashSet<Trap>> GetTrapObjects(ThreadSafetyInterceptionPoint threadSafetyInterceptionPoint)
        {
            string groupName = threadSafetyInterceptionPoint.ThreadSafetyGroup.Name;
            Dictionary<Guid, HashSet<Trap>> trapObjects = null;
            lock (this.trapPoints)
            {
                if (!this.trapPoints.ContainsKey(groupName))
                {
                    this.trapPoints.Add(groupName, new Dictionary<Guid, HashSet<Trap>>());
                }

                trapObjects = this.trapPoints[groupName];
            }

            return trapObjects;
        }

        /// <summary>
        /// Determines whether [is inside danger list] [the specified TPSTR].
        /// </summary>
        /// <param name="tpstr">The TPSTR.</param>
        /// <returns>System.Int32.</returns>
        private int IsInsideDangerList(string tpstr)
        {
            lock (this.dangerousTPPairs)
            {
                if (this.hitTime.ContainsKey(tpstr))
                {
                    return this.hitTime[tpstr]++;
                }
            }

            // return false;
            return -1;
        }

        /// <summary>
        /// Loads the known buggy InterceptionPoints.
        /// </summary>
        /// <param name="file">The file.</param>
        private void LoadKnownBuggyInterceptionPoints(string file)
        {
            using (TextReader tr = new StreamReader(file))
            {
                try
                {
                    string line;
                    while ((line = tr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        string[] tokens = line.Split(' ');
                        if (tokens.Length != 2)
                        {
                            continue;
                        }

                        this.blacklist.Add(this.GetPairID(tokens[0], tokens[1]));
                    }
                }
                catch
                {
                    ControllerHelper.Debug("ERROR when accessing the prebugfile");
                }
            }
        }

        /// <summary>
        /// Loads the plans from last run.
        /// </summary>
        /// <param name="file">The file.</param>
        private void LoadPlansFromLastRun(string file)
        {
            try
            {
                using (TextReader tr = new StreamReader(file))
                {
                    string line;
                    Dictionary<string, int> planset = new Dictionary<string, int>();
                    while ((line = tr.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        string[] tokens = line.Split(' ');
                        if (tokens.Length != 3)
                        {
                            continue;
                        }

                        if (!planset.ContainsKey(tokens[0]))
                        {
                            planset[tokens[0]] = 0;
                        }

                        if (!planset.ContainsKey(tokens[2]))
                        {
                            planset[tokens[2]] = 0;
                        }

                        planset[tokens[0]]++;
                        planset[tokens[2]]++;
                    }

                    foreach (string s in planset.Keys)
                    {
                        this.AddNewPlan(s, planset[s]);
                    }
                }
            }
            catch
            {
                ControllerHelper.Debug("PrePlan file access error");
            }
        }

        /// <summary>
        /// Locks the related op.
        /// </summary>
        /// <param name="tp">The tp.</param>
        /// <returns><see langword="true" /> if XXXX, <see langword="false" /> otherwise.</returns>
        private bool LockRelatedOP(InterceptionPoint tp)
        {
            if (tp.API.StartsWith("System.Threading.Monitor.Enter") || tp.API.StartsWith("System.Threading.Monitor.Exit"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Logs the bug for next run.
        /// </summary>
        /// <param name="tp1">The TP1.</param>
        /// <param name="tp2">The TP2.</param>
        private void LogBugForNextRun(InterceptionPoint tp1, InterceptionPoint tp2)
        {
            string bugSignature = this.GetPairID(tp1.Location, tp2.Location);
            if (!this.knownBugs.ContainsKey(bugSignature) && !this.blacklist.Contains(bugSignature))
            {
                this.knownBugs[bugSignature] = 1;
                this.lastRunBugLogger.Log(bugSignature);
            }
        }

        /// <summary>
        /// Logs the plan for next run.
        /// </summary>
        /// <param name="plan">The plan.</param>
        private void LogPlanForNextRun(string plan)
        {
            if (!this.knownPlans.Contains(plan))
            {
                this.knownPlans.Add(plan);
                this.planBuffer.Add(plan);
                if ((DateTime.Now - this.lastFlushed).TotalMilliseconds > 100)
                {
                    this.lastRunPlanLogger.Log(this.planBuffer);
                    this.planBuffer.Clear();
                    this.lastFlushed = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Removes the danger item.
        /// </summary>
        /// <param name="tpstr1">The TPSTR1.</param>
        /// <param name="tpstr2">The TPSTR2.</param>
        private void RemoveDangerItem(string tpstr1, string tpstr2)
        {
            string st = this.GetPairID(tpstr1, tpstr2);
            lock (this.dangerousTPPairs)
            {
                this.dangerousTPPairs.Remove(st);
            }
        }

        /// <summary>
        /// Removes the dependent InterceptionPoints.
        /// </summary>
        /// <param name="tp">The tp.</param>
        private void RemoveDependentInterceptionPoints(InterceptionPoint tp)
        {
            lock (this.perThreadLastTP)
            {
                if (this.perThreadBlockedTP.ContainsKey(tp.ThreadId))
                {
                    // find the InterceptionPoint that blocked this thread if exists.
                    InterceptionPoint tp2 = this.perThreadBlockedTP[tp.ThreadId];

                    // delaycredit is k means a InterceptionPoint block the next-k operation in other thread
                    if (tp2.DelayCredit > 0)
                    {
                        this.RemoveDangerItem(tp.Location, tp2.Location);

                        // DebugLog.Log("HB Order " + tp.Tostring() + " " + tp2.Tostring());
                        tp2.DelayCredit--;
                    }
                    else
                    {
                        this.perThreadBlockedTP.Remove(tp.ThreadId);
                    }
                }
            }
        }

        /// <summary>
        /// Shoulds the return without trap.
        /// </summary>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        /// <param name="instance">The instance.</param>
        /// <returns><see langword="true" /> if XXXX, <see langword="false" /> otherwise.</returns>
        private bool ShouldReturnWithoutTrap(InterceptionPoint interceptionPoint, object instance)
        {
            // this.Updatelockinformation(InterceptionPoint);
            if (this.LockRelatedOP(interceptionPoint))
            {
                return true;
            }

            // Locate the APIs bypass the lock operation
            ThreadSafetyInterceptionPoint threadSafetyInterceptionPoint = this.GetThreadSafetyInterceptionPoint(interceptionPoint);
            if (threadSafetyInterceptionPoint == null)
            {
                return true;
            }

            if (!threadSafetyInterceptionPoint.ThreadSafetyGroup.IsStatic && instance == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Updates the tp history.
        /// </summary>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        private void UpdateTPHistory(InterceptionPoint interceptionPoint)
        {
            // update the last operation for each thread
            lock (this.perThreadLastTP)
            {
                this.perThreadLastTP[interceptionPoint.ThreadId] = interceptionPoint;
            }

            // this block monitor the history of InterceptionPoints to infer the concurrent threads
            lock (this.globalTPHistory)
            {
                int thdid = interceptionPoint.ThreadId;
                this.globalTPHistory.Enqueue(interceptionPoint);
                if (this.perThreadTPCount.ContainsKey(thdid))
                {
                    this.perThreadTPCount[thdid]++;
                }
                else
                {
                    this.perThreadTPCount[thdid] = 1;
                }

                if (this.globalTPHistory.Count > this.HistoryWindowSize)
                {
                    InterceptionPoint tp = this.globalTPHistory.Dequeue();
                    this.perThreadTPCount[tp.ThreadId]--;
                    if (this.perThreadTPCount[tp.ThreadId] == 0)
                    {
                        this.perThreadTPCount.Remove(tp.ThreadId);
                    }
                }

                interceptionPoint.ActiveThdNum = this.perThreadTPCount.Keys.Count;
            }
        }

        /// <summary>
        /// Updates the tp hit counts.
        /// </summary>
        /// <param name="interceptionPoint">The InterceptionPoint.</param>
        private void UpdateTPHitCounts(InterceptionPoint interceptionPoint)
        {
            string location = interceptionPoint.API + "|" + interceptionPoint.Method + "|" + interceptionPoint.ILOffset.ToString();
            interceptionPoint.Location = location;
            lock (this.perTPHitCount)
            {
                if (!this.perTPHitCount.ContainsKey(location))
                {
                    this.perTPHitCount[location] = 0;
                }

                interceptionPoint.LocalHitcount = this.perTPHitCount[location];
                this.perTPHitCount[location] = interceptionPoint.LocalHitcount + 1;
            }
        }
    }
}
