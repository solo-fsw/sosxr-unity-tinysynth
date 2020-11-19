using System;
using UnityEngine;

namespace usfxr {
	
	[AttributeUsage(AttributeTargets.Field)]
	public class SfxrDefault : Attribute {
		public readonly float value;
		public SfxrDefault(float value) {
			this.value = value;
		}
	}

	/// <summary>
	/// Holds parameters used by SfxrSynth
	/// </summary>

	[Serializable]
	public struct SfxrParams {
		/// Shape of wave to generate 
		[SfxrDefault(1)] [SerializeField] public WaveType waveType;

		/// Overall volume of the sound (0 to 1)
		[Tooltip("Overall volume of the sound")]
		[SfxrDefault(1)] [Range(0, 1)] public float masterVolume;

		/// Length of the volume envelope attack (0 to 1)
		[Tooltip("Length of the volume envelope attack")]
		[SfxrDefault(0)] [Range(0, 1)] public float attackTime;
		/// Length of the volume envelope sustain (0 to 1)
		[Tooltip("Length of the volume envelope sustain")]
		[SfxrDefault(0)] [Range(0, 1)] public float sustainTime;
		/// Tilts the sustain envelope for more 'pop' (0 to 1)
		[Tooltip("Tilts the sustain envelope for more 'pop'")]
		[SfxrDefault(0)] [Range(0, 1)] public float sustainPunch;
		/// Length of the volume envelope decay (yes, I know it's called release) (0 to 1)
		[Tooltip("Length of the volume envelope decay (yes, I know it's called release)")]
		[SfxrDefault(0)] [Range(0, 1)] public float decayTime;

		/// Base note of the sound (0 to 1)
		[Tooltip("Base note of the sound")]
		[SfxrDefault(.3f)] [Range(0, 1)] public float startFrequency;

		/// If sliding, the sound will stop at this frequency, to prevent really low notes (0 to 1)
		[Tooltip("If sliding, the sound will stop at this frequency, to prevent really low notes")]
		[SfxrDefault(0)] [Range(0, 1)] public float minFrequency;

