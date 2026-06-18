using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Gets the state of a quest entry in a quest.")]
	public class GetQuestEntryState : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The name of the quest")]
		public FsmString questName;

		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The quest entry number (from 1)")]
		public FsmInt entryNumber;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("Store the result in a String variable ('unassigned', 'active', 'success', or 'failure')")]
		public FsmString storeResult;

		public FsmEvent unassignedStateEvent;

		public FsmEvent activeStateEvent;

		public FsmEvent successStateEvent;

		public FsmEvent failureStateEvent;

		public override void Reset()
		{
			if (questName != null)
			{
				questName.Value = string.Empty;
			}
			if (entryNumber != null)
			{
				entryNumber.Value = 0;
			}
			storeResult = null;
		}

		public override void OnEnter()
		{
			if (PlayMakerTools.IsValueAssigned(questName) && PlayMakerTools.IsValueAssigned(entryNumber))
			{
				QuestState questEntryState = QuestLog.GetQuestEntryState(questName.Value, Mathf.Max(1, entryNumber.Value));
				if (storeResult != null)
				{
					storeResult.Value = questEntryState.ToString().ToLower();
				}
				switch (questEntryState)
				{
				case QuestState.Unassigned:
					base.Fsm.Event(unassignedStateEvent);
					break;
				case QuestState.Active:
					base.Fsm.Event(activeStateEvent);
					break;
				case QuestState.Success:
					base.Fsm.Event(successStateEvent);
					break;
				case QuestState.Failure:
					base.Fsm.Event(failureStateEvent);
					break;
				}
			}
			else
			{
				LogError(string.Format("{0}: Quest Name and Entry Number must be assigned first.", "Dialogue System"));
			}
			Finish();
		}
	}
}
