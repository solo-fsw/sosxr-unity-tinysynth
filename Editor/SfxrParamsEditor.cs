using UnityEditor;
using UnityEngine;

namespace usfxr {
	[CustomPropertyDrawer(typeof(SfxrParams))]
	public class SfxrParamsEditor : PropertyDrawer {

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("LASER/SHOOT")) SetParam(property, SfxrPreset.LaserShoot());
			if (GUILayout.Button("PICKUP/COIN")) SetParam(property, SfxrPreset.PickupCoin());
			if (GUILayout.Button("EXPLOSION")) SetParam(property, SfxrPreset.Explosion());
			if (GUILayout.Button("POWER UP")) SetParam(property, SfxrPreset.PowerUp());
			if (GUILayout.Button("HIT/HURT")) SetParam(property, SfxrPreset.HitHurt());
			if (GUILayout.Button("JUMP")) SetParam(property, SfxrPreset.Jump());
			if (GUILayout.Button("BLIP/SELECT")) SetParam(property, SfxrPreset.BlipSelect());
			EditorGUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(property, label, true);
			if (!EditorGUI.EndChangeCheck()) return;
			Preview(property);
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