using System;
using UnityEngine;
using Random = System.Random;


namespace SOSXR.TinySynth
{
    /// <summary>
    ///     Renders audio in preparation for playing
    /// </summary>
    public class TinySynthRenderer
    {
        public TinySynthSound param;

        // Synth properties
        private float masterVolume; // masterVolume * masterVolume (for quick calculations)

        private TinySynthWaveType _tinySynthWaveType; // Shape of wave to generate (see enum WaveType)

        private float envelopeVolume; // Current volume of the envelope
        private int envelopeStage; // Current stage of the envelope (attack, sustain, decay, end)
        private float envelopeTime; // Current time through current enelope stage
        private float envelopeLength; // Length of the current envelope stage
        private float envelopeLength0; // Length of the attack stage
        private float envelopeLength1; // Length of the sustain stage
        private float envelopeLength2; // Length of the decay stage
        private float envelopeOverLength0; // 1 / _envelopeLength0 (for quick calculations)
        private float envelopeOverLength1; // 1 / _envelopeLength1 (for quick calculations)
        private float envelopeOverLength2; // 1 / _envelopeLength2 (for quick calculations)
        private uint envelopeFullLength; // Full length of the volume envelop (and therefore sound)

        private float sustainPunch; // The punch factor (louder at begining of sustain)

        private int phase; // Phase through the wave
        private float pos; // Phase expressed as a Number from 0-1, used for fast sin approx
        private float period; // Period of the wave
        private float periodTemp; // Period modified by vibrato
        private int periodTempInt; // Period modified by vibrato (as an Int)
        private float maxPeriod; // Maximum period before sound stops (from minFrequency)

        private float slide; // Note slide
        private float deltaSlide; // Change in slide
        private float minFrequency; // Minimum frequency before stopping

        private float vibratoPhase; // Phase through the vibrato sine wave
        private float vibratoSpeed; // Speed at which the vibrato phase moves
        private float vibratoAmplitude; // Amount to change the period of the wave by at the peak of the vibrato wave

        private float changeAmount; // Amount to change the note by
        private int changeTime; // Counter for the note change
        private int changeLimit; // Once the time reaches this limit, the note changes

        private float squareDuty; // Offset of center switching point in the square wave
        private float dutySweep; // Amount to change the duty by

        private int repeatTime; // Counter for the repeats
        private int repeatLimit; // Once the time reaches this limit, some of the variables are reset

        private bool phaser; // If the phaser is active
        private float phaserOffset; // Phase offset for phaser effect
        private float phaserDeltaOffset; // Change in phase offset
        private int phaserInt; // Integer phaser offset, for bit maths
        private int phaserPos; // Position through the phaser buffer
        private float[] phaserBuffer; // Buffer of wave values used to create the out of phase second wave

        private bool filters; // If the filters are active
        private float lpFilterPos; // Adjusted wave position after low-pass filter
        private float lpFilterOldPos; // Previous low-pass wave position
        private float lpFilterDeltaPos; // Change in low-pass wave position, as allowed by the cutoff and damping
        private float lpFilterCutoff; // Cutoff multiplier which adjusts the amount the wave position can move
        private float lpFilterDeltaCutoff; // Speed of the low-pass cutoff multiplier
        private float lpFilterDamping; // Damping multiplier which restricts how fast the wave position can move
        private bool lpFilterOn; // If the low pass filter is active

        private float hpFilterPos; // Adjusted wave position after high-pass filter
        private float hpFilterCutoff; // Cutoff multiplier which adjusts the amount the wave position can move
        private float hpFilterDeltaCutoff; // Speed of the high-pass cutoff multiplier

        // From BFXR
        private float changePeriod;
        private int changePeriodTime;

        private bool changeReached;

        private float changeAmount2; // Amount to change the note by
        private int changeTime2; // Counter for the note change
        private int changeLimit2; // Once the time reaches this limit, the note changes
        private bool changeReached2;

        private int overtones; // Minimum frequency before stopping
        private float overtoneFalloff; // Minimum frequency before stopping

        private float bitcrushFreq; // Inversely proportional to the number of samples to skip
        private float bitcrushFreqSweep; // Change of the above
        private float bitcrushPhase; // Samples when this > 1
        private float bitcrushLast; // Last sample value

        private float compressionFactor;

