using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the type of a Dialogue System Lua variable.")]
	public class GetVariableType : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of the variable")]
		public FsmString variableName;

		public FsmEvent stringTypeEvent;

		public FsmEvent boolTypeEvent;

		public FsmEvent numberTypeEvent;

		public FsmEvent otherTypeEvent;

		public override void Reset()
		{
			if (variableName != null)
			{
				variableName.Value = string.Empty;
			}
		}

		public override void OnEnter()
		{
			if (variableName == null || string.IsNullOrEmpty(variableName.Value))
			{
				LogError(string.Format("{0}: Variable Name is null or blank.", "Dialogue System"));
			}
			else
			{
				Lua.Result variable = DialogueLua.GetVariable(variableName.Value);
				if (variable.isBool)
				{
					base.Fsm.Event(boolTypeEvent);
				}
				else if (variable.isNumber)
				{
					base.Fsm.Event(numberTypeEvent);
				}
				else if (variable.isString)
				{
					base.Fsm.Event(stringTypeEvent);
				}
				else
				{
					base.Fsm.Event(otherTypeEvent);
				}
			}
			Finish();
		}
	}
}
