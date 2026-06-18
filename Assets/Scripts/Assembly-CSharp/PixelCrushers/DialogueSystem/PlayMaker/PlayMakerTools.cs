using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	public static class PlayMakerTools
	{
		public static bool IsValueAssigned(FsmString fsmString)
		{
			if (fsmString != null && !fsmString.IsNone)
			{
				return !string.IsNullOrEmpty(fsmString.Value);
			}
			return false;
		}

		public static bool IsValueAssigned(FsmInt fsmInt)
		{
			if (fsmInt != null)
			{
				return !fsmInt.IsNone;
			}
			return false;
		}

		public static string LuaTableName(LuaTableEnum table)
		{
			return table switch
			{
				LuaTableEnum.ActorTable => "Actor", 
				LuaTableEnum.ItemTable => "Item", 
				LuaTableEnum.LocationTable => "Location", 
				_ => null, 
			};
		}
	}
}
