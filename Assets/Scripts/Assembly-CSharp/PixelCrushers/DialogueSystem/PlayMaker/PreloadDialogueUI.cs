using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Preloads the Dialogue Manager's dialogue UI, which is normally loaded just before the first conversation/bark/alert.")]
	public class PreloadDialogueUI : FsmStateAction
	{
		public override void OnEnter()
		{
			if (DialogueManager.DialogueUI == null && DialogueDebug.LogWarnings)
			{
				Debug.LogWarning(string.Format("{0}: Unable to load the dialogue UI.", "Dialogue System"));
			}
			Finish();
		}
	}
}
