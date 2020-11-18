using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace usfxr {
	[CustomPropertyDrawer(typeof(SfxrParams))]
	public class SfxrParamsEditor : PropertyDrawer {

		bool expand;
		bool expandPresets;
		
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			expand = EditorGUILayout.BeginFoldoutHeaderGroup(expand, property.name, null, ShowHeaderContextMenu);
			if (expand) OnExpandedGUI(property);
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		void OnExpandedGUI(SerializedProperty property) {
			EditorGUI.BeginChangeCheck();
			
			expandPresets = EditorGUILayout.Foldout(expandPresets, "Presets");
			if (expandPresets) OnPresetGUI(property);
			
			foreach (var child in GetVisibleChildren(property)) {
				EditorGUILayout.PropertyField(child, null, false);
			}
			
			if (!EditorGUI.EndChangeCheck()) return;
			Preview(property);
		}

		void OnPresetGUI(SerializedProperty property) {
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
			menu.AddItem(new GUIContent("This is a placeholder menu for export/import things"), false, OnItemClicked);
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

		static void SetParam(SerializedProperty property, SfxrParams param) {
			var target = property.serializedObject.targetObject;
			var type   = target.GetType();
			var field  = type.GetField(property.name);
			field.SetValue(target, param);
			SfxrPlayer.Play(param);
		}

		static void Preview(SerializedProperty property) {
			var target = property.serializedObject.targetObject;
			var type   = target.GetType();
			var field  = type.GetField(property.name);
			SfxrPlayer.Play((SfxrParams) field.GetValue(target));
		}
	}
}