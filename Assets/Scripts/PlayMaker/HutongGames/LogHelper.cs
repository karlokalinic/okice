using System.Diagnostics;
using UnityEngine;

namespace HutongGames
{
	public static class LogHelper
	{
		[Conditional("DEBUG_LOG")]
		public static void LogWarning(object prefix, object message)
		{
			UnityEngine.Debug.LogWarning(prefix?.ToString() + ": " + message);
		}

		[Conditional("DEBUG_LOG")]
		public static void Log(object message, LogColor logColor = LogColor.None)
		{
			UnityEngine.Debug.Log(FormatLog(message, logColor));
		}

		[Conditional("DEBUG_LOG")]
		public static void Log(object prefix, object message, LogColor logColor = LogColor.None)
		{
			UnityEngine.Debug.Log(prefix?.ToString() + ": " + FormatLog(message, logColor));
		}

		[Conditional("DEBUG_LOG")]
		public static void Log(object prefix, object message, object postfix, LogColor logColor = LogColor.None)
		{
			UnityEngine.Debug.Log(prefix?.ToString() + ": " + FormatLog(message, logColor)?.ToString() + " \t" + postfix);
		}

		private static object FormatLog(object message, LogColor logColor)
		{
			return logColor switch
			{
				LogColor.Green => "<color=green>" + message?.ToString() + "</color>", 
				LogColor.Yellow => "<color=yellow>" + message?.ToString() + "</color>", 
				LogColor.Red => "<color=red>" + message?.ToString() + "</color>", 
				_ => message, 
			};
		}
	}
}
