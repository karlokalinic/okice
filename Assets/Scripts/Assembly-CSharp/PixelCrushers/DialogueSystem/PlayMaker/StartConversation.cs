using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Starts a conversation.")]
	public class StartConversation : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The conversation to start")]
		public FsmString conversation;

		[HutongGames.PlayMaker.Tooltip("The starting dialogue entry ID. Leave at -1 to start at the beginning")]
		public FsmInt startingEntryID;

		[HutongGames.PlayMaker.Tooltip("The starting dialogue entry Title. If set, takes precedence over Starting Entry ID")]
		public FsmString startingEntryTitle;

		[HutongGames.PlayMaker.Tooltip("The primary participant in the conversation (e.g., the player)")]
		public FsmGameObject actor;

		[HutongGames.PlayMaker.Tooltip("The other participant in the conversation (e.g., the NPC)")]
		public FsmGameObject conversant;

		[HutongGames.PlayMaker.Tooltip("Do not start this conversation if a conversation is already active")]
		public FsmBool exclusive;

		[HutongGames.PlayMaker.Tooltip("Stop any active conversations when starting this conversation")]
		public FsmBool replace;

		public override void Reset()
		{
			if (conversation != null)
			{
				conversation.Value = string.Empty;
			}
			startingEntryID = new FsmInt();
			startingEntryID.Value = -1;
			startingEntryTitle = new FsmString();
			if (actor != null)
			{
				actor.Value = null;
			}
			if (conversant != null)
			{
				conversant.Value = null;
			}
		}

		public override void OnEnter()
		{
			bool isConversationActive = DialogueManager.isConversationActive;
			if (!(exclusive.Value && isConversationActive))
			{
				string obj = ((conversation != null) ? conversation.Value : string.Empty);
				Transform transform = ((actor != null && actor.Value != null) ? actor.Value.transform : null);
				Transform transform2 = ((conversant != null && conversant.Value != null) ? conversant.Value.transform : null);
				if (transform == null)
				{
					LogWarning(string.Format("{0}: PlayMaker Action Start Conversation - actor is null", "Dialogue System"));
				}
				if (string.IsNullOrEmpty(obj))
				{
					LogWarning(string.Format("{0}: PlayMaker Action Start Conversation - conversation title is blank", "Dialogue System"));
				}
				int initialDialogueEntryID = ((!string.IsNullOrEmpty(startingEntryTitle.Value)) ? DialogueSystemPlayMakerTools.GetEntryIDFromTitle(conversation.Value, startingEntryTitle.Value) : ((startingEntryID != null) ? startingEntryID.Value : (-1)));
				if (replace.Value && isConversationActive)
				{
					DialogueManager.StopAllConversations();
				}
				DialogueManager.StartConversation(obj, transform, transform2, initialDialogueEntryID);
			}
			Finish();
		}
	}
}
