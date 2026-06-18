using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PixelCrushers
{
	public static class ScriptableObjectUtility
	{
		public static bool reportNullElementsInLists { get; set; }

		public static T CreateScriptableObject<T>() where T : ScriptableObject
		{
			T val = ScriptableObject.CreateInstance<T>();
			InitializeScriptableObject(val);
			return val;
		}

		public static ScriptableObject CreateScriptableObject(Type type)
		{
			ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);
			InitializeScriptableObject(scriptableObject);
			return scriptableObject;
		}

		public static void InitializeScriptableObject(ScriptableObject scriptableObject)
		{
			if (!(scriptableObject == null))
			{
				MethodInfo method = scriptableObject.GetType().GetMethod("Initialize");
				if (method != null)
				{
					method.Invoke(scriptableObject, null);
				}
			}
		}

		public static List<T> CloneList<T>(List<T> original, UnityEngine.Object source = null) where T : ScriptableObject
		{
			List<T> list = new List<T>();
			if (original != null)
			{
				for (int i = 0; i < original.Count; i++)
				{
					if (original[i] != null && (object)original[i] != null)
					{
						list.Add(UnityEngine.Object.Instantiate(original[i]));
					}
					else if (Debug.isDebugBuild && reportNullElementsInLists)
					{
						if (source != null)
						{
							Debug.LogWarning("CloneList<" + typeof(T).Name + ">: Element " + i + " is null in a list in " + source?.ToString() + ".", source);
						}
						else
						{
							Debug.LogWarning("CloneList<" + typeof(T).Name + ">: Element " + i + " is null.");
						}
					}
				}
			}
			return list;
		}
	}
}
