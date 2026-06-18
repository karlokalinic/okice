using System;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmEventMapping
	{
		public FsmEvent fromEvent;

		public FsmEvent toEvent;

		public FsmEventMapping()
		{
		}

		public FsmEventMapping(FsmEvent fromEvent, FsmEvent toEvent)
		{
			this.fromEvent = fromEvent;
			this.toEvent = toEvent;
		}

		public FsmEventMapping(FsmEventMapping source)
		{
			fromEvent = source.fromEvent;
			toEvent = source.toEvent;
		}

		public FsmEventMapping Init()
		{
			fromEvent = FsmEvent.GetFsmEvent(fromEvent);
			toEvent = FsmEvent.GetFsmEvent(toEvent);
			return this;
		}
	}
}
