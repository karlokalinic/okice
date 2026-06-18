using System;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[AddComponentMenu("Dialogue System/Third Party/PlayMaker/Dialogue System PlayMaker Lua")]
	public class DialogueSystemPlayMakerLua : MonoBehaviour
	{
		protected static bool areFunctionsRegistered;

		private bool didIRegisterFunctions;

		private void OnEnable()
		{
			if (areFunctionsRegistered)
			{
				didIRegisterFunctions = false;
				return;
			}
			didIRegisterFunctions = true;
			areFunctionsRegistered = true;
			Lua.RegisterFunction("FSMEvent", this, SymbolExtensions.GetMethodInfo(() => FSMEvent(string.Empty, string.Empty, string.Empty)));
			Lua.RegisterFunction("GetFsmFloat", this, SymbolExtensions.GetMethodInfo(() => GetFsmFloat(string.Empty)));
			Lua.RegisterFunction("GetFsmInt", this, SymbolExtensions.GetMethodInfo(() => GetFsmInt(string.Empty)));
			Lua.RegisterFunction("GetFsmBool", this, SymbolExtensions.GetMethodInfo(() => GetFsmBool(string.Empty)));
			Lua.RegisterFunction("GetFsmString", this, SymbolExtensions.GetMethodInfo(() => GetFsmString(string.Empty)));
			Lua.RegisterFunction("SetFsmFloat", this, SymbolExtensions.GetMethodInfo(() => SetFsmFloat(string.Empty, 0.0)));
			Lua.RegisterFunction("AddFsmFloat", this, SymbolExtensions.GetMethodInfo(() => AddFsmFloat(string.Empty, 0.0)));
			Lua.RegisterFunction("SubtractFsmFloat", this, SymbolExtensions.GetMethodInfo(() => SubtractFsmFloat(string.Empty, 0.0)));
			Lua.RegisterFunction("SetFsmInt", this, SymbolExtensions.GetMethodInfo(() => SetFsmInt(string.Empty, 0.0)));
			Lua.RegisterFunction("AddFsmInt", this, SymbolExtensions.GetMethodInfo(() => AddFsmInt(string.Empty, 0.0)));
			Lua.RegisterFunction("SubtractFsmInt", this, SymbolExtensions.GetMethodInfo(() => SubtractFsmInt(string.Empty, 0.0)));
			Lua.RegisterFunction("SetFsmBool", this, SymbolExtensions.GetMethodInfo(() => SetFsmBool(string.Empty, false)));
			Lua.RegisterFunction("SetFsmString", this, SymbolExtensions.GetMethodInfo(() => SetFsmString(string.Empty, string.Empty)));
		}

		private void OnDisable()
		{
			if (didIRegisterFunctions)
			{
				didIRegisterFunctions = false;
				areFunctionsRegistered = false;
				Lua.UnregisterFunction("FSMEvent");
				Lua.UnregisterFunction("GetFsmFloat");
				Lua.UnregisterFunction("GetFsmInt");
				Lua.UnregisterFunction("GetFsmBool");
				Lua.UnregisterFunction("GetFsmString");
				Lua.UnregisterFunction("SetFsmFloat");
				Lua.UnregisterFunction("SetFsmInt");
				Lua.UnregisterFunction("SetFsmBool");
				Lua.UnregisterFunction("SetFsmString");
			}
		}

		public void FSMEvent(string eventName, string objectName, string fsmName)
		{
			bool num = string.Equals(objectName, "all", StringComparison.OrdinalIgnoreCase);
			GameObject gameObject = (num ? null : GameObject.Find(objectName));
			if (num)
			{
				DialogueSystemPlayMakerTools.SendEventToAllFSMs(eventName, fsmName);
			}
			else if (gameObject != null)
			{
				DialogueSystemPlayMakerTools.SendEventToFSMs(gameObject.transform, eventName, fsmName);
			}
		}

		public double GetFsmFloat(string name)
		{
			return DialogueSystemPlayMakerTools.GetFsmFloat(name);
		}

		public double GetFsmInt(string name)
		{
			return DialogueSystemPlayMakerTools.GetFsmInt(name);
		}

		public bool GetFsmBool(string name)
		{
			return DialogueSystemPlayMakerTools.GetFsmBool(name);
		}

		public string GetFsmString(string name)
		{
			return DialogueSystemPlayMakerTools.GetFsmString(name);
		}

		public void SetFsmFloat(string name, double value)
		{
			DialogueSystemPlayMakerTools.SetFsmFloat(name, (float)value);
		}

		public void AddFsmFloat(string name, double value)
		{
			DialogueSystemPlayMakerTools.AddFsmFloat(name, (float)value);
		}

		public void SubtractFsmFloat(string name, double value)
		{
			DialogueSystemPlayMakerTools.SubtractFsmFloat(name, (float)value);
		}

		public void SetFsmInt(string name, double value)
		{
			DialogueSystemPlayMakerTools.SetFsmInt(name, (int)value);
		}

		public void AddFsmInt(string name, double value)
		{
			DialogueSystemPlayMakerTools.AddFsmInt(name, (int)value);
		}

		public void SubtractFsmInt(string name, double value)
		{
			DialogueSystemPlayMakerTools.SubtractFsmInt(name, (int)value);
		}

		public void SetFsmBool(string name, bool value)
		{
			DialogueSystemPlayMakerTools.SetFsmBool(name, value);
		}

		public void SetFsmString(string name, string value)
		{
			DialogueSystemPlayMakerTools.SetFsmString(name, value);
		}
	}
}
