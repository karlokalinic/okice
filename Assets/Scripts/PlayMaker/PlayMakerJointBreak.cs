using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/JointBreak")]
public class PlayMakerJointBreak : PlayMakerProxyBase
{
	public void OnJointBreak(float breakForce)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleJointBreak)
			{
				playMakerFSM.Fsm.OnJointBreak(breakForce);
			}
		}
	}
}
