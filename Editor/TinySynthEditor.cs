using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace SOSXR.TinySynth
{
    /// <summary>
    ///     Custom property drawer for <see cref="TinySynthSound" />. Renders a waveform preview, preset buttons,
    ///     per-parameter sliders with reset and lock controls, and an export-to-WAV option in the context menu.
    /// </summary>
    [CustomPropertyDrawer(typeof(TinySynthSound))]
    public class TinySynthEditor : PropertyDrawer
    {
        private bool _expand = true;
        private bool _expandPresets;
        private float _height;

        private const int RangeScale = 100;
        private const float ButtonWidth = 20;
        private const float ButtonMargin = 5;
        private const float RangeScaleToNormalized = 1f / RangeScale;
        private const float Margin = 2;
        private const int MaxPreviewCacheSize = 16;

        private static FieldInfo[] _paramFields;
        private static Dictionary<string, ParamData> _paramData;

        private static readonly GUIStyle LockButtonStyle = "IN LockButton";
        private static readonly Color CurveColor = new(1.0f, 140.0f / 255.0f, 0.0f, 1.0f);

        /// <summary>
        ///     Preview samples per sound, so a repaint does not allocate and copy the whole clip.
        ///     Keyed by value: any parameter change is a new key. Cleared when it grows past <see cref="MaxPreviewCacheSize" />.
        /// </summary>
        private readonly Dictionary<TinySynthSound, float[]> _previewSamples = new();


        /// <summary>
        ///     Draws the full <see cref="TinySynthSound" /> property in the Inspector, including the waveform
        ///     preview, preset row, and all parameter sliders.
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UpdateReflection();

            var startY = position.y;
            position.height = EditorGUIUtility.singleLineHeight;

            _expand = EditorGUI.BeginFoldoutHeaderGroup(position, _expand, property.name, null, rect => ShowHeaderContextMenu(rect, property));

            if (_expand)
            {
                OnExpandedGUI(ref position, property);
            }

            EditorGUI.EndFoldoutHeaderGroup();
            _height = position.y + position.height - startY;
        }


        /// <summary>
        ///     Returns the total pixel height required by this property drawer so the Inspector can reserve the correct amount of space.
        /// </summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _height;
        }


        private void OnExpandedGUI(ref Rect position, SerializedProperty property)
        {
            position.y += EditorGUIUtility.singleLineHeight + Margin;
            var previewRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight * 5);

            // stick a button behind the preview so we can click it to play
            if (GUI.Button(previewRect, "Play"))
            {
                PlayPreview(property);
            }

            // don't render the preview if it's tiny
            if (previewRect.width > 1)
            {
                DoRenderPreview(GetPreviewSamples(PropertyToParams(property)), previewRect);
            }

            position.y += previewRect.height + Margin;

            _expandPresets = EditorGUI.Foldout(position, _expandPresets, "Presets");
            // the change check needs to go after the foldout, if not it'll trigger a preview on open/close
            EditorGUI.BeginChangeCheck();

            if (_expandPresets)
            {
                position.y += EditorGUIUtility.singleLineHeight + Margin;
                OnPresetGUI(position, property);
                position.y += EditorGUIUtility.singleLineHeight + Margin;
            }

            foreach (var prop in GetVisibleChildren(property))
            {
                position.y += EditorGUIUtility.singleLineHeight + Margin;

                if (prop.type == "Enum")
                {
                    WidgetWaveType(position, prop);
                }
                else if (prop.type == "float")
                {
                    WidgetSlider(position, prop);
                }
            }

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            property.serializedObject.ApplyModifiedProperties();

            PlayPreview(property);
        }


        private static void WidgetSlider(Rect position, SerializedProperty property)
        {
            if (!_paramData.TryGetValue(property.name, out var data))
            {
                return;
            }

            var label = new GUIContent(property.displayName, data.Tooltip);

            var sliderPosition = new Rect(position);
            sliderPosition.width -= (ButtonWidth + ButtonMargin) * 2;

            property.floatValue = EditorGUI.IntSlider(
                sliderPosition,
                label,
                Mathf.RoundToInt(property.floatValue * RangeScale),
                data.Min,
                data.Max) * RangeScaleToNormalized;

            ExtraButtons(position, property);
        }


        private static void ExtraButtons(Rect position, SerializedProperty property)
        {
            if (!_paramData.TryGetValue(property.name, out var data))
            {
                return;
            }

            var buttonPosition = new Rect(position) {x = position.x + position.width - ButtonWidth * 2 - ButtonMargin, width = ButtonWidth};

            if (GUI.Button(buttonPosition, new GUIContent("R", "Reset this parameter to its default value")))
            {
                if (property.type == "Enum")
                {
                    property.enumValueIndex = Mathf.FloorToInt(data.Default * RangeScaleToNormalized);
                }
                else if (property.type == "float")
                {
                    property.floatValue = data.Default * RangeScaleToNormalized;
                }
            }

            buttonPosition.x = position.x + position.width - ButtonWidth;
            data.Locked = GUI.Toggle(buttonPosition, data.Locked, new GUIContent("", "Lock this parameter from changes via templates/mutation"), LockButtonStyle);
        }


        private static void WidgetWaveType(Rect position, SerializedProperty property)
        {
            var wavePickerPos = new Rect(position);
            wavePickerPos.width -= (ButtonWidth + ButtonMargin) * 2;
            EditorGUI.PropertyField(wavePickerPos, property);
            ExtraButtons(position, property);
        }


        private static void OnPresetGUI(Rect position, SerializedProperty property)
        {
            var rect = new Rect(position)
            {
                width = (position.width + ButtonMargin) / 7 - ButtonMargin,
                height = EditorGUIUtility.singleLineHeight * 2
            };

            if (PresetButton(ref rect, "LASER\nSHOOT"))
            {
                SetParam(property, TinySynthPreset.LaserShoot());
            }

            if (PresetButton(ref rect, "PICKUP\nCOIN"))
            {
                SetParam(property, TinySynthPreset.PickupCoin());
            }

            if (PresetButton(ref rect, "EXPLOSION"))
            {
                SetParam(property, TinySynthPreset.Explosion());
            }

            if (PresetButton(ref rect, "POWER UP"))
            {
                SetParam(property, TinySynthPreset.PowerUp());
            }

            if (PresetButton(ref rect, "HIT\nHURT"))
            {
                SetParam(property, TinySynthPreset.HitHurt());
            }

            if (PresetButton(ref rect, "JUMP"))
            {
                SetParam(property, TinySynthPreset.Jump());
            }

            if (PresetButton(ref rect, "BLIP\nSELECT"))
            {
                SetParam(property, TinySynthPreset.BlipSelect());
            }
        }


        private static bool PresetButton(ref Rect position, string label)
        {
            var pressed = GUI.Button(position, label);
            position.x += position.width + ButtonMargin;

            return pressed;
        }


        private static void ShowHeaderContextMenu(Rect position, SerializedProperty property)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Export WAV"), false, () => OnExportWav(property));
            menu.DropDown(position);
        }


        private static void OnExportWav(SerializedProperty property)
        {
            var path = EditorUtility.SaveFilePanel("Export as WAV", "", property.name + ".wav", "wav");

            if (path.Length == 0)
            {
                return;
            }

            var synth = new TinySynthRenderer {param = PropertyToParams(property)};
            File.WriteAllBytes(path, synth.GetWavFile());
        }


        /// <summary>
        ///     Gets visible children of `SerializedProperty` at 1 level depth.
        /// </summary>
        /// <param name="serializedProperty">Parent `SerializedProperty`.</param>
        /// <returns>Collection of `SerializedProperty` children.</returns>
        private static IEnumerable<SerializedProperty> GetVisibleChildren(SerializedProperty serializedProperty)
        {
            var currentProperty = serializedProperty.Copy();
            var nextSiblingProperty = serializedProperty.Copy();
            nextSiblingProperty.NextVisible(false);

            if (!currentProperty.NextVisible(true))
            {
                yield break;
            }

            do
            {
                if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
                {
                    break;
                }

                yield return currentProperty;
            } while (currentProperty.NextVisible(false));
        }


        /// <summary>
        ///     Iterates over all the fields in a SfxrParams instance and applies these to a serialized property
        ///     This is needed to make the code that directly modifies SfxrParams play nice with the Unity undo system
        /// </summary>
        private static void SetParam(SerializedProperty property, TinySynthSound param)
        {
            // iterate over all the fields
            foreach (var field in _paramFields)
            {
                // find the corresponding property
                var prop = property.FindPropertyRelative(field.Name);

                if (_paramData[field.Name].Locked)
                {
                    continue;
                }

                // apply the value from the SfxrParams struct to the SerializedProperty
                // only enums and floats for now, add more as needed
                if (prop.type == "Enum")
                {
                    prop.enumValueIndex = Convert.ToInt32(field.GetValue(param));
                }
                else if (prop.type == "float")
                {
                    prop.floatValue = (float) field.GetValue(param);
                }
            }
        }


        private static void UpdateReflection()
        {
            if (_paramFields != null && _paramFields.Length > 0)
            {
                return;
            }

            // cache the fields on SfxrParams, these won't change 
            _paramFields = typeof(TinySynthSound).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // now we build a little lookup table with the range and default value attributes
            _paramData = new Dictionary<string, ParamData>();

            foreach (var field in _paramFields)
            {
                var data = new ParamData {Default = 0, Min = 0, Max = 1};

                if (field.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttribute)
                {
                    data.Max = Mathf.RoundToInt(rangeAttribute.max * RangeScale);
                    data.Min = Mathf.RoundToInt(rangeAttribute.min * RangeScale);
                }

                if (field.GetCustomAttribute(typeof(TooltipAttribute)) is TooltipAttribute tooltipAttribute)
                {
                    data.Tooltip = tooltipAttribute.tooltip;
                }

                if (field.GetCustomAttribute(typeof(TinySynthDefaultAttribute)) is TinySynthDefaultAttribute sfxDefault)
                {
                    data.Default = Mathf.RoundToInt(sfxDefault.value * RangeScale);
                }

                _paramData.Add(field.Name, data);
            }
        }


        private static void PlayPreview(SerializedProperty property)
        {
            TinySynthPlayer.Play(PropertyToParams(property), true);
        }


        /// <summary>
        ///     Builds the sound from the property's children (the inverse of <see cref="SetParam" />).
        ///     Reading the SerializedProperty works for array/list elements, nested classes and base-class
        ///     private fields, where looking up <c>property.name</c> on the target type does not.
        /// </summary>
        private static TinySynthSound PropertyToParams(SerializedProperty property)
        {
            if (property?.serializedObject?.targetObject == null)
            {
                return new TinySynthSound();
            }

            UpdateReflection();

            object boxed = new TinySynthSound(); // boxed, so FieldInfo.SetValue writes into this copy

            foreach (var field in _paramFields)
            {
                var prop = property.FindPropertyRelative(field.Name);

                if (prop == null)
                {
                    continue;
                }

                if (field.FieldType.IsEnum)
                {
                    field.SetValue(boxed, Enum.ToObject(field.FieldType, prop.intValue));
                }
                else if (field.FieldType == typeof(float))
                {
                    field.SetValue(boxed, prop.floatValue);
                }
            }

            return (TinySynthSound) boxed;
        }


        /// <summary>Returns the clip samples for <paramref name="sound" />, rendering and copying them only on a cache miss.</summary>
        private float[] GetPreviewSamples(TinySynthSound sound)
        {
            if (_previewSamples.TryGetValue(sound, out var samples))
            {
                return samples;
            }

            if (_previewSamples.Count >= MaxPreviewCacheSize)
            {
                _previewSamples.Clear();
            }

            var clip = TinySynthPlayer.GetClip(sound);
            samples = clip != null && clip.samples > 0 ? new float[clip.samples] : Array.Empty<float>();

            if (samples.Length > 0)
            {
                clip.GetData(samples, 0);
            }

            _previewSamples.Add(sound, samples);

            return samples;
        }


        private static void DoRenderPreview(float[] samples, Rect rect)
        {
            AudioCurveRendering.AudioCurveAndColorEvaluator dlg =
                delegate(float x, out Color color)
                {
                    color = CurveColor;

                    if (samples.Length == 0)
                    {
                        return 0;
                    }

                    var p = Mathf.FloorToInt(Mathf.Clamp(x * (samples.Length - 1), 0.0f, samples.Length - 1));

                    return samples[p];
                };

            rect = AudioCurveRendering.BeginCurveFrame(rect);
            AudioCurveRendering.DrawSymmetricFilledCurve(rect, dlg);
            AudioCurveRendering.EndCurveFrame();
        }


        private class ParamData
        {
            public int Min;
            public int Max;
            public int Default;
            public string Tooltip;
            public bool Locked;
        }
    }
}