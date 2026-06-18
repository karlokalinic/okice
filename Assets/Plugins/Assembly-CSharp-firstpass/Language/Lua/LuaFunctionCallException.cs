using System;

namespace Language.Lua
{
	public class LuaFunctionCallException : Exception
	{
		public LuaFunctionCallException(string message)
			: base(message)
		{
		}
	}
}
