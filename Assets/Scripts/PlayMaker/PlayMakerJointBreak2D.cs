using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/JointBreak2D")]
public class PlayMakerJointBreak2D : PlayMakerProxyBase
{
	public void OnJointBreak2D(Joint2D brokenJoint)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleJointBreak2D)
			{
				playMakerFSM.Fsm.OnJointBreak2D(brokenJoint);
			}
		}
	}
}
