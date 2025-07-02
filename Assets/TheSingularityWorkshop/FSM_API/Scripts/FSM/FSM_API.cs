using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// The core API for managing Finite State Machines (FSMs).
    /// This system allows you to define flexible FSM structures, create instances
    /// of those FSMs for various game objects or entities, and manage their updates.
    /// <para>All FSM operations are designed to run on the main thread for thread safety.</para>
    /// <para>Use <see cref="CreateFiniteStateMachine"/> to design your FSM, then
    /// <see cref="CreateInstance"/> to bring it to life and attach it to your game logic.</para>
    /// </summary>
    public static class FSM_API
    {
        private static readonly Queue<Action> _deferredModifications = new();
        private static readonly Dictionary<FSMHandle, int> _errorCounts = new();
        private static readonly Dictionary<string, int> _fsmDefinitionErrorCounts = new();
        public static int ErrorCountThreshold { get; set; } = 5;
        public static int DefinitionErrorThreshold { get; set; } = 3;

        /// <summary>
        /// Occurs when an internal, non-state-logic API operation throws an unexpected exception.
        /// Provides a mechanism for users to capture internal API errors without forcing runtime logging.
        /// </summary>
        public static event Action<string, Exception> OnInternalApiError;

        /// <summary>
        /// Safely invokes the OnInternalApiError event, checking for null subscribers.
        /// This method is the only way to trigger OnInternalApiError from within FSM_API.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="ex">The exception that occurred, if any.</param>
        internal static void InvokeInternalApiError(string message, Exception ex)
        {
            OnInternalApiError?.Invoke(message, ex);
        }

        /// <summary>
        /// Represents a container for an FSM definition and all its active instances.
        /// Used internally by the FSM_API to manage updates.
        /// </summary>
        public class FsmBucket
        {
            public FSM Definition;
            public List<FSMHandle> Instances = new();
            public int ProcessRate;      // –1 (every frame), 0 (event-driven), or >0 (every Nth frame)
            public int Counter;          // Internal counter for frame-skipping
        }

        /// <summary>
        /// Sets a time limit (in milliseconds) for how long an FSM processing group update can take
        /// before a warning message is logged via OnInternalApiError. Helps identify performance bottlenecks.
        /// Default is 5ms.
        /// </summary>
        public static long TickPerformanceWarningThresholdMs { get; set; } = 5;

        // _buckets: Stores all FSM definitions and their instances, organized by
        // their processing group (e.g., "Update", "FixedUpdate") and then by FSM name.
        public static Dictionary<string, Dictionary<string, FsmBucket>> _buckets = new();

        /// <summary>
        /// Safely retrieves the dictionary of FSMs for a given processing group.
        /// If the group does not exist, it will be created automatically.
        /// </summary>
        /// <param name="processingGroup">The name of the processing group (e.g., "Update", "FixedUpdate").</param>
        /// <returns>A dictionary containing FSM buckets for the specified group, keyed by FSM name.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="processingGroup"/> is null or empty.</exception>
        private static Dictionary<string, FsmBucket> GetOrCreateBucketCategory(string processingGroup)
        {
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets))
            {
                categoryBuckets = new Dictionary<string, FsmBucket>();
                _buckets[processingGroup] = categoryBuckets;
            }
            return categoryBuckets;
        }

        /// <summary>
        /// Ensures an FSM processing group exists. FSMs registered under this group
        /// will be processed when the corresponding <c>Tick</c> method (e.g., <see cref="Update"/>, <see cref="FixedUpdate"/>) is called.
        /// </summary>
        /// <param name="processingGroup">The unique name for the processing group.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="processingGroup"/> is null or empty.</exception>
        public static void CreateProcessingGroup(string processingGroup)
        {
            GetOrCreateBucketCategory(processingGroup);
        }

        /// <summary>
        /// Removes an entire FSM processing group, along with all FSM definitions and
        /// their active instances within that group. This effectively unregisters
        /// all FSMs associated with it.
        /// </summary>
        /// <param name="processingGroup">The name of the processing group to remove.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="processingGroup"/> is null or empty.</exception>
        public static void RemoveProcessingGroup(string processingGroup)
        {
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (_buckets.TryGetValue(processingGroup, out var categoryBuckets))
            {
                foreach (var bucket in categoryBuckets.Values)
                {
                    bucket.Instances.Clear();
                    _fsmDefinitionErrorCounts.Remove(bucket.Definition.Name);
                }
                _buckets.Remove(processingGroup);
            }
            else
            {
                InvokeInternalApiError($"Attempted to remove non-existent processing group '{processingGroup}'.", null);
            }
        }

        /// <summary>
        /// Initiates the definition process for a new Finite State Machine,
        /// or provides an <see cref="FSMBuilder"/> to modify an existing FSM's definition.
        /// </summary>
        /// <param name="fsmName">A unique name for this FSM definition.</param>
        /// <param name="processRate">The default process rate for new FSMs.</param>
        /// <param name="processingGroup">The processing group this FSM belongs to.</param>
        /// <returns>An <see cref="FSMBuilder"/> object, which you use to define or modify the FSM.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null or empty.</exception>
        public static FSMBuilder CreateFiniteStateMachine(
            string fsmName = "UnNamedFSM",
            int processRate = 0, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }
            if (processRate < -1)
            {
                InvokeInternalApiError($"Invalid processRate '{processRate}' for FSM '{fsmName}'. Setting to 0 (event-driven).", null);
                processRate = 0;
            }

            var buckets = GetOrCreateBucketCategory(processingGroup);
            if (buckets.TryGetValue(fsmName, out var existingBucket))
            {
                // FSM definition already exists, return a builder initialized with its data for modification.
                return new FSMBuilder(existingBucket.Definition);
            }

            // New FSM definition, return a fresh builder.
            return new FSMBuilder(fsmName, processRate, processingGroup);
        }

        /// <summary>
        /// Internally registers or updates a fully defined FSM into the system. This method is called by
        /// <see cref="FSMBuilder.BuildDefinition()"/> and should not be invoked directly by users.
        /// </summary>
        /// <param name="fsmName">The unique name of the FSM definition.</param>
        /// <param name="fsm">The complete <see cref="FSM"/> definition object.</param>
        /// <param name="processRate">The update rate for this FSM.</param>
        /// <param name="processingGroup">The processing group this FSM belongs to.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/>, <paramref name="fsm"/>, or <paramref name="processingGroup"/> is null or empty.</exception>
        internal static void Register(
            string fsmName,
            FSM fsm,
            int processRate, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty during registration.", nameof(fsmName));
            }
            if (fsm == null)
            {
                throw new ArgumentNullException(nameof(fsm), "FSM definition cannot be null during registration.");
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty during registration.", nameof(processingGroup));
            }

            var buckets = GetOrCreateBucketCategory(processingGroup);
            if (buckets.TryGetValue(fsmName, out var existingBucket))
            {
                // FSM already exists, update its definition and process rate.
                // Existing instances will automatically use the new definition on their next tick.
                existingBucket.Definition = fsm;
                existingBucket.ProcessRate = processRate;
                existingBucket.Counter = processRate > 0 ? processRate : 0; // Reset counter for new rate
                InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' definition updated at runtime.", null);
            }
            else
            {
                // New FSM definition
                buckets[fsmName] = new FsmBucket
                {
                    Definition = fsm,
                    ProcessRate = processRate,
                    Counter = processRate > 0 ? processRate : 0,
                };
                InvokeInternalApiError($"FSM '{fsmName}' in processing group '{processingGroup}' newly registered.", null);
            }
        }

        /// <summary>
        /// Checks if an FSM definition with the given name exists within the specified processing group.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to check.</param>
        /// <param name="processingGroup">The processing group to search within.</param>
        /// <returns><c>true</c> if the FSM definition exists, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null or empty.</exception>
        public static bool Exists(string fsmName, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets))
            {
                return false;
            }
            return categoryBuckets.ContainsKey(fsmName);
        }

        /// <summary>
        /// Retrieves the names of all FSM definitions currently registered
        /// within a specific processing group.
        /// </summary>
        /// <param name="processingGroup">The processing group to query.</param>
        /// <returns>A read-only collection of FSM definition names.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="processingGroup"/> is null or empty.</exception>
        public static IReadOnlyCollection<string> GetAllDefinitionNames(string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets))
            {
                return new List<string>().AsReadOnly();
            }
            return categoryBuckets.Keys.ToList().AsReadOnly();
        }

        /// <summary>
        /// Retrieves a specific FSM definition by its name from the designated processing group.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to retrieve.</param>
        /// <param name="processingGroup">The processing group where the FSM is registered.</param>
        /// <returns>The <see cref="FSM"/> definition object.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the FSM definition is not found in the specified group.</exception>
        public static FSM GetDefinition(string fsmName, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
            }
            return bucket.Definition;
        }

        /// <summary>
        /// Retrieves all active instances of a specific FSM definition within a given processing group.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition whose instances you want to retrieve.</param>
        /// <param name="processingGroup">The processing group where the FSM definition is registered.</param>
        /// <returns>A read-only list of <see cref="FSMHandle"/> objects representing the live FSM instances.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the FSM definition is not found in the specified group.</exception>
        public static IReadOnlyList<FSMHandle> GetInstances(string fsmName, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'.");
            }
            return bucket.Instances.AsReadOnly();
        }

        /// <summary>
        /// Creates a live instance of a registered FSM definition, binding it to a specific context object.
        /// This allows the FSM to manage the state and behaviors of that context.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to instantiate.</param>
        /// <param name="ctx">The object that will serve as the FSM's context. This object must implement <see cref="IStateContext"/>.</param>
        /// <param name="processingGroup">The processing group of the FSM definition you wish to instantiate.</param>
        /// <returns>An <see cref="FSMHandle"/> that provides control over the newly created FSM instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ctx"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the FSM definition is not found in the specified group.</exception>
        public static FSMHandle CreateInstance(
            string fsmName,
            IStateContext ctx, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "State context (ctx) cannot be null when creating an FSM instance.");
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in processing group '{processingGroup}'. Cannot create instance.");
            }

            var handle = new FSMHandle(bucket.Definition, ctx);
            bucket.Instances.Add(handle);
            return handle;
        }

        /// <summary>
        /// Processes all FSMs in the "LateUpdate" processing group. Call this method from a
        /// MonoBehaviour's <c>LateUpdate()</c> method in your main application to
        /// ensure FSMs update at the end of each frame.
        /// </summary>
        public static void LateUpdate()
        {
            var sw = Stopwatch.StartNew();
            TickAll("LateUpdate");
            ProcessDeferredModifications();
            sw.Stop();
            if (sw.ElapsedMilliseconds > TickPerformanceWarningThresholdMs)
            {
                InvokeInternalApiError($"'LateUpdate' tick took {sw.ElapsedMilliseconds}ms. Threshold: {TickPerformanceWarningThresholdMs}ms.", null);
            }
        }

        /// <summary>
        /// Processes all FSMs in the "FixedUpdate" processing group. Call this method from a
        /// MonoBehaviour's <c>FixedUpdate()</c> method in your main application to
        /// ensure FSMs update on a fixed time interval, ideal for physics-related FSMs.
        /// </summary>
        public static void FixedUpdate()
        {
            var sw = Stopwatch.StartNew();
            TickAll("FixedUpdate");
            ProcessDeferredModifications();
            sw.Stop();
            if (sw.ElapsedMilliseconds > TickPerformanceWarningThresholdMs)
            {
                InvokeInternalApiError($"'FixedUpdate' tick took {sw.ElapsedMilliseconds}ms. Threshold: {TickPerformanceWarningThresholdMs}ms.", null);
            }
        }

        /// <summary>
        /// Processes all FSMs in the "Update" processing group. Call this method from a
        /// MonoBehaviour's <c>Update()</c> method in your main application to
        /// ensure FSMs update once per frame.
        /// </summary>
        public static void Update(string processGroup = "Update")
        {
            var sw = Stopwatch.StartNew();
            TickAll(processGroup);
            ProcessDeferredModifications();
            sw.Stop();
            if (sw.ElapsedMilliseconds > TickPerformanceWarningThresholdMs)
            {
                InvokeInternalApiError($"'Update' tick took {sw.ElapsedMilliseconds}ms. Threshold: {TickPerformanceWarningThresholdMs}ms.", null);
            }
        }

        /// <summary>
        /// Executes any pending modifications (e.g., adding/removing FSM instances)
        /// that were deferred during the FSM update cycle. This ensures collection safety.
        /// </summary>
        private static void ProcessDeferredModifications()
        {
            while (_deferredModifications.Count > 0)
            {
                var action = _deferredModifications.Dequeue();
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    InvokeInternalApiError("Error during deferred API modification.", ex);
                }
            }
        }

        /// <summary>
        /// The internal core method that ticks (updates) all FSM instances within a
        /// specified processing group, respecting each FSM's defined process rate.
        /// </summary>
        /// <param name="processingGroup">The group of FSMs to tick (e.g., "Update", "FixedUpdate").</param>
        private static void TickAll(string processingGroup)
        {
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                InvokeInternalApiError("TickAll called with null or empty processing group.", null);
                return;
            }

            if (!_buckets.TryGetValue(processingGroup, out var fsmDefinitionsForCategory))
            {
                return;
            }

            var bucketsToTick = fsmDefinitionsForCategory.Values.ToList();

            foreach (var bucket in bucketsToTick)
            {
                if (bucket.ProcessRate == 0)
                {
                    continue;
                }

                if (bucket.ProcessRate > 0)
                {
                    bucket.Counter--;
                    if (bucket.Counter > 0)
                    {
                        continue;
                    }
                    bucket.Counter = bucket.ProcessRate;
                }

                var instancesToTick = bucket.Instances.ToList();
                foreach (var h in instancesToTick)
                {
                    // Ensure the handle itself is not null, its context is not null,
                    // AND its context is reported as valid by the context itself.
                    if (h != null && h.Context != null && h.Context.IsValid)
                    {
                        try
                        {
                            h.Update();
                        }
                        catch (Exception ex)
                        {
                            // If an exception occurs during the FSM's internal step, report it as a "error".
                            ReportError(h, ex);
                        }
                    }
                    else
                    {
                        // If an instance is null or its context is null/invalid, it should be removed.
                        ReportError(h, new ApplicationException("FSM instance or its context became null/invalid (IsValid returned false)."));
                    }
                }
            }
        }

        /// <summary>
        /// Removes a registered FSM definition and all its associated active instances
        /// from a specific processing group. This effectively unregisters the FSM definition.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to destroy.</param>
        /// <param name="processingGroup">The processing group where the FSM definition is registered.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="processingGroup"/> is null or empty.</exception>
        public static void DestroyFiniteStateMachine(string fsmName, string processingGroup = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(processingGroup))
            {
                throw new ArgumentException("Processing group cannot be null or empty.", nameof(processingGroup));
            }

            if (!_buckets.TryGetValue(processingGroup, out var categoryBuckets))
            {
                InvokeInternalApiError($"Attempted to destroy FSM '{fsmName}' from non-existent processing group '{processingGroup}'.", null);
                return;
            }

            if (!categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                InvokeInternalApiError($"Attempted to destroy non-existent FSM '{fsmName}' in processing group '{processingGroup}'.", null);
                return;
            }

            // Clear instances and their error counts
            foreach (var instance in bucket.Instances)
            {
                _errorCounts.Remove(instance);
            }
            bucket.Instances.Clear();

            // Clean up the definition's error count
            _fsmDefinitionErrorCounts.Remove(fsmName);

            categoryBuckets.Remove(fsmName);
        }

        /// <summary>
        /// Unregisters a specific FSM instance from the system. This method searches across all
        /// active FSM definitions in all groups to find and deregister the instance.
        /// </summary>
        /// <param name="instance">The <see cref="FSMHandle"/> of the instance to unregister.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is null.</exception>
        public static void Unregister(FSMHandle instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Cannot unregister a null FSM instance.");
            }

            bool removed = false;
            // Iterate through all processing groups
            foreach (var categoryKvp in _buckets)
            {
                // Iterate through all FSM definitions within each processing group
                foreach (var fsmBucket in categoryKvp.Value.Values)
                {
                    // Attempt to remove the instance from the bucket's list of instances
                    if (fsmBucket.Instances.Remove(instance))
                    {
                        removed = true;
                        // Also remove its error count entry if it exists
                        _errorCounts.Remove(instance);
                        return; // Instance found and removed, exit early.
                    }
                }
            }

            if (!removed)
            {
                // Log a warning if the instance wasn't found, indicating it might have already been removed.
                InvokeInternalApiError(
                    $"FSMHandle '{instance.Name}' not found in any registered FSM in any group for unregistration. It might have already been unregistered or was never registered.",
                    null
                );
            }
        }


        // Internal method to report FSM instance errors
        internal static void ReportError(FSMHandle handle, Exception ex)
        {
            if (handle == null)
            {
                InvokeInternalApiError("Attempted to report error for a null FSMHandle.", ex);
                return;
            }

            // 1. Handle instance-level error count
            int newInstanceCount;
            if (_errorCounts.TryGetValue(handle, out int currentInstanceCount))
            {
                newInstanceCount = currentInstanceCount + 1;
                _errorCounts[handle] = newInstanceCount;
            }
            else
            {
                newInstanceCount = 1;
                _errorCounts.Add(handle, newInstanceCount);
            }

            // Report the specific error
            InvokeInternalApiError(
                $"FSM Instance '{handle.Name}' (Definition: '{handle.Definition.Name}') in processing group '{handle.Definition.ProcessingGroup}' reported an error. Error count: {newInstanceCount}/{ErrorCountThreshold}. Exception: {ex?.Message}",
                ex
            );

            if (newInstanceCount >= ErrorCountThreshold)
            {
                // Instance hit its error threshold, schedule its removal
                _deferredModifications.Enqueue(() =>
                {
                    InvokeInternalApiError($"FSM Instance '{handle.Name}' (Definition: '{handle.Definition.Name}') hit Error Threshold ({ErrorCountThreshold}). Scheduling unregistration.", null);
                    Unregister(handle); // This will also clear its _errorCounts entry
                    IncrementDefinitionError(handle.Definition.Name, handle.Definition.ProcessingGroup);
                });
            }
        }

        /// <summary>
        /// Internal method to increment the error count for an FSM definition.
        /// If the definition's error count exceeds DefinitionErrorThreshold, the entire definition is destroyed.
        /// </summary>
        /// <param name="fsmDefinitionName">The name of the FSM definition that had a failing instance.</param>
        /// <param name="processingGroup">The processing group of the FSM definition.</param>
        private static void IncrementDefinitionError(string fsmDefinitionName, string processingGroup)
        {
            int newDefinitionCount;
            if (_fsmDefinitionErrorCounts.TryGetValue(fsmDefinitionName, out int currentDefinitionCount))
            {
                newDefinitionCount = currentDefinitionCount + 1;
                _fsmDefinitionErrorCounts[fsmDefinitionName] = newDefinitionCount;
            }
            else
            {
                newDefinitionCount = 1;
                _fsmDefinitionErrorCounts.Add(fsmDefinitionName, newDefinitionCount);
            }

            InvokeInternalApiError(
                $"FSM Definition '{fsmDefinitionName}' in processing group '{processingGroup}' has had a failing instance removed. Definition failure count: {newDefinitionCount}/{DefinitionErrorThreshold}.",
                null
            );

            if (newDefinitionCount >= DefinitionErrorThreshold)
            {
                // Definition hit its error threshold, schedule its complete destruction
                _deferredModifications.Enqueue(() =>
                {
                    InvokeInternalApiError($"FSM Definition '{fsmDefinitionName}' in processing group '{processingGroup}' hit DefinitionErrorThreshold ({DefinitionErrorThreshold}). Scheduling complete destruction.", null);
                    DestroyFiniteStateMachine(fsmDefinitionName, processingGroup);
                });
            }
        }
    }
}