using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Debug = UnityEngine.Debug;


namespace SOSXR.TinySynth
{
    /// <summary>
    ///     This is the script responsible for providing rendered audio to the engine, it also handles caching
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class TinySynthPlayer : MonoBehaviour
    {
        /// <summary>Maximum number of sound effects that can play simultaneously. Each polyphony slot maps to one <see cref="AudioSource" /> on this GameObject.</summary>
        [Header("A higher polyphony means you can play more sound effects simultaneously.")] [Range(1, 16)]
        public int polyphony = 1;

        /// <summary>Minimum time in seconds that must elapse before the same <see cref="TinySynthSound" /> can be triggered again. Prevents rapid retriggering artifacts.</summary>
        [Header("Minimum duration (seconds) before allowing to play the same sfx again.")] [Range(0, .5f)]
        public float minRetriggerTime = .017f;

        /// <summary>Rendered clips, keyed by the sound parameters (compared by value).</summary>
        private static readonly Dictionary<TinySynthSound, ClipTimeTuple> cache = new();

        /// <summary>The player that owns <see cref="sources" />.</summary>
        private static TinySynthPlayer instance;
        /// <summary>Renders clips on demand.</summary>
        private static TinySynthRenderer _tinySynthRenderer;
        /// <summary>AudioSources of the active player, one per polyphony slot.</summary>
        private static AudioSource[] sources;
        /// <summary>Next polyphony slot to use.</summary>
        private static int sourceIndex;

        /// <summary>Maximum number of rendered clips kept in the cache. The least recently played clip is destroyed first.</summary>
        public const int MaxCacheSize = 32;

        /// <summary>Number of rendered clips currently cached.</summary>
        public static int CacheCount => cache.Count;


        /// <summary>
        ///     Resets static state before the first scene loads, so Play mode with domain reload disabled starts clean.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ResetStaticState()
        {
            ClearCache();
            instance = null;
            sources = null;
            sourceIndex = 0;
            _tinySynthRenderer = null;
        }


        /// <summary>Clears the cache and finds the AudioSources.</summary>
        private void Start()
        {
            ClearCache();
            UpdateSources();
        }


        /// <summary>Clears the static references when this player is destroyed.</summary>
        private void OnDestroy()
        {
            if (instance != this)
            {
                return;
            }

            instance = null;
            sources = null;
            sourceIndex = 0;
        }


        /// <summary>Destroys every cached clip and empties the cache.</summary>
        public static void ClearCache()
        {
            foreach (var entry in cache.Values)
            {
                DestroyClip(entry.clip);
            }

            cache.Clear();
        }


        /// <summary>
        ///     Call this from any MonoBehaviour to pre-cache all your sfx
        /// </summary>
        /// <param name="behaviour">Any of your games MonoBehaviours</param>
        public static void PreCache(MonoBehaviour behaviour)
        {
            var monobehaviourCount = 0;
            var fieldCount = 0;

            var s = new Stopwatch();
            s.Start();

            foreach (var type in Assembly.GetAssembly(behaviour.GetType()).GetTypes())
            {
                monobehaviourCount++;

                if (!type.IsClass || type.IsAbstract || !type.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    continue;
                }

                var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
                    var objects = FindObjectsByType(type, FindObjectsInactive.Exclude);

                foreach (var obj in objects)
                {
                    monobehaviourCount++;

                    foreach (var field in fields)
                    {
                        if (field.FieldType != typeof(TinySynthSound))
                        {
                            continue;
                        }

                        CacheGet((TinySynthSound) field.GetValue(obj));
                        fieldCount++;
                    }
                }
            }

            Debug.Log($"Pre cached {fieldCount} sfx found across {monobehaviourCount} components in {s.Elapsed.TotalMilliseconds:F1} ms");
        }


        /// <summary>
        ///     Renders and plays the supplied SfxParams
        /// </summary>
        /// <param name="param">The sound effect parameters to use</param>
        /// <param name="asPreview">
        ///     If set, the effect will always play on the first channel (this stops any previous preview that
        ///     is still playing)
        /// </param>
        public static void Play(TinySynthSound param, bool asPreview = false)
        {
            UpdateInstance();

            if (instance == null)
            {
                return; // UpdateInstance already logged the error
            }

            var entry = CacheGet(param);

            if (!entry.firstPlay && !asPreview && entry.timeSinceLastTrigger < instance.minRetriggerTime)
            {
                return;
            }

            entry.UpdateTime();
            PlayClip(entry.clip, asPreview);
        }


        /// <summary>
        ///     Retrieves an AudioClip along with some other data if it's cached, otherwise it is generated
        /// </summary>
        private static ClipTimeTuple CacheGet(TinySynthSound param)
        {
            // make sure we have a renderer
            if (_tinySynthRenderer == null)
            {
                _tinySynthRenderer = new TinySynthRenderer();
            }

            if (cache.TryGetValue(param, out var entry))
            {
                // sometimes it seems the audio clip will get lost despite the cache having a reference to it, so we may need to regenerate it
                if (entry.clip == null)
                {
                    entry.clip = _tinySynthRenderer.GenerateClip(param);
                }

                entry.firstPlay = false;

                return entry;
            }

            // Make room before adding, so the cache never grows past MaxCacheSize (the Inspector preview adds a clip per edit).
            while (cache.Count >= MaxCacheSize)
            {
                PurgeCache();
            }

            entry = new ClipTimeTuple(_tinySynthRenderer.GenerateClip(param));
            cache.Add(param, entry);

            return entry;
        }


        /// <summary>
        ///     Returns the rendered <see cref="AudioClip" /> for the supplied parameters, generating and caching it if needed.
        /// </summary>
        /// <param name="param">The sound effect parameters to render.</param>
        /// <returns>A cached or freshly generated <see cref="AudioClip" />.</returns>
        public static AudioClip GetClip(TinySynthSound param)
        {
            return CacheGet(param).clip;
        }


        /// <summary>Plays the clip on the next polyphony slot, or on the first slot for previews.</summary>
        private static void PlayClip(AudioClip clip, bool asPreview)
        {
            UpdateInstance();

            // Refresh when sources belong to a destroyed player or polyphony changed.
            if (sources == null || sources.Length == 0 || sources[0] == null)
            {
                UpdateSources();
            }

            if (sources == null || sources.Length == 0 || sources[0] == null)
            {
                Debug.LogError($"No {nameof(AudioSource)} found in on GameObject that has {nameof(TinySynthPlayer)}. Add one!");

                return;
            }

            if (asPreview)
            {
                sources[0].Stop();
                sources[0].PlayOneShot(clip);
            }
            else
            {
                if (sourceIndex >= sources.Length)
                {
                    sourceIndex = 0;
                }

                sources[sourceIndex].PlayOneShot(clip);
                sourceIndex = (sourceIndex + 1) % sources.Length;
            }
        }


        /// <summary>Finds the TinySynthPlayer in the scene when none is known. Logs an error when there is none.</summary>
        private static void UpdateInstance()
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<TinySynthPlayer>();
                sources = null; // belong to the previous player, if any
                sourceIndex = 0;
            }

