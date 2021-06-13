#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGen))]
public class MazeGenEditor : Editor
{
	private MazeGen mazeGen;

	private SerializedProperty gridDims;
	private SerializedProperty gridScale;
	private SerializedProperty player;
	private SerializedProperty objective;
	private SerializedProperty objectParent;
	private SerializedProperty segmentMap;

	private void OnEnable()
	{
		mazeGen = target as MazeGen;

		gridDims = serializedObject.FindProperty("GridDims");
		gridScale = serializedObject.FindProperty("GridScale");
		player = serializedObject.FindProperty("Player");
		objective = serializedObject.FindProperty("Objective");
		objectParent = serializedObject.FindProperty("ObjectParent");
		segmentMap = serializedObject.FindProperty("SegmentMap");
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(gridDims, new GUIContent("Grid Dimensions"));
		EditorGUILayout.PropertyField(gridScale, new GUIContent("Grid Scale"));
		EditorGUILayout.PropertyField(player, new GUIContent("Player Transform"));
		EditorGUILayout.PropertyField(objective, new GUIContent("Objective Transform"));
		EditorGUILayout.PropertyField(objectParent, new GUIContent("Object Parent Transform"));

		System.Array.Resize(ref mazeGen.SegmentMap, 16);

		EditorGUILayout.PropertyField(segmentMap, new GUIContent("Segment Mapping"));

		serializedObject.ApplyModifiedProperties();

		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button(new GUIContent("Generate"), GUILayout.Width(120), GUILayout.Height(25)))
		{
			mazeGen.Generate();
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
	}
}

#endif