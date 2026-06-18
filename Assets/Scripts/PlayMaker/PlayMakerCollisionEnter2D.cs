using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/CollisionEnter2D")]
public class PlayMakerCollisionEnter2D : PlayMakerProxyBase
{
	public void OnCollisionEnter2D(Collision2D collisionInfo)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleCollisionEnter2D)
			{
				playMakerFSM.Fsm.OnCollisionEnter2D(collisionInfo);
			}
		}
		DoCollision2DEventCallback(collisionInfo);
	}
}
