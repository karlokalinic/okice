using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/ControllerColliderHit")]
public class PlayMakerControllerColliderHit : PlayMakerProxyBase
{
	public void OnControllerColliderHit(ControllerColliderHit hitCollider)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleControllerColliderHit)
			{
				playMakerFSM.Fsm.OnControllerColliderHit(hitCollider);
			}
		}
		DoControllerCollisionEventCallback(hitCollider);
	}
}
