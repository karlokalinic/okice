using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Syncs a GameObject variable between PlayMaker and the Dialogue System's Variable[] Lua table.")]
	public class SyncGameObject : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the variable in the Dialogue System")]
		public FsmString dialogueSystemVariable;

		[UIHint(UIHint.Variable)]
		[Tooltip("The PlayMaker variable")]
		public FsmGameObject playMakerVariable;

		[RequiredField]
		[Tooltip("If ticked, copy PlayMaker value to Dialogue System; if unticked, copy Dialogue System value to PlayMaker")]
		public FsmBool toDialogueSystem;

		[RequiredField]
		[Tooltip("If syncing FROM Dialogue System, search scene for GameObject matching name stored in Dialogue System value")]
		public FsmBool searchSceneObjects = new FsmBool(true);

		[RequiredField]
		[Tooltip("If syncing FROM Dialogue System, search Resources and AssetBundles for prefab matching name stored in Dialogue System value")]
		public FsmBool searchPrefabs = new FsmBool(false);

		[Tooltip("Repeat every frame while the state is active")]
		public bool everyFrame;

		public override void Reset()
		{
			if (dialogueSystemVariable != null)
			{
				dialogueSystemVariable.Value = string.Empty;
			}
			playMakerVariable = new FsmGameObject
			{
				UseVariable = true
			};
			toDialogueSystem = null;
			everyFrame = false;
		}

		public override string ErrorCheck()
		{
			if (dialogueSystemVariable == null || playMakerVariable == null)
			{
				return "Assign Dialogue System and PlayMaker variables.";
			}
			return base.ErrorCheck();
		}

		public override void OnEnter()
		{
			Sync();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (everyFrame)
			{
				Sync();
			}
			else
			{
				Finish();
			}
		}

		private void Sync()
		{
			if (dialogueSystemVariable == null || string.IsNullOrEmpty(dialogueSystemVariable.Value))
			{
				LogWarning("Dialogue System: Dialogue System Variable isn't assigned or is blank.");
			}
			else if (playMakerVariable == null)
			{
				LogWarning("Dialogue System: PlayMaker Variable isn't assigned or is blank.");
			}
			else if (toDialogueSystem != null && toDialogueSystem.Value)
			{
				string value = ((playMakerVariable.Value != null) ? playMakerVariable.Value.name : "null-object");
				DialogueLua.SetVariable(dialogueSystemVariable.Value, value);
			}
			else
			{
				string asString = DialogueLua.GetVariable(dialogueSystemVariable.Value).AsString;
				playMakerVariable.Value = DialogueSystemPlayMakerTools.FindOrLoadGameObject(asString, searchSceneObjects.Value, searchPrefabs.Value);
			}
		}
	}
}
