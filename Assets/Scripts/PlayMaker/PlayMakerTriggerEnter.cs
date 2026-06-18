using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/TriggerEnter")]
public class PlayMakerTriggerEnter : PlayMakerProxyBase
{
	public void OnTriggerEnter(Collider other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleTriggerEnter)
			{
				playMakerFSM.Fsm.OnTriggerEnter(other);
			}
		}
		DoTriggerEventCallback(other);
	}
}
