//-----------------------------------------------------------------------
// <summary>
//   SfxrEditor implements a Unity window to generate sounds with usfxr
//   using a more friendly GUI.
// </summary>
// <copyright file="SfxrGenerator.cs">
//   Copyright 2013 Tiaan Geldenhuys, 2014 Zeh Fernando
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-----------------------------------------------------------------------


/// <summary>
/// Implements a Unity window to generate sounds and their parameters with usfxr.
/// </summary>
/// <remarks>
/// Open the generator from the Window menu. You can then create a sound and
/// when you are ready, copy the equivalent parameters to the clipboard to be
/// used inside your game.
/// </remarks>
/*public partial class SfxrGenerator : EditorWindow {
    // Properties
    Vector2 scrollPosition;		// Position of the scroll window
    Vector2 scrollPositionRoot;
    SfxrParams soundParameters;

    string suggestedName;

    SfxrRenderer renderer;

    // ================================================================================================================
    // PUBLIC INTERFACE -----------------------------------------------------------------------------------------------

    [MenuItem("SOSXR/usfxr")]
    public static SfxrGenerator Initialize() {
        var window = CreateInstance<SfxrGenerator>();
        window.titleContent = new GUIContent("Sound Effects");
        window.Show();

        return window;
    }

    public void SetTarget(SfxrParams param) {
        soundParameters = param;
    }

    protected virtual void OnGUI() {
        // Initializations
        if (renderer == null) renderer = new SfxrRenderer();

        bool soundChanged;

        // Begin UI
        scrollPositionRoot = GUILayout.BeginScrollView(scrollPositionRoot);
        GUILayout.BeginHorizontal();

        // Left column (generator buttons, copy & paste)
        soundChanged = RenderLeftColumn(soundParameters);

        // Main settings column
        soundChanged = RenderSettingsColumn(soundParameters) || soundChanged;

        // Ends the UI
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();

        // Play sound if necessary
        if (!soundChanged) return;
        renderer.parameters.SetSettingsString(soundParameters.GetSettingsString());
        // PlaySound();
    }

    bool RenderLeftColumn(SfxrParams parameters) {
        var soundChanged = false;

        // Begin generator column
        GUILayout.BeginVertical("box", GUILayout.Width(110));
        GUILayout.Label("GENERATOR", EditorStyles.boldLabel);
        GUILayout.Space(8);


        if (GUILayout.Button("MUTATE")) {
            parameters.Mutate();
            soundChanged = true;
        }
        if (GUILayout.Button("RANDOMIZE")) {
            suggestedName = "Random";
            parameters.Randomize();
            soundChanged = true;
        }

        GUILayout.Space(30);

        if (GUILayout.Button("COPY (OLD)")) {
            EditorGUIUtility.systemCopyBuffer = parameters.GetSettingsStringLegacy();
        }
        if (GUILayout.Button("COPY")) {
            EditorGUIUtility.systemCopyBuffer = parameters.GetSettingsString();
        }
        if (GUILayout.Button("PASTE")) {
            suggestedName = null;
            parameters.SetSettingsString(EditorGUIUtility.systemCopyBuffer);
            soundChanged = true;
        }

        GUILayout.Space(30);

        if (GUILayout.Button("PLAY SOUND")) {
            // PlaySound();
        }

        GUILayout.Space(30);

        if (GUILayout.Button("EXPORT WAV")) {
            var path = EditorUtility.SaveFilePanel("Export as WAV", "", getSuggestedName() + ".wav", "wav");
            if (path.Length != 0) {
                var synth = new SfxrRenderer();
                synth.parameters.SetSettingsString(parameters.GetSettingsString());
                File.WriteAllBytes(path, synth.GetWavFile());
            }
        }

        // End generator column
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        return soundChanged;
    }

    bool RenderSettingsColumn(SfxrParams parameters) {
        var soundChanged = false;

        // Begin manual settings column
        GUILayout.BeginVertical("box");
        GUILayout.Label("MANUAL SETTINGS", EditorStyles.boldLabel);
        GUILayout.Space(8);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        soundChanged = RenderParameters(soundParameters) || soundChanged;
        GUILayout.EndScrollView();

        // End manual settings column
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        return soundChanged;
    }

    /// <summary>
    /// Renders the specified SFXR parameters in the editor.
    /// </summary>
    /// <param name="p">The current parameters to be rendered.</param>
    /// <remarks>
    /// This method is called automatically for the standalone editor window
    /// when a game-object with parameters is selected.  However, this public
    /// method can also be called by CustomEditor implementations for specific
    /// game-components to render the editor in the Inspector window
    /// (see UnityEditor.Editor for details).  Also, this method can be used
    /// from PropertyDrawer implementations; future releases of the code may
    /// include such a default drawer (once SfxrSynth and SfxrParams supports
    /// native serialization for Unity).
    /// </remarks>
    static bool RenderParameters(SfxrParams p) {
        var soundChanged = false;

        var waveTypeStyle = EditorStyles.popup;
        waveTypeStyle.fontSize = 12;
        waveTypeStyle.fixedHeight = 22;

        EditorGUI.BeginChangeCheck();
        try {
            p.waveType = (WaveType)EditorGUILayout.EnumPopup(new GUIContent("Wave Type", "Shape of the wave"), p.waveType, waveTypeStyle);
            GUILayout.Space(12);

            //RenderPopup(waveTypeOptions, ((int)(parameters.waveType)), (value => parameters.waveType = ((uint)(value))), new GUIContent("Wave Type", "Shape of the wave"));
            var isSquareWaveType = p.waveType == 0;
            RenderSlider(+0, +1, p.masterVolume, value => p.masterVolume = value, new GUIContent("Volume", "Overall volume of the sound (0 to 1)"));

            RenderHeading("Wave Envelope");
            RenderSlider(+0, +1, p.attackTime, value => p.attackTime = value, new GUIContent("Attack Time", "Length of the volume envelope attack (0 to 1)"));
            RenderSlider(+0, +1, p.sustainTime, value => p.sustainTime = value, new GUIContent("Sustain Time", "Length of the volume envelope sustain (0 to 1)"));
            RenderSlider(+0, +1, p.sustainPunch, value => p.sustainPunch = value, new GUIContent("Sustain Punch", "Tilts the sustain envelope for more 'pop' (0 to 1)"));
            RenderSlider(+0, +1, p.decayTime, value => p.decayTime = value, new GUIContent("Decay Time", "Length of the volume envelope decay (yes, I know it's called release) (0 to 1)"));

            // BFXR
            RenderSlider(+0, +1, p.compressionAmount, value => p.compressionAmount = value, new GUIContent("Compression", "Pushes amplitudes together into a narrower range to make them stand out more. Very good for sound effects, where you want them to stick out against background music (0 to 1)"));

            RenderHeading("Frequency");
            RenderSlider(+0, +1, p.startFrequency, value => p.startFrequency = value, new GUIContent("Start Frequency", "Base note of the sound (0 to 1)"));
            RenderSlider(+0, +1, p.minFrequency, value => p.minFrequency = value, new GUIContent("Minimum Frequency", "If sliding, the sound will stop at this frequency, to prevent really low notes (0 to 1)"));
            RenderSlider(-1, +1, p.slide, value => p.slide = value, new GUIContent("Slide", "Slides the note up or down (-1 to 1)"));
            RenderSlider(-1, +1, p.deltaSlide, value => p.deltaSlide = value, new GUIContent("Delta Slide", "Accelerates the slide (-1 to 1)"));
            RenderSlider(+0, +1, p.vibratoDepth, value => p.vibratoDepth = value, new GUIContent("Vibrato Depth", "Strength of the vibrato effect (0 to 1)"));
            RenderSlider(+0, +1, p.vibratoSpeed, value => p.vibratoSpeed = value, new GUIContent("Vibrato Speed", "Speed of the vibrato effect (i.e. frequency) (0 to 1)"));

            // BFXR
            RenderSlider(+0, +1, p.overtones, value => p.overtones = value, new GUIContent("Harmonics", "Overlays copies of the waveform with copies and multiples of its frequency. Good for bulking out or otherwise enriching the texture of the sounds (warning: this is the number 1 cause of usfxr slowdown!) (0 to 1)"));
            RenderSlider(+0, +1, p.overtoneFalloff, value => p.overtoneFalloff = value, new GUIContent("Harmonics falloff", "The rate at which higher overtones should decay (0 to 1)"));

            RenderHeading("Tone Change/Pitch Jump");
            // BFXR
            RenderSlider(+0, +1, p.changeRepeat, value => p.changeRepeat = value, new GUIContent("Change Repeat Speed", "Larger Values means more pitch jumps, which can be useful for arpeggiation (0 to 1)"));

            RenderSlider(-1, +1, p.changeAmount, value => p.changeAmount = value, new GUIContent("Change Amount 1", "Shift in note, either up or down (-1 to 1)"));
            RenderSlider(+0, +1, p.changeSpeed, value => p.changeSpeed = value, new GUIContent("Change Speed 1", "How fast the note shift happens (only happens once) (0 to 1)"));

            // BFXR
            RenderSlider(-1, +1, p.changeAmount2, value => p.changeAmount2 = value, new GUIContent("Change Amount 2", "Shift in note, either up or down (-1 to 1)"));
            RenderSlider(+0, +1, p.changeSpeed2, value => p.changeSpeed2 = value, new GUIContent("Change Speed 2", "How fast the note shift happens (only happens once) (0 to 1)"));

            RenderHeading("Square Waves");
            RenderSlider(+0, +1, p.squareDuty, value => p.squareDuty = value, new GUIContent("Square Duty", "Controls the ratio between the up and down states of the square wave, changing the tibre (0 to 1)"), isSquareWaveType);
            RenderSlider(-1, +1, p.dutySweep, value => p.dutySweep = value, new GUIContent("Duty Sweep", "Sweeps the duty up or down (-1 to 1)"), isSquareWaveType);

            RenderHeading("Repeats");
            RenderSlider(+0, +1, p.repeatSpeed, value => p.repeatSpeed = value, new GUIContent("Repeat Speed", "Speed of the note repeating - certain variables are reset each time (0 to 1)"));

            RenderHeading("Phaser");
            RenderSlider(-1, +1, p.phaserOffset, value => p.phaserOffset = value, new GUIContent("Phaser Offset", "Offsets a second copy of the wave by a small phase, changing the tibre (-1 to 1)"));
            RenderSlider(-1, +1, p.phaserSweep, value => p.phaserSweep = value, new GUIContent("Phaser Sweep", "Sweeps the phase up or down (-1 to 1)"));

            RenderHeading("Filters");
            RenderSlider(+0, +1, p.lpFilterCutoff, value => p.lpFilterCutoff = value, new GUIContent("Low-Pass Cutoff", "Frequency at which the low-pass filter starts attenuating higher frequencies (0 to 1)"));
            RenderSlider(-1, +1, p.lpFilterCutoffSweep, value => p.lpFilterCutoffSweep = value, new GUIContent("Low-Pass Cutoff Sweep", "Sweeps the low-pass cutoff up or down (-1 to 1)"));
            RenderSlider(+0, +1, p.lpFilterResonance, value => p.lpFilterResonance = value, new GUIContent("Low-Pass Resonance", "Changes the attenuation rate for the low-pass filter, changing the timbre (0 to 1)"));
            RenderSlider(+0, +1, p.hpFilterCutoff, value => p.hpFilterCutoff = value, new GUIContent("High-Pass Cutoff", "Frequency at which the high-pass filter starts attenuating lower frequencies (0 to 1)"));
            RenderSlider(-1, +1, p.hpFilterCutoffSweep, value => p.hpFilterCutoffSweep = value, new GUIContent("High-Pass Cutoff Sweep", "Sweeps the high-pass cutoff up or down (-1 to 1)"));

            RenderHeading("Bit Crushing");

            // BFXR
            RenderSlider(+0, +1, p.bitCrush, value => p.bitCrush = value, new GUIContent("Bit Crush", "Resamples the audio at a lower frequency (0 to 1)"));
            RenderSlider(-1, +1, p.bitCrushSweep, value => p.bitCrushSweep = value, new GUIContent("Bit Crush Sweep", "Sweeps the Bit Crush filter up or down (-1 to 1)"));
        } finally {
            if (EditorGUI.EndChangeCheck()) {
                soundChanged = true;
            }
        }

        return soundChanged;
    }

    static void RenderHeading(string heading) {
        EditorGUILayout.LabelField(heading, EditorStyles.boldLabel);
    }

    static void RenderSlider(float minValue, float maxValue, float value, Action<float> valueChangeAction = null,
                             GUIContent label = null, bool? isEnabled = null) {
        if (label == null) 			label = GUIContent.none;
        RenderGenericEditor(ref value,
            () => EditorGUILayout.Slider(label, value, minValue, maxValue),
            valueChangeAction,
            isEnabled);
    }

    static bool RenderGenericEditor<T>(
        ref T value,
        Func<T> valueEditFunction,
        Action<T> valueChangeAction = null,
        bool? isEnabled = null) {
        bool isChanged;
        if (valueEditFunction == null) {
            isChanged = false;
        }
        else {
            bool? wasEnabled;
            if (isEnabled.HasValue) {
                wasEnabled  = GUI.enabled;
                GUI.enabled = isEnabled.Value;
            }
            else {
                wasEnabled = null;
            }

            try {
                EditorGUI.BeginChangeCheck();
                try {
                    value = valueEditFunction();
                }
                finally {
                    isChanged = EditorGUI.EndChangeCheck();
                }

                if (isChanged
                    && (valueChangeAction != null)) {
                    valueChangeAction(value);
                }
            }
            finally {
                if (wasEnabled.HasValue) {
                    GUI.enabled = wasEnabled.Value;
                }
            }
        }

        return isChanged;
    }

    static bool RenderGenericEditor<T>(
        ref T value,
        Func<T> valueEditFunction,
        Action valueChangeAction,
        bool? isEnabled = null)
    {
        Action<T> valueChangeActionWrapped = null;
        if (valueChangeAction != null)
        {
            valueChangeActionWrapped = (dummyValue) => valueChangeAction();
        }

        return RenderGenericEditor(
            ref value, valueEditFunction, valueChangeActionWrapped, isEnabled);
    }

    string getSuggestedName() {
        return !string.IsNullOrEmpty(suggestedName) ? suggestedName : "Audio";
    }
}
*/

