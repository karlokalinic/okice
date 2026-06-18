using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Stores savegame data in a string variable.")]
	public class RecordSavegameData : FsmStateAction
	{
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("Store the result in a String variable")]
		public FsmString storeResult;

		public override void Reset()
		{
			storeResult = null;
		}

		public override void OnEnter()
		{
			if (storeResult != null)
			{
				if (Object.FindObjectOfType<SaveSystem>() != null)
				{
					storeResult.Value = JsonUtility.ToJson(SaveSystem.RecordSavedGameData());
				}
				else
				{
					storeResult.Value = PersistentDataManager.GetSaveData();
				}
			}
			Finish();
		}
	}
}
