using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Checks whether a conversation is currently active.")]
	public class IsConversationActive : FsmStateAction
	{
		[Tooltip("Repeat every frame while the state is active")]
		public FsmBool everyFrame;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a Bool variable")]
		public FsmBool storeResult;

		public FsmEvent activeEvent;

		public FsmEvent inactiveEvent;

		public override void Reset()
		{
			if (everyFrame != null)
			{
				everyFrame.Value = false;
			}
			if (storeResult != null)
			{
				storeResult.Value = false;
			}
		}

		public override void OnEnter()
		{
			CheckIsConversationActive();
			if (everyFrame == null || !everyFrame.Value)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (everyFrame != null)
			{
				if (everyFrame.Value)
				{
					CheckIsConversationActive();
				}
				else
				{
					Finish();
				}
			}
		}

		private void CheckIsConversationActive()
		{
			bool isConversationActive = DialogueManager.IsConversationActive;
			if (storeResult != null)
			{
				storeResult.Value = isConversationActive;
			}
			if (isConversationActive)
			{
				base.Fsm.Event(activeEvent);
			}
			else
			{
				base.Fsm.Event(inactiveEvent);
			}
		}
	}
}