            if (instance == null)
            {
                Debug.LogError($"No {nameof(TinySynthPlayer)} found in Scene. Add one!");
            }
        }


        /// <summary>Caches the AudioSources of the active player.</summary>
        private static void UpdateSources()
        {
            UpdateInstance();

            if (instance == null)
            {
                return;
            }

            sources = instance.GetComponents<AudioSource>();
        }


        /// <summary>
        ///     Drops the least recently played sfx from the cache and destroys its clip.
        /// </summary>
        internal static void PurgeCache()
        {
            if (cache.Count == 0)
            {
                return;
            }

            var now = GetTimestamp();
            var maxAge = float.MinValue;
            var oldest = new TinySynthSound();

            foreach (var entry in cache)
            {
                var age = now - entry.Value.triggerTime;

                if (age < maxAge)
                {
                    continue;
                }

                maxAge = age;
                oldest = entry.Key;
            }

            if (cache.TryGetValue(oldest, out var removed))
            {
                DestroyClip(removed.clip);
            }

            cache.Remove(oldest);
        }


        /// <summary>
        ///     <see cref="AudioClip.Create(string, int, int, int, bool)" /> clips are native objects; dropping the reference does not free them.
        /// </summary>
        private static void DestroyClip(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(clip);
            }
            else
            {
                DestroyImmediate(clip);
            }
        }


        /// <summary>Returns the unscaled time used for retrigger and cache age checks.</summary>
        private static float GetTimestamp()
        {
            return Time.unscaledTime;
        }


        /// <summary>A cached clip with the time it was last triggered.</summary>
        private class ClipTimeTuple
        {
            /// <summary>The cached clip.</summary>
            public AudioClip clip;
            /// <summary>True until the sound is requested for the first time.</summary>
            public bool firstPlay = true;


            /// <summary>Creates an entry for the supplied clip.</summary>
            public ClipTimeTuple(AudioClip clip)
            {
                this.clip = clip;
            }


            /// <summary>Time of the last trigger, from <see cref="GetTimestamp" />.</summary>
            public float triggerTime { get; private set; }

            /// <summary>Seconds since the last trigger.</summary>
            public float timeSinceLastTrigger => GetTimestamp() - triggerTime;


            /// <summary>Records the current time as the last trigger.</summary>
            public void UpdateTime()
            {
                triggerTime = GetTimestamp();
            }
        }


        #if UNITY_EDITOR
        /// <summary>Schedules a check of the AudioSource count when polyphony changes in the Inspector.</summary>
        private void OnValidate()
        {
            // make sure we have the correct amount of audio sources
            // this needs to be done later since unity gets grumpy if we add/remove components in OnValidate
            if (GetComponents<AudioSource>().Length != polyphony)
            {
                EditorApplication.delayCall += PurgeAndAddSources;
            }
        }


        /// <summary>
        ///     Adds or removes AudioSources on this GameObject until there are <see cref="polyphony" />.
        ///     Runs from <c>delayCall</c>, so this component may already be destroyed. Uses its own sources, not the static ones,
        ///     which can belong to another player.
        /// </summary>
        internal void PurgeAndAddSources()
        {
            if (this == null)
            {
                return;
            }

            var own = GetComponents<AudioSource>();
            var numSources = own.Length;

            while (numSources < polyphony)
            {
                gameObject.AddComponent<AudioSource>();
                numSources++;
            }

            while (numSources > polyphony && numSources > 1)
            {
                DestroyImmediate(own[numSources - 1]);
                numSources--;
            }

            if (instance == this)
            {
                sources = null; // refreshed on next play
            }
        }


        #endif
    }
}