using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Changes the Dialogue Manager's default dialogue UI.")]
	public class SetDialogueUI : FsmStateAction
	{
		public FsmGameObject dialogueUI;

		public override void OnEnter()
		{
			if (dialogueUI != null && !(dialogueUI.Value == null))
			{
				DialogueManager.UseDialogueUI(dialogueUI.Value);
				Finish();
			}
		}
	}
}
