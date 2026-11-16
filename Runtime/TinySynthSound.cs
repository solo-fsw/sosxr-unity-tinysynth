using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


namespace SOSXR.TinySynth
{
    /// <summary>
    ///     Holds all synthesis parameters for a single sound effect.
    ///     Assign one as a public field on any <see cref="MonoBehaviour" /> and it gets a rich
    ///     property drawer in the Inspector. Pass it to <see cref="TinySynthPlayer.Play" /> at runtime.
    /// </summary>
    [Serializable]
    public struct TinySynthSound
    {
        /// <summary>Shape of the wave to generate.</summary>
        [FormerlySerializedAs("m_tinySynthWaveType")] [Tooltip("Shape of wave to generate")] [TinySynthDefault(2)] [SerializeField]
        public TinySynthWaveType WaveType;

        /// <summary>Overall volume of the sound (0 to 1).</summary>
        [Tooltip("Overall volume of the sound")] [TinySynthDefault(1)] [Range(0, 1)]
        public float MasterVolume;

        /// <summary>Length of the volume envelope attack (0 to 1).</summary>
        [Tooltip("Length of the volume envelope attack")] [TinySynthDefault(0)] [Range(0, 1)]
        public float attackTime;

        /// <summary>Length of the volume envelope sustain (0 to 1).</summary>
        [Tooltip("Length of the volume envelope sustain")] [TinySynthDefault(0)] [Range(0, 1)]
        public float sustainTime;

        /// <summary>Tilts the sustain envelope for more 'pop' (0 to 1).</summary>
        [Tooltip("Tilts the sustain envelope for more 'pop'")] [TinySynthDefault(0)] [Range(0, 1)]
        public float sustainPunch;

        /// <summary>Length of the volume envelope decay — effectively the release time (0 to 1).</summary>
        [Tooltip("Length of the volume envelope decay (yes, I know it's called release)")] [TinySynthDefault(0)] [Range(0, 1)]
        public float decayTime;

        /// <summary>Base note of the sound (0 to 1).</summary>
        [Tooltip("Base note of the sound")] [TinySynthDefault(.3f)] [Range(0, 1)]
        public float startFrequency;

        /// <summary>If sliding, the sound stops at this frequency to prevent overly low notes (0 to 1).</summary>
        [Tooltip("If sliding, the sound will stop at this frequency, to prevent really low notes")] [TinySynthDefault(0)] [Range(0, 1)]
        public float minFrequency;

