using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	public abstract class FsmStateAction : IFsmStateAction
	{
		public static Color ActiveHighlightColor;

		public static bool Repaint;

		private string name;

		private bool enabled = true;

		private bool isOpen = true;

		private bool active;

		private bool finished;

		private bool autoName;

		private bool blocksFinish = true;

		private GameObject owner;

		[NonSerialized]
		private FsmState fsmState;

		[NonSerialized]
		private Fsm fsm;

		[NonSerialized]
		private PlayMakerFSM fsmComponent;

		public string Name
		{
			get
			{
				return name;
			}
			set
			{
				name = value;
			}
		}

		public string DisplayName { get; set; }

		public Fsm Fsm
		{
			get
			{
				return fsm;
			}
			set
			{
				fsm = value;
			}
		}

		public GameObject Owner
		{
			get
			{
				return owner;
			}
			set
			{
				owner = value;
			}
		}

		public FsmState State
		{
			get
			{
				return fsmState;
			}
			set
			{
				fsmState = value;
			}
		}

		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				enabled = value;
			}
		}

		public bool IsOpen
		{
			get
			{
				return isOpen;
			}
			set
			{
				isOpen = value;
			}
		}

		public bool IsAutoNamed
		{
			get
			{
				return autoName;
			}
			set
			{
				autoName = value;
			}
		}

		public bool Entered { get; set; }

		public bool Finished
		{
			get
			{
				return finished;
			}
			set
			{
				if (value)
				{
					active = false;
				}
				finished = value;
			}
		}

		public bool BlocksFinish
		{
			get
			{
				return blocksFinish;
			}
			set
			{
				blocksFinish = value;
			}
		}

		public bool HandlesOnEvent
		{
			set
			{
				if (State != null)
				{
					State.HandlesOnEvent = value;
				}
			}
		}

		public bool Active
		{
			get
			{
				return active;
			}
			set
			{
				active = value;
			}
		}

		public virtual void Init(FsmState state)
		{
			fsmState = state;
			fsm = state.Fsm;
			owner = fsm.GameObject;
			fsmComponent = fsm.FsmComponent;
		}

		public virtual void InitEditor(Fsm fsmOwner)
		{
		}

		public virtual void Reset()
		{
		}

		public void BaseReset()
		{
			autoName = false;
			name = "";
		}

		public virtual void OnPreprocess()
		{
		}

		public virtual void Awake()
		{
		}

		public virtual bool Event(FsmEvent fsmEvent)
		{
			return false;
		}

		public void Finish()
		{
			if (!finished)
			{
				active = false;
				finished = true;
				State.FinishAction(this);
			}
		}

		public Coroutine StartCoroutine(IEnumerator routine)
		{
			return fsmComponent.StartCoroutine("DoCoroutine", routine);
		}

		public void StopCoroutine(Coroutine routine)
		{
			fsmComponent.StopCoroutine(routine);
		}

		public virtual void OnEnter()
		{
		}

		public virtual void OnFixedUpdate()
		{
		}

		public virtual void OnUpdate()
		{
		}

		public virtual void OnGUI()
		{
		}

		public virtual void OnLateUpdate()
		{
		}

		public virtual void OnExit()
		{
		}

		public virtual void OnDrawActionGizmos()
		{
		}

		public virtual void OnDrawActionGizmosSelected()
		{
		}

		public virtual string AutoName()
		{
			return null;
		}

		public virtual void OnActionTargetInvoked(object targetObject)
		{
		}

		public virtual float GetProgress()
		{
			return 0f;
		}

		public virtual void DoCollisionEnter(Collision collisionInfo)
		{
		}

		public virtual void DoCollisionStay(Collision collisionInfo)
		{
		}

		public virtual void DoCollisionExit(Collision collisionInfo)
		{
		}

		public virtual void DoTriggerEnter(Collider other)
		{
		}

		public virtual void DoTriggerStay(Collider other)
		{
		}

		public virtual void DoTriggerExit(Collider other)
		{
		}

		public virtual void DoParticleCollision(GameObject other)
		{
		}

		public virtual void DoCollisionEnter2D(Collision2D collisionInfo)
		{
		}

		public virtual void DoCollisionStay2D(Collision2D collisionInfo)
		{
		}

		public virtual void DoCollisionExit2D(Collision2D collisionInfo)
		{
		}

		public virtual void DoTriggerEnter2D(Collider2D other)
		{
		}

		public virtual void DoTriggerStay2D(Collider2D other)
		{
		}

		public virtual void DoTriggerExit2D(Collider2D other)
		{
		}

		public virtual void DoControllerColliderHit(ControllerColliderHit collider)
		{
		}

		public virtual void DoJointBreak(float force)
		{
		}

		public virtual void DoJointBreak2D(Joint2D joint)
		{
		}

		public virtual void DoAnimatorMove()
		{
		}

		public virtual void DoAnimatorIK(int layerIndex)
		{
		}

		public void Log(string text)
		{
			if (FsmLog.LoggingEnabled)
			{
				fsm.MyLog.LogAction(FsmLogType.Info, text);
			}
		}

		public void LogWarning(string text)
		{
			if (FsmLog.LoggingEnabled)
			{
				fsm.MyLog.LogAction(FsmLogType.Warning, text);
			}
		}

		public void LogError(string text)
		{
			if (FsmLog.LoggingEnabled)
			{
				fsm.MyLog.LogAction(FsmLogType.Error, text);
			}
		}

		public virtual string ErrorCheck()
		{
			return string.Empty;
		}

		protected static bool TagMatches(FsmString tag, Component other)
		{
			if (!FsmString.IsNullOrEmpty(tag))
			{
				return other.gameObject.CompareTag(tag.Value);
			}
			return true;
		}

		protected static bool TagMatches(FsmString tag, Collision collisionInfo)
		{
			if (!FsmString.IsNullOrEmpty(tag))
			{
				return collisionInfo.collider.gameObject.CompareTag(tag.Value);
			}
			return true;
		}

		protected static bool TagMatches(FsmString tag, Collision2D collisionInfo)
		{
			if (!FsmString.IsNullOrEmpty(tag))
			{
				return collisionInfo.collider.gameObject.CompareTag(tag.Value);
			}
			return true;
		}

		protected static bool TagMatches(FsmString tag, ControllerColliderHit collisionInfo)
		{
			if (!FsmString.IsNullOrEmpty(tag))
			{
				return collisionInfo.collider.gameObject.CompareTag(tag.Value);
			}
			return true;
		}

		protected static bool TagMatches(FsmString tag, GameObject go)
		{
			if (!FsmString.IsNullOrEmpty(tag))
			{
				return go.CompareTag(tag.Value);
			}
			return true;
		}

		[Conditional("DEBUG_LOG")]
		private void DebugLog(object message, LogColor logColor = LogColor.None)
		{
		}
	}
}
