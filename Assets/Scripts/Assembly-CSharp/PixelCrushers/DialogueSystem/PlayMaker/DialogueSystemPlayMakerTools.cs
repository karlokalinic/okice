using System;
using System.Collections.Generic;
using System.Globalization;
using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	public static class DialogueSystemPlayMakerTools
	{
		public static void SendEventToAllFSMs(string eventName, string fsmName)
		{
			GameObject[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			for (int i = 0; i < array.Length; i++)
			{
				SendEventToFSMs(array[i].transform, eventName, fsmName);
			}
		}

		public static void SendEventToFSMs(Transform subject, string eventName, string fsmName)
		{
			if (subject == null)
			{
				return;
			}
			PlayMakerFSM[] components = subject.GetComponents<PlayMakerFSM>();
			foreach (PlayMakerFSM playMakerFSM in components)
			{
				if (string.IsNullOrEmpty(fsmName) || string.Equals(fsmName, playMakerFSM.FsmName))
				{
					playMakerFSM.SendEvent(eventName);
				}
			}
		}

		public static PlayMakerFSM GetFSM(GameObject subject, string fsmName)
		{
			if (subject != null)
			{
				PlayMakerFSM[] components = subject.GetComponents<PlayMakerFSM>();
				for (int i = 0; i < components.Length; i++)
				{
					PlayMakerFSM playMakerFSM = components[i];
					if (playMakerFSM != null && (string.IsNullOrEmpty(fsmName) || string.Equals(fsmName, playMakerFSM.FsmName)))
					{
						return components[i];
					}
				}
			}
			if (DialogueDebug.LogWarnings)
			{
				Debug.LogWarning("Dialogue System: Can't find FSM named '" + fsmName + "'.");
			}
			return null;
		}

		public static float GetFsmFloat(string name)
		{
			return FsmVariables.GlobalVariables.GetFsmFloat(name).Value;
		}

		public static int GetFsmInt(string name)
		{
			return FsmVariables.GlobalVariables.GetFsmInt(name).Value;
		}

		public static bool GetFsmBool(string name)
		{
			return FsmVariables.GlobalVariables.GetFsmBool(name).Value;
		}

		public static string GetFsmString(string name)
		{
			return FsmVariables.GlobalVariables.GetFsmString(name).Value;
		}

		public static void SetFsmFloat(string name, float value)
		{
			FsmFloat fsmFloat = FsmVariables.GlobalVariables.FindFsmFloat(name);
			if (fsmFloat == null)
			{
				Debug.LogWarning("Dialogue System: Can't find global variable named '" + name + "'.");
			}
			else
			{
				fsmFloat.Value = value;
			}
		}

		public static void AddFsmFloat(string name, float value)
		{
			SetFsmFloat(name, GetFsmFloat(name) + value);
		}

		public static void SubtractFsmFloat(string name, float value)
		{
			SetFsmFloat(name, GetFsmFloat(name) - value);
		}

		public static void SetFsmInt(string name, int value)
		{
			FsmInt fsmInt = FsmVariables.GlobalVariables.FindFsmInt(name);
			if (fsmInt == null)
			{
				Debug.LogWarning("Dialogue System: Can't find global variable named '" + name + "'.");
			}
			else
			{
				fsmInt.Value = value;
			}
		}

		public static void AddFsmInt(string name, int value)
		{
			SetFsmInt(name, GetFsmInt(name) + value);
		}

		public static void SubtractFsmInt(string name, int value)
		{
			SetFsmInt(name, GetFsmInt(name) - value);
		}

		public static void SetFsmBool(string name, bool value)
		{
			FsmBool fsmBool = FsmVariables.GlobalVariables.FindFsmBool(name);
			if (fsmBool == null)
			{
				Debug.LogWarning("Dialogue System: Can't find global variable named '" + name + "'.");
			}
			else
			{
				fsmBool.Value = value;
			}
		}

		public static void SetFsmString(string name, string value)
		{
			FsmString fsmString = FsmVariables.GlobalVariables.FindFsmString(name);
			if (fsmString == null)
			{
				Debug.LogWarning("Dialogue System: Can't find global variable named '" + name + "'.");
			}
			else
			{
				fsmString.Value = value;
			}
		}

		public static Vector3 StringToVector3(string s)
		{
			string[] array = ((!string.IsNullOrEmpty(s)) ? s.Split(':') : new string[0]);
			float x = ((array.Length >= 1) ? Tools.StringToFloat(array[0]) : 0f);
			float y = ((array.Length >= 2) ? Tools.StringToFloat(array[1]) : 0f);
			float z = ((array.Length >= 3) ? Tools.StringToFloat(array[2]) : 0f);
			return new Vector3(x, y, z);
		}

		public static string Vector3ToString(Vector3 v)
		{
			return v.x.ToString(CultureInfo.InvariantCulture) + ":" + v.y.ToString(CultureInfo.InvariantCulture) + ":" + v.z.ToString(CultureInfo.InvariantCulture);
		}

		public static Quaternion StringToQuaternion(string s)
		{
			string[] array = ((!string.IsNullOrEmpty(s)) ? s.Split(':') : new string[0]);
			float x = ((array.Length >= 1) ? Tools.StringToFloat(array[0]) : 0f);
			float y = ((array.Length >= 2) ? Tools.StringToFloat(array[1]) : 0f);
			float z = ((array.Length >= 3) ? Tools.StringToFloat(array[2]) : 0f);
			float w = ((array.Length >= 4) ? Tools.StringToFloat(array[3]) : 0f);
			return new Quaternion(x, y, z, w);
		}

		public static string QuaternionToString(Quaternion q)
		{
			return q.x.ToString(CultureInfo.InvariantCulture) + ":" + q.y.ToString(CultureInfo.InvariantCulture) + ":" + q.z.ToString(CultureInfo.InvariantCulture) + ":" + q.w.ToString(CultureInfo.InvariantCulture);
		}

		public static GameObject StringToGameObject(string s, bool searchSceneObjects = true, bool searchPrefabs = false)
		{
			return FindOrLoadGameObject(s, searchSceneObjects, searchPrefabs);
		}

		public static object[] StringToArray(string s, VariableType variableType, bool searchSceneObjects = true, bool searchPrefabs = false)
		{
			List<object> list = new List<object>();
			string[] array = ((!string.IsNullOrEmpty(s) && !string.Equals(s, "nil")) ? s.Split(new string[1] { "%;%" }, StringSplitOptions.None) : new string[0]);
			foreach (string text in array)
			{
				object item = text;
				switch (variableType)
				{
				case VariableType.Bool:
					item = Tools.StringToBool(text);
					break;
				case VariableType.Int:
					item = Tools.StringToInt(text);
					break;
				case VariableType.Float:
					item = Tools.StringToFloat(text);
					break;
				case VariableType.Vector3:
					item = StringToVector3(text);
					break;
				case VariableType.Quaternion:
					item = StringToQuaternion(text);
					break;
				case VariableType.GameObject:
					item = StringToGameObject(text);
					break;
				}
				list.Add(item);
			}
			return list.ToArray();
		}

		public static string ArrayToString(object[] values)
		{
			string text = string.Empty;
			if (values != null)
			{
				bool flag = true;
				for (int i = 0; i < values.Length; i++)
				{
					if (!flag)
					{
						text += "%;%";
					}
					flag = false;
					object obj = values[i];
					string text2 = string.Empty;
					if (obj != null)
					{
						Type type = obj.GetType();
						text2 = ((!(type == typeof(Vector3))) ? ((!(type == typeof(Quaternion))) ? ((!(type == typeof(GameObject))) ? ((!(type == typeof(float)) && !(type == typeof(double))) ? obj.ToString() : ((float)obj).ToString(CultureInfo.InvariantCulture)) : ((GameObject)obj).name) : QuaternionToString((Quaternion)obj)) : Vector3ToString((Vector3)obj));
					}
					text += text2;
				}
			}
			return text;
		}

		public static GameObject FindOrLoadGameObject(string gameObjectName, bool searchSceneObjects, bool searchPrefabs)
		{
			if (string.IsNullOrEmpty(gameObjectName) || string.Equals(gameObjectName, "null-object"))
			{
				return null;
			}
			GameObject gameObject = null;
			if (searchSceneObjects)
			{
				gameObject = Tools.GameObjectHardFind(gameObjectName);
			}
			if (gameObject != null)
			{
				return gameObject;
			}
			if (!searchPrefabs)
			{
				return null;
			}
			return DialogueManager.LoadAsset(gameObjectName, typeof(GameObject)) as GameObject;
		}

		public static int GetEntryIDFromTitle(string conversation, string entryTitle)
		{
			if (string.IsNullOrEmpty(conversation) || string.IsNullOrEmpty(entryTitle))
			{
				return -1;
			}
			Conversation conversation2 = DialogueManager.MasterDatabase.GetConversation(conversation);
			if (conversation2 == null)
			{
				return -1;
			}
			return conversation2.dialogueEntries.Find((DialogueEntry x) => string.Equals(x.Title, entryTitle))?.id ?? (-1);
		}
	}
}
