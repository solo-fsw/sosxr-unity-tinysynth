using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.TestTools;


namespace SOSXR.TinySynth.Tests
{
    public class TinySynthPlayerTests
    {
        [SetUp]
        public void SetUp()
        {
            TinySynthPlayer.ResetStaticState();
        }


        [TearDown]
        public void TearDown()
        {
            TinySynthPlayer.ResetStaticState();
        }


        /// <summary>A fixed, short sound. The presets are random, so they cannot be used as stable cache keys.</summary>
        private static TinySynthSound Sound(int variant)
        {
            var sound = new TinySynthSound();
            sound.Reset();
            sound.sustainTime = 0.05f;
            sound.decayTime = 0.1f;
            sound.startFrequency = 0.2f + variant * 0.005f;

            return sound;
        }


        [Test]
        public void Play_WithoutPlayerInScene_DoesNotThrowOnRepeatedCalls()
        {
            // If the open scene has no TinySynthPlayer, Play logs an error each time. Either way it must not throw.
            LogAssert.ignoreFailingMessages = true;

            try
            {
                Assert.DoesNotThrow(() =>
                {
                    TinySynthPlayer.Play(Sound(0));
                    TinySynthPlayer.Play(Sound(0));
                });
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }
        }


        [Test]
        public void Cache_NeverGrowsPastMaxSize()
        {
            for (var i = 0; i < TinySynthPlayer.MaxCacheSize + 5; i++)
            {
                TinySynthPlayer.GetClip(Sound(i));
            }

            Assert.That(TinySynthPlayer.CacheCount, Is.EqualTo(TinySynthPlayer.MaxCacheSize));
        }


        [Test]
        public void EvictedClips_AreDestroyed()
        {
            var first = TinySynthPlayer.GetClip(Sound(0));

            for (var i = 1; i <= TinySynthPlayer.MaxCacheSize; i++)
            {
                TinySynthPlayer.GetClip(Sound(i));
            }

            Assert.That(first == null, Is.True, "The evicted clip is a destroyed Unity object.");
        }


        [Test]
        public void RecentlyUsedClip_IsKeptWhenEvicting()
        {
            var first = TinySynthPlayer.GetClip(Sound(0));
            var second = TinySynthPlayer.GetClip(Sound(1));

            for (var i = 2; i < TinySynthPlayer.MaxCacheSize; i++)
            {
                TinySynthPlayer.GetClip(Sound(i));
            }

            TinySynthPlayer.GetClip(Sound(0)); // use the first again, so the second is now the oldest
            TinySynthPlayer.GetClip(Sound(TinySynthPlayer.MaxCacheSize)); // forces one eviction

            Assert.That(first == null, Is.False, "Recently used clip is kept.");
            Assert.That(second == null, Is.True, "Least recently used clip is evicted.");
        }


        [Test]
        public void ClearCache_DestroysAllClips()
        {
            var clip = TinySynthPlayer.GetClip(Sound(0));

            TinySynthPlayer.ClearCache();

            Assert.That(TinySynthPlayer.CacheCount, Is.Zero);
            Assert.That(clip == null, Is.True);
        }


        [Test]
        public void SameSound_ReturnsCachedClip()
        {
            var a = TinySynthPlayer.GetClip(Sound(3));
            var b = TinySynthPlayer.GetClip(Sound(3));

            Assert.That(b, Is.SameAs(a));
            Assert.That(TinySynthPlayer.CacheCount, Is.EqualTo(1));
        }


        [Test]
        public void PurgeAndAddSources_OnDestroyedComponent_DoesNothing()
        {
            var go = new GameObject("TinySynthPlayerTests");

            try
            {
                var player = go.AddComponent<TinySynthPlayer>();
                Object.DestroyImmediate(player);

                Assert.DoesNotThrow(() => player.PurgeAndAddSources());
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }


        [Test]
        public void PurgeAndAddSources_MatchesPolyphony()
        {
            var go = new GameObject("TinySynthPlayerTests");

            try
            {
                var player = go.AddComponent<TinySynthPlayer>();

                player.polyphony = 4;
                player.PurgeAndAddSources();
                Assert.That(go.GetComponents<AudioSource>().Length, Is.EqualTo(4));

                player.polyphony = 2;
                player.PurgeAndAddSources();
                Assert.That(go.GetComponents<AudioSource>().Length, Is.EqualTo(2));
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }
    }


    public class TinySynthPackageTests
    {
        /// <summary>Unguarded <c>using UnityEditor;</c> in runtime code breaks every player build.</summary>
        [Test]
        public void RuntimeScripts_GuardEveryUsingUnityEditor()
        {
            var asmdef = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName("SOSXR.TinySynth");
            Assert.That(asmdef, Is.Not.Null.And.Not.Empty);

            var runtimeFolder = Path.GetDirectoryName(asmdef)!;
            var offenders = Directory.GetFiles(runtimeFolder, "*.cs", SearchOption.AllDirectories)
                                     .Where(HasUnguardedEditorUsing)
                                     .ToArray();

            Assert.That(offenders, Is.Empty);
        }


        private static bool HasUnguardedEditorUsing(string path)
        {
            var depth = 0;
            var guarded = new System.Collections.Generic.Stack<bool>();

            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();

                if (line.StartsWith("#if"))
                {
                    guarded.Push(line.Contains("UNITY_EDITOR") && !line.Contains("!UNITY_EDITOR"));
                    depth++;
                }
                else if (line.StartsWith("#endif") && depth > 0)
                {
                    guarded.Pop();
                    depth--;
                }
                else if (Regex.IsMatch(line, @"^using\s+UnityEditor(\.|;)") && !guarded.Contains(true))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
