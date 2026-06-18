using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the value of a Lua variable from the Variable[] table.")]
	public class GetVariable : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the variable")]
		public FsmString variableName;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the variable as a string")]
		public FsmString storeStringResult;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the variable as a float")]
		public FsmFloat storeFloatResult;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the variable as an int")]
		public FsmInt storeIntResult;

		[UIHint(UIHint.Variable)]
		[Tooltip("The value of the variable as a bool")]
		public FsmBool storeBoolResult;

		[Tooltip("Repeat every frame while the state is active")]
		public bool everyFrame;

		public override void Reset()
		{
			if (variableName != null)
			{
				variableName.Value = string.Empty;
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
			if (variableName == null || string.IsNullOrEmpty(variableName.Value))
			{
				LogWarning(string.Format("{0}: Variable Name isn't assigned or is blank.", "Dialogue System"));
				return;
			}
			Lua.Result variable = DialogueLua.GetVariable(variableName.Value);
			if (storeStringResult != null)
			{
				storeStringResult.Value = variable.AsString;
			}
			if (storeFloatResult != null)
			{
				storeFloatResult.Value = variable.AsFloat;
			}
			if (storeIntResult != null)
			{
				storeIntResult.Value = variable.AsInt;
			}
			if (storeBoolResult != null)
			{
				storeBoolResult.Value = variable.AsBool;
			}
		}
	}
}
