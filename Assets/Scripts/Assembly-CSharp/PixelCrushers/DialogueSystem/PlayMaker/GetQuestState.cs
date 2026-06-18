using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the state of a quest.")]
	public class GetQuestState : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the quest")]
		public FsmString questName;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a String variable ('unassigned', 'active', 'success', or 'failure')")]
		public FsmString storeResult;

		public FsmEvent unassignedStateEvent;

		public FsmEvent grantableStateEvent;

		public FsmEvent activeStateEvent;

		public FsmEvent returnToNPCStateEvent;

		public FsmEvent successStateEvent;

		public FsmEvent failureStateEvent;

		public override void Reset()
		{
			if (questName != null)
			{
				questName.Value = string.Empty;
			}
			storeResult = null;
		}

		public override void OnEnter()
		{
			if (questName == null || string.IsNullOrEmpty(questName.Value))
			{
				LogError(string.Format("{0}: Quest Name is null or blank.", "Dialogue System"));
			}
			else
			{
				QuestState questState = QuestLog.GetQuestState(questName.Value);
				if (storeResult != null)
				{
					storeResult.Value = questState.ToString().ToLower();
				}
				switch (questState)
				{
				case QuestState.Unassigned:
					base.Fsm.Event(unassignedStateEvent);
					break;
				case QuestState.Grantable:
					base.Fsm.Event(grantableStateEvent);
					break;
				case QuestState.Active:
					base.Fsm.Event(activeStateEvent);
					break;
				case QuestState.ReturnToNPC:
					base.Fsm.Event(returnToNPCStateEvent);
					break;
				case QuestState.Success:
					base.Fsm.Event(successStateEvent);
					break;
				case QuestState.Failure:
					base.Fsm.Event(failureStateEvent);
					break;
				}
			}
			Finish();
		}
	}
}
