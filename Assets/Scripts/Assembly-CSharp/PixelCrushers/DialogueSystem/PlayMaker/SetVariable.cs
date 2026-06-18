using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Sets the value of a Lua variable in the Variable[] table.")]
	public class SetVariable : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the variable")]
		public FsmString variableName;

		[Tooltip("The value of the variable as a string")]
		public FsmString stringValue = new FsmString
		{
			UseVariable = true
		};

		[Tooltip("The value of the variable as a float")]
		public FsmFloat floatValue = new FsmFloat
		{
			UseVariable = true
		};

		[Tooltip("The value of the variable as an int")]
		public FsmInt intValue = new FsmInt
		{
			UseVariable = true
		};

		[Tooltip("The value of the variable as a bool")]
		public FsmBool boolValue = new FsmBool
		{
			UseVariable = true
		};

		public override void Reset()
		{
			if (variableName != null)
			{
				variableName.Value = string.Empty;
			}
			stringValue = new FsmString
			{
				UseVariable = true
			};
			floatValue = new FsmFloat
			{
				UseVariable = true
			};
			intValue = new FsmInt
			{
				UseVariable = true
			};
			boolValue = new FsmBool
			{
				UseVariable = true
			};
		}

		public override string ErrorCheck()
		{
			if (stringValue == null && floatValue == null && boolValue == null)
			{
				return "Assign at least one value field.";
			}
			return base.ErrorCheck();
		}

		public override void OnEnter()
		{
			if (variableName == null || string.IsNullOrEmpty(variableName.Value))
			{
				LogWarning(string.Format("{0}: Variable Name isn't assigned or is blank.", "Dialogue System"));
			}
			else
			{
				if (stringValue != null && !stringValue.IsNone)
				{
					DialogueLua.SetVariable(variableName.Value, stringValue.Value);
				}
				if (floatValue != null && !floatValue.IsNone)
				{
					DialogueLua.SetVariable(variableName.Value, floatValue.Value);
				}
				if (intValue != null && !intValue.IsNone)
				{
					DialogueLua.SetVariable(variableName.Value, intValue.Value);
				}
				if (boolValue != null && !boolValue.IsNone)
				{
					DialogueLua.SetVariable(variableName.Value, boolValue.Value);
				}
			}
			Finish();
		}
	}
}
