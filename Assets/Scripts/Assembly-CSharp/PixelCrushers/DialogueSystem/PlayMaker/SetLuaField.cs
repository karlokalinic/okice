using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Sets the value of a field in an element of a Lua table..")]
	public class SetLuaField : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The table to set")]
		public LuaTableEnum table;

		[RequiredField]
		[Tooltip("The element in the table (e.g., 'Player' in Actor['Player'].Age)")]
		public FsmString element;

		[RequiredField]
		[Tooltip("The field in the element (e.g., 'Age' in Actor['Player'].Age)")]
		public FsmString field;

		[Tooltip("The value of the field as a string")]
		public FsmString stringValue = new FsmString
		{
			UseVariable = true
		};

		[Tooltip("The value of the field as a float")]
		public FsmFloat floatValue = new FsmFloat
		{
			UseVariable = true
		};

		[Tooltip("The value of the field as an int")]
		public FsmInt intValue = new FsmInt
		{
			UseVariable = true
		};

		[Tooltip("The value of the field as a bool")]
		public FsmBool boolValue = new FsmBool
		{
			UseVariable = true
		};

		public override void Reset()
		{
			table = LuaTableEnum.ItemTable;
			if (element != null)
			{
				element.Value = string.Empty;
			}
			if (field != null)
			{
				field.Value = string.Empty;
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
			if (PlayMakerTools.IsValueAssigned(element) && PlayMakerTools.IsValueAssigned(field))
			{
				string text = PlayMakerTools.LuaTableName(table);
				if (stringValue != null && !stringValue.IsNone)
				{
					DialogueLua.SetTableField(text, element.Value, field.Value, stringValue.Value);
				}
				if (floatValue != null && !floatValue.IsNone)
				{
					DialogueLua.SetTableField(text, element.Value, field.Value, floatValue.Value);
				}
				if (intValue != null && !intValue.IsNone)
				{
					DialogueLua.SetTableField(text, element.Value, field.Value, intValue.Value);
				}
				if (boolValue != null && !boolValue.IsNone)
				{
					DialogueLua.SetTableField(text, element.Value, field.Value, boolValue.Value);
				}
			}
			else
			{
				LogWarning(string.Format("{0}: Element and Field must be assigned first.", "Dialogue System"));
			}
			Finish();
		}
	}
}
