using System;

namespace PlayMaker.ConditionalExpression
{
	public class VariableNotFoundException : Exception
	{
		public VariableNotFoundException(string variableName)
			: base($"Variable was not found '{variableName}'.")
		{
		}
	}
}
