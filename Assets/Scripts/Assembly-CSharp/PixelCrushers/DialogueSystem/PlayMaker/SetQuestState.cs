using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Sets the state of a quest.")]
	public class SetQuestState : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the quest")]
		public FsmString questName;

		[Tooltip("The quest state (unassigned, grantable, active, returnToNPC, success, or failure)")]
		public FsmString state;

		[Tooltip("The quest state as a dropdown menu (used if State is blank)")]
		public QuestState stateDropdown;

		public override void Reset()
		{
			if (questName != null)
			{
				questName.Value = string.Empty;
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
				if (string.IsNullOrEmpty(state.Value))
				{
					QuestLog.SetQuestState(questName.Value, stateDropdown);
				}
				else
				{
					QuestLog.SetQuestState(questName.Value, QuestLog.StringToState(state.Value.ToLower()));
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
