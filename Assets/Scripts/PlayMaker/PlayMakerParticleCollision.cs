using UnityEngine;

[AddComponentMenu("PlayMaker/Event Handlers/ParticleCollision")]
public class PlayMakerParticleCollision : PlayMakerProxyBase
{
	public void OnParticleCollision(GameObject other)
	{
		for (int i = 0; i < TargetFSMs.Count; i++)
		{
			PlayMakerFSM playMakerFSM = TargetFSMs[i];
			if (!(playMakerFSM == null) && playMakerFSM.Fsm != null && playMakerFSM.Active && playMakerFSM.Fsm.HandleParticleCollision)
			{
				playMakerFSM.Fsm.OnParticleCollision(other);
			}
		}
	}
}
