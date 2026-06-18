using System;
using PixelCrushers.DialogueSystem.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
	public class SequencerCommandFSMEvent : SequencerCommand
	{
		public void Start()
		{
			string parameter = GetParameter(0);
			bool flag = string.Equals(GetParameter(1), "all", StringComparison.OrdinalIgnoreCase);
			Transform transform = (flag ? null : GetSubject(1, base.Sequencer.Speaker));
			string parameter2 = GetParameter(2);
			if (string.IsNullOrEmpty(parameter))
			{
				if (DialogueDebug.LogWarnings)
				{
					Debug.LogWarning(string.Format("{0}: FSMEvent(): event name is empty", "Dialogue System"));
				}
			}
			else if (!flag && transform == null)
			{
				if (DialogueDebug.LogWarnings)
				{
					Debug.LogWarning(string.Format("{0}: FSMEvent({1}, {2}, {3}): subject is null", "Dialogue System", parameter, GetParameter(1), parameter2));
				}
			}
			else
			{
				if (DialogueDebug.LogInfo)
				{
					Debug.Log(string.Format("{0}: FSMEvent({1}, {2}, {3}) sending event to FSM(s)", "Dialogue System", parameter, transform.name, parameter2));
				}
				if (flag)
				{
					DialogueSystemPlayMakerTools.SendEventToAllFSMs(parameter, parameter2);
				}
				else
				{
					DialogueSystemPlayMakerTools.SendEventToFSMs(transform, parameter, parameter2);
				}
			}
			Stop();
		}
	}
}