		/// Slides the note up or down (-1 to 1)
		[Tooltip("Slides the note up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float slide;
		/// Accelerates the slide (-1 to 1)
		[Tooltip("Accelerates the slide")]
		[SfxrDefault(0)] [Range(-1, 1)] public float deltaSlide;

		/// Strength of the vibrato effect (0 to 1)
		[Tooltip("Strength of the vibrato effect")]
		[SfxrDefault(0)] [Range(0, 1)] public float vibratoDepth;
		/// Speed of the vibrato effect (i.e. frequency) (0 to 1)
		[Tooltip("Speed of the vibrato effect (i.e. frequency)")]
		[SfxrDefault(0)] [Range(0, 1)] public float vibratoSpeed;

		/// Shift in note, either up or down (-1 to 1)
		[Tooltip("Shift in note, either up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float changeAmount;
		/// How fast the note shift happens (only happens once) (0 to 1)
		[Tooltip("How fast the note shift happens (only happens once)")]
		[SfxrDefault(0)] [Range(0, 1)] public float changeSpeed;

		/// Controls the ratio between the up and down states of the square wave, changing the timbre (0 to 1)
		[Tooltip("Controls the ratio between the up and down states of the square wave, changing the timbre")]
		[SfxrDefault(0)] [Range(0, 1)] public float squareDuty;

		/// Sweeps the duty up or down (-1 to 1)
		[Tooltip("Sweeps the duty up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float dutySweep;
		/// Speed of the note repeating - certain variables are reset each time (0 to 1)
		[Tooltip("Speed of the note repeating - certain variables are reset each time")]
		[SfxrDefault(0)] [Range(0, 1)] public float repeatSpeed;

		/// Offsets a second copy of the wave by a small phase, changing the timbre (-1 to 1)
		[Tooltip("Offsets a second copy of the wave by a small phase, changing the timbre")]
		[SfxrDefault(0)] [Range(-1, 1)] public float phaserOffset;
		/// Sweeps the phase up or down (-1 to 1)
		[Tooltip("Sweeps the phase up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float phaserSweep;
	
		/// Frequency at which the low-pass filter starts attenuating higher frequencies (0 to 1)
		[Tooltip("Frequency at which the low-pass filter starts attenuating higher frequencies")]
		[SfxrDefault(1)] [Range(0, 1)] public float lpFilterCutoff;
		/// Sweeps the low-pass cutoff up or down (-1 to 1)
		[Tooltip("Sweeps the low-pass cutoff up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float lpFilterCutoffSweep;
		/// Changes the attenuation rate for the low-pass filter, changing the timbre (0 to 1)
		[Tooltip("Changes the attenuation rate for the low-pass filter, changing the timbre")]
		[SfxrDefault(0)] [Range(0, 1)] public float lpFilterResonance;

		/// Frequency at which the high-pass filter starts attenuating lower frequencies (0 to 1)
		[Tooltip("Frequency at which the high-pass filter starts attenuating lower frequencies")]
		[SfxrDefault(0)] [Range(0, 1)] public float hpFilterCutoff;

		/// Sweeps the high-pass cutoff up or down (-1 to 1)
		[Tooltip("Sweeps the high-pass cutoff up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float hpFilterCutoffSweep;

		// From BFXR
		/// Pitch Jump Repeat Speed: larger Values means more pitch jumps, which can be useful for arpeggiation (0 to 1)
		[Tooltip("Pitch Jump Repeat Speed: larger Values means more pitch jumps, which can be useful for arpeggiation")]
		[SfxrDefault(0)] [Range(0, 1)] public float changeRepeat;
		/// Shift in note, either up or down (-1 to 1)
		[Tooltip("Shift in note, either up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float changeAmount2;
		/// How fast the note shift happens (only happens once) (0 to 1)
		[Tooltip("How fast the note shift happens (only happens once)")]
		[SfxrDefault(0)] [Range(0, 1)] public float changeSpeed2;
		/// Compression: pushes amplitudes together into a narrower range to make them stand out more. Very good for sound
		/// effects, where you want them to stick out against background music (0 to 1)
		[Tooltip("Compression: pushes amplitudes together into a narrower range to make them stand out more. Very good for sound effects, where you want them to stick out against background music")]
		[SfxrDefault(.3f)] [Range(0, 1)] public float compressionAmount;
		/// Harmonics: overlays copies of the waveform with copies and multiples of its frequency. Good for bulking out or
		/// otherwise enriching the texture of the sounds (warning: this is the number 1 cause of usfxr slowdown!) (0 to 1)
		[Tooltip("Harmonics: overlays copies of the waveform with copies and multiples of its frequency. Good for bulking out or otherwise enriching the texture of the sounds (warning: this is the number 1 cause of usfxr slowdown!)")]
		[SfxrDefault(0)] [Range(0, 1)] public float overtones;
		/// Harmonics falloff: the rate at which higher overtones should decay (0 to 1)
		[Tooltip("Harmonics falloff: the rate at which higher overtones should decay")]
		[SfxrDefault(0)] [Range(0, 1)] public float overtoneFalloff;
		/// Bit crush: resamples the audio at a lower frequency (0 to 1)
		[Tooltip("Bit crush: resamples the audio at a lower frequency")]
		[SfxrDefault(0)] [Range(0, 1)] public float bitCrush;
		/// Bit crush sweep: sweeps the Bit Crush filter up or down (-1 to 1)
		[Tooltip("Bit crush sweep: sweeps the Bit Crush filter up or down")]
		[SfxrDefault(0)] [Range(-1, 1)] public float bitCrushSweep;


		/// <summary>
		/// Resets the parameters, used at the start of each generate function
		/// </summary>
		public void Reset() {
			waveType       = 0;
			masterVolume   = 1f;
			startFrequency = 0.3f;
			minFrequency   = 0.0f;
			slide          = 0.0f;
			deltaSlide     = 0.0f;
			squareDuty     = 0.0f;
			dutySweep      = 0.0f;

			vibratoDepth = 0.0f;
			vibratoSpeed = 0.0f;

			attackTime   = 0.0f;
			sustainTime  = 0.3f;
			decayTime    = 0.4f;
			sustainPunch = 0.0f;

			lpFilterResonance   = 0.0f;
			lpFilterCutoff      = 1.0f;
			lpFilterCutoffSweep = 0.0f;
			hpFilterCutoff      = 0.0f;
			hpFilterCutoffSweep = 0.0f;

			phaserOffset = 0.0f;
			phaserSweep  = 0.0f;

			repeatSpeed = 0.0f;

			changeSpeed  = 0.0f;
			changeAmount = 0.0f;

			// From BFXR
			changeRepeat  = 0.0f;
			changeAmount2 = 0.0f;
			changeSpeed2  = 0.0f;

			compressionAmount = 0.3f;

			overtones       = 0.0f;
			overtoneFalloff = 0.0f;

			bitCrush      = 0.0f;
			bitCrushSweep = 0.0f;
		}

		/// <summary>
		/// Randomly adjust the parameters ever so slightly
		/// </summary>
		/// <param name="amount"></param>
		public void Mutate(float amount = 0.05f) {
			if (GetRandomBool()) startFrequency      += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) minFrequency        += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) slide               += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) deltaSlide          += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) squareDuty          += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) dutySweep           += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) vibratoDepth        += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) vibratoSpeed        += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) attackTime          += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) sustainTime         += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) decayTime           += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) sustainPunch        += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) lpFilterCutoff      += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) lpFilterCutoffSweep += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) lpFilterResonance   += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) hpFilterCutoff      += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) hpFilterCutoffSweep += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) phaserOffset        += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) phaserSweep         += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) repeatSpeed         += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) changeSpeed         += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) changeAmount        += GetRandom() * amount * 2f - amount;

			// From BFXR
			if (GetRandomBool()) changeRepeat      += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) changeAmount2     += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) changeSpeed2      += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) compressionAmount += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) overtones         += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) overtoneFalloff   += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) bitCrush          += GetRandom() * amount * 2f - amount;
			if (GetRandomBool()) bitCrushSweep     += GetRandom() * amount * 2f - amount;
		}
		
		/// <summary>
		/// Sets all parameters to random values
		/// </summary>
		public void Randomize() {
			Reset();

			waveType = (WaveType) (uint) (GetRandom() * 9f);

			attackTime   = Pow(GetRandom() * 2f - 1f, 4);
			sustainTime  = Pow(GetRandom() * 2f - 1f, 2);
			sustainPunch = Pow(GetRandom() * 0.8f, 2);
			decayTime    = GetRandom();

			startFrequency = GetRandomBool() ? Pow(GetRandom() * 2f - 1f, 2) : Pow(GetRandom() * 0.5f, 3) + 0.5f;
			minFrequency   = 0.0f;

			slide      = Pow(GetRandom() * 2f - 1f, 3);
			deltaSlide = Pow(GetRandom() * 2f - 1f, 3);

			vibratoDepth = Pow(GetRandom() * 2f - 1f, 3);
			vibratoSpeed = GetRandom() * 2f - 1f;

			changeAmount = GetRandom() * 2f - 1f;
			changeSpeed  = GetRandom() * 2f - 1f;

			squareDuty = GetRandom() * 2f - 1f;
			dutySweep  = Pow(GetRandom() * 2f - 1f, 3);

			repeatSpeed = GetRandom() * 2f - 1f;

			phaserOffset = Pow(GetRandom() * 2f - 1f, 3);
			phaserSweep  = Pow(GetRandom() * 2f - 1f, 3);

			lpFilterCutoff      = 1f - Pow(GetRandom(), 3);
			lpFilterCutoffSweep = Pow(GetRandom() * 2f - 1f, 3);
			lpFilterResonance   = GetRandom() * 2f - 1f;

			hpFilterCutoff      = Pow(GetRandom(), 5);
			hpFilterCutoffSweep = Pow(GetRandom() * 2f - 1f, 5);

			if (attackTime + sustainTime + decayTime < 0.2f) {
				sustainTime = 0.2f + GetRandom() * 0.3f;
				decayTime   = 0.2f + GetRandom() * 0.3f;
			}

			if ((startFrequency > 0.7f && slide > 0.2) || (startFrequency < 0.2 && slide < -0.05)) {
				slide = -slide;
			}

			if (lpFilterCutoff < 0.1f && lpFilterCutoffSweep < -0.05f) {
				lpFilterCutoffSweep = -lpFilterCutoffSweep;
			}

			// From BFXR
			changeRepeat  = GetRandom();
			changeAmount2 = GetRandom() * 2f - 1f;
			changeSpeed2  = GetRandom();

			compressionAmount = GetRandom();

			overtones       = GetRandom();
			overtoneFalloff = GetRandom();

			bitCrush      = GetRandom();
			bitCrushSweep = GetRandom() * 2f - 1f;
		}

		/// <summary>
		/// Returns a string representation of the parameters for copy/paste sharing in the old format (24 parameters, SFXR/AS3SFXR compatible)
		/// </summary>
		/// <returns>A comma-delimited list of parameter values</returns>
		public string GetSettingsStringLegacy() {
			var str = "";

			// 24 params
			str += waveType + ",";
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
			str += To4DP(masterVolume);

			return str;
		}

		/// <summary>
		/// Returns a string representation of the parameters for copy/paste sharing in the new format (32 parameters, BFXR compatible)
		/// </summary>
		/// <returns>A comma-delimited list of parameter values</returns>
		public string GetSettingsString() {
			var str = "";

			// 32 params

			str += waveType + ",";
			str += To4DP(masterVolume) + ",";
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
			str += To4DP(changeSpeed2) + ",";  // changespeed2
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
		/// Parses a settings string into the parameters
		/// </summary>
		/// <returns>If the string successfully parsed</returns>
		public bool SetSettingsString(string input) {
			var values = input.Split(',');

			if (values.Length == 24) {
				// Old format (SFXR): 24 parameters
				Reset();

				waveType            = (WaveType) ParseUint(values[0]);
				attackTime          = ParseFloat(values[1]);
				sustainTime         = ParseFloat(values[2]);
				sustainPunch        = ParseFloat(values[3]);
				decayTime           = ParseFloat(values[4]);
				startFrequency      = ParseFloat(values[5]);
				minFrequency        = ParseFloat(values[6]);
				slide               = ParseFloat(values[7]);
				deltaSlide          = ParseFloat(values[8]);
				vibratoDepth        = ParseFloat(values[9]);
				vibratoSpeed        = ParseFloat(values[10]);
				changeAmount        = ParseFloat(values[11]);
				changeSpeed         = ParseFloat(values[12]);
				squareDuty          = ParseFloat(values[13]);
				dutySweep           = ParseFloat(values[14]);
				repeatSpeed         = ParseFloat(values[15]);
				phaserOffset        = ParseFloat(values[16]);
				phaserSweep         = ParseFloat(values[17]);
				lpFilterCutoff      = ParseFloat(values[18]);
				lpFilterCutoffSweep = ParseFloat(values[19]);
				lpFilterResonance   = ParseFloat(values[20]);
				hpFilterCutoff      = ParseFloat(values[21]);
				hpFilterCutoffSweep = ParseFloat(values[22]);
				masterVolume        = ParseFloat(values[23]);
			} else if (values.Length >= 32) {
				// New format (BFXR): 32 parameters (or more, but locked parameters are ignored)
				Reset();

				waveType            = (WaveType) ParseUint(values[0]);
				masterVolume        = ParseFloat(values[1]);
				attackTime          = ParseFloat(values[2]);
				sustainTime         = ParseFloat(values[3]);
				sustainPunch        = ParseFloat(values[4]);
				decayTime           = ParseFloat(values[5]);
				compressionAmount   = ParseFloat(values[6]);
				startFrequency      = ParseFloat(values[7]);
				minFrequency        = ParseFloat(values[8]);
				slide               = ParseFloat(values[9]);
				deltaSlide          = ParseFloat(values[10]);
				vibratoDepth        = ParseFloat(values[11]);
				vibratoSpeed        = ParseFloat(values[12]);
				overtones           = ParseFloat(values[13]);
				overtoneFalloff     = ParseFloat(values[14]);
				changeRepeat        = ParseFloat(values[15]);
				changeAmount        = ParseFloat(values[16]);
				changeSpeed         = ParseFloat(values[17]);
				changeAmount2       = ParseFloat(values[18]);
				changeSpeed2        = ParseFloat(values[19]);
				squareDuty          = ParseFloat(values[20]);
				dutySweep           = ParseFloat(values[21]);
				repeatSpeed         = ParseFloat(values[22]);
				phaserOffset        = ParseFloat(values[23]);
				phaserSweep         = ParseFloat(values[24]);
				lpFilterCutoff      = ParseFloat(values[25]);
				lpFilterCutoffSweep = ParseFloat(values[26]);
				lpFilterResonance   = ParseFloat(values[27]);
				hpFilterCutoff      = ParseFloat(values[28]);
				hpFilterCutoffSweep = ParseFloat(values[29]);
				bitCrush            = ParseFloat(values[30]);
				bitCrushSweep       = ParseFloat(values[31]);
			} else {
				Debug.LogError(
					$"Could not paste settings string: parameters contain {values.Length} values (was expecting 24 or >32)");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Faster power function; this function takes about 36% of the time Mathf.Pow() would take in our use cases
		/// </summary>
		float Pow(float @base, int power) {
			switch(power) {
				case 2: return @base * @base;
				case 3: return @base * @base * @base;
				case 4: return @base * @base * @base * @base;
				case 5: return @base * @base * @base * @base * @base;
			}

			return 1f;
		}

		/// <summary>
		/// Returns the number as a string to 4 decimal places
		/// </summary>
		string To4DP(float value) {
			if (value < 0.0001f && value > -0.0001f) return "";
			return value.ToString("#.####");
		}

		/// <summary>
		/// Parses a string into an uint value; also returns 0 if the string is empty, rather than an error
		/// </summary>
		uint ParseUint(string value) {
			return value.Length == 0 ? 0 : uint.Parse(value);
		}

		/// <summary>
		/// Parses a string into a float value; also returns 0 if the string is empty, rather than an error
		/// </summary>
		float ParseFloat(string value) {
			if (value.Length == 0) return 0;
			return float.Parse(value);
		}

		/// <summary>
		/// Returns a random value: bigger than or equal to 0, smaller than 1
		/// This function is needed so we can follow the original code more strictly, Unitys random may return 1
		/// </summary>
		public static float GetRandom() {
			return UnityEngine.Random.value % 1;
		}

		/// <summary>
		/// Returns a boolean value
		/// </summary>
		public static bool GetRandomBool() {
			return UnityEngine.Random.value > 0.5f;
		}
	}
}