        // Pre-calculated data
        private float[] noiseBuffer; // Buffer of random values used to generate noise
        private float[] pinkNoiseBuffer; // Buffer of random values used to generate pink noise
        private PinkNumber pinkNumber; // Used to generate pink noise
        private float[] loResNoiseBuffer; // Buffer of random values used to generate Tan waveform

        // Temp
        private float superSample; // Actual sample written to the wave
        private float sample; // Sub-sample calculated 8 times per actual sample, averaged out to get the super sample
        private float sample2; // Used in other calculations
        private float amp; // Used in other calculations

        private const int LoResNoisePeriod = 8; // Should be < 32


        /// <summary>
        ///     Synchronously generates a Unity AudioClip
        /// </summary>
        public AudioClip GenerateClip(TinySynthSound param)
        {
            // var s = new Stopwatch();
            // s.Start();
            this.param = param;
            Reset(true);

            var clip = AudioClip.Create("usfxr", (int) envelopeFullLength, 1, 44100, false);

            var sampleData = new float[envelopeFullLength];
            SynthWave(sampleData, 0, envelopeFullLength);
            clip.SetData(sampleData, 0);

            // Debug.Log($"Generated sfx in {s.Elapsed.TotalMilliseconds:F1} ms.");
            return clip;
        }


        public byte[] GetWavFile(uint sampleRate = 44100, uint bitDepth = 16)
        {
            Reset(true);

            if (sampleRate != 44100)
            {
                sampleRate = 22050;
            }

            if (bitDepth != 16)
            {
                bitDepth = 8;
            }

            var soundLength = envelopeFullLength;

            if (bitDepth == 16)
            {
                soundLength *= 2;
            }

            if (sampleRate == 22050)
            {
                soundLength /= 2;
            }

            var fileSize = 36 + soundLength;
            var blockAlign = bitDepth / 8;
            var bytesPerSec = sampleRate * blockAlign;

            // The file size is actually 8 bytes more than the fileSize
            var wav = new byte[fileSize + 8];

            var bytePos = 0;

            // Header

            // Chunk ID "RIFF"
            writeUintToBytes(wav, ref bytePos, 0x52494646, Endian.BigEndian);

            // Chunck Data Size
            writeUintToBytes(wav, ref bytePos, fileSize, Endian.LittleEndian);

            // RIFF Type "WAVE"
            writeUintToBytes(wav, ref bytePos, 0x57415645, Endian.BigEndian);

            // Format Chunk

            // Chunk ID "fmt "
            writeUintToBytes(wav, ref bytePos, 0x666D7420, Endian.BigEndian);

            // Chunk Data Size
            writeUintToBytes(wav, ref bytePos, 16, Endian.LittleEndian);

            // Compression Code PCM
            writeShortToBytes(wav, ref bytePos, 1, Endian.LittleEndian);
            // Number of channels
            writeShortToBytes(wav, ref bytePos, 1, Endian.LittleEndian);
            // Sample rate
            writeUintToBytes(wav, ref bytePos, sampleRate, Endian.LittleEndian);
            // Average bytes per second
            writeUintToBytes(wav, ref bytePos, bytesPerSec, Endian.LittleEndian);
            // Block align
            writeShortToBytes(wav, ref bytePos, (short) blockAlign, Endian.LittleEndian);
            // Significant bits per sample
            writeShortToBytes(wav, ref bytePos, (short) bitDepth, Endian.LittleEndian);

            // Data Chunk

            // Chunk ID "data"
            writeUintToBytes(wav, ref bytePos, 0x64617461, Endian.BigEndian);
            // Chunk Data Size
            writeUintToBytes(wav, ref bytePos, soundLength, Endian.LittleEndian);

            // Generate normal synth data
            var audioData = new float[envelopeFullLength];
            SynthWave(audioData, 0, envelopeFullLength);

            // Write data as bytes
            var sampleCount = 0;
            var bufferSample = 0f;

            foreach (var t in audioData)
            {
                bufferSample += t;
                sampleCount++;

                if (sampleRate != 44100 && sampleCount != 2)
                {
                    continue;
                }

                bufferSample /= sampleCount;
                sampleCount = 0;

                if (bitDepth == 16)
                {
                    writeShortToBytes(wav, ref bytePos, (short) Math.Round(32000f * bufferSample), Endian.LittleEndian);
                }
                else
                {
                    writeBytes(wav, ref bytePos, new[] {(byte) (Math.Round(bufferSample * 127f) + 128)}, Endian.LittleEndian);
                }

                bufferSample = 0f;
            }

            return wav;
        }


