#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MazeGen))]
public class MazeGenEditor : Editor
{
	private MazeGen mazeGen;

	private SerializedProperty drawGizmos;
	private SerializedProperty gridDims;
	private SerializedProperty gridScale;
	private SerializedProperty player;
	private SerializedProperty objective;
	private SerializedProperty objectParent;
	private SerializedProperty prefabsTypeA;
	private SerializedProperty prefabsTypeB;
	private SerializedProperty prefabsTypeC;
	private SerializedProperty prefabsTypeD;
	private SerializedProperty prefabsTypeE;
	private SerializedProperty prefabsTypeF;

	private void OnEnable()
	{
		mazeGen = target as MazeGen;

		drawGizmos = serializedObject.FindProperty("DrawGizmos");
		gridDims = serializedObject.FindProperty("GridDims");
		gridScale = serializedObject.FindProperty("GridScale");
		player = serializedObject.FindProperty("Player");
		objective = serializedObject.FindProperty("Objective");
		objectParent = serializedObject.FindProperty("ObjectParent");
		prefabsTypeA = serializedObject.FindProperty("PrefabsTypeA");
		prefabsTypeB = serializedObject.FindProperty("PrefabsTypeB");
		prefabsTypeC = serializedObject.FindProperty("PrefabsTypeC");
		prefabsTypeD = serializedObject.FindProperty("PrefabsTypeD");
		prefabsTypeE = serializedObject.FindProperty("PrefabsTypeE");
		prefabsTypeF = serializedObject.FindProperty("PrefabsTypeF");
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(drawGizmos, new GUIContent("Draw Connection Gizmos"));

		EditorGUILayout.PropertyField(gridDims, new GUIContent("Grid Dimensions"));
		EditorGUILayout.PropertyField(gridScale, new GUIContent("Grid Scale"));
		EditorGUILayout.PropertyField(player, new GUIContent("Player Transform"));
		EditorGUILayout.PropertyField(objective, new GUIContent("Objective Transform"));
		EditorGUILayout.PropertyField(objectParent, new GUIContent("Object Parent Transform"));

		EditorGUILayout.PropertyField(prefabsTypeA, new GUIContent("Prefabs A (no connection)"));
		EditorGUILayout.PropertyField(prefabsTypeB, new GUIContent("Prefabs B (N)"));
		EditorGUILayout.PropertyField(prefabsTypeC, new GUIContent("Prefabs C (N+E)"));
		EditorGUILayout.PropertyField(prefabsTypeD, new GUIContent("Prefabs D (N+S)"));
		EditorGUILayout.PropertyField(prefabsTypeE, new GUIContent("Prefabs E (N+E+S)"));
		EditorGUILayout.PropertyField(prefabsTypeF, new GUIContent("Prefabs F (N+E+S+W)"));

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