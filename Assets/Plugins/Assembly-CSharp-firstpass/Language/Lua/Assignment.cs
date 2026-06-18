using System;
using System.Collections.Generic;
using UnityEngine;

namespace Language.Lua
{
	public class Assignment : Statement
	{
		public static HashSet<string> MonitoredLocalVariables = new HashSet<string>();

		public static Action<string, object> LocalVariableChanged = null;

		public static HashSet<string> MonitoredVariables = new HashSet<string>();

		public static Action<string, object> VariableChanged = null;

		private static LuaValue VariableTableToMonitor = null;

		public List<Var> VarList = new List<Var>();

		public List<Expr> ExprList = new List<Expr>();

		public static void InitializeVariableMonitoring()
		{
			MonitoredLocalVariables = new HashSet<string>();
			LocalVariableChanged = null;
			MonitoredVariables = new HashSet<string>();
			VariableChanged = null;
			VariableTableToMonitor = null;
		}

		public static void InvokeVariableChanged(string variable, object value)
		{
			VariableChanged?.Invoke(variable, value);
		}

		private static bool AreValuesEqual(object obj1, object obj2)
		{
			if (obj1 == null && obj2 == null)
			{
				return true;
			}
			if (obj1 == null || obj2 == null)
			{
				return false;
			}
			Type type = obj1.GetType();
			Type type2 = obj2.GetType();
			if (type != type2)
			{
				return false;
			}
			if (type == typeof(bool))
			{
				return (bool)obj1 == (bool)obj2;
			}
			if (type == typeof(double))
			{
				return Mathf.Approximately(0f, (float)((double)obj1 - (double)obj2));
			}
			if (type == typeof(string))
			{
				return string.Equals(obj1.ToString(), obj2.ToString());
			}
			return obj1 == obj2;
		}

		public override LuaValue Execute(LuaTable environment, out bool isBreak)
		{
			LuaValue[] array = LuaInterpreterExtensions.EvaluateAll(ExprList, environment).ToArray();
			LuaValue[] array2 = LuaMultiValue.UnWrapLuaValues(array);
			for (int i = 0; i < Math.Min(VarList.Count, array2.Length); i++)
			{
				Var var = VarList[i];
				if (var.Accesses.Count == 0)
				{
					if (!(var.Base is VarName varName))
					{
						continue;
					}
					SetKeyValue(environment, new LuaString(varName.Name), array[i]);
					if (varName.Name == "Variable")
					{
						VariableTableToMonitor = array[0];
					}
					if (MonitoredLocalVariables.Contains(varName.Name) && array.Length >= 1)
					{
						object value = array[0].Value;
						try
						{
							LocalVariableChanged?.Invoke(varName.Name, value);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
					continue;
				}
				LuaValue luaValue = var.Base.Evaluate(environment);
				for (int j = 0; j < var.Accesses.Count - 1; j++)
				{
					luaValue = var.Accesses[j].Evaluate(luaValue, environment);
				}
				Access access = var.Accesses[var.Accesses.Count - 1];
				if (access is NameAccess nameAccess)
				{
					if (luaValue == null || luaValue is LuaNil)
					{
						throw new NullReferenceException("Cannot assign to a null value. Are you trying to assign to a nonexistent table element?.");
					}
					SetKeyValue(luaValue, new LuaString(nameAccess.Name), array[i]);
				}
				else
				{
					KeyAccess keyAccess = access as KeyAccess;
					if (access != null)
					{
						SetKeyValue(luaValue, keyAccess.Key.Evaluate(environment), array[i]);
					}
				}
			}
			isBreak = false;
			return null;
		}

		private static void SetKeyValue(LuaValue baseValue, LuaValue key, LuaValue value)
		{
			LuaValue luaValue = LuaNil.Nil;
			if (baseValue is LuaTable luaTable)
			{
				bool flag = baseValue == VariableTableToMonitor && key != null && MonitoredVariables.Contains(key.ToString());
				object obj = null;
				if (flag)
				{
					LuaValue value2 = luaTable.GetValue(key);
					if (value2 != null)
					{
						obj = value2.Value;
					}
				}
				try
				{
					if (luaTable.ContainsKey(key))
					{
						luaTable.SetKeyValue(key, value);
						return;
					}
					if (luaTable.MetaTable != null)
					{
						luaValue = luaTable.MetaTable.GetValue("__newindex");
					}
					if (luaValue == LuaNil.Nil)
					{
						luaTable.SetKeyValue(key, value);
						return;
					}
				}
				finally
				{
					if (baseValue == VariableTableToMonitor && key != null && value != null && flag && !AreValuesEqual(value.Value, obj))
					{
						try
						{
							VariableChanged?.Invoke(key.ToString(), value.Value);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
			}
			else if (baseValue is LuaUserdata luaUserdata)
			{
				if (luaUserdata.MetaTable != null)
				{
					luaValue = luaUserdata.MetaTable.GetValue("__newindex");
				}
				if (luaValue == LuaNil.Nil)
				{
					throw new Exception("Assign field of userdata without __newindex defined.");
				}
			}
			if (luaValue is LuaFunction luaFunction)
			{
				luaFunction.Invoke(new LuaValue[3] { baseValue, key, value });
			}
			else
			{
				SetKeyValue(luaValue, key, value);
			}
		}
	}
}
