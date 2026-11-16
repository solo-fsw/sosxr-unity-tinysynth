using System.Globalization;
using System.Threading;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace SOSXR.TinySynth.Tests
{
    public class TinySynthSoundTests
    {
        private CultureInfo _previousCulture;


        [SetUp]
        public void SetUp()
        {
            _previousCulture = Thread.CurrentThread.CurrentCulture;
            // A culture with a decimal comma: the settings string is comma-delimited.
            Thread.CurrentThread.CurrentCulture = new CultureInfo("nl-NL");
        }


        [TearDown]
        public void TearDown()
        {
            Thread.CurrentThread.CurrentCulture = _previousCulture;
        }


        private static TinySynthSound Sound()
        {
            var sound = new TinySynthSound();
            sound.Reset();
            sound.WaveType = TinySynthWaveType.Triangle;
            sound.sustainTime = 0.3f;
            sound.startFrequency = 0.1234f;
            sound.slide = -0.25f;

            return sound;
        }


        [Test]
        public void SettingsString_RoundTrips_UnderDecimalCommaCulture()
        {
            var sound = Sound();
            var text = sound.GetSettingsString();

            Assert.That(text.Split(',').Length, Is.EqualTo(32));

            var parsed = new TinySynthSound();
            Assert.That(parsed.SetSettingsString(text), Is.True);

            Assert.That(parsed.WaveType, Is.EqualTo(TinySynthWaveType.Triangle));
            Assert.That(parsed.sustainTime, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(parsed.startFrequency, Is.EqualTo(0.1234f).Within(0.0001f));
            Assert.That(parsed.slide, Is.EqualTo(-0.25f).Within(0.0001f));
        }


        [Test]
        public void LegacySettingsString_RoundTrips_UnderDecimalCommaCulture()
        {
            var text = Sound().GetSettingsStringLegacy();

            Assert.That(text.Split(',').Length, Is.EqualTo(24));

            var parsed = new TinySynthSound();
            Assert.That(parsed.SetSettingsString(text), Is.True);
            Assert.That(parsed.sustainTime, Is.EqualTo(0.3f).Within(0.0001f));
        }


        [Test]
        public void SetSettingsString_WithNonNumber_ReturnsFalseAndKeepsValues()
        {
            var sound = Sound();
            var text = sound.GetSettingsString();
            var fields = text.Split(',');
            fields[3] = "abc";

            LogAssert.Expect(LogType.Error, new Regex("not a number"));
            Assert.That(sound.SetSettingsString(string.Join(",", fields)), Is.False);

            Assert.That(sound.sustainTime, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(sound.WaveType, Is.EqualTo(TinySynthWaveType.Triangle));
        }
    }
}
