using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/TriggerExit2D")]
public class PlayMakerTriggerExit2D : PlayMakerProxyBase
{
	public void OnTriggerExit2D(Collider2D other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleTriggerExit2D)
			{
				playMakerFSM.Fsm.OnTriggerExit2D(other);
			}
		}
		DoTrigger2DEventCallback(other);
	}
}
