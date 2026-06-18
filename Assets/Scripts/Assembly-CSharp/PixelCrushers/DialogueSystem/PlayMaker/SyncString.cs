using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Syncs a string variable between PlayMaker and the Dialogue System's Variable[] Lua table.")]
	public class SyncString : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the variable in the Dialogue System")]
		public FsmString dialogueSystemVariable;

		[UIHint(UIHint.Variable)]
		[Tooltip("The PlayMaker variable")]
		public FsmString playMakerVariable;

		[RequiredField]
		[Tooltip("If ticked, copy PlayMaker value to Dialogue System; if unticked, copy Dialogue System value to PlayMaker")]
		public FsmBool toDialogueSystem;

		[Tooltip("Repeat every frame while the state is active")]
		public bool everyFrame;

		public override void Reset()
		{
			if (dialogueSystemVariable != null)
			{
				dialogueSystemVariable.Value = string.Empty;
			}
			playMakerVariable = new FsmString
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
				LogWarning("Dialogue System: PlayMaker Variable isn't assigned.");
			}
			else if (toDialogueSystem != null && toDialogueSystem.Value)
			{
				DialogueLua.SetVariable(dialogueSystemVariable.Value, playMakerVariable.Value);
			}
			else
			{
				playMakerVariable.Value = DialogueLua.GetVariable(dialogueSystemVariable.Value).AsString;
			}
		}
	}
}
