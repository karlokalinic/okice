using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/TriggerExit")]
public class PlayMakerTriggerExit : PlayMakerProxyBase
{
	public void OnTriggerExit(Collider other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleTriggerExit)
			{
				playMakerFSM.Fsm.OnTriggerExit(other);
			}
		}
		DoTriggerEventCallback(other);
	}
}
