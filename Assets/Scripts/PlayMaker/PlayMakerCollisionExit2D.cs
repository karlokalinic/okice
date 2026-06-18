using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/CollisionExit2D")]
public class PlayMakerCollisionExit2D : PlayMakerProxyBase
{
	public void OnCollisionExit2D(Collision2D collisionInfo)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleCollisionExit2D)
			{
				playMakerFSM.Fsm.OnCollisionExit2D(collisionInfo);
			}
		}
		DoCollision2DEventCallback(collisionInfo);
	}
}
