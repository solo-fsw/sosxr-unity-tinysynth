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

		static SfxrPlayer   instance;
		static SfxrRenderer sfxrRenderer;

		const int MaxCacheSize = 32;

		void Start() {
			cache.Clear();
		}

		public static void Play(SfxrParams param) {
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

			PlayClip(entry.clip);
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

		static void PlayClip(AudioClip clip) {
			if (instance == null) instance = FindObjectOfType<SfxrPlayer>();
			if (instance == null) Debug.LogError($"No {nameof(SfxrPlayer)} found in Scene. Add one!");
			var audioSource = instance.GetComponent<AudioSource>();
			audioSource.clip = clip;
			audioSource.Play();
		}


		static long GetTimestamp() {
			return DateTimeOffset.Now.ToUnixTimeSeconds();
		}
	}
}