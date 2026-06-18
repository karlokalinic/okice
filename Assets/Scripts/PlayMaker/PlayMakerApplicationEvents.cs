using HutongGames.PlayMaker;
using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/Application Events")]
public class PlayMakerApplicationEvents : PlayMakerProxyBase
{
	public void OnApplicationFocus()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.HandleApplicationEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.ApplicationFocus);
			}
		}
	}

	public void OnApplicationPause()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.HandleApplicationEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.ApplicationPause);
			}
		}
	}
}
