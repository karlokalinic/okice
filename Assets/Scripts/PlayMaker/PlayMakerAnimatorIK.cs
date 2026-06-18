using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/AnimatorIK")]
public class PlayMakerAnimatorIK : PlayMakerProxyBase
{
	public void OnAnimatorIK(int layerIndex)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleAnimatorIK)
			{
				playMakerFSM.Fsm.OnAnimatorIK(layerIndex);
			}
		}
	}
}
