using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/TriggerEnter2D")]
public class PlayMakerTriggerEnter2D : PlayMakerProxyBase
{
	public void OnTriggerEnter2D(Collider2D other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleTriggerEnter2D)
			{
				playMakerFSM.Fsm.OnTriggerEnter2D(other);
			}
		}
		DoTrigger2DEventCallback(other);
	}
}
