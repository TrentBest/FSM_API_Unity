using NUnit.Framework; // Keep for testing purposes if this file is part of your test assembly

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace TheSingularityWorkshop.FSM.API
{
    /// <summary>
    /// The core API for managing Finite State Machines (FSMs) in Unity.
    /// This system allows you to define flexible FSM structures, create instances
    /// of those FSMs for various game objects or entities, and manage their updates.
    /// <para>All FSM operations are designed to run on the main thread for thread safety.</para>
    /// <para>Use <see cref="CreateFiniteStateMachine"/> to design your FSM, then
    /// <see cref="CreateInstance"/> to bring it to life and attach it to your game logic.</para>
    /// </summary>
    public static class FSM_API
    {
        // _deferredModifications: This queue allows FSMs to request changes (like adding/removing
        // instances) that should happen at the end of the current update cycle, preventing
        // collection modification errors during iteration.
        private static readonly ConcurrentQueue<Action> _deferredModifications = new();

        /// <summary>
        /// Represents a container for an FSM definition and all its active instances.
        /// Used internally by the FSM_API to manage updates.
        /// </summary>
        class FsmBucket
        {
            public FSM Definition;
            public List<FSMHandle> Instances = new();
            public int ProcessRate;      // –1 (every frame), 0 (event-driven), or >0 (every Nth frame)
            public int Counter;          // Internal counter for frame-skipping
        }

        /// <summary>
        /// Sets a time limit (in milliseconds) for how long an FSM category update can take
        /// before a warning message is logged. Helps identify performance bottlenecks.
        /// Default is 5ms.
        /// </summary>
        public static long TickPerformanceWarningThresholdMs { get; set; } = 5;

        // _buckets: Stores all FSM definitions and their instances, organized by
        // their update category (e.g., "Update", "FixedUpdate") and then by FSM name.
        static Dictionary<string, Dictionary<string, FsmBucket>> _buckets = new();

        /// <summary>
        /// Safely retrieves the dictionary of FSMs for a given update category.
        /// If the category does not exist, it will be created automatically.
        /// </summary>
        /// <param name="updateCategory">The name of the update category (e.g., "Update", "FixedUpdate").</param>
        /// <returns>A dictionary containing FSM buckets for the specified category, keyed by FSM name.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="updateCategory"/> is null or empty.</exception>
        private static Dictionary<string, FsmBucket> GetOrCreateBucketCategory(string updateCategory)
        {
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets))
            {
                categoryBuckets = new Dictionary<string, FsmBucket>();
                _buckets[updateCategory] = categoryBuckets;
                Debug.Log($"[FSM_API] New update category created: '{updateCategory}'.");
            }
            return categoryBuckets;
        }

        /// <summary>
        /// Ensures an FSM update category exists. FSMs registered under this category
        /// will be processed when the corresponding <c>Tick</c> method (e.g., <see cref="Update"/>, <see cref="FixedUpdate"/>) is called.
        /// </summary>
        /// <param name="updateCategory">The unique name for the update category.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="updateCategory"/> is null or empty.</exception>
        public static void CreateBucketUpdateCategory(string updateCategory)
        {
            // Simply calling GetOrCreateBucketCategory ensures it exists and handles validation.
            GetOrCreateBucketCategory(updateCategory);
            Debug.Log($"[FSM_API] Ensured update category '{updateCategory}' exists.");
        }

        /// <summary>
        /// Removes an entire FSM update category, along with all FSM definitions and
        /// their active instances within that category. This effectively unregisters
        /// all FSMs associated with it.
        /// </summary>
        /// <param name="updateCategory">The name of the update category to remove.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="updateCategory"/> is null or empty.</exception>
        public static void RemoveBucketUpdateCategory(string updateCategory)
        {
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (_buckets.TryGetValue(updateCategory, out var categoryBuckets))
            {
                // Clear instances for all FSMs in this category to help with garbage collection
                foreach (var bucket in categoryBuckets.Values)
                {
                    bucket.Instances.Clear();
                }
                _buckets.Remove(updateCategory);
                Debug.Log($"[FSM_API] Update category '{updateCategory}' and all its FSMs were removed.");
            }
            else
            {
                Debug.LogWarning($"[FSM_API] Attempted to remove non-existent update category '{updateCategory}'.");
            }
        }

        /// <summary>
        /// Initiates the definition process for a new Finite State Machine,
        /// or provides an existing FSM's builder if a definition with the
        /// same name and category already exists.
        /// </summary>
        /// <param name="fsmName">A unique name for this FSM definition. Important for creating instances later.</param>
        /// <param name="processRate">
        /// Controls how often this FSM's instances will be updated:
        /// <list type="bullet">
        ///     <item><term>-1</term><description>Updates every single frame.</description></item>
        ///     <item><term>0</term><description>Never updated automatically by the API's Tick methods (must be driven by events or manual calls).</description></item>
        ///     <item><term>&gt;0</term><description>Updates every Nth frame (e.g., a value of 5 means update every 5th frame).</description></item>
        /// </list>
        /// </param>
        /// <param name="updateCategory">
        /// The update loop category this FSM belongs to (e.g., "Update", "FixedUpdate", "LateUpdate").
        /// This dictates when the FSM's <c>TickAll</c> method will be called.
        /// </param>
        /// <returns>An <see cref="FSMBuilder"/> object, which you use to define the FSM's states, transitions, and behaviors.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="updateCategory"/> is null or empty.</exception>
        public static FSMBuilder CreateFiniteStateMachine(
            string fsmName = "UnNamedFSM",
            int processRate = 0, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }
            if (processRate < -1)
            {
                Debug.LogWarning($"[FSM_API] Invalid processRate '{processRate}' for FSM '{fsmName}'. Setting to 0 (event-driven).");
                processRate = 0;
            }

            var buckets = GetOrCreateBucketCategory(updateCategory);
            if (buckets.ContainsKey(fsmName))
            {
                Debug.Log($"[FSM_API] FSM '{fsmName}' already exists in category '{updateCategory}'. Returning existing definition for modification.");
                return new FSMBuilder(buckets[fsmName].Definition);
            }

            Debug.Log($"[FSM_API] Initiating FSM builder for '{fsmName}' (rate={processRate}, category='{updateCategory}').");
            // Pass all parameters to the builder, it will call Register when BuildDefinition is complete.
            return new FSMBuilder(fsmName, processRate, updateCategory);
        }

        /// <summary>
        /// Internally registers a fully defined FSM into the system. This method is called by
        /// <see cref="FSMBuilder.BuildDefinition()"/> and should not be invoked directly by users.
        /// </summary>
        /// <param name="fsmName">The unique name of the FSM definition.</param>
        /// <param name="fsm">The complete <see cref="FSM"/> definition object.</param>
        /// <param name="processRate">The update rate for this FSM (as defined in <see cref="CreateFiniteStateMachine"/>).</param>
        /// <param name="updateCategory">The update category this FSM belongs to.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/>, <paramref name="fsm"/>, or <paramref name="updateCategory"/> is null or empty.</exception>
        internal static void Register( // Changed to internal - users should not call directly
            string fsmName,
            FSM fsm,
            int processRate, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty during registration.", nameof(fsmName));
            }
            if (fsm == null)
            {
                throw new ArgumentNullException(nameof(fsm), "FSM definition cannot be null during registration.");
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty during registration.", nameof(updateCategory));
            }

            var buckets = GetOrCreateBucketCategory(updateCategory);
            if (buckets.ContainsKey(fsmName))
            {
                Debug.LogWarning($"[FSM_API] FSM '{fsmName}' already registered in category '{updateCategory}'. Skipping registration to avoid overwrite.");
                return;
            }

            buckets[fsmName] = new FsmBucket
            {
                Definition = fsm,
                ProcessRate = processRate,
                Counter = processRate > 0 ? processRate : 0, // Initialize counter if processRate > 0
            };
            Debug.Log($"[FSM_API] Registered FSM '{fsmName}' with rate {processRate} in category '{updateCategory}'.");
        }

        /// <summary>
        /// Checks if an FSM definition with the given name exists within the specified update category.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to check.</param>
        /// <param name="updateCategory">The update category to search within.</param>
        /// <returns><c>true</c> if the FSM definition exists, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="updateCategory"/> is null or empty.</exception>
        public static bool Exists(string fsmName, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets))
            {
                Debug.Log($"[FSM_API] FSM '{fsmName}': False (category '{updateCategory}' does not exist).");
                return false;
            }
            var result = categoryBuckets.ContainsKey(fsmName);
            Debug.Log($"[FSM_API] FSM '{fsmName}' in category '{updateCategory}': {result}.");
            return result;
        }

        /// <summary>
        /// Retrieves the names of all FSM definitions currently registered
        /// within a specific update category.
        /// </summary>
        /// <param name="updateCategory">The update category to query.</param>
        /// <returns>A read-only collection of FSM definition names.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="updateCategory"/> is null or empty.</exception>
        public static IReadOnlyCollection<string> GetAllDefinitionNames(string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets))
            {
                return new List<string>().AsReadOnly(); // Return an empty list if category doesn't exist
            }
            return categoryBuckets.Keys.ToList().AsReadOnly();
        }

        /// <summary>
        /// Retrieves a specific FSM definition by its name from the designated update category.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to retrieve.</param>
        /// <param name="updateCategory">The update category where the FSM is registered.</param>
        /// <returns>The <see cref="FSM"/> definition object.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="updateCategory"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the FSM definition is not found in the specified category.</exception>
        public static FSM GetDefinition(string fsmName, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in category '{updateCategory}'.");
            }
            return bucket.Definition;
        }

        /// <summary>
        /// Retrieves all active instances of a specific FSM definition within a given update category.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition whose instances you want to retrieve.</param>
        /// <param name="updateCategory">The update category where the FSM definition is registered.</param>
        /// <returns>A read-only list of <see cref="FSMHandle"/> objects representing the live FSM instances.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="updateCategory"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the FSM definition is not found in the specified category.</exception>
        public static IReadOnlyList<FSMHandle> GetInstances(string fsmName, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in category '{updateCategory}'.");
            }
            return bucket.Instances.AsReadOnly(); // Return as read-only to prevent external modification
        }

        /// <summary>
        /// Creates a live instance of a registered FSM definition, binding it to a specific context object.
        /// This allows the FSM to manage the state and behaviors of that context.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to instantiate.</param>
        /// <param name="ctx">The object that will serve as the FSM's context. This object must implement <see cref="IStateContext"/>.</param>
        /// <param name="updateCategory">The update category of the FSM definition you wish to instantiate.</param>
        /// <returns>An <see cref="FSMHandle"/> that provides control over the newly created FSM instance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="updateCategory"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="ctx"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">Thrown if the FSM definition is not found in the specified category.</exception>
        public static FSMHandle CreateInstance(
            string fsmName,
            IStateContext ctx, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (ctx == null)
            {
                throw new ArgumentNullException(nameof(ctx), "State context (ctx) cannot be null when creating an FSM instance.");
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets) || !categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                throw new KeyNotFoundException($"FSM definition '{fsmName}' not found in category '{updateCategory}'. Cannot create instance.");
            }

            var handle = new FSMHandle(bucket.Definition, ctx);
            bucket.Instances.Add(handle);
            Debug.Log($"[FSM_API] Created FSMHandle '{handle.Name}' for context '{ctx.Name}' in category '{updateCategory}'.");
            return handle;
        }

        /// <summary>
        /// Processes all FSMs in the "LateUpdate" category. Call this method from a
        /// MonoBehaviour's <c>LateUpdate()</c> method in your main application to
        /// ensure FSMs update at the end of each frame.
        /// Internally measures performance and logs warnings if the process takes too long.
        /// </summary>
        public static void LateUpdate()
        {
            var sw = Stopwatch.StartNew();
            TickAll("LateUpdate");
            // Deferred modifications are processed after all FSMs have ticked to ensure
            // collection safety during iteration.
            ProcessDeferredModifications();
            sw.Stop();
            if (sw.ElapsedMilliseconds > TickPerformanceWarningThresholdMs)
                Debug.LogWarning($"[FSM_API] 'LateUpdate' tick took {sw.ElapsedMilliseconds}ms. Threshold: {TickPerformanceWarningThresholdMs}ms.");
        }

        /// <summary>
        /// Processes all FSMs in the "FixedUpdate" category. Call this method from a
        /// MonoBehaviour's <c>FixedUpdate()</c> method in your main application to
        /// ensure FSMs update on a fixed time interval, ideal for physics-related FSMs.
        /// Internally measures performance and logs warnings if the process takes too long.
        /// </summary>
        public static void FixedUpdate()
        {
            var sw = Stopwatch.StartNew();
            TickAll("FixedUpdate");
            // Deferred modifications are processed after all FSMs have ticked to ensure
            // collection safety during iteration.
            ProcessDeferredModifications();
            sw.Stop();
            if (sw.ElapsedMilliseconds > TickPerformanceWarningThresholdMs)
                Debug.LogWarning($"[FSM_API] 'FixedUpdate' tick took {sw.ElapsedMilliseconds}ms. Threshold: {TickPerformanceWarningThresholdMs}ms.");
        }

        /// <summary>
        /// Processes all FSMs in the "Update" category. Call this method from a
        /// MonoBehaviour's <c>Update()</c> method in your main application to
        /// ensure FSMs update once per frame.
        /// Internally measures performance and logs warnings if the process takes too long.
        /// </summary>
        public static void Update()
        {
            var sw = Stopwatch.StartNew();
            TickAll("Update");
            // Deferred modifications are processed after all FSMs have ticked to ensure
            // collection safety during iteration.
            ProcessDeferredModifications();
            sw.Stop();
            if (sw.ElapsedMilliseconds > TickPerformanceWarningThresholdMs)
                Debug.LogWarning($"[FSM_API] 'Update' tick took {sw.ElapsedMilliseconds}ms. Threshold: {TickPerformanceWarningThresholdMs}ms.");
        }

        /// <summary>
        /// Executes any pending modifications (e.g., adding/removing FSM instances)
        /// that were deferred during the FSM update cycle. This ensures thread-safe
        /// collection modifications.
        /// </summary>
        private static void ProcessDeferredModifications()
        {
            while (_deferredModifications.TryDequeue(out var action))
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FSM_API] Error processing deferred modification: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// The internal core method that ticks (updates) all FSM instances within a
        /// specified update category, respecting each FSM's defined process rate.
        /// </summary>
        /// <param name="updateCategory">The category of FSMs to tick (e.g., "Update", "FixedUpdate").</param>
        private static void TickAll(string updateCategory)
        {
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                // This shouldn't happen if called by public Tick methods, but as a private helper, good to be safe.
                Debug.LogError("[FSM_API] TickAll called with null or empty update category.");
                return;
            }

            if (!_buckets.TryGetValue(updateCategory, out var fsmDefinitionsForCategory))
            {
                // No FSMs for this category, so just return, no warning needed for normal operation.
                return;
            }

            // Iterate over a copy of the FSM definitions to prevent issues if a definition is removed during tick
            var bucketsToTick = fsmDefinitionsForCategory.Values.ToList();

            foreach (var bucket in bucketsToTick)
            {
                // ProcessRate 0 means event-driven, not frame-driven by TickAll.
                if (bucket.ProcessRate == 0)
                {
                    continue;
                }

                if (bucket.ProcessRate > 0) // Skip frames based on process rate
                {
                    bucket.Counter--;
                    if (bucket.Counter > 0)
                    {
                        continue; // Not time to tick yet
                    }
                    bucket.Counter = bucket.ProcessRate; // Reset counter for next cycle
                }
                // ProcessRate -1 means tick every frame, no counter needed.

                // Create a copy of instances to iterate over. This prevents issues if
                // instances are added/removed *during* the update loop (e.g., an FSM enters
                // a state that destroys another FSM instance).
                var instancesToTick = bucket.Instances.ToList();
                foreach (var h in instancesToTick)
                {
                    // Ensure the handle is still valid before attempting to update it.
                    // This handles cases where an instance might have been removed or
                    // its context destroyed by other game logic during the current frame.
                    if (h != null && h.Context != null) // Assumes IStateContext is a Unity object or has IsDestroyed check
                    {
                        h.Update();
                    }
                    else
                    {
                        // If an instance is null or its context is null, it should be removed.
                        // We defer this removal to avoid modifying the collection during iteration.
                        // We will call RemoveInstance through the deferred queue.
                        _deferredModifications.Enqueue(() => RemoveInstance(h));
                        Debug.LogWarning($"[FSM_API] Detected null/destroyed FSM instance or context. Scheduling removal for instance: {(h != null ? h.Name : "N/A")}.");
                    }
                }
            }
        }

        /// <summary>
        /// Removes a registered FSM definition and all its associated active instances
        /// from a specific update category. This effectively unregisters the FSM definition.
        /// </summary>
        /// <param name="fsmName">The name of the FSM definition to destroy.</param>
        /// <param name="updateCategory">The update category where the FSM definition is registered.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fsmName"/> or <paramref name="updateCategory"/> is null or empty.</exception>
        public static void DestroyFiniteStateMachine(string fsmName, string updateCategory = "Update")
        {
            if (string.IsNullOrWhiteSpace(fsmName))
            {
                throw new ArgumentException("FSM name cannot be null or empty.", nameof(fsmName));
            }
            if (string.IsNullOrWhiteSpace(updateCategory))
            {
                throw new ArgumentException("Update category cannot be null or empty.", nameof(updateCategory));
            }

            if (!_buckets.TryGetValue(updateCategory, out var categoryBuckets))
            {
                Debug.LogWarning($"[FSM_API] Attempted to destroy FSM '{fsmName}' from non-existent category '{updateCategory}'.");
                return;
            }

            if (!categoryBuckets.TryGetValue(fsmName, out var bucket))
            {
                Debug.LogWarning($"[FSM_API] Attempted to destroy non-existent FSM '{fsmName}' in category '{updateCategory}'.");
                return;
            }

            bucket.Instances.Clear(); // Clear all instances associated with this FSM definition
            categoryBuckets.Remove(fsmName); // Remove the FSM definition itself from the category

            Debug.Log($"[FSM_API] FSM '{fsmName}' and all its instances were destroyed from category '{updateCategory}'.");
        }

        /// <summary>
        /// Removes a specific FSM instance from the system. This method searches across all
        /// active FSM definitions in all categories to find and deregister the instance.
        /// </summary>
        /// <param name="instance">The <see cref="FSMHandle"/> of the instance to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="instance"/> is null.</exception>
        public static void RemoveInstance(FSMHandle instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "Cannot remove a null FSM instance.");
            }

            bool removed = false;
            // Iterate through all categories and then all FSM definitions to find the instance
            foreach (var categoryKvp in _buckets)
            {
                foreach (var fsmBucket in categoryKvp.Value.Values)
                {
                    if (fsmBucket.Instances.Remove(instance))
                    {
                        Debug.Log($"[FSM_API] FSMHandle '{instance.Name}' removed from FSM '{fsmBucket.Definition.name}' in category '{categoryKvp.Key}'.");
                        removed = true;
                        // An instance belongs to only one definition, so we can stop searching here.
                        return; // Exit method immediately upon successful removal
                    }
                }
            }

            if (!removed) // This will only be reached if the instance wasn't found in any bucket
            {
                Debug.LogWarning($"[FSM_API] FSMHandle '{instance.Name}' not found in any registered FSM in any category for removal. It might have already been removed or was never registered.");
            }
        }
    }
}