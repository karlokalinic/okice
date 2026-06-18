using System;

namespace PlayMaker.ConditionalExpression.Lexer
{
	public class TokenizerInvalidCharacterException : Exception
	{
		public TokenizerInvalidCharacterException(char character)
			: base($"Invalid character '{character}' encountered whilst attempting to parse expression.")
		{
		}
	}
}
