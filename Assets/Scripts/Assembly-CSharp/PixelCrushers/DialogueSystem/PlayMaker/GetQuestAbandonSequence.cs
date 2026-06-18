using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the quest's Abandon Sequence, which is played if the quest is abandoned.")]
	public class GetQuestAbandonSequence : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the quest")]
		public FsmString questName;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a String variable")]
		public FsmString storeResult;

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
			if (PlayMakerTools.IsValueAssigned(questName))
			{
				if (storeResult != null)
				{
					storeResult.Value = QuestLog.GetQuestAbandonSequence(questName.Value);
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
