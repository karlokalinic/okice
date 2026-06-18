using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/TriggerStay")]
public class PlayMakerTriggerStay : PlayMakerProxyBase
{
	public void OnTriggerStay(Collider other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleTriggerStay)
			{
				playMakerFSM.Fsm.OnTriggerStay(other);
			}
		}
		DoTriggerEventCallback(other);
	}
}
