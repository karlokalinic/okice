using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/TriggerStay2D")]
public class PlayMakerTriggerStay2D : PlayMakerProxyBase
{
	public void OnTriggerStay2D(Collider2D other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleTriggerStay2D)
			{
				playMakerFSM.Fsm.OnTriggerStay2D(other);
			}
		}
		DoTrigger2DEventCallback(other);
	}
}
