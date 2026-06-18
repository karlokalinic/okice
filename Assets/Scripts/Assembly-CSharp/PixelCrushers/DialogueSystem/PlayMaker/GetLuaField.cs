using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the value of a field in a Lua table.")]
	public class GetLuaField : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The table to get")]
		public LuaTableEnum table;

		[RequiredField]
		[Tooltip("The element in the table (e.g., 'Player' in Actor['Player'].Age)")]
		public FsmString element;

		[RequiredField]
		[Tooltip("The field in the element (e.g., 'Age' in Actor['Player'].Age)")]
		public FsmString field;

		[Tooltip("Get the localized version of the field")]
		public FsmBool getLocalizedVersion;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the field as a string")]
		public FsmString storeStringResult;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the field as a float")]
		public FsmFloat storeFloatResult;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the field as an int")]
		public FsmInt storeIntResult;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the field as a bool")]
		public FsmBool storeBoolResult;

		[Tooltip("Repeat every frame while the state is active.")]
		public bool everyFrame;

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
			if (getLocalizedVersion != null)
			{
				getLocalizedVersion.Value = false;
			}
			storeStringResult = null;
			storeFloatResult = null;
			storeIntResult = null;
			storeBoolResult = null;
		}

		public override string ErrorCheck()
		{
			if (storeStringResult == null && storeFloatResult == null && storeBoolResult == null)
			{
				return "Assign at least one store result variable.";
			}
			return base.ErrorCheck();
		}

		public override void OnEnter()
		{
			GetAndStore();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			if (everyFrame)
			{
				GetAndStore();
			}
			else
			{
				Finish();
			}
		}

		private void GetAndStore()
		{
			if (PlayMakerTools.IsValueAssigned(element) && PlayMakerTools.IsValueAssigned(field))
			{
				string text = PlayMakerTools.LuaTableName(table);
				Lua.Result result = ((getLocalizedVersion != null && getLocalizedVersion.Value) ? DialogueLua.GetLocalizedTableField(text, element.Value, field.Value) : DialogueLua.GetTableField(text, element.Value, field.Value));
				if (storeStringResult != null)
				{
					storeStringResult.Value = result.AsString;
				}
				if (storeFloatResult != null)
				{
					storeFloatResult.Value = result.AsFloat;
				}
				if (storeIntResult != null)
				{
					storeIntResult.Value = result.AsInt;
				}
				if (storeBoolResult != null)
				{
					storeBoolResult.Value = result.AsBool;
				}
			}
			else
			{
				LogWarning(string.Format("{0}: Element and Field must be assigned first.", "Dialogue System"));
			}
		}
	}
}
