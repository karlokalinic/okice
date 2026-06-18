using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Unpauses the Dialogue System.")]
	public class Unpause : FsmStateAction
	{
		public override void OnEnter()
		{
			DialogueManager.Unpause();
			Finish();
		}
	}
}
