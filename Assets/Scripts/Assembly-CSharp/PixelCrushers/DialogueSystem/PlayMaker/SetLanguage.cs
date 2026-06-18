using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Set the language to use for localization.")]
	public class SetLanguage : FsmStateAction
	{
		[Tooltip("The current language to use")]
		public FsmString language;

		public override void Reset()
		{
			if (language != null)
			{
				language.Value = string.Empty;
			}
		}

		public override void OnEnter()
		{
			DialogueManager.SetLanguage((language != null) ? language.Value : string.Empty);
			Finish();
		}
	}
}
