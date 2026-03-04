namespace SOSXR.TinySynth
{
    /// <summary>
    ///     Defines the wave shape used by the synthesiser
    /// </summary>
    public enum TinySynthWaveType : uint
    {
        /// <summary>Classic square wave; duty cycle controlled by <see cref="TinySynthSound.squareDuty" />.</summary>
        Square = 0,

        /// <summary>Sawtooth wave; bright, buzzy tone that slides from high to low amplitude each cycle.</summary>
        Sawtooth = 1,

        /// <summary>Smooth sinusoidal wave; the purest, softest tone.</summary>
        Sine = 2,

        /// <summary>White noise; randomised amplitude each sample, useful for explosions and impacts.</summary>
        Noise = 3,

        /// <summary>Triangle wave; softer than square, slightly brighter than sine.</summary>
        Triangle = 4,

        /// <summary>Pink noise; random with a 1/f spectral distribution, producing a warmer noise character than white noise.</summary>
        PinkNoise = 5,

        /// <summary>Tangent wave; produces harsh, clipping tones with a distinctive distorted character.</summary>
        Tan = 6,

        /// <summary>Sine wave with a high-frequency overtone layered on top, producing a whistle-like timbre.</summary>
        Whistle = 7,

        /// <summary>Breaker wave; a folded quadratic waveform that adds harmonic richness.</summary>
        Breaker = 8,
    }
}

