using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;


namespace usfxr
{
	/// <summary>
	///     This is the script responsible for providing rendered audio to the engine, it also handles caching
	/// </summary>
	[RequireComponent(typeof(AudioSource))]
    public class TinySynthPlayer : MonoBehaviour
    {
        [Header("A higher polyphony means you can play more sound effects simultaneously.")]
        [Range(1, 16)]
        public int polyphony = 1;

        [Header("Minimum duration (seconds) before allowing to play the same sfx again.")]
        [Range(0, .5f)]
        public float minRetriggerTime = .017f;

        private static readonly Dictionary<TinySynthSound, ClipTimeTuple> cache = new();

        private static TinySynthPlayer instance;
        private static SfxrRenderer sfxrRenderer;
        private static AudioSource[] sources;
        private static int sourceIndex;

        private const int MaxCacheSize = 32;


        private void Start()
        {
            cache.Clear();
            UpdateSources();
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
                var objects = FindObjectsOfType(type);

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
            PurgeCache();
            UpdateInstance();

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
            if (sfxrRenderer == null)
            {
                sfxrRenderer = new SfxrRenderer();
            }

            if (cache.TryGetValue(param, out var entry))
            {
                // sometimes it seems the audio clip will get lost despite the cache having a reference to it, so we may need to regenerate it
                if (entry.clip == null)
                {
                    entry.clip = sfxrRenderer.GenerateClip(param);
                }

                entry.firstPlay = false;

                return entry;
            }

            entry = new ClipTimeTuple(sfxrRenderer.GenerateClip(param));
            cache.Add(param, entry);

            return entry;
        }


        public static AudioClip GetClip(TinySynthSound param)
        {
            return CacheGet(param).clip;
        }


        private static void PlayClip(AudioClip clip, bool asPreview)
        {
            UpdateInstance();

            if (sources == null)
            {
                UpdateSources();
            }

            if (sources == null || sources.Length == 0)
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
                sources[sourceIndex].PlayOneShot(clip);
                sourceIndex = (sourceIndex + 1) % sources.Length;
            }
        }


        private static void UpdateInstance()
        {
            if (instance == null)
            {
                instance = FindObjectOfType<TinySynthPlayer>();
            }

            if (instance == null)
            {
                Debug.LogError($"No {nameof(TinySynthPlayer)} found in Scene. Add one!");
            }
        }


        private static void UpdateSources()
        {
            UpdateInstance();
            sources = instance.GetComponents<AudioSource>();
        }


        /// <summary>
        ///     Drops the oldest N sfx from the cache
        /// </summary>
        private static void PurgeCache()
        {
            if (cache.Count < MaxCacheSize)
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

            cache.Remove(oldest);
        }


        private static float GetTimestamp()
        {
            return Time.unscaledTime;
        }


        private class ClipTimeTuple
        {
            public AudioClip clip;
            public bool firstPlay = true;


            public ClipTimeTuple(AudioClip clip)
            {
                this.clip = clip;
            }


            public float triggerTime { get; private set; }

            public float timeSinceLastTrigger => GetTimestamp() - triggerTime;


            public void UpdateTime()
            {
                triggerTime = GetTimestamp();
            }
        }


        #if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateSources();

            // make sure we have the correct amount of audio sources
            // this needs to be done later since unity gets grumpy if we add/remove components in OnValidate
            if (sources.Length != polyphony)
            {
                EditorApplication.delayCall += PurgeAndAddSources;
            }
        }


        private void PurgeAndAddSources()
        {
            var numSources = sources.Length;

            while (numSources < polyphony)
            {
                gameObject.AddComponent<AudioSource>();
                numSources++;
            }

            while (numSources > polyphony)
            {
                DestroyImmediate(sources[numSources - 1]);
                numSources--;
            }
        }


        #endif
    }
}