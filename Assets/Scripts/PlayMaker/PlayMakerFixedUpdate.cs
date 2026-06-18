using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/FixedUpdate")]
public class PlayMakerFixedUpdate : PlayMakerProxyBase
{
	public void FixedUpdate()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleFixedUpdate)
			{
				playMakerFSM.Fsm.FixedUpdate();
			}
		}
	}
}
