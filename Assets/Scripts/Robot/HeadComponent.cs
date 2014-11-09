using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HeadComponent : RobotComponent 
{

	public override void GetInputHints(ref List<InputHintsGUI.InputHint> hints)
	{
		hints.Add(new InputHintsGUI.InputHint("F", "Enter Attachment Mode"));
		base.GetInputHints(ref hints);
	}

}
