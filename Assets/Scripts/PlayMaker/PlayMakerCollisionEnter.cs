using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/CollisionEnter")]
public class PlayMakerCollisionEnter : PlayMakerProxyBase
{
	public void OnCollisionEnter(Collision collisionInfo)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleCollisionEnter)
			{
				playMakerFSM.Fsm.OnCollisionEnter(collisionInfo);
			}
		}
		DoCollisionEventCallback(collisionInfo);
	}
}
