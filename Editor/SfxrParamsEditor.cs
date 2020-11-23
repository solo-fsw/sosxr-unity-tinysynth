using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace usfxr {
	[CustomPropertyDrawer(typeof(SfxrParams))]
	public class SfxrParamsEditor : PropertyDrawer {
		
		const int   RangeScale             = 100;
		const float ButtonWidth            = 20;
		const float ButtonMargin           = 5;
		const float RangeScaleToNormalized = 1f / RangeScale;

		struct ParamData {
			public int    min;
			public int    max;
			public int    @default;
			public string tooltip;
		}
		
		bool                                 expand;
		bool                                 expandPresets;
		static FieldInfo[]                   paramFields;
		static Dictionary<string, ParamData> paramData;
		float                                height;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			UpdateReflection();
			
			var startY = position.y;
			position.height = EditorGUIUtility.singleLineHeight;
			
			expand = EditorGUI.BeginFoldoutHeaderGroup(position, expand, property.name, null, rect => ShowHeaderContextMenu(rect, property));
			if (expand) OnExpandedGUI(ref position, property);
			EditorGUI.EndFoldoutHeaderGroup();
			height = position.y + position.height - startY;
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return height;
		}

		void OnExpandedGUI(ref Rect position, SerializedProperty property) {
			EditorGUI.BeginChangeCheck();
			
			position.y    += EditorGUIUtility.singleLineHeight + 2;
			expandPresets =  EditorGUI.Foldout(position, expandPresets, "Presets");
			if (expandPresets) {
				position.y += EditorGUIUtility.singleLineHeight + 2;
				OnPresetGUI(position, property);
				position.y += EditorGUIUtility.singleLineHeight + 2;
			}
			
			foreach (var prop in GetVisibleChildren(property)) {
				position.y += EditorGUIUtility.singleLineHeight + 2;
				
				if (prop.type == "Enum") {
					WidgetWaveType(position, prop);
				} else if (prop.type == "float") {
					WidgetSlider(position, prop);
				}

			}
			
			if (!EditorGUI.EndChangeCheck()) return;
			property.serializedObject.ApplyModifiedProperties();
		
			PlayPreview(property);
		}

		static void WidgetSlider(Rect position, SerializedProperty property) {
			if (!paramData.TryGetValue(property.name, out var data)) return;
			
			var label = new GUIContent(property.displayName, data.tooltip);

			var sliderPosition = new Rect(position);
			sliderPosition.width -= ButtonWidth + ButtonMargin;
			
			property.floatValue = EditorGUI.IntSlider(
				sliderPosition, 
				label,
				Mathf.RoundToInt(property.floatValue * RangeScale),
				data.min,
				data.max) * RangeScaleToNormalized;

			var buttonPosition = new Rect(position) { x = position.x + position.width - ButtonWidth, width = ButtonWidth };

			if (GUI.Button(buttonPosition, new GUIContent("R", "Reset this parameter to its default value"))) {
				property.floatValue = data.@default * RangeScaleToNormalized;
			}
		}

		static void WidgetWaveType(Rect position, SerializedProperty property) {
			EditorGUI.PropertyField(position, property);
		}

		static void OnPresetGUI(Rect position, SerializedProperty property) {
			var rect = new Rect(position) {
				width = (position.width + ButtonMargin) / 7 - ButtonMargin,
				height = EditorGUIUtility.singleLineHeight * 2,
			};

			if (PresetButton(ref rect,"LASER\nSHOOT")) SetParam(property, SfxrPreset.LaserShoot());
			if (PresetButton(ref rect,"PICKUP\nCOIN")) SetParam(property, SfxrPreset.PickupCoin());
			if (PresetButton(ref rect,"EXPLOSION")) SetParam(property, SfxrPreset.Explosion());
			if (PresetButton(ref rect,"POWER UP")) SetParam(property, SfxrPreset.PowerUp());
			if (PresetButton(ref rect,"HIT\nHURT")) SetParam(property, SfxrPreset.HitHurt());
			if (PresetButton(ref rect,"JUMP")) SetParam(property, SfxrPreset.Jump());
			if (PresetButton(ref rect,"BLIP\nSELECT")) SetParam(property, SfxrPreset.BlipSelect());
		}

		static bool PresetButton(ref Rect position, string label) {
			var pressed = GUI.Button(position, label);
			position.x += position.width + ButtonMargin;
			return pressed;
		}

		static void ShowHeaderContextMenu(Rect position, SerializedProperty property) {
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("Export WAV"), false, () => OnExportWav(property));
			menu.DropDown(position);
		}

		static void OnExportWav(SerializedProperty property) {
			var path = EditorUtility.SaveFilePanel("Export as WAV", "", property.name + ".wav", "wav");
			if (path.Length == 0) return;
			var synth = new SfxrRenderer { param = PropertyToParams(property) };
			File.WriteAllBytes(path, synth.GetWavFile());
		}

		/// <summary>
		/// Gets visible children of `SerializedProperty` at 1 level depth.
		/// </summary>
		/// <param name="serializedProperty">Parent `SerializedProperty`.</param>
		/// <returns>Collection of `SerializedProperty` children.</returns>
		static IEnumerable<SerializedProperty> GetVisibleChildren(SerializedProperty serializedProperty) {
			var currentProperty     = serializedProperty.Copy();
			var nextSiblingProperty = serializedProperty.Copy();
			nextSiblingProperty.NextVisible(false);
			if (!currentProperty.NextVisible(true)) yield break;
			
			do {
				if (SerializedProperty.EqualContents(currentProperty, nextSiblingProperty))
					break;

				yield return currentProperty;
			} while (currentProperty.NextVisible(false));
		}

		/// <summary>
		/// Iterates over all the fields in a SfxrParams instance and applies these to a serialized property
		/// This is needed to make the code that directly modifies SfxrParams play nice with the Unity undo system
		/// </summary>
		static void SetParam(SerializedProperty property, SfxrParams param) {
			// iterate over all the fields
			foreach (var field in paramFields) {
				// find the corresponding property
				var prop = property.FindPropertyRelative(field.Name);
				
				// apply the value from the SfxrParams struct to the SerializedProperty
				// only enums and floats for now, add more as needed
				if (prop.type == "Enum") {
					prop.enumValueIndex = Convert.ToInt32(field.GetValue(param));
				} else if (prop.type == "float") {
					prop.floatValue = (float) field.GetValue(param);
				}
			}
		}

		static void UpdateReflection() {
			if (paramFields != null && paramFields.Length > 0) return;
			
			// cache the fields on SfxrParams, these won't change 
			paramFields = typeof(SfxrParams).GetFields(BindingFlags.Public | BindingFlags.Instance);
			
			// now we build a little lookup table with the range and default value attributes
			paramData = new Dictionary<string, ParamData>();
			foreach (var field in paramFields) {
				var data = new ParamData { @default = 0, min = 0, max = 1 };

				if (field.GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute rangeAttribute) {
					data.max = Mathf.RoundToInt(rangeAttribute.max * RangeScale);
					data.min = Mathf.RoundToInt(rangeAttribute.min * RangeScale);
				} 
				
				if (field.GetCustomAttribute(typeof(TooltipAttribute)) is TooltipAttribute tooltipAttribute) {
					data.tooltip = tooltipAttribute.tooltip;
				}

				if (field.GetCustomAttribute(typeof(SfxrDefault)) is SfxrDefault sfxDefault) {
					data.@default = Mathf.RoundToInt(sfxDefault.value * RangeScale);
				}
				
				paramData.Add(field.Name, data);
			}
		}

		static void PlayPreview(SerializedProperty property) {
			SfxrPlayer.Play(PropertyToParams(property), true);
		}

		static SfxrParams PropertyToParams(SerializedProperty property) {
			var target = property.serializedObject.targetObject;
			var type   = target.GetType();
			var field  = type.GetField(property.name);
			return (SfxrParams) field.GetValue(target);
		}
	}
}