        /// <summary>Slides the pitch up or down over time (-1 to 1).</summary>
        [Tooltip("Slides the note up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float slide;

        /// <summary>Accelerates the slide over time (-1 to 1).</summary>
        [Tooltip("Accelerates the slide")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float deltaSlide;

        /// <summary>Strength of the vibrato effect (0 to 1).</summary>
        [Tooltip("Strength of the vibrato effect")] [TinySynthDefault(0)] [Range(0, 1)]
        public float vibratoDepth;

        /// <summary>Speed (frequency) of the vibrato oscillation (0 to 1).</summary>
        [Tooltip("Speed of the vibrato effect (i.e. frequency)")] [TinySynthDefault(0)] [Range(0, 1)]
        public float vibratoSpeed;

        /// <summary>First pitch-jump amount; shifts the note up or down once during playback (-1 to 1).</summary>
        [Tooltip("Shift in note, either up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float changeAmount;

        /// <summary>Controls when the first pitch jump occurs (0 to 1).</summary>
        [Tooltip("How fast the note shift happens (only happens once)")] [TinySynthDefault(0)] [Range(0, 1)]
        public float changeSpeed;

        /// <summary>Ratio between the high and low states of the square wave, shaping the timbre (0 to 1). Only relevant for <see cref="TinySynthWaveType.Square" />.</summary>
        [Tooltip("Controls the ratio between the up and down states of the square wave, changing the timbre")] [TinySynthDefault(0)] [Range(0, 1)]
        public float squareDuty;

        /// <summary>Sweeps the square duty up or down over time (-1 to 1). Only relevant for <see cref="TinySynthWaveType.Square" />.</summary>
        [Tooltip("Sweeps the duty up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float dutySweep;

        /// <summary>Speed at which certain parameters reset, producing an arpeggiation-like repeat (0 to 1).</summary>
        [Tooltip("Speed of the note repeating - certain variables are reset each time")] [TinySynthDefault(0)] [Range(0, 1)]
        public float repeatSpeed;

        /// <summary>Offsets a delayed copy of the wave to create a phaser/chorus effect (-1 to 1).</summary>
        [Tooltip("Offsets a second copy of the wave by a small phase, changing the timbre")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float phaserOffset;

        /// <summary>Sweeps the phaser offset up or down over time (-1 to 1).</summary>
        [Tooltip("Sweeps the phase up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float phaserSweep;

        /// <summary>Low-pass filter cutoff frequency; higher values let more high frequencies through (0 to 1).</summary>
        [Tooltip("Frequency at which the low-pass filter starts attenuating higher frequencies")] [TinySynthDefault(1)] [Range(0, 1)]
        public float lpFilterCutoff;

        /// <summary>Sweeps the low-pass cutoff up or down over time (-1 to 1).</summary>
        [Tooltip("Sweeps the low-pass cutoff up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float lpFilterCutoffSweep;

        /// <summary>Low-pass filter resonance; higher values create a more pronounced resonant peak (0 to 1).</summary>
        [Tooltip("Changes the attenuation rate for the low-pass filter, changing the timbre")] [TinySynthDefault(0)] [Range(0, 1)]
        public float lpFilterResonance;

        /// <summary>High-pass filter cutoff frequency; higher values remove more low-frequency content (0 to 1).</summary>
        [Tooltip("Frequency at which the high-pass filter starts attenuating lower frequencies")] [TinySynthDefault(0)] [Range(0, 1)]
        public float hpFilterCutoff;

        /// <summary>Sweeps the high-pass cutoff up or down over time (-1 to 1).</summary>
        [Tooltip("Sweeps the high-pass cutoff up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float hpFilterCutoffSweep;

        // From BFXR
        /// <summary>Rate at which pitch jumps repeat; higher values produce faster arpeggiation (0 to 1).</summary>
        [Tooltip("Pitch Jump Repeat Speed: larger Values means more pitch jumps, which can be useful for arpeggiation")] [TinySynthDefault(0)] [Range(0, 1)]
        public float changeRepeat;

        /// <summary>Second pitch-jump amount; shifts the note up or down a second time during playback (-1 to 1).</summary>
        [Tooltip("Shift in note, either up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float changeAmount2;

        /// <summary>Controls when the second pitch jump occurs (0 to 1).</summary>
        [Tooltip("How fast the note shift happens (only happens once)")] [TinySynthDefault(0)] [Range(0, 1)]
        public float changeSpeed2;

        /// <summary>
        ///     Compression amount; pushes amplitude peaks closer together so the sound cuts through background music (0 to 1).
        /// </summary>
        [Tooltip("Compression: pushes amplitudes together into a narrower range to make them stand out more. Very good for sound effects, where you want them to stick out against background music")] [TinySynthDefault(.3f)] [Range(0, 1)]
        public float compressionAmount;

        /// <summary>
        ///     Harmonics (overtones) strength; overlays copies of the waveform at integer multiples of the base frequency.
        ///     Warning: this is the primary cause of synthesis slowdown (0 to 1).
        /// </summary>
        [Tooltip("Harmonics: overlays copies of the waveform with copies and multiples of its frequency. Good for bulking out or otherwise enriching the texture of the sounds (warning: this is the number 1 cause of usfxr slowdown!)")] [TinySynthDefault(0)] [Range(0, 1)]
        public float overtones;

        /// <summary>Rate at which higher harmonics decay relative to the fundamental (0 to 1).</summary>
        [Tooltip("Harmonics falloff: the rate at which higher overtones should decay")] [TinySynthDefault(0)] [Range(0, 1)]
        public float overtoneFalloff;

        /// <summary>Bit crush amount; resamples the audio at a lower effective frequency, adding lo-fi grit (0 to 1).</summary>
        [Tooltip("Bit crush: resamples the audio at a lower frequency")] [TinySynthDefault(0)] [Range(0, 1)]
        public float bitCrush;

        /// <summary>Sweeps the bit crush frequency up or down over time (-1 to 1).</summary>
        [Tooltip("Bit crush sweep: sweeps the Bit Crush filter up or down")] [TinySynthDefault(0)] [Range(-1, 1)]
        public float bitCrushSweep;


        /// <summary>
        ///     Resets the parameters, used at the start of each generate function
        /// </summary>
        public void Reset()
        {
            WaveType = 0;
            MasterVolume = 1f;
            startFrequency = 0.3f;
            minFrequency = 0.0f;
            slide = 0.0f;
            deltaSlide = 0.0f;
            squareDuty = 0.0f;
            dutySweep = 0.0f;

            vibratoDepth = 0.0f;
            vibratoSpeed = 0.0f;

            attackTime = 0.0f;
            sustainTime = 0.3f;
            decayTime = 0.4f;
            sustainPunch = 0.0f;

            lpFilterResonance = 0.0f;
            lpFilterCutoff = 1.0f;
            lpFilterCutoffSweep = 0.0f;
            hpFilterCutoff = 0.0f;
            hpFilterCutoffSweep = 0.0f;

            phaserOffset = 0.0f;
            phaserSweep = 0.0f;

            repeatSpeed = 0.0f;

            changeSpeed = 0.0f;
            changeAmount = 0.0f;

            // From BFXR
            changeRepeat = 0.0f;
            changeAmount2 = 0.0f;
            changeSpeed2 = 0.0f;

            compressionAmount = 0.3f;

            overtones = 0.0f;
            overtoneFalloff = 0.0f;

            bitCrush = 0.0f;
            bitCrushSweep = 0.0f;
        }


        /// <summary>
        ///     Randomly adjust the parameters ever so slightly
        /// </summary>
        /// <param name="amount">Maximum per-parameter deviation; defaults to 0.05.</param>
        public void Mutate(float amount = 0.05f)
        {
            if (GetRandomBool())
            {
                startFrequency += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                minFrequency += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                slide += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                deltaSlide += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                squareDuty += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                dutySweep += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                vibratoDepth += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                vibratoSpeed += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                attackTime += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                sustainTime += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                decayTime += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                sustainPunch += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                lpFilterCutoff += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                lpFilterCutoffSweep += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                lpFilterResonance += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                hpFilterCutoff += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                hpFilterCutoffSweep += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                phaserOffset += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                phaserSweep += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                repeatSpeed += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                changeSpeed += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                changeAmount += GetRandom() * amount * 2f - amount;
            }

            // From BFXR
            if (GetRandomBool())
            {
                changeRepeat += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                changeAmount2 += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                changeSpeed2 += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                compressionAmount += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                overtones += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                overtoneFalloff += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                bitCrush += GetRandom() * amount * 2f - amount;
            }

            if (GetRandomBool())
            {
                bitCrushSweep += GetRandom() * amount * 2f - amount;
            }
        }


        /// <summary>
        ///     Sets all parameters to random values
        /// </summary>
        public void Randomize()
        {
            Reset();

            WaveType = (TinySynthWaveType) (uint) (GetRandom() * 9f);

            attackTime = Pow(GetRandom() * 2f - 1f, 4);
            sustainTime = Pow(GetRandom() * 2f - 1f, 2);
            sustainPunch = Pow(GetRandom() * 0.8f, 2);
            decayTime = GetRandom();

            startFrequency = GetRandomBool() ? Pow(GetRandom() * 2f - 1f, 2) : Pow(GetRandom() * 0.5f, 3) + 0.5f;
            minFrequency = 0.0f;

            slide = Pow(GetRandom() * 2f - 1f, 3);
            deltaSlide = Pow(GetRandom() * 2f - 1f, 3);

            vibratoDepth = Pow(GetRandom() * 2f - 1f, 3);
            vibratoSpeed = GetRandom() * 2f - 1f;

            changeAmount = GetRandom() * 2f - 1f;
            changeSpeed = GetRandom() * 2f - 1f;

            squareDuty = GetRandom() * 2f - 1f;
            dutySweep = Pow(GetRandom() * 2f - 1f, 3);

            repeatSpeed = GetRandom() * 2f - 1f;

            phaserOffset = Pow(GetRandom() * 2f - 1f, 3);
            phaserSweep = Pow(GetRandom() * 2f - 1f, 3);

            lpFilterCutoff = 1f - Pow(GetRandom(), 3);
            lpFilterCutoffSweep = Pow(GetRandom() * 2f - 1f, 3);
            lpFilterResonance = GetRandom() * 2f - 1f;

            hpFilterCutoff = Pow(GetRandom(), 5);
            hpFilterCutoffSweep = Pow(GetRandom() * 2f - 1f, 5);

            if (attackTime + sustainTime + decayTime < 0.2f)
            {
                sustainTime = 0.2f + GetRandom() * 0.3f;
                decayTime = 0.2f + GetRandom() * 0.3f;
            }

            if ((startFrequency > 0.7f && slide > 0.2) || (startFrequency < 0.2 && slide < -0.05))
            {
                slide = -slide;
            }

            if (lpFilterCutoff < 0.1f && lpFilterCutoffSweep < -0.05f)
            {
                lpFilterCutoffSweep = -lpFilterCutoffSweep;
            }

            // From BFXR
            changeRepeat = GetRandom();
            changeAmount2 = GetRandom() * 2f - 1f;
            changeSpeed2 = GetRandom();

            compressionAmount = GetRandom();

            overtones = GetRandom();
            overtoneFalloff = GetRandom();

            bitCrush = GetRandom();
            bitCrushSweep = GetRandom() * 2f - 1f;
        }


        /// <summary>
        ///     Returns a string representation of the parameters for copy/paste sharing in the old format (24 parameters,
        ///     SFXR/AS3SFXR compatible)
        /// </summary>
        /// <returns>A comma-delimited list of parameter values</returns>
        public string GetSettingsStringLegacy()
        {
            var str = "";

            // 24 params
            str += (uint) WaveType + ",";
            str += To4DP(attackTime) + ",";
            str += To4DP(sustainTime) + ",";
            str += To4DP(sustainPunch) + ",";
            str += To4DP(decayTime) + ",";
            str += To4DP(startFrequency) + ",";
            str += To4DP(minFrequency) + ",";
            str += To4DP(slide) + ",";
            str += To4DP(deltaSlide) + ",";
            str += To4DP(vibratoDepth) + ",";
            str += To4DP(vibratoSpeed) + ",";
            str += To4DP(changeAmount) + ",";
            str += To4DP(changeSpeed) + ",";
            str += To4DP(squareDuty) + ",";
            str += To4DP(dutySweep) + ",";
            str += To4DP(repeatSpeed) + ",";
            str += To4DP(phaserOffset) + ",";
            str += To4DP(phaserSweep) + ",";
            str += To4DP(lpFilterCutoff) + ",";
            str += To4DP(lpFilterCutoffSweep) + ",";
            str += To4DP(lpFilterResonance) + ",";
            str += To4DP(hpFilterCutoff) + ",";
            str += To4DP(hpFilterCutoffSweep) + ",";
            str += To4DP(MasterVolume);

            return str;
        }


        /// <summary>
        ///     Returns a string representation of the parameters for copy/paste sharing in the new format (32 parameters, BFXR
        ///     compatible)
        /// </summary>
        /// <returns>A comma-delimited list of parameter values</returns>
        public string GetSettingsString()
        {
            var str = "";

            // 32 params

            str += (uint) WaveType + ",";
            str += To4DP(MasterVolume) + ",";
            str += To4DP(attackTime) + ",";
            str += To4DP(sustainTime) + ",";
            str += To4DP(sustainPunch) + ",";
            str += To4DP(decayTime) + ",";
            str += To4DP(compressionAmount) + ",";
            str += To4DP(startFrequency) + ",";
            str += To4DP(minFrequency) + ",";
            str += To4DP(slide) + ",";
            str += To4DP(deltaSlide) + ",";
            str += To4DP(vibratoDepth) + ",";
            str += To4DP(vibratoSpeed) + ",";
            str += To4DP(overtones) + ",";
            str += To4DP(overtoneFalloff) + ",";
            str += To4DP(changeRepeat) + ","; // changeRepeat?
            str += To4DP(changeAmount) + ",";
            str += To4DP(changeSpeed) + ",";
            str += To4DP(changeAmount2) + ","; // changeamount2
            str += To4DP(changeSpeed2) + ","; // changespeed2
            str += To4DP(squareDuty) + ",";
            str += To4DP(dutySweep) + ",";
            str += To4DP(repeatSpeed) + ",";
            str += To4DP(phaserOffset) + ",";
            str += To4DP(phaserSweep) + ",";
            str += To4DP(lpFilterCutoff) + ",";
            str += To4DP(lpFilterCutoffSweep) + ",";
            str += To4DP(lpFilterResonance) + ",";
            str += To4DP(hpFilterCutoff) + ",";
            str += To4DP(hpFilterCutoffSweep) + ",";
            str += To4DP(bitCrush) + ",";
            str += To4DP(bitCrushSweep);

            return str;
        }


        /// <summary>
        ///     Parses a settings string into the parameters
        /// </summary>
        /// <returns>If the string successfully parsed</returns>
        public bool SetSettingsString(string input)
        {
            if (input == null)
            {
                Debug.LogError("Could not paste settings string: input is null");

                return false;
            }

            var values = input.Split(',');
            var backup = this;
            var ok = true;

            if (values.Length == 24)
            {
                // Old format (SFXR): 24 parameters
                Reset();

                WaveType = ParseWaveType(values[0], ref ok);
                attackTime = ParseFloat(values[1], ref ok);
                sustainTime = ParseFloat(values[2], ref ok);
                sustainPunch = ParseFloat(values[3], ref ok);
                decayTime = ParseFloat(values[4], ref ok);
                startFrequency = ParseFloat(values[5], ref ok);
                minFrequency = ParseFloat(values[6], ref ok);
                slide = ParseFloat(values[7], ref ok);
                deltaSlide = ParseFloat(values[8], ref ok);
                vibratoDepth = ParseFloat(values[9], ref ok);
                vibratoSpeed = ParseFloat(values[10], ref ok);
                changeAmount = ParseFloat(values[11], ref ok);
                changeSpeed = ParseFloat(values[12], ref ok);
                squareDuty = ParseFloat(values[13], ref ok);
                dutySweep = ParseFloat(values[14], ref ok);
                repeatSpeed = ParseFloat(values[15], ref ok);
                phaserOffset = ParseFloat(values[16], ref ok);
                phaserSweep = ParseFloat(values[17], ref ok);
                lpFilterCutoff = ParseFloat(values[18], ref ok);
                lpFilterCutoffSweep = ParseFloat(values[19], ref ok);
                lpFilterResonance = ParseFloat(values[20], ref ok);
                hpFilterCutoff = ParseFloat(values[21], ref ok);
                hpFilterCutoffSweep = ParseFloat(values[22], ref ok);
                MasterVolume = ParseFloat(values[23], ref ok);
            }
            else if (values.Length >= 32)
            {
                // New format (BFXR): 32 parameters (or more, but locked parameters are ignored)
                Reset();

                WaveType = ParseWaveType(values[0], ref ok);
                MasterVolume = ParseFloat(values[1], ref ok);
                attackTime = ParseFloat(values[2], ref ok);
                sustainTime = ParseFloat(values[3], ref ok);
                sustainPunch = ParseFloat(values[4], ref ok);
                decayTime = ParseFloat(values[5], ref ok);
                compressionAmount = ParseFloat(values[6], ref ok);
                startFrequency = ParseFloat(values[7], ref ok);
                minFrequency = ParseFloat(values[8], ref ok);
                slide = ParseFloat(values[9], ref ok);
                deltaSlide = ParseFloat(values[10], ref ok);
                vibratoDepth = ParseFloat(values[11], ref ok);
                vibratoSpeed = ParseFloat(values[12], ref ok);
                overtones = ParseFloat(values[13], ref ok);
                overtoneFalloff = ParseFloat(values[14], ref ok);
                changeRepeat = ParseFloat(values[15], ref ok);
                changeAmount = ParseFloat(values[16], ref ok);
                changeSpeed = ParseFloat(values[17], ref ok);
                changeAmount2 = ParseFloat(values[18], ref ok);
                changeSpeed2 = ParseFloat(values[19], ref ok);
                squareDuty = ParseFloat(values[20], ref ok);
                dutySweep = ParseFloat(values[21], ref ok);
                repeatSpeed = ParseFloat(values[22], ref ok);
                phaserOffset = ParseFloat(values[23], ref ok);
                phaserSweep = ParseFloat(values[24], ref ok);
                lpFilterCutoff = ParseFloat(values[25], ref ok);
                lpFilterCutoffSweep = ParseFloat(values[26], ref ok);
                lpFilterResonance = ParseFloat(values[27], ref ok);
                hpFilterCutoff = ParseFloat(values[28], ref ok);
                hpFilterCutoffSweep = ParseFloat(values[29], ref ok);
                bitCrush = ParseFloat(values[30], ref ok);
                bitCrushSweep = ParseFloat(values[31], ref ok);
            }
            else
            {
                Debug.LogError(
                    $"Could not paste settings string: parameters contain {values.Length} values (was expecting 24 or >32)");

                return false;
            }

            if (!ok)
            {
                // Leave the parameters untouched when any value is not a number.
                this = backup;
                Debug.LogError("Could not paste settings string: it contains a value that is not a number");

                return false;
            }

            return true;
        }


        /// <summary>
        ///     Faster power function; this function takes about 36% of the time Mathf.Pow() would take in our use cases
        /// </summary>
        private float Pow(float @base, int power)
        {
            switch (power)
            {
                case 2: return @base * @base;
                case 3: return @base * @base * @base;
                case 4: return @base * @base * @base * @base;
                case 5: return @base * @base * @base * @base * @base;
            }

            return 1f;
        }


        /// <summary>
        ///     Returns the number as a string to 4 decimal places
        /// </summary>
        private string To4DP(float value)
        {
            if (value < 0.0001f && value > -0.0001f)
            {
                return "";
            }

            // Invariant culture: a culture with a decimal comma would add extra fields to the comma-delimited string.
            return value.ToString("#.####", CultureInfo.InvariantCulture);
        }


        /// <summary>
        ///     Parses a wave type from its number (or, for strings written by older versions, its name);
        ///     returns 0 if the string is empty. Sets <paramref name="ok" /> to false when the value cannot be parsed.
        /// </summary>
        private static TinySynthWaveType ParseWaveType(string value, ref bool ok)
        {
            value = value.Trim();

            if (value.Length == 0)
            {
                return 0;
            }

            if (uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                return (TinySynthWaveType) number;
            }

            if (Enum.TryParse(value, true, out TinySynthWaveType waveType))
            {
                return waveType;
            }

            ok = false;

            return 0;
        }


        /// <summary>
        ///     Parses a string into a float value (invariant culture); also returns 0 if the string is empty, rather than an error.
        ///     Sets <paramref name="ok" /> to false when the value cannot be parsed.
        /// </summary>
        private static float ParseFloat(string value, ref bool ok)
        {
            if (value.Trim().Length == 0)
            {
                return 0;
            }

            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }

            ok = false;

            return 0;
        }


        /// <summary>
        ///     Returns a random value: bigger than or equal to 0, smaller than 1
        ///     This function is needed so we can follow the original code more strictly, Unitys random may return 1
        /// </summary>
        public static float GetRandom()
        {
            return Random.value % 1;
        }


        /// <summary>
        ///     Returns a random boolean value
        /// </summary>
        public static bool GetRandomBool()
        {
            return Random.value > 0.5f;
        }
    }
}
