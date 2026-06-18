using UnityEngine;

namespace HutongGames.PlayMaker
{
	public class DelayedEvent
	{
		private readonly Fsm fsm;

		private readonly FsmEvent fsmEvent;

		private readonly FsmEventTarget eventTarget;

		private FsmEventData eventData;

		private float timer;

		private float delay;

		private bool eventFired;

		public FsmEvent FsmEvent => fsmEvent;

		public float Timer => timer;

		public bool Finished => eventFired;

		public DelayedEvent(Fsm fsm, FsmEvent fsmEvent, float delay)
		{
			this.fsm = fsm;
			timer = delay;
			this.delay = delay;
			this.fsmEvent = fsmEvent;
			eventData = new FsmEventData(Fsm.EventData)
			{
				SentByFsm = FsmExecutionStack.ExecutingFsm,
				SentByState = FsmExecutionStack.ExecutingState,
				SentByAction = FsmExecutionStack.ExecutingAction
			};
		}

		public DelayedEvent(Fsm fsm, string fsmEventName, float delay)
			: this(fsm, FsmEvent.GetFsmEvent(fsmEventName), delay)
		{
		}

		public DelayedEvent(Fsm fsm, FsmEventTarget eventTarget, FsmEvent fsmEvent, float delay)
			: this(fsm, fsmEvent, delay)
		{
			this.eventTarget = eventTarget;
		}

		public DelayedEvent(Fsm fsm, FsmEventTarget eventTarget, string fsmEvent, float delay)
			: this(fsm, fsmEvent, delay)
		{
			this.eventTarget = eventTarget;
		}

		public DelayedEvent(PlayMakerFSM fsm, FsmEvent fsmEvent, float delay)
			: this(fsm.Fsm, fsmEvent, delay)
		{
		}

		public DelayedEvent(PlayMakerFSM fsm, string fsmEventName, float delay)
			: this(fsm.Fsm, fsmEventName, delay)
		{
		}

		public void Update()
		{
			timer -= Time.deltaTime;
			if (timer < 0f)
			{
				FsmEventData fsmEventData = Fsm.EventData;
				Fsm.EventData = eventData;
				if (eventTarget == null)
				{
					fsm.Event(fsmEvent);
				}
				else
				{
					fsm.Event(eventTarget, fsmEvent);
				}
				fsm.UpdateStateChanges();
				eventFired = true;
				eventData = null;
				Fsm.EventData = fsmEventData;
			}
		}

		public float GetProgress()
		{
			return 1f - timer / delay;
		}

		public static bool WasSent(DelayedEvent delayedEvent)
		{
			return delayedEvent?.eventFired ?? true;
		}
	}
}
