using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Tells the active conversation's dialogue UI to select a response. Typically used in an FSM that responds to the OnConversationResponseMenu event.")]
	public class SelectResponse : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The index of the response to select (starting from zero).")]
		public FsmInt index;

		public override void Reset()
		{
			index = new FsmInt();
		}

		public override void OnEnter()
		{
			(DialogueManager.DialogueUI as AbstractDialogueUI).OnClick(DialogueManager.CurrentConversationState.pcResponses[index.Value]);
			Finish();
		}
	}
}
