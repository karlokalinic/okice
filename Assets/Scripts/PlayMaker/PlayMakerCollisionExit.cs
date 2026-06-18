using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/CollisionExit")]
public class PlayMakerCollisionExit : PlayMakerProxyBase
{
	public void OnCollisionExit(Collision collisionInfo)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleCollisionExit)
			{
				playMakerFSM.Fsm.OnCollisionExit(collisionInfo);
			}
		}
		DoCollisionEventCallback(collisionInfo);
	}
}