        /// <summary>
        ///     Resets the running variables from the params
        ///     Used once at the start (total reset) and for the repeat effect (partial reset)
        ///     @param	totalReset	If the reset is total
        /// </summary>
        private void Reset(bool totalReset)
        {
            // Shorter reference
            var p = param;

            period = 100.0f / (p.startFrequency * p.startFrequency + 0.001f);
            maxPeriod = 100.0f / (p.minFrequency * p.minFrequency + 0.001f);

            slide = 1.0f - p.slide * p.slide * p.slide * 0.01f;
            deltaSlide = -p.deltaSlide * p.deltaSlide * p.deltaSlide * 0.000001f;

            if (p.WaveType == 0)
            {
                squareDuty = 0.5f - p.squareDuty * 0.5f;
                dutySweep = -p.dutySweep * 0.00005f;
            }

            changePeriod = Mathf.Max((1f - p.changeRepeat + 0.1f) / 1.1f) * 20000f + 32f;
            changePeriodTime = 0;

            if (p.changeAmount > 0.0)
            {
                changeAmount = 1.0f - p.changeAmount * p.changeAmount * 0.9f;
            }
            else
            {
                changeAmount = 1.0f + p.changeAmount * p.changeAmount * 10.0f;
            }

            changeTime = 0;
            changeReached = false;

            if (p.changeSpeed == 1.0f)
            {
                changeLimit = 0;
            }
            else
            {
                changeLimit = (int) ((1f - p.changeSpeed) * (1f - p.changeSpeed) * 20000f + 32f);
            }

            if (p.changeAmount2 > 0f)
            {
                changeAmount2 = 1f - p.changeAmount2 * p.changeAmount2 * 0.9f;
            }
            else
            {
                changeAmount2 = 1f + p.changeAmount2 * p.changeAmount2 * 10f;
            }

            changeTime2 = 0;
            changeReached2 = false;

            if (p.changeSpeed2 == 1.0f)
            {
                changeLimit2 = 0;
            }
            else
            {
                changeLimit2 = (int) ((1f - p.changeSpeed2) * (1f - p.changeSpeed2) * 20000f + 32f);
            }

            changeLimit = (int) (changeLimit * ((1f - p.changeRepeat + 0.1f) / 1.1f));
            changeLimit2 = (int) (changeLimit2 * ((1f - p.changeRepeat + 0.1f) / 1.1f));

            if (!totalReset)
            {
                return;
            }

            masterVolume = p.MasterVolume * p.MasterVolume;

            _tinySynthWaveType = p.WaveType;

            if (p.sustainTime < 0.01)
            {
                p.sustainTime = 0.01f;
            }

            var totalTime = p.attackTime + p.sustainTime + p.decayTime;

            if (totalTime < 0.18f)
            {
                var multiplier = 0.18f / totalTime;
                p.attackTime *= multiplier;
                p.sustainTime *= multiplier;
                p.decayTime *= multiplier;
            }

            sustainPunch = p.sustainPunch;

            phase = 0;

            overtones = (int) (p.overtones * 10f);
            overtoneFalloff = p.overtoneFalloff;

            minFrequency = p.minFrequency;

            bitcrushFreq = 1f - Mathf.Pow(p.bitCrush, 1f / 3f);
            bitcrushFreqSweep = -p.bitCrushSweep * 0.000015f;
            bitcrushPhase = 0;
            bitcrushLast = 0;

            compressionFactor = 1f / (1f + 4f * p.compressionAmount);

            filters = p.lpFilterCutoff != 1.0 || p.hpFilterCutoff != 0.0;

            lpFilterPos = 0.0f;
            lpFilterDeltaPos = 0.0f;
            lpFilterCutoff = p.lpFilterCutoff * p.lpFilterCutoff * p.lpFilterCutoff * 0.1f;
            lpFilterDeltaCutoff = 1.0f + p.lpFilterCutoffSweep * 0.0001f;
            lpFilterDamping = 5.0f / (1.0f + p.lpFilterResonance * p.lpFilterResonance * 20.0f) * (0.01f + lpFilterCutoff);

            if (lpFilterDamping > 0.8f)
            {
                lpFilterDamping = 0.8f;
            }

            lpFilterDamping = 1.0f - lpFilterDamping;
            lpFilterOn = p.lpFilterCutoff != 1.0f;

            hpFilterPos = 0.0f;
            hpFilterCutoff = p.hpFilterCutoff * p.hpFilterCutoff * 0.1f;
            hpFilterDeltaCutoff = 1.0f + p.hpFilterCutoffSweep * 0.0003f;

            vibratoPhase = 0.0f;
            vibratoSpeed = p.vibratoSpeed * p.vibratoSpeed * 0.01f;
            vibratoAmplitude = p.vibratoDepth * 0.5f;

            envelopeVolume = 0.0f;
            envelopeStage = 0;
            envelopeTime = 0;
            envelopeLength0 = p.attackTime * p.attackTime * 100000.0f;
            envelopeLength1 = p.sustainTime * p.sustainTime * 100000.0f;
            envelopeLength2 = p.decayTime * p.decayTime * 100000.0f + 10f;
            envelopeLength = envelopeLength0;
            envelopeFullLength = (uint) (envelopeLength0 + envelopeLength1 + envelopeLength2);

            envelopeOverLength0 = 1.0f / envelopeLength0;
            envelopeOverLength1 = 1.0f / envelopeLength1;
            envelopeOverLength2 = 1.0f / envelopeLength2;

            phaser = p.phaserOffset != 0.0f || p.phaserSweep != 0.0f;

            phaserOffset = p.phaserOffset * p.phaserOffset * 1020.0f;

            if (p.phaserOffset < 0.0f)
            {
                phaserOffset = -phaserOffset;
            }

            phaserDeltaOffset = p.phaserSweep * p.phaserSweep * p.phaserSweep * 0.2f;
            phaserPos = 0;

            if (phaserBuffer == null)
            {
                phaserBuffer = new float[1024];
            }

            if (noiseBuffer == null)
            {
                noiseBuffer = new float[32];
            }

            if (pinkNoiseBuffer == null)
            {
                pinkNoiseBuffer = new float[32];
            }

            if (pinkNumber == null)
            {
                pinkNumber = new PinkNumber();
            }

            if (loResNoiseBuffer == null)
            {
                loResNoiseBuffer = new float[32];
            }

            uint i;

            for (i = 0; i < 1024; i++)
            {
                phaserBuffer[i] = 0.0f;
            }

            for (i = 0; i < 32; i++)
            {
                noiseBuffer[i] = TinySynthSound.GetRandom() * 2.0f - 1.0f;
            }

            for (i = 0; i < 32; i++)
            {
                pinkNoiseBuffer[i] = pinkNumber.getNextValue();
            }

            for (i = 0; i < 32; i++)
            {
                loResNoiseBuffer[i] = i % LoResNoisePeriod == 0 ? TinySynthSound.GetRandom() * 2.0f - 1.0f : loResNoiseBuffer[i - 1];
            }

            repeatTime = 0;

            if (p.repeatSpeed == 0.0)
            {
                repeatLimit = 0;
            }
            else
            {
                repeatLimit = (int) ((1.0 - p.repeatSpeed) * (1.0 - p.repeatSpeed) * 20000) + 32;
            }
        }


