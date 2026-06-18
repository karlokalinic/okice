using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Applies savegame data stored in a string variable.")]
	public class ApplySavegameData : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("The variable containing savegame data")]
		public FsmString savegameData;

		[HutongGames.PlayMaker.Tooltip("Tick to reset to the default dialogue database, clear to keep all loaded databases")]
		public FsmBool resetToInitialDatabase;

		public override void Reset()
		{
			savegameData = null;
			if (resetToInitialDatabase != null)
			{
				resetToInitialDatabase.Value = false;
			}
		}

		public override void OnEnter()
		{
			if (savegameData != null)
			{
				if (Object.FindObjectOfType<SaveSystem>() != null)
				{
					SaveSystem.LoadGame(JsonUtility.FromJson<SavedGameData>(savegameData.Value));
				}
				else
				{
					DatabaseResetOptions databaseResetOptions = (resetToInitialDatabase.Value ? DatabaseResetOptions.RevertToDefault : DatabaseResetOptions.KeepAllLoaded);
					PersistentDataManager.ApplySaveData(savegameData.Value, databaseResetOptions);
				}
			}
			Finish();
		}
	}
}
