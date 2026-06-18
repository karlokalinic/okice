using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/CollisionStay2D")]
public class PlayMakerCollisionStay2D : PlayMakerProxyBase
{
	public void OnCollisionStay2D(Collision2D collisionInfo)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleCollisionStay2D)
			{
				playMakerFSM.Fsm.OnCollisionStay2D(collisionInfo);
			}
		}
		DoCollision2DEventCallback(collisionInfo);
	}
}
