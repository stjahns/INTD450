using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

[AttributeUsage(AttributeTargets.Field)]
class OutputEventConnectionsAttribute : Attribute
{
}

class TargetInfo
{
	public string info;
	public Vector3 position;
}

public class TriggerBase : MonoBehaviour
{

	private Dictionary<GameObject, TargetInfo> targetInfos = new Dictionary<GameObject, TargetInfo>();

	virtual public void OnDrawGizmos()
	{
		IEnumerable<FieldInfo> outputSockets = this.GetType().GetFields()
			.Where(f => Attribute.IsDefined(f, typeof(OutputEventConnectionsAttribute)));

		foreach (FieldInfo socketField in outputSockets)
		{
			IEnumerable<SignalConnection> socket = socketField.GetValue(this)
				as IEnumerable<SignalConnection>;

			foreach (SignalConnection conn in socket)
			{
				if (conn.target != null)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(transform.position, conn.target.transform.position);
				}
			}
		}
	}

	virtual public void OnDrawGizmosSelected()
	{
		IEnumerable<string> events = this.GetType().GetFields()
			.Where(f => Attribute.IsDefined(f, typeof(OutputEventConnectionsAttribute)))
			.Select(f => f.Name);

		targetInfos.Clear();

		foreach (string eventLabel in events)
		{
			FieldInfo field = this.GetType().GetField(eventLabel);
			var connections = field.GetValue(this) as List<SignalConnection>;
			foreach (SignalConnection conn in connections)
			{
				TargetInfo target = new TargetInfo();
				if (conn.target != null)
				{
					if (targetInfos.TryGetValue(conn.target, out target))
					{
						target.info += eventLabel + " -> " + conn.message + "\n";
					}
					else
					{
						target = new TargetInfo();
						target.position = conn.target.transform.position;
						target.info = eventLabel + " -> " + conn.message + "\n";
					}

					targetInfos[conn.target] = target;
				}
			}
		}

		foreach (TargetInfo target in targetInfos.Values)
		{
			Handles.color = Color.white;
			Handles.Label(target.position, target.info);
		}

	}

}
