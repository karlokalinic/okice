using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the number of quests in a specified state.")]
	public class GetQuestCount : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The quest state (unassigned, active, success, or failure)")]
		public FsmString state;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in an Int variable")]
		public FsmInt storeResult;

		public override void Reset()
		{
			if (state != null)
			{
				state.Value = string.Empty;
			}
			storeResult = null;
		}

		public override void OnEnter()
		{
			if (PlayMakerTools.IsValueAssigned(state))
			{
				string[] allQuests = QuestLog.GetAllQuests(QuestLog.StringToState(state.Value));
				if (storeResult != null)
				{
					storeResult.Value = ((allQuests != null) ? allQuests.Length : 0);
				}
			}
			else
			{
				LogError(string.Format("{0}: State must be assigned first.", "Dialogue System"));
			}
			Finish();
		}
	}
}
