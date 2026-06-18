using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker
{
	public class ActionReport
	{
		public static readonly List<ActionReport> ActionReportList = new List<ActionReport>();

		public static int InfoCount;

		public static int ErrorCount;

		public Fsm fsm;

		public FsmState state;

		public FsmStateAction action;

		public int actionIndex;

		public string logText;

		public bool isError;

		public string parameter;

		public static void Start()
		{
			ActionReportList.Clear();
			InfoCount = 0;
			ErrorCount = 0;
		}

		public static ActionReport Log(Fsm fsm, FsmState state, FsmStateAction action, int actionIndex, string parameter, string logLine, bool isError = false)
		{
			if (!PlayMakerGlobals.IsEditor)
			{
				return null;
			}
			if (fsm == null)
			{
				Debug.LogWarning("Cannot log report: Fsm == null!");
				return null;
			}
			ActionReport actionReport = new ActionReport
			{
				fsm = fsm,
				state = state,
				action = action,
				actionIndex = actionIndex,
				parameter = parameter,
				logText = logLine,
				isError = isError
			};
			if (!ActionReportContains(actionReport))
			{
				ActionReportList.Add(actionReport);
				InfoCount++;
				return actionReport;
			}
			return null;
		}

		private static bool ActionReportContains(ActionReport report)
		{
			foreach (ActionReport actionReport in ActionReportList)
			{
				if (actionReport.SameAs(report))
				{
					return true;
				}
			}
			return false;
		}

		private bool SameAs(ActionReport actionReport)
		{
			if (actionReport.fsm == fsm && actionReport.state == state && actionReport.actionIndex == actionIndex && actionReport.logText == logText && actionReport.isError == isError)
			{
				return actionReport.parameter == parameter;
			}
			return false;
		}

		public static void LogWarning(Fsm fsm, FsmState state, FsmStateAction action, int actionIndex, string parameter, string logLine)
		{
			Log(fsm, state, action, actionIndex, parameter, logLine, isError: true);
			Debug.LogWarning(FsmUtility.GetPath(state, action) + logLine, fsm.OwnerObject);
			ErrorCount++;
		}

		public static void LogError(Fsm fsm, FsmState state, FsmStateAction action, int actionIndex, string parameter, string logLine)
		{
			Log(fsm, state, action, actionIndex, parameter, logLine, isError: true);
			Debug.LogError(FsmUtility.GetPath(state, action) + logLine, fsm.OwnerObject);
			ErrorCount++;
		}

		public static void LogError(Fsm fsm, FsmState state, FsmStateAction action, int actionIndex, string logLine)
		{
			Log(fsm, state, action, actionIndex, logLine, "", isError: true);
			Debug.LogError(FsmUtility.GetPath(state, action) + logLine, fsm.OwnerObject);
			ErrorCount++;
		}

		public static void Clear()
		{
			ActionReportList.Clear();
		}

		public static void Remove(Fsm fsm)
		{
			ActionReportList.RemoveAll((ActionReport x) => x.fsm == fsm);
		}

		public static int GetCount()
		{
			return ActionReportList.Count;
		}
	}
}
