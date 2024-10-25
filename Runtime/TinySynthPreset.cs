namespace SOSXR.TinySynth
{
    public static class TinySynthPreset
    {
        /// <summary>
        ///     Sets the parameters to generate a pickup/coin sound
        /// </summary>
        public static TinySynthSound PickupCoin()
        {
            var p = new TinySynthSound();
            p.Reset();
            p.startFrequency = 0.4f + GetRandom() * 0.5f;
            p.sustainTime = GetRandom() * 0.1f;
            p.decayTime = 0.1f + GetRandom() * 0.4f;
            p.sustainPunch = 0.3f + GetRandom() * 0.3f;

            if (!GetRandomBool())
            {
                return p;
            }

            p.changeSpeed = 0.5f + GetRandom() * 0.2f;
            var cnum = (int) (GetRandom() * 7f) + 1;
            var cden = cnum + (int) (GetRandom() * 7f) + 2;
            p.changeAmount = cnum / (float) cden;

            return p;
        }


        /// <summary>
        ///     Sets the parameters to generate a laser/shoot sound
        /// </summary>
        public static TinySynthSound LaserShoot()
        {
            var p = new TinySynthSound();
            p.Reset();

            p.WaveType = (TinySynthWaveType) (uint) (GetRandom() * 3);

            if (p.WaveType == TinySynthWaveType.Sine && GetRandomBool())
            {
                p.WaveType = (TinySynthWaveType) (uint) (GetRandom() * 2f);
            }

            p.startFrequency = 0.5f + GetRandom() * 0.5f;
            p.minFrequency = p.startFrequency - 0.2f - GetRandom() * 0.6f;

            if (p.minFrequency < 0.2f)
            {
                p.minFrequency = 0.2f;
            }

            p.slide = -0.15f - GetRandom() * 0.2f;

            if (GetRandom() < 0.33f)
            {
                p.startFrequency = 0.3f + GetRandom() * 0.6f;
                p.minFrequency = GetRandom() * 0.1f;
                p.slide = -0.35f - GetRandom() * 0.3f;
            }

            if (GetRandomBool())
            {
                p.squareDuty = GetRandom() * 0.5f;
                p.dutySweep = GetRandom() * 0.2f;
            }
            else
            {
                p.squareDuty = 0.4f + GetRandom() * 0.5f;
                p.dutySweep = -GetRandom() * 0.7f;
            }

            p.sustainTime = 0.1f + GetRandom() * 0.2f;
            p.decayTime = GetRandom() * 0.4f;

            if (GetRandomBool())
            {
                p.sustainPunch = GetRandom() * 0.3f;
            }

            if (GetRandom() < 0.33f)
            {
                p.phaserOffset = GetRandom() * 0.2f;
                p.phaserSweep = -GetRandom() * 0.2f;
            }

            if (GetRandomBool())
            {
                p.hpFilterCutoff = GetRandom() * 0.3f;
            }

            return p;
        }


        /// <summary>
        ///     Sets the parameters to generate an explosion sound
        /// </summary>
        public static TinySynthSound Explosion()
        {
            var p = new TinySynthSound();
            p.Reset();

            p.WaveType = TinySynthWaveType.Noise;

            if (GetRandomBool())
            {
                p.startFrequency = 0.1f + GetRandom() * 0.4f;
                p.slide = -0.1f + GetRandom() * 0.4f;
            }
            else
            {
                p.startFrequency = 0.2f + GetRandom() * 0.7f;
                p.slide = -0.2f - GetRandom() * 0.2f;
            }

            p.startFrequency *= p.startFrequency;

            if (GetRandom() < 0.2f)
            {
                p.slide = 0.0f;
            }

            if (GetRandom() < 0.33f)
            {
                p.repeatSpeed = 0.3f + GetRandom() * 0.5f;
            }

            p.sustainTime = 0.1f + GetRandom() * 0.3f;
            p.decayTime = GetRandom() * 0.5f;
            p.sustainPunch = 0.2f + GetRandom() * 0.6f;

            if (GetRandomBool())
            {
                p.phaserOffset = -0.3f + GetRandom() * 0.9f;
                p.phaserSweep = -GetRandom() * 0.3f;
            }

            if (GetRandom() < 0.33f)
            {
                p.changeSpeed = 0.6f + GetRandom() * 0.3f;
                p.changeAmount = 0.8f - GetRandom() * 1.6f;
            }

            return p;
        }


        /// <summary>
        ///     Sets the parameters to generate a powerup sound
        /// </summary>
        public static TinySynthSound PowerUp()
        {
            var p = new TinySynthSound();
            p.Reset();

            if (GetRandomBool())
            {
                p.WaveType = TinySynthWaveType.Sawtooth;
            }
            else
            {
                p.squareDuty = GetRandom() * 0.6f;
            }

            if (GetRandomBool())
            {
                p.startFrequency = 0.2f + GetRandom() * 0.3f;
                p.slide = 0.1f + GetRandom() * 0.4f;
                p.repeatSpeed = 0.4f + GetRandom() * 0.4f;
            }
            else
            {
                p.startFrequency = 0.2f + GetRandom() * 0.3f;
                p.slide = 0.05f + GetRandom() * 0.2f;

                if (GetRandomBool())
                {
                    p.vibratoDepth = GetRandom() * 0.7f;
                    p.vibratoSpeed = GetRandom() * 0.6f;
                }
            }

            p.sustainTime = GetRandom() * 0.4f;
            p.decayTime = 0.1f + GetRandom() * 0.4f;

            return p;
        }


        /// <summary>
        ///     Sets the parameters to generate a hit/hurt sound
        /// </summary>
        public static TinySynthSound HitHurt()
        {
            var p = new TinySynthSound();
            p.Reset();

            p.WaveType = (TinySynthWaveType) (uint) (GetRandom() * 3f);

            if (p.WaveType == TinySynthWaveType.Sine)
            {
                p.WaveType = TinySynthWaveType.Noise;
            }
            else if (p.WaveType == 0)
            {
                p.squareDuty = GetRandom() * 0.6f;
            }

            p.startFrequency = 0.2f + GetRandom() * 0.6f;
            p.slide = -0.3f - GetRandom() * 0.4f;

            p.sustainTime = GetRandom() * 0.1f;
            p.decayTime = 0.1f + GetRandom() * 0.2f;

            if (GetRandomBool())
            {
                p.hpFilterCutoff = GetRandom() * 0.3f;
            }

            return p;
        }


        /// <summary>
        ///     Sets the parameters to generate a jump sound
        /// </summary>
        public static TinySynthSound Jump()
        {
            var p = new TinySynthSound();
            p.Reset();

            p.WaveType = 0;
            p.squareDuty = GetRandom() * 0.6f;
            p.startFrequency = 0.3f + GetRandom() * 0.3f;
            p.slide = 0.1f + GetRandom() * 0.2f;

            p.sustainTime = 0.1f + GetRandom() * 0.3f;
            p.decayTime = 0.1f + GetRandom() * 0.2f;

            if (GetRandomBool())
            {
                p.hpFilterCutoff = GetRandom() * 0.3f;
            }

            if (GetRandomBool())
            {
                p.lpFilterCutoff = 1.0f - GetRandom() * 0.6f;
            }

            return p;
        }


        /// <summary>
        ///     Sets the parameters to generate a blip/select sound
        /// </summary>
        public static TinySynthSound BlipSelect()
        {
            var p = new TinySynthSound();
            p.Reset();

            p.WaveType = (TinySynthWaveType) (uint) (GetRandom() * 2f);

            if (p.WaveType == 0)
            {
                p.squareDuty = GetRandom() * 0.6f;
            }

            p.startFrequency = 0.2f + GetRandom() * 0.4f;

            p.sustainTime = 0.1f + GetRandom() * 0.1f;
            p.decayTime = GetRandom() * 0.2f;
            p.hpFilterCutoff = 0.1f;

            return p;
        }


        private static float GetRandom()
        {
            return TinySynthSound.GetRandom();
        }


        private static bool GetRandomBool()
        {
            return TinySynthSound.GetRandomBool();
        }
    }
}