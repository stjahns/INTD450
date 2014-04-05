//using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System;

[AttributeUsage(AttributeTargets.Field)]
public class OutputEventConnectionsAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method)]
public class InputSocketAttribute : Attribute
{
}

public class TargetInfo
{
	public string info;
	public Vector3 position;
}

public class TriggerBase : MonoBehaviour
{

	private Dictionary<GameObject, TargetInfo> targetInfos = new Dictionary<GameObject, TargetInfo>();

	public Dictionary<GameObject, TargetInfo> getTargetInfos()
	{
		return targetInfos;
	}

	public static bool DrawLines = true;

	virtual public void OnDrawGizmos()
	{
		if (DrawLines)
		{
			IEnumerable<FieldInfo> outputSockets = this.GetType().GetFields()
				.Where(f => Attribute.IsDefined(f, typeof(OutputEventConnectionsAttribute)));

			foreach (FieldInfo socketField in outputSockets)
			{
				IEnumerable<SignalConnection> socket = socketField.GetValue(this)
					as IEnumerable<SignalConnection>;

				foreach (SignalConnection conn in socket)
				{
					if (conn && conn.target != null)
					{
						Gizmos.color = Color.red;
						Gizmos.DrawLine(transform.position, conn.target.transform.position);
					}
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
				if (conn && conn.target != null)
				{
					if (targetInfos.TryGetValue(conn.target, out target))
					{
						target.info += eventLabel + " -> " + conn.message;
						if (conn.argument != null)
						{
							target.info += "(" + conn.argument + ")";
						}
						target.info += "\n";
					}
					else
					{
						target = new TargetInfo();
						target.position = conn.target.transform.position;
						target.info = eventLabel + " -> " + conn.message;
						if (conn.argument != null)
						{
							target.info += "(" + conn.argument + ")";
						}
						target.info += "\n";
					}

					targetInfos[conn.target] = target;
				}
			}
		}

	}

}
