using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the title of the most recently-ended conversation.")]
	public class GetLastConversationEnded : FsmStateAction
	{
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the conversation title in a String variable")]
		public FsmString conversationTitle;

		public override void Reset()
		{
			conversationTitle = null;
		}

		public override void OnEnter()
		{
			if (conversationTitle != null)
			{
				conversationTitle.Value = DialogueManager.lastConversationEnded;
			}
			Finish();
		}
	}
}
