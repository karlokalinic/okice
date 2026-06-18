using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Reevaluates conditions for the active conversation's response menu.")]
	public class UpdateResponses : FsmStateAction
	{
		public override void OnEnter()
		{
			DialogueManager.UpdateResponses();
			Finish();
		}
	}
}
