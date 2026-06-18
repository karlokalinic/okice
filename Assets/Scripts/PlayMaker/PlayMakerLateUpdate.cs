using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/LateUpdate")]
public class PlayMakerLateUpdate : PlayMakerProxyBase
{
	public void LateUpdate()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleLateUpdate)
			{
				playMakerFSM.Fsm.LateUpdate();
			}
		}
	}
}
