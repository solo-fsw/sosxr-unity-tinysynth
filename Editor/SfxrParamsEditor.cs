using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace usfxr {
	[CustomPropertyDrawer(typeof(SfxrParams))]
	public class SfxrParamsEditor : PropertyDrawer {
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

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			UpdateReflection();
			expand = EditorGUILayout.BeginFoldoutHeaderGroup(expand, property.name, null, ShowHeaderContextMenu);
			if (expand) OnExpandedGUI(property);
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			return base.GetPropertyHeight(property, label) - 19;
		}

		void OnExpandedGUI(SerializedProperty property) {
			EditorGUI.BeginChangeCheck();
			
			expandPresets = EditorGUILayout.Foldout(expandPresets, "Presets");
			if (expandPresets) OnPresetGUI(property);
			
			foreach (var prop in GetVisibleChildren(property)) {
				if (prop.type == "Enum") {
					WidgetWaveType(prop);
				} else if (prop.type == "float") {
					WidgetSlider(prop);
				}
			}
			
			if (!EditorGUI.EndChangeCheck()) return;
			property.serializedObject.ApplyModifiedProperties();
		
			PlayPreview(property);
		}

		static void WidgetSlider(SerializedProperty property) {
			if (!paramData.TryGetValue(property.name, out var data)) return;
			
			var label = new GUIContent(property.displayName, data.tooltip);

			EditorGUILayout.BeginHorizontal();
			property.floatValue = EditorGUILayout.IntSlider(label,
				Mathf.RoundToInt(property.floatValue * 100f),
				data.min,
				data.max) * .01f;

			if (GUILayout.Button(new GUIContent("R", "Reset this parameter to its default value"),
				GUILayout.Width(20))) {
				property.floatValue = data.@default;
			}
			
			EditorGUILayout.EndHorizontal();
		}

		static void WidgetWaveType(SerializedProperty property) {
			EditorGUILayout.PropertyField(property);
		}

		static void OnPresetGUI(SerializedProperty property) {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("LASER/SHOOT")) SetParam(property, SfxrPreset.LaserShoot());
			if (GUILayout.Button("PICKUP/COIN")) SetParam(property, SfxrPreset.PickupCoin());
			if (GUILayout.Button("EXPLOSION")) SetParam(property, SfxrPreset.Explosion());
			if (GUILayout.Button("POWER UP")) SetParam(property, SfxrPreset.PowerUp());
			if (GUILayout.Button("HIT/HURT")) SetParam(property, SfxrPreset.HitHurt());
			if (GUILayout.Button("JUMP")) SetParam(property, SfxrPreset.Jump());
			if (GUILayout.Button("BLIP/SELECT")) SetParam(property, SfxrPreset.BlipSelect());
			EditorGUILayout.EndHorizontal();
		}

		static void ShowHeaderContextMenu(Rect position) {
			var menu = new GenericMenu();
			menu.AddItem(new GUIContent("This is a placeholder menu for export and import things"), false, OnItemClicked);
			menu.DropDown(position);
		}

		static void OnItemClicked() {
			Debug.Log("Sorry, it doesn't work yet :(");
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
					data.max = Mathf.RoundToInt(rangeAttribute.max * 100);
					data.min = Mathf.RoundToInt(rangeAttribute.min * 100);
				} 
				
				if (field.GetCustomAttribute(typeof(TooltipAttribute)) is TooltipAttribute tooltipAttribute) {
					data.tooltip = tooltipAttribute.tooltip;
				} 
				
				var sfxDefault = field.GetCustomAttribute(typeof(SfxrDefault)) as SfxrDefault;
				if (sfxDefault != null) {
					data.@default = Mathf.RoundToInt(sfxDefault.value * 100);
				}
				
				paramData.Add(field.Name, data);
			}
		}

		static void PlayPreview(SerializedProperty property) {
			var target = property.serializedObject.targetObject;
			var type   = target.GetType();
			var field  = type.GetField(property.name);
			SfxrPlayer.Play((SfxrParams) field.GetValue(target), true);
		}
	}
}