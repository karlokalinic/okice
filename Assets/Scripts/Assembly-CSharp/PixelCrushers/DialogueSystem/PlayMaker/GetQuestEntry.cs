using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Gets the description of a quest entry in a quest.")]
	public class GetQuestEntry : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The name of the quest")]
		public FsmString questName;

		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The quest entry number (from 1)")]
		public FsmInt entryNumber;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("Store the result in a String variable")]
		public FsmString storeResult;

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
			if (questName == null || string.IsNullOrEmpty(questName.Value))
			{
				LogError(string.Format("{0}: Quest Name is null or blank.", "Dialogue System"));
			}
			else if (entryNumber == null)
			{
				LogError(string.Format("{0}: Entry Number is not assigned.", "Dialogue System"));
			}
			else if (storeResult != null)
			{
				storeResult.Value = QuestLog.GetQuestEntry(questName.Value, Mathf.Max(1, entryNumber.Value));
			}
			Finish();
		}
	}
}
