using System.Collections.Generic;
using UnityEngine;

public abstract class PlayMakerProxyBase : MonoBehaviour
{
	public delegate void TriggerEvent(Collider other);

	public delegate void CollisionEvent(Collision collisionInfo);

	public delegate void Trigger2DEvent(Collider2D other);

	public delegate void Collision2DEvent(Collision2D collisionInfo);

	public delegate void ParticleCollisionEvent(GameObject gameObject);

	public delegate void ControllerCollisionEvent(ControllerColliderHit hitCollider);

	public List<PlayMakerFSM> TargetFSMs = new List<PlayMakerFSM>();

	protected PlayMakerFSM[] playMakerFSMs => TargetFSMs.ToArray();

	private event TriggerEvent TriggerEventCallback;

	private event CollisionEvent CollisionEventCallback;

	private event ParticleCollisionEvent ParticleCollisionEventCallback;

	private event ControllerCollisionEvent ControllerCollisionEventCallback;

	private event Trigger2DEvent Trigger2DEventCallback;

	private event Collision2DEvent Collision2DEventCallback;

	public void AddTarget(PlayMakerFSM fsmTarget)
	{
		if (!TargetFSMs.Contains(fsmTarget))
		{
			TargetFSMs.Add(fsmTarget);
		}
	}

	public bool HasTriggerEventDelegates()
	{
		return this.TriggerEventCallback != null;
	}

	public void AddTriggerEventCallback(TriggerEvent triggerEvent)
	{
		TriggerEventCallback += triggerEvent;
	}

	public void RemoveTriggerEventCallback(TriggerEvent triggerEvent)
	{
		TriggerEventCallback -= triggerEvent;
	}

	public void DoTriggerEventCallback(Collider other)
	{
		if (this.TriggerEventCallback != null)
		{
			this.TriggerEventCallback(other);
		}
	}

	public bool HasTrigger2DEventDelegates()
	{
		return this.Trigger2DEventCallback != null;
	}

	public void AddTrigger2DEventCallback(Trigger2DEvent triggerEvent)
	{
		Trigger2DEventCallback += triggerEvent;
	}

	public void RemoveTrigger2DEventCallback(Trigger2DEvent triggerEvent)
	{
		Trigger2DEventCallback -= triggerEvent;
	}

	public void DoTrigger2DEventCallback(Collider2D other)
	{
		if (this.Trigger2DEventCallback != null)
		{
			this.Trigger2DEventCallback(other);
		}
	}

	public bool HasCollisionEventDelegates()
	{
		return this.CollisionEventCallback != null;
	}

	public void AddCollisionEventCallback(CollisionEvent collisionEvent)
	{
		CollisionEventCallback += collisionEvent;
	}

	public void RemoveCollisionEventCallback(CollisionEvent collisionEvent)
	{
		CollisionEventCallback -= collisionEvent;
	}

	public void DoCollisionEventCallback(Collision collisionInfo)
	{
		if (this.CollisionEventCallback != null)
		{
			this.CollisionEventCallback(collisionInfo);
		}
	}

	public bool HasCollision2DEventDelegates()
	{
		return this.Collision2DEventCallback != null;
	}

	public void AddCollision2DEventCallback(Collision2DEvent collisionEvent)
	{
		Collision2DEventCallback += collisionEvent;
	}

	public void RemoveCollision2DEventCallback(Collision2DEvent collisionEvent)
	{
		Collision2DEventCallback -= collisionEvent;
	}

	public void DoCollision2DEventCallback(Collision2D collisionInfo)
	{
		if (this.Collision2DEventCallback != null)
		{
			this.Collision2DEventCallback(collisionInfo);
		}
	}

	public bool HasParticleCollisionEventDelegates()
	{
		return this.ParticleCollisionEventCallback != null;
	}

	public void AddParticleCollisionEventCallback(ParticleCollisionEvent collisionEvent)
	{
		ParticleCollisionEventCallback += collisionEvent;
	}

	public void RemoveParticleCollisionEventCallback(ParticleCollisionEvent collisionEvent)
	{
		ParticleCollisionEventCallback -= collisionEvent;
	}

	public void DoParticleCollisionEventCallback(GameObject go)
	{
		if (this.ParticleCollisionEventCallback != null)
		{
			this.ParticleCollisionEventCallback(go);
		}
	}

	public bool HasControllerCollisionEventDelegates()
	{
		return this.ControllerCollisionEventCallback != null;
	}

	public void AddControllerCollisionEventCallback(ControllerCollisionEvent collisionEvent)
	{
		ControllerCollisionEventCallback += collisionEvent;
	}

	public void RemoveControllerCollisionEventCallback(ControllerCollisionEvent collisionEvent)
	{
		ControllerCollisionEventCallback -= collisionEvent;
	}

	public void DoControllerCollisionEventCallback(ControllerColliderHit hitCollider)
	{
		if (this.ControllerCollisionEventCallback != null)
		{
			this.ControllerCollisionEventCallback(hitCollider);
		}
	}
}
