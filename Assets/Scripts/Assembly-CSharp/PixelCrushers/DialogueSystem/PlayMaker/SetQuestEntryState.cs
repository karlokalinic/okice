using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Sets the state of a quest entry in a quest.")]
	public class SetQuestEntryState : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The name of the quest")]
		public FsmString questName;

		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The quest entry number (from 1)")]
		public FsmInt entryNumber;

		[HutongGames.PlayMaker.Tooltip("The quest state (unassigned, active, success, or failure)")]
		public FsmString state;

		[HutongGames.PlayMaker.Tooltip("The quest state as a dropdown menu (used if State is blank)")]
		public QuestState stateDropdown;

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
			if (state != null)
			{
				state.Value = string.Empty;
			}
		}

		public override void OnEnter()
		{
			if (PlayMakerTools.IsValueAssigned(questName))
			{
				int num = Mathf.Max(1, entryNumber.Value);
				if (string.IsNullOrEmpty(state.Value))
				{
					QuestLog.SetQuestEntryState(questName.Value, num, stateDropdown);
				}
				else
				{
					QuestLog.SetQuestEntryState(questName.Value, num, QuestLog.StringToState(state.Value.ToLower()));
				}
			}
			else
			{
				LogError(string.Format("{0}: Quest Name is null or blank.", "Dialogue System"));
			}
			Finish();
		}
	}
}
