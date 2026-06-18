using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Jumps the active conversation to a specific entry.")]
	public class JumpToEntry : FsmStateAction
	{
		[Tooltip("Conversation containing the dialogue entry. Leave blank to use active conversation. Must select conversation if you want to specify entry by Title from a dropdown menu. (This is the conversation originally started; not linked conversation if it crossed links.)")]
		public FsmString conversationTitle;

		[Tooltip("The dialogue entry ID to jump to.")]
		public FsmInt entryID = new FsmInt();

		[Tooltip("The dialogue entry Title to jump to. If set, takes precedence over Entry ID.")]
		public FsmString entryTitle = new FsmString();

		public override void Reset()
		{
			conversationTitle = new FsmString();
			entryID = new FsmInt();
			entryID.Value = -1;
			entryTitle = new FsmString();
			entryTitle.Value = string.Empty;
		}

		public override void OnEnter()
		{
			if (!DialogueManager.isConversationActive)
			{
				LogError("Can't jump to entry " + entryID.Value + ". No conversation is active.");
			}
			else
			{
				int dialogueEntryID = ((!string.IsNullOrEmpty(entryTitle.Value)) ? DialogueSystemPlayMakerTools.GetEntryIDFromTitle(conversationTitle.Value, entryTitle.Value) : ((entryID != null) ? entryID.Value : (-1)));
				DialogueEntry dialogueEntry = ((conversationTitle != null && !string.IsNullOrEmpty(conversationTitle.Value)) ? DialogueManager.masterDatabase.GetConversation(conversationTitle.Value) : DialogueManager.masterDatabase.GetConversation(DialogueManager.lastConversationID))?.GetDialogueEntry(dialogueEntryID);
				if (dialogueEntry == null)
				{
					LogError("Can't find entry  " + entryID.Value + " to jump to it.");
				}
				else
				{
					ConversationState state = DialogueManager.conversationModel.GetState(dialogueEntry);
					DialogueManager.conversationController.GotoState(state);
				}
			}
			Finish();
		}
	}
}
