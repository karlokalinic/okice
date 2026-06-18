using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/AnimatorMove")]
public class PlayMakerAnimatorMove : PlayMakerProxyBase
{
	public void OnAnimatorMove()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleAnimatorMove)
			{
				playMakerFSM.Fsm.OnAnimatorMove();
			}
		}
	}
}