        /// <summary>
        ///     Writes the wave to the supplied buffer array of floats (it'll contain the mono audio)
        /// </summary>
        private bool SynthWave(float[] buffer, int bufferPos, uint length)
        {
            var finished = false;

            for (var i = 0; i < (int) length; i++)
            {
                if (finished)
                {
                    return true;
                }

                // Repeats every repeatLimit times, partially resetting the sound parameters
                if (repeatLimit != 0)
                {
                    if (++repeatTime >= repeatLimit)
                    {
                        repeatTime = 0;
                        Reset(false);
                    }
                }

                changePeriodTime++;

                if (changePeriodTime >= changePeriod)
                {
                    changeTime = 0;
                    changeTime2 = 0;
                    changePeriodTime = 0;

                    if (changeReached)
                    {
                        period /= changeAmount;
                        changeReached = false;
                    }

                    if (changeReached2)
                    {
                        period /= changeAmount2;
                        changeReached2 = false;
                    }
                }

                // If changeLimit is reached, shifts the pitch
                if (!changeReached)
                {
                    if (++changeTime >= changeLimit)
                    {
                        changeReached = true;
                        period *= changeAmount;
                    }
                }

                // If changeLimit is reached, shifts the pitch
                if (!changeReached2)
                {
                    if (++changeTime2 >= changeLimit2)
                    {
                        changeReached2 = true;
                        period *= changeAmount2;
                    }
                }

                // Accelerate and apply slide
                slide += deltaSlide;
                period *= slide;

                // Checks for frequency getting too low, and stops the sound if a minFrequency was set
                if (period > maxPeriod)
                {
                    period = maxPeriod;

                    if (minFrequency > 0)
                    {
                        finished = true;
                    }
                }

                periodTemp = period;

                // Applies the vibrato effect
                if (vibratoAmplitude > 0)
                {
                    vibratoPhase += vibratoSpeed;
                    periodTemp = period * (1.0f + Mathf.Sin(vibratoPhase) * vibratoAmplitude);
                }

                periodTempInt = (int) periodTemp;

                if (periodTemp < 8)
                {
                    periodTemp = periodTempInt = 8;
                }

                // Sweeps the square duty
                if (_tinySynthWaveType == 0)
                {
                    squareDuty += dutySweep;

                    if (squareDuty < 0.0)
                    {
                        squareDuty = 0.0f;
                    }
                    else if (squareDuty > 0.5)
                    {
                        squareDuty = 0.5f;
                    }
                }

                // Moves through the different stages of the volume envelope
                if (++envelopeTime > envelopeLength)
                {
                    envelopeTime = 0;

                    switch (++envelopeStage)
                    {
                        case 1:
                            envelopeLength = envelopeLength1;

                            break;
                        case 2:
                            envelopeLength = envelopeLength2;

                            break;
                    }
                }

                // Sets the volume based on the position in the envelope
                switch (envelopeStage)
                {
                    case 0:
                        envelopeVolume = envelopeTime * envelopeOverLength0;

                        break;
                    case 1:
                        envelopeVolume = 1.0f + (1.0f - envelopeTime * envelopeOverLength1) * 2.0f * sustainPunch;

                        break;
                    case 2:
                        envelopeVolume = 1.0f - envelopeTime * envelopeOverLength2;

                        break;
                    case 3:
                        envelopeVolume = 0.0f;
                        finished = true;

                        break;
                }

                // Moves the phaser offset
                if (phaser)
                {
                    phaserOffset += phaserDeltaOffset;
                    phaserInt = (int) phaserOffset;

                    if (phaserInt < 0)
                    {
                        phaserInt = -phaserInt;
                    }
                    else if (phaserInt > 1023)
                    {
                        phaserInt = 1023;
                    }
                }

                // Moves the high-pass filter cutoff
                if (filters && hpFilterDeltaCutoff != 0)
                {
                    hpFilterCutoff *= hpFilterDeltaCutoff;

                    if (hpFilterCutoff < 0.00001f)
                    {
                        hpFilterCutoff = 0.00001f;
                    }
                    else if (hpFilterCutoff > 0.1f)
                    {
                        hpFilterCutoff = 0.1f;
                    }
                }

                superSample = 0;
                int j;

                for (j = 0; j < 8; j++)
                {
                    // Cycles through the period
                    phase++;

                    if (phase >= periodTempInt)
                    {
                        phase = phase % periodTempInt;

                        // Generates new random noise for this period
                        int n;

                        if (_tinySynthWaveType == TinySynthWaveType.Noise)
                        {
                            // Noise
                            for (n = 0; n < 32; n++)
                            {
                                noiseBuffer[n] = TinySynthSound.GetRandom() * 2.0f - 1.0f;
                            }
                        }
                        else if (_tinySynthWaveType == TinySynthWaveType.PinkNoise)
                        {
                            // Pink noise
                            for (n = 0; n < 32; n++)
                            {
                                pinkNoiseBuffer[n] = pinkNumber.getNextValue();
                            }
                        }
                        else if (_tinySynthWaveType == TinySynthWaveType.Tan)
                        {
                            // Tan
                            for (n = 0; n < 32; n++)
                            {
                                loResNoiseBuffer[n] = n % LoResNoisePeriod == 0
                                    ? TinySynthSound.GetRandom() * 2.0f - 1.0f
                                    : loResNoiseBuffer[n - 1];
                            }
                        }
                    }

                    sample = 0;
                    float sampleTotal = 0;
                    var overtoneStrength = 1f;

                    int k;

                    for (k = 0; k <= overtones; k++)
                    {
                        var tempPhase = phase * (k + 1) % periodTemp;

                        // Gets the sample from the oscillator
                        switch (_tinySynthWaveType)
                        {
                            case TinySynthWaveType.Square:
                                sample = tempPhase / periodTemp < squareDuty ? 0.5f : -0.5f;

                                break;
                            case TinySynthWaveType.Sawtooth:
                                sample = 1.0f - tempPhase / periodTemp * 2.0f;

                                break;
                            case TinySynthWaveType.Sine:
                                pos = tempPhase / periodTemp;
                                pos = pos > 0.5f ? (pos - 1.0f) * 6.28318531f : pos * 6.28318531f;

                                sample = pos < 0
                                    ? 1.27323954f * pos + 0.405284735f * pos * pos
                                    : 1.27323954f * pos - 0.405284735f * pos * pos;

                                sample = sample < 0
                                    ? 0.225f * (sample * -sample - sample) + sample
                                    : 0.225f * (sample * sample - sample) + sample;

                                break;
                            case TinySynthWaveType.Noise:
                                // Noise
                                sample = noiseBuffer[(uint) (tempPhase * 32f / periodTempInt) % 32];

                                break;
                            case TinySynthWaveType.Triangle:
                                sample = Math.Abs(1f - tempPhase / periodTemp * 2f) - 1f;

                                break;
                            case TinySynthWaveType.PinkNoise:
                                sample = pinkNoiseBuffer[(uint) (tempPhase * 32f / periodTempInt) % 32];

                                break;
                            case TinySynthWaveType.Tan:
                                // Tan
                                sample = (float) Math.Tan(Math.PI * tempPhase / periodTemp);

                                break;
                            case TinySynthWaveType.Whistle:
                                // Sine wave code
                                pos = tempPhase / periodTemp;
                                pos = pos > 0.5f ? (pos - 1.0f) * 6.28318531f : pos * 6.28318531f;

                                sample = pos < 0
                                    ? 1.27323954f * pos + 0.405284735f * pos * pos
                                    : 1.27323954f * pos - 0.405284735f * pos * pos;

                                sample = 0.75f * (sample < 0
                                    ? 0.225f * (sample * -sample - sample) + sample
                                    : 0.225f * (sample * sample - sample) + sample);

                                // Then whistle (essentially an overtone with frequencyx20 and amplitude0.25
                                pos = tempPhase * 20f % periodTemp / periodTemp;
                                pos = pos > 0.5f ? (pos - 1.0f) * 6.28318531f : pos * 6.28318531f;

                                sample2 = pos < 0
                                    ? 1.27323954f * pos + .405284735f * pos * pos
                                    : 1.27323954f * pos - 0.405284735f * pos * pos;

                                sample += 0.25f * (sample2 < 0
                                    ? .225f * (sample2 * -sample2 - sample2) + sample2
                                    : .225f * (sample2 * sample2 - sample2) + sample2);

                                break;
                            case TinySynthWaveType.Breaker:
                                // Breaker
                                amp = tempPhase / periodTemp;
                                sample = Math.Abs(1f - amp * amp * 2f) - 1f;

                                break;
                        }

                        sampleTotal += overtoneStrength * sample;
                        overtoneStrength *= 1f - overtoneFalloff;
                    }

                    sample = sampleTotal;

                    // Applies the low and high pass filters
                    if (filters)
                    {
                        lpFilterOldPos = lpFilterPos;
                        lpFilterCutoff *= lpFilterDeltaCutoff;

                        if (lpFilterCutoff < 0.0)
                        {
                            lpFilterCutoff = 0.0f;
                        }
                        else if (lpFilterCutoff > 0.1)
                        {
                            lpFilterCutoff = 0.1f;
                        }

                        if (lpFilterOn)
                        {
                            lpFilterDeltaPos += (sample - lpFilterPos) * lpFilterCutoff;
                            lpFilterDeltaPos *= lpFilterDamping;
                        }
                        else
                        {
                            lpFilterPos = sample;
                            lpFilterDeltaPos = 0.0f;
                        }

                        lpFilterPos += lpFilterDeltaPos;

                        hpFilterPos += lpFilterPos - lpFilterOldPos;
                        hpFilterPos *= 1.0f - hpFilterCutoff;
                        sample = hpFilterPos;
                    }

                    // Applies the phaser effect
                    if (phaser)
                    {
                        phaserBuffer[phaserPos & 1023] = sample;
                        sample += phaserBuffer[(phaserPos - phaserInt + 1024) & 1023];
                        phaserPos = (phaserPos + 1) & 1023;
                    }

                    superSample += sample;
                }

                // Averages out the super samples and applies volumes
                superSample = masterVolume * envelopeVolume * superSample * 0.125f;

                // Bit crush
                bitcrushPhase += bitcrushFreq;

                if (bitcrushPhase > 1f)
                {
                    bitcrushPhase = 0;
                    bitcrushLast = superSample;
                }

                bitcrushFreq = Mathf.Max(Mathf.Min(bitcrushFreq + bitcrushFreqSweep, 1f), 0f);

                superSample = bitcrushLast;

                // Compressor
                if (superSample > 0f)
                {
                    superSample = Mathf.Pow(superSample, compressionFactor);
                }
                else
                {
                    superSample = -Mathf.Pow(-superSample, compressionFactor);
                }

                // Clipping if too loud
                if (superSample < -1f)
                {
                    superSample = -1f;
                }
                else if (superSample > 1f)
                {
                    superSample = 1f;
                }

                // Writes value to list, ignoring left/right sound channels (this is applied when filtering the audio later)
                buffer[i + bufferPos] = superSample;
            }

            return false;
        }


