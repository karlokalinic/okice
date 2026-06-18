using HutongGames.PlayMaker;
using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/Mouse Events")]
public class PlayMakerMouseEvents : PlayMakerProxyBase
{
	public void OnMouseEnter()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseEnter);
			}
		}
	}

	public void OnMouseDown()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseDown);
			}
		}
	}

	public void OnMouseUp()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseUp);
				Fsm.LastClickedObject = base.gameObject;
			}
		}
	}

	public void OnMouseUpAsButton()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseUpAsButton);
				Fsm.LastClickedObject = base.gameObject;
			}
		}
	}

	public void OnMouseExit()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseExit);
			}
		}
	}

	public void OnMouseDrag()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseDrag);
			}
		}
	}

	public void OnMouseOver()
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Fsm.MouseEvents)
			{
				playMakerFSM.Fsm.Event(FsmEvent.MouseOver);
			}
		}
	}
}
