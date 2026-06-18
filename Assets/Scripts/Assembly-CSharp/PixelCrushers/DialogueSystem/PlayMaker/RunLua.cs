using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Runs Lua code.")]
	public class RunLua : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The Lua code to run")]
		[HideInInspector]
		public FsmString luaCode;

		[HutongGames.PlayMaker.Tooltip("Tick to log Lua debug output to the console")]
		public FsmBool debug;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("Store the result in a variable")]
		public FsmVar storeResult;

		public override void Reset()
		{
			if (luaCode != null)
			{
				luaCode.Value = string.Empty;
			}
			if (debug != null)
			{
				debug.Value = false;
			}
			storeResult = null;
		}

		public override void OnEnter()
		{
			string obj = ((luaCode != null) ? luaCode.Value : string.Empty);
			bool flag = debug != null && debug.Value;
			Lua.Result result = Lua.Run(obj, flag);
			if (storeResult != null && storeResult.useVariable)
			{
				switch (storeResult.Type)
				{
				case VariableType.Bool:
					storeResult.SetValue(result.AsBool);
					break;
				case VariableType.Float:
					storeResult.SetValue(result.AsFloat);
					break;
				case VariableType.Int:
					storeResult.SetValue(result.AsInt);
					break;
				case VariableType.String:
					storeResult.SetValue(result.AsString);
					break;
				default:
					if (DialogueDebug.LogWarnings)
					{
						Debug.LogWarning(string.Format("{0}: Variable type must be Bool, Float, Int, or String for Lua code '{1}'", "Dialogue System", luaCode));
					}
					break;
				}
			}
			Finish();
		}
	}
}