        /// <summary>
        ///     Writes a short (Int16) to a byte array.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        private static void writeShortToBytes(byte[] bytes, ref int position, short newShort, Endian endian)
        {
            writeBytes(bytes, ref position, new byte[2] {(byte) ((newShort >> 8) & 0xff), (byte) (newShort & 0xff)}, endian);
        }


        /// <summary>
        ///     Writes a uint (UInt32) to a byte array.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        private static void writeUintToBytes(byte[] bytes, ref int position, uint newUint, Endian endian)
        {
            writeBytes(bytes, ref position, new[] {(byte) ((newUint >> 24) & 0xff), (byte) ((newUint >> 16) & 0xff), (byte) ((newUint >> 8) & 0xff), (byte) (newUint & 0xff)}, endian);
        }


        /// <summary>
        ///     Writes any number of bytes into a byte array, at a given position.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        private static void writeBytes(byte[] bytes, ref int position, byte[] newBytes, Endian endian)
        {
            // Writes newBytes to bytes at position position, increasing the position depending on the length of newBytes
            for (var i = 0; i < newBytes.Length; i++)
            {
                bytes[position] = newBytes[endian == Endian.BigEndian ? i : newBytes.Length - i - 1];
                position++;
            }
        }


        private enum Endian
        {
            BigEndian,
            LittleEndian
        }
    }


