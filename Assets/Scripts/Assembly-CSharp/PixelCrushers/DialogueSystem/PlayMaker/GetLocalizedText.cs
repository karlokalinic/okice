using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the value of a text table field.")]
	public class GetLocalizedText : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The text table")]
		public TextTable textTable;

		[RequiredField]
		[Tooltip("The field in the table")]
		public FsmString field;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a String variable")]
		public FsmString storeResult;

		public override void Reset()
		{
			textTable = null;
			if (field != null)
			{
				field.Value = string.Empty;
			}
			storeResult = null;
		}

		public override void OnEnter()
		{
			if (this.textTable == null && DialogueManager.DisplaySettings.localizationSettings.textTable == null)
			{
				LogError(string.Format("{0}: Text table is null. Assign one to this action or the Dialogue Manager.", "Dialogue System"));
			}
			else if (field == null || string.IsNullOrEmpty(field.Value))
			{
				LogError(string.Format("{0}: Field is null or blank.", "Dialogue System"));
			}
			else
			{
				TextTable textTable = this.textTable ?? DialogueManager.DisplaySettings.localizationSettings.textTable;
				if (!textTable.HasField(field.Value))
				{
					object[] args = new string[3] { "Dialogue System", textTable.name, field.Value };
					LogError(string.Format("{0}: Text table {1} does not contain a field '{2}'. (Field must match exactly, including case.)", args));
				}
				else if (storeResult != null)
				{
					storeResult.Value = textTable.GetFieldTextForLanguage(field.Value, Localization.language);
				}
			}
			Finish();
		}
	}
}
