using System;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmState : INameable
	{
		private bool active;

		private bool finished;

		private FsmStateAction activeAction;

		private int activeActionIndex;

		[NonSerialized]
		private Fsm fsm;

		[SerializeField]
		private string name;

		[SerializeField]
		[TextArea(0, 5)]
		private string description;

		[SerializeField]
		private byte colorIndex;

		[SerializeField]
		private Rect position;

		[SerializeField]
		private bool isBreakpoint;

		[SerializeField]
		private bool isSequence;

		[SerializeField]
		private bool hideUnused;

		[SerializeField]
		private FsmTransition[] transitions = new FsmTransition[0];

		[NonSerialized]
		private FsmStateAction[] actions;

		[SerializeField]
		private ActionData actionData = new ActionData();

		[NonSerialized]
		public bool HandlesOnEvent;

		[NonSerialized]
		private List<FsmStateAction> activeActions;

		[NonSerialized]
		private List<FsmStateAction> _finishedActions;

		public float StateTime { get; private set; }

		public float StateRealTime => FsmTime.RealtimeSinceStartup - RealStartTime;

		public float RealStartTime { get; private set; }

		public int loopCount { get; private set; }

		public int maxLoopCount { get; private set; }

		public bool HasErrors { get; set; }

		public List<FsmStateAction> ActiveActions => activeActions ?? (activeActions = new List<FsmStateAction>());

		private List<FsmStateAction> finishedActions => _finishedActions ?? (_finishedActions = new List<FsmStateAction>());

		public bool Active => active;

		public FsmStateAction ActiveAction => activeAction;

		public bool IsInitialized => fsm != null;

		public Fsm Fsm
		{
			get
			{
				if (fsm == null)
				{
					Debug.LogError("get_fsm: Fsm not initialized: " + name);
				}
				return fsm;
			}
			set
			{
				if (value == null)
				{
					Debug.LogWarning("set_fsm: value == null: " + name);
				}
				fsm = value;
			}
		}

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

		public bool IsSequence
		{
			get
			{
				return isSequence;
			}
			set
			{
				isSequence = value;
			}
		}

		public int ActiveActionIndex => activeActionIndex;

		public Rect Position
		{
			get
			{
				return position;
			}
			set
			{
				if (!float.IsNaN(value.x) && !float.IsNaN(value.y))
				{
					position = value;
				}
			}
		}

		public bool IsBreakpoint
		{
			get
			{
				return isBreakpoint;
			}
			set
			{
				isBreakpoint = value;
			}
		}

		public bool HideUnused
		{
			get
			{
				return hideUnused;
			}
			set
			{
				hideUnused = value;
			}
		}

		public FsmStateAction[] Actions
		{
			get
			{
				if (fsm == null)
				{
					Debug.LogError("get_actions: Fsm not initialized: " + name);
				}
				return actions ?? (actions = actionData.LoadActions(this));
			}
			set
			{
				actions = value;
			}
		}

		public bool ActionsLoaded => actions != null;

		public ActionData ActionData => actionData;

		public FsmTransition[] Transitions
		{
			get
			{
				return transitions;
			}
			set
			{
				transitions = value;
			}
		}

		public string Description
		{
			get
			{
				return description ?? (description = "");
			}
			set
			{
				description = value;
			}
		}

		public int ColorIndex
		{
			get
			{
				return colorIndex;
			}
			set
			{
				colorIndex = (byte)value;
			}
		}

		public static string GetFullStateLabel(FsmState state)
		{
			if (state == null)
			{
				return "None (State)";
			}
			return Fsm.GetFullFsmLabel(state.Fsm) + " : " + state.Name;
		}

		public FsmState(Fsm fsm)
		{
			this.fsm = fsm;
		}

		public FsmState(FsmState source)
		{
			fsm = source.Fsm;
			name = source.Name;
			description = source.description;
			colorIndex = source.colorIndex;
			position = new Rect(source.position);
			hideUnused = source.hideUnused;
			isBreakpoint = source.isBreakpoint;
			isSequence = source.isSequence;
			transitions = new FsmTransition[source.transitions.Length];
			for (int i = 0; i < source.Transitions.Length; i++)
			{
				transitions[i] = new FsmTransition(source.Transitions[i]);
			}
			actionData = source.actionData.Copy();
		}

		public void CopyActionData(FsmState state)
		{
			actionData = state.actionData.Copy();
		}

		public void LoadActions()
		{
			actions = actionData.LoadActions(this);
		}

		public void SaveActions()
		{
			if (actions != null)
			{
				actionData.SaveActions(this, actions);
			}
		}

		public void OnEnter()
		{
			loopCount++;
			if (loopCount > maxLoopCount)
			{
				maxLoopCount = loopCount;
			}
			active = true;
			finished = false;
			finishedActions.Clear();
			RealStartTime = FsmTime.RealtimeSinceStartup;
			StateTime = 0f;
			ActiveActions.Clear();
			if (ActivateActions(0))
			{
				CheckAllActionsFinished();
			}
		}

		private bool ActivateActions(int startIndex)
		{
			for (int i = startIndex; i < Actions.Length; i++)
			{
				activeActionIndex = i;
				FsmStateAction fsmStateAction = Actions[i];
				if (!fsmStateAction.Enabled)
				{
					fsmStateAction.Finished = true;
					continue;
				}
				activeAction = fsmStateAction;
				fsmStateAction.Active = true;
				fsmStateAction.Finished = false;
				fsmStateAction.Init(this);
				fsmStateAction.Entered = true;
				fsmStateAction.OnEnter();
				if (!fsmStateAction.Finished)
				{
					ActiveActions.Add(fsmStateAction);
				}
				if (Fsm.IsSwitchingState)
				{
					return false;
				}
				if (!fsmStateAction.Finished && isSequence)
				{
					return false;
				}
			}
			return true;
		}

		public bool OnEvent(FsmEvent fsmEvent)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				FsmStateAction fsmStateAction = ActiveActions[i];
				if (fsmStateAction != null && fsmStateAction.Event(fsmEvent))
				{
					return true;
				}
			}
			return fsm.IsSwitchingState;
		}

		public void OnFixedUpdate()
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].OnFixedUpdate();
			}
			CheckAllActionsFinished();
		}

		public void OnUpdate()
		{
			if (!finished || ActiveActions.Count != 0)
			{
				StateTime += Time.deltaTime;
				for (int i = 0; i < ActiveActions.Count; i++)
				{
					ActiveActions[i].OnUpdate();
				}
				CheckAllActionsFinished();
			}
		}

		public void OnLateUpdate()
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].OnLateUpdate();
			}
			CheckAllActionsFinished();
		}

		public bool OnAnimatorMove()
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoAnimatorMove();
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnAnimatorIK(int layerIndex)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoAnimatorIK(layerIndex);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnCollisionEnter(Collision collisionInfo)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoCollisionEnter(collisionInfo);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnCollisionStay(Collision collisionInfo)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoCollisionStay(collisionInfo);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnCollisionExit(Collision collisionInfo)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoCollisionExit(collisionInfo);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnTriggerEnter(Collider other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoTriggerEnter(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnTriggerStay(Collider other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoTriggerStay(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnTriggerExit(Collider other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoTriggerExit(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnParticleCollision(GameObject other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoParticleCollision(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnCollisionEnter2D(Collision2D collisionInfo)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoCollisionEnter2D(collisionInfo);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnCollisionStay2D(Collision2D collisionInfo)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoCollisionStay2D(collisionInfo);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnCollisionExit2D(Collision2D collisionInfo)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoCollisionExit2D(collisionInfo);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnTriggerEnter2D(Collider2D other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoTriggerEnter2D(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnTriggerStay2D(Collider2D other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoTriggerStay2D(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnTriggerExit2D(Collider2D other)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoTriggerExit2D(other);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnControllerColliderHit(ControllerColliderHit collider)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoControllerColliderHit(collider);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnJointBreak(float force)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoJointBreak(force);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public bool OnJointBreak2D(Joint2D joint)
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].DoJointBreak2D(joint);
			}
			RemoveFinishedActions();
			return fsm.IsSwitchingState;
		}

		public void OnGUI()
		{
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				ActiveActions[i].OnGUI();
			}
			RemoveFinishedActions();
		}

		public void FinishAction(FsmStateAction action)
		{
			finishedActions.Add(action);
		}

		private void RemoveFinishedActions()
		{
			for (int i = 0; i < finishedActions.Count; i++)
			{
				ActiveActions.Remove(finishedActions[i]);
			}
			finishedActions.Clear();
		}

		private void CheckAllActionsFinished()
		{
			if (finished || !active || fsm.IsSwitchingState)
			{
				return;
			}
			RemoveFinishedActions();
			if (ActiveActions.Count == 0)
			{
				if (!isSequence || ++activeActionIndex >= actions.Length || ActivateActions(activeActionIndex))
				{
					finished = true;
					fsm.Event(FsmEventTarget.TargetSelf, FsmEvent.Finished);
				}
				return;
			}
			for (int i = 0; i < ActiveActions.Count; i++)
			{
				if (ActiveActions[i].BlocksFinish)
				{
					return;
				}
			}
			finished = true;
			fsm.Event(FsmEventTarget.TargetSelf, FsmEvent.Finished);
		}

		public void OnExit()
		{
			active = false;
			finished = false;
			FsmStateAction[] array = Actions;
			foreach (FsmStateAction fsmStateAction in array)
			{
				if (fsmStateAction.Entered)
				{
					activeAction = fsmStateAction;
					fsmStateAction.OnExit();
				}
			}
		}

		public void ResetLoopCount()
		{
			loopCount = 0;
		}

		public FsmTransition GetTransition(int transitionIndex)
		{
			if (transitionIndex < 0 || transitionIndex > transitions.Length - 1)
			{
				return null;
			}
			return transitions[transitionIndex];
		}

		public FsmEvent GetTransitionEvent(int transitionIndex)
		{
			return GetTransition(transitionIndex)?.FsmEvent;
		}

		public int GetTransitionIndex(FsmTransition transition)
		{
			if (transition == null)
			{
				return -1;
			}
			for (int i = 0; i < transitions.Length; i++)
			{
				if (transitions[i] == transition)
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetStateIndex(FsmState state)
		{
			if (state.Fsm == null)
			{
				return -1;
			}
			for (int i = 0; i < state.Fsm.States.Length; i++)
			{
				if (state.Fsm.States[i] == state)
				{
					return i;
				}
			}
			Debug.LogError("State not in FSM!");
			return -1;
		}

		public bool HasTransition(FsmTransition transition)
		{
			FsmTransition[] array = transitions;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == transition)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasFinishedTransition()
		{
			FsmTransition[] array = transitions;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].FsmEvent.Name == FsmEvent.Finished.Name)
				{
					return true;
				}
			}
			return false;
		}

		public List<FsmTransition> GetGlobalTransitions()
		{
			return fsm.GetGlobalTransitionsToState(this);
		}
	}
}
