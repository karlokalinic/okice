using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Resets the Dialogue Manager's master database.")]
	public class ResetDialogueDatabase : FsmStateAction
	{
		[Tooltip("Tick to reset to the default dialogue database, clear to keep all loaded databases")]
		public FsmBool resetToInitialDatabase;

		public override void Reset()
		{
			if (resetToInitialDatabase != null)
			{
				resetToInitialDatabase.Value = false;
			}
		}

		public override void OnEnter()
		{
			if (DialogueManager.instance == null)
			{
				LogError("Dialogue System: Can't reset dialogue database because there is no Dialogue Manager in the scene.");
			}
			else
			{
				DialogueManager.ResetDatabase(resetToInitialDatabase.Value ? DatabaseResetOptions.RevertToDefault : DatabaseResetOptions.KeepAllLoaded);
			}
			Finish();
		}
	}
}
