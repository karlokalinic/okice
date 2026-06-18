using System.Collections.Generic;
using HutongGames.PlayMaker;
using Language.Lua;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the names of all dialogue database elements of a specified type.")]
	public class GetAllLuaElements : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The type of elements to get")]
		public LuaElementTypeEnum elementType;

		[Tooltip("Include elements that were added at runtime, not just elements in the dialogue database. Only valid when using the Dialogue System's default Lua implementation. If you've switched to a different Lua, will get elements defined in database instead.")]
		public FsmBool allRuntimeElements = new FsmBool(true);

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[VariableType(VariableType.Array)]
		[ArrayEditor(VariableType.String, "", 0, 0, 65536)]
		[Tooltip("Store the names of the elements in this string array")]
		public FsmArray storeStringArrayResult;

		public override void Reset()
		{
			elementType = LuaElementTypeEnum.Actors;
			storeStringArrayResult = null;
		}

		public override string ErrorCheck()
		{
			if (storeStringArrayResult == null)
			{
				return "Assign at least one store result variable.";
			}
			return base.ErrorCheck();
		}

		public override void OnEnter()
		{
			GetAndStore();
			Finish();
		}

		private void GetAndStore()
		{
			if (storeStringArrayResult != null && !storeStringArrayResult.IsNone)
			{
				storeStringArrayResult.Values = GetNames();
			}
		}

		private object[] GetNames()
		{
			List<object> list = new List<object>();
			if (allRuntimeElements != null && allRuntimeElements.Value)
			{
				switch (elementType)
				{
				case LuaElementTypeEnum.Actors:
					foreach (KeyValuePair<LuaValue, LuaValue> item in (Lua.Environment.GetValue("Actor") as LuaTable).Dict)
					{
						list.Add(item.Key.ToString());
					}
					break;
				case LuaElementTypeEnum.Items:
					foreach (KeyValuePair<LuaValue, LuaValue> item2 in (Lua.Environment.GetValue("Item") as LuaTable).Dict)
					{
						if ((item2.Value as LuaTable).GetValue("Is Item").GetBooleanValue())
						{
							list.Add(item2.Key.ToString());
						}
					}
					break;
				case LuaElementTypeEnum.Quests:
					list.AddRange(QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure | QuestState.Abandoned | QuestState.Grantable));
					break;
				case LuaElementTypeEnum.Locations:
					foreach (KeyValuePair<LuaValue, LuaValue> item3 in (Lua.Environment.GetValue("Location") as LuaTable).Dict)
					{
						list.Add(item3.Key.ToString());
					}
					break;
				case LuaElementTypeEnum.Variables:
					foreach (KeyValuePair<LuaValue, LuaValue> item4 in (Lua.Environment.GetValue("Variable") as LuaTable).Dict)
					{
						list.Add(item4.Key.ToString());
					}
					break;
				}
			}
			else
			{
				switch (elementType)
				{
				case LuaElementTypeEnum.Actors:
					foreach (Actor actor in DialogueManager.masterDatabase.actors)
					{
						list.Add(actor.Name);
					}
					break;
				case LuaElementTypeEnum.Items:
					foreach (Item item5 in DialogueManager.masterDatabase.items)
					{
						if (item5.IsItem)
						{
							list.Add(item5.Name);
						}
					}
					break;
				case LuaElementTypeEnum.Quests:
					list.AddRange(QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure | QuestState.Abandoned | QuestState.Grantable));
					break;
				case LuaElementTypeEnum.Locations:
					foreach (Location location in DialogueManager.masterDatabase.locations)
					{
						list.Add(location.Name);
					}
					break;
				case LuaElementTypeEnum.Variables:
					foreach (Variable variable in DialogueManager.masterDatabase.variables)
					{
						list.Add(variable.Name);
					}
					break;
				}
			}
			return list.ToArray();
		}
	}
}
