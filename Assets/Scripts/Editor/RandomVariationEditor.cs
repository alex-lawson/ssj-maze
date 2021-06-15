using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

// solution taken from https://forum.unity.com/threads/extending-instead-of-replacing-built-in-inspectors.407612/

[CustomEditor(typeof(Transform))]
[CanEditMultipleObjects]
public class RandomVariationEditor : Editor
{
	private Editor defaultEditor;
	private Transform transform;

	private void OnEnable()
	{
		defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.TransformInspector, UnityEditor"));
		transform = target as Transform;
	}

	private void OnDisable()
	{
		DestroyImmediate(defaultEditor);
	}

	public override void OnInspectorGUI()
	{
		defaultEditor.OnInspectorGUI();

		if (GUILayout.Button("Randomize Y Rotation"))
		{
			for (int i = 0; i < targets.Length; i++)
			{
				Transform t = targets[i] as Transform;

				Vector3 eulers = t.localRotation.eulerAngles;
				//eulers.x *= 1 + RotationVariance.x * Random.value - 0.5f * RotationVariance.x;
				//eulers.y *= 1 + RotationVariance.y * Random.value - 0.5f * RotationVariance.y;
				//eulers.z *= 1 + RotationVariance.z * Random.value - 0.5f * RotationVariance.z;

				eulers.y = UnityEngine.Random.value * 360f;

				t.localRotation = Quaternion.Euler(eulers);
			}
		}

		if (GUILayout.Button("Randomize Scale"))
		{
			for (int i = 0; i < targets.Length; i++)
			{
				Transform t = targets[i] as Transform;

				float uniformScaleVariance = 0.5f;
				Vector3 scale = t.localScale;
				scale *= 1 + uniformScaleVariance * UnityEngine.Random.value - 0.5f * uniformScaleVariance;
				t.localScale = scale;
			}
		}
	}
}
