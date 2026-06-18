using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Loads a saved game.")]
	public class LoadGame : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("The variable containing savegame data")]
		public FsmString saveData;

		public override void Reset()
		{
			saveData = null;
		}

		public override void OnEnter()
		{
			string text = ((saveData == null) ? null : saveData.Value);
			if (string.IsNullOrEmpty(text))
			{
				LogError("Saved game data is an empty string");
			}
			else if (Object.FindObjectOfType<SaveSystem>() != null)
			{
				SaveSystem.LoadGame(JsonUtility.FromJson<SavedGameData>(text));
			}
			else
			{
				LevelManager levelManager = Object.FindObjectOfType<LevelManager>();
				if (levelManager != null)
				{
					levelManager.LoadGame(text);
				}
				else
				{
					PersistentDataManager.ApplySaveData(text);
				}
			}
			Finish();
		}
	}
}
