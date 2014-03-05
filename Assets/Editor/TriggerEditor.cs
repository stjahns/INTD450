using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

[CustomEditor(typeof(TriggerBase), true)]
public class TriggerEditor : Editor
{
	public void OnSceneGUI()
	{
		TriggerBase trigger = target as TriggerBase;

		foreach (TargetInfo targetInfo in trigger.getTargetInfos().Values)
		{
			Handles.color = Color.white;
			Handles.Label(targetInfo.position, targetInfo.info);
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		TriggerBase trigger = target as TriggerBase;

		// for every output socket...
		IEnumerable<FieldInfo> outputSockets = trigger.GetType().GetFields()
			.Where(f => Attribute.IsDefined(f, typeof(OutputEventConnectionsAttribute)));

		EditorGUILayout.BeginToggleGroup("Output Connections", true);
		foreach (FieldInfo socketField in outputSockets)
		{

			EditorGUILayout.Separator();
			EditorGUILayout.BeginToggleGroup(socketField.Name, true);

			List<SignalConnection> socket = socketField.GetValue(trigger)
				as List<SignalConnection>;
			

			int connectionIndex = 0;

			foreach (SignalConnection conn in socket.ToList())	
			{
				EditorGUILayout.BeginVertical();
				EditorGUILayout.Separator();

				if (conn)
				{
					var targetObject = EditorGUILayout.ObjectField(
							"Target",
							conn ? conn.target : null,
							typeof(GameObject),
							true) as GameObject;

					conn.target = targetObject;
				}

				if (conn && conn.target != null)
				{
					List<string> messageOptions = new List<string>();

					messageOptions.Add(conn.message);

					var components = conn.target.GetComponents<MonoBehaviour>();

					foreach (var component in components)
					{
						IEnumerable<MethodInfo> methods = component.GetType().GetMethods();
						messageOptions.AddRange(methods
								.Where(m => Attribute.IsDefined(m, typeof(InputSocketAttribute)))
								.Select(m => m.Name)
								.OrderBy(m => m));
					}
					
					// Choose message
					var message = EditorGUILayout.Popup("Message", 0, messageOptions.ToArray());
					conn.message = messageOptions[message];


					// Specify argument for message if applicable
					MethodInfo methodInfo = null;
					foreach (var component in components)
					{
						IEnumerable<MethodInfo> methods = component.GetType().GetMethods();

						methodInfo = methods
								.Where(m => Attribute.IsDefined(m, typeof(InputSocketAttribute)))
								.FirstOrDefault(m => m.Name == conn.message);

						if (methodInfo != null)
						{
							break;
						}
					}

					if (methodInfo != null && methodInfo.GetParameters().Count() > 0)
					{
						ParameterInfo param = methodInfo.GetParameters().First();
						string argument = EditorGUILayout.TextField("Argument: " + param.Name.ToUpper(),
								conn.argument != null ? conn.argument : "");
						if (argument.Length > 0)
						{
							conn.argument = argument;
						}
						else
						{
							conn.argument = null;
						}
					}
					else
					{
						conn.argument = null;
					}
				}

				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Delete Connection"))
				{
					int undoIndex = Undo.GetCurrentGroup();
					Undo.RecordObject(trigger.gameObject, "Remove Connection");
					socket.Remove(conn);
					var socketProperty = serializedObject.FindProperty(socketField.Name);

					// WTF?
					// http://forum.unity3d.com/threads/144346-Custom-inspector-initializing-array
					socketProperty.DeleteArrayElementAtIndex(connectionIndex);
					socketProperty.DeleteArrayElementAtIndex(connectionIndex);

					Undo.DestroyObjectImmediate(conn);
					Undo.CollapseUndoOperations(undoIndex);
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.EndVertical();
				connectionIndex++;
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("New Connection"))
			{
				var conn = Undo.AddComponent<SignalConnection>(trigger.gameObject);
				var socketProperty = serializedObject.FindProperty(socketField.Name);
				conn.hideFlags = HideFlags.HideInInspector;
				socketProperty.InsertArrayElementAtIndex(socketProperty.arraySize);
				var connProperty = socketProperty.GetArrayElementAtIndex(socketProperty.arraySize - 1);
				connProperty.objectReferenceValue = conn;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndToggleGroup();
		}
		EditorGUILayout.EndToggleGroup();

		serializedObject.ApplyModifiedProperties();
	}
}

