using System;
using System.Collections.Generic;
using UnityEngine;

namespace usfxr {
	
	/// <summary>
	/// This is the script responsible for providing rendered audio to the engine, it also handles caching
	/// </summary>
	
	[RequireComponent(typeof(AudioSource))]
	public class SfxrPlayer : MonoBehaviour {
		struct ClipTimeTuple {
			public AudioClip clip;
			public long      time;
		}

		static readonly Dictionary<SfxrParams, ClipTimeTuple> cache = new Dictionary<SfxrParams, ClipTimeTuple>();

		static SfxrPlayer    instance;
		static SfxrRenderer  sfxrRenderer;
		static AudioSource[] sources;
		static int           sourceIndex;

		[Header("A higher polyphony means you can play more sound effects simultaneously.")]
		[Range(1, 16)]
		public int polyphony = 1;

		const int MaxCacheSize = 32;

		void Start() {
			cache.Clear();
		}

		void OnValidate() {
			var audioSources = GetComponents<AudioSource>();
			var numSources   = audioSources.Length;

			while (numSources < polyphony) {
				gameObject.AddComponent<AudioSource>();
				numSources++;
			}
			
			while (numSources > polyphony) {
				DestroyImmediate(audioSources[numSources - 1]);
				numSources--;
			}

		}

		/// <summary>
		/// Renders and plays the supplied SfxParams
		/// </summary>
		/// <param name="param">The sound effect parameters to use</param>
		/// <param name="asPreview">If set, the effect will always play on the first channel (this stops any previous preview that is still playing)</param>
		public static void Play(SfxrParams param, bool asPreview = false) {
			Purge();

			if (!cache.TryGetValue(param, out var entry)) {
				if (sfxrRenderer == null) sfxrRenderer = new SfxrRenderer(param);
				sfxrRenderer.param = param;

				entry = new ClipTimeTuple {
					clip = sfxrRenderer.GenerateClip(),
					time = GetTimestamp(),
				};
				cache.Add(param, entry);
			}

			PlayClip(entry.clip, asPreview);
		}

		static void PlayClip(AudioClip clip, bool asPreview) {
			if (instance == null) instance = FindObjectOfType<SfxrPlayer>();
			if (instance == null) {
				Debug.LogError($"No {nameof(SfxrPlayer)} found in Scene. Add one!");
				return;
			}
			
			if (sources == null)  sources = instance.GetComponents<AudioSource>();
			if (sources.Length == 0) {
				Debug.LogError($"No {nameof(AudioSource)} found in on GameObject that has {nameof(SfxrPlayer)}. Add one!");
				return;
			}

			if (asPreview) {
				sources[0].PlayOneShot(clip);
			} else {
				sources[sourceIndex].PlayOneShot(clip);
				sourceIndex = (sourceIndex + 1) % sources.Length;
			}
		}
		
		static void Purge() {
			if (cache.Count < MaxCacheSize) return;

			var now    = GetTimestamp();
			var maxAge = long.MinValue;
			var oldest = new SfxrParams();

			foreach (var entry in cache) {
				var age = now - entry.Value.time;
				if (age < maxAge) continue;
				maxAge = age;
				oldest = entry.Key;
			}

			cache.Remove(oldest);
		}

		static long GetTimestamp() {
			return DateTimeOffset.Now.ToUnixTimeSeconds();
		}
	}
}