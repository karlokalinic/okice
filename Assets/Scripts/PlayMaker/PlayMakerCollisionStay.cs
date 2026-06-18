using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/CollisionStay")]
public class PlayMakerCollisionStay : PlayMakerProxyBase
{
	public void OnCollisionStay(Collision collisionInfo)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleCollisionStay)
			{
				playMakerFSM.Fsm.OnCollisionStay(collisionInfo);
			}
		}
		DoCollisionEventCallback(collisionInfo);
	}
}