    /// <summary>
    ///     From BFXR
    ///     Class taken from http: //www.firstpr.com.au/dsp/pink-noise/#Filtering
    /// </summary>
    internal class PinkNumber
    {
        // Properties
        private readonly int maxKey;
        private int key;
        private readonly uint[] whiteValues;
        private readonly Random randomGenerator;

        // Temp
        private readonly float rangeBy5;
        private int last_key;
        private uint sum;
        private int diff;
        private int i;


        public PinkNumber()
        {
            maxKey = 0x1f; // Five bits set
            const uint range = 128;
            rangeBy5 = range / 5f;
            key = 0;
            whiteValues = new uint[5];
            randomGenerator = new Random();

            for (i = 0; i < 5; i++)
            {
                whiteValues[i] = (uint) (randomGenerator.NextDouble() % 1 * rangeBy5);
            }
        }


        public float getNextValue()
        {
            // Returns a number between -1 and 1
            last_key = key;
            sum = 0;

            key++;

            if (key > maxKey)
            {
                key = 0;
            }

            // Exclusive-Or previous value with current value. This gives
            // a list of bits that have changed.
            diff = last_key ^ key;
            sum = 0;

            for (i = 0; i < 5; i++)
            {
                // If bit changed get new random number for corresponding
                // white_value
                if ((diff & (1 << i)) > 0)
                {
                    whiteValues[i] = (uint) (randomGenerator.NextDouble() % 1 * rangeBy5);
                }

                ;
                sum += whiteValues[i];
            }

            return sum / 64f - 1f;
        }
    }
}