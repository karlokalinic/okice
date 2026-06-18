using System;
using System.Text;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.Serialization;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[AddComponentMenu("Dialogue System/Third Party/PlayMaker/Dialogue System Events To PlayMaker")]
	public class DialogueSystemEventsToPlayMaker : MonoBehaviour
	{
		[Serializable]
		public class EventData
		{
			[UnityEngine.Tooltip("Separate responses with this string. If blank, the OnConversationResponseMenu event doesn't set event string data.")]
			public string responseSeparator;
		}

		[UnityEngine.Tooltip("Notify all FSMs. If ticked, the Fsms list below is ignored.")]
		public bool notifyAllFsms;

		[UnityEngine.Tooltip("If Notify All Fsms In Scene is unticked, notify these FSMs.")]
		[FormerlySerializedAs("FSMs")]
		public PlayMakerFSM[] Fsms;

		public EventData eventData = new EventData();

		public bool debug;

		public virtual void SendEvent(string fsmEventName)
		{
			if (Fsms == null)
			{
				if (debug || DialogueDebug.LogWarnings)
				{
					Debug.LogWarning("Dialogue System: Want to send event " + fsmEventName + " but no FSMs are assigned to Dialogue System Events To Play Maker.", this);
				}
				return;
			}
			PlayMakerFSM[] array = (notifyAllFsms ? UnityEngine.Object.FindObjectsOfType<PlayMakerFSM>() : Fsms);
			foreach (PlayMakerFSM playMakerFSM in array)
			{
				if (!(playMakerFSM == null) && playMakerFSM.enabled)
				{
					if (debug)
					{
						Debug.Log("Dialogue System: Sending " + fsmEventName + " to " + playMakerFSM.Fsm.Name);
					}
					playMakerFSM.Fsm.Event(fsmEventName);
				}
			}
		}

		protected virtual void OnConversationStart(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnConversationStart");
		}

		protected virtual void OnConversationEnd(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnConversationEnd");
		}

		protected virtual void OnConversationCancelled(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnConversationCancelled");
		}

		protected virtual void OnConversationLine(Subtitle subtitle)
		{
			Fsm.EventData.StringData = subtitle.formattedText.text;
			SendEvent("OnConversationLine");
		}

		protected virtual void OnConversationLineCancelled(Subtitle subtitle)
		{
			SendEvent("OnConversationLineCancelled");
		}

		protected virtual void OnConversationResponseMenu(Response[] responses)
		{
			if (!string.IsNullOrEmpty(eventData.responseSeparator) && responses != null)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < responses.Length; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append(eventData.responseSeparator);
					}
					stringBuilder.Append(responses[i].destinationEntry.MenuText);
				}
				if (debug)
				{
					Debug.Log("Dialogue System: FSM Event OnConversationResponseMenu '" + stringBuilder.ToString() + "'.");
				}
				Fsm.EventData.StringData = stringBuilder.ToString();
			}
			SendEvent("OnConversationResponseMenu");
		}

		protected virtual void OnConversationTimeout()
		{
			SendEvent("OnConversationTimeout");
		}

		protected virtual void OnLinkedConversationStart(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnLinkedConversationStart");
		}

		protected virtual void OnBarkStart(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnBarkStart");
		}

		protected virtual void OnBarkEnd(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnBarkEnd");
		}

		protected virtual void OnBarkLine(Subtitle subtitle)
		{
			Fsm.EventData.GameObjectData = ((subtitle.speakerInfo.transform != null) ? subtitle.speakerInfo.transform.gameObject : null);
			SendEvent("OnBarkLine");
		}

		protected virtual void OnSequenceStart(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnSequenceStart");
		}

		protected virtual void OnSequenceEnd(Transform actor)
		{
			Fsm.EventData.GameObjectData = ((actor != null) ? actor.gameObject : null);
			SendEvent("OnSequenceEnd");
		}

		protected virtual void OnQuestStateChange(string title)
		{
			Fsm.EventData.StringData = title;
			SendEvent("OnQuestStateChange");
		}

		protected virtual void OnQuestTrackingEnabled(string title)
		{
			Fsm.EventData.StringData = title;
			SendEvent("OnQuestTrackingEnabled");
		}

		protected virtual void OnQuestTrackingDisabled(string title)
		{
			Fsm.EventData.StringData = title;
			SendEvent("OnQuestTrackingDisabled");
		}

		public virtual void OnRecordPersistentData()
		{
			SendEvent("OnRecordPersistentData");
		}

		public virtual void OnApplyPersistentData()
		{
			SendEvent("OnApplyPersistentData");
		}

		public virtual void OnLevelWillBeUnloaded()
		{
			SendEvent("OnLevelWillBeUnloaded");
		}

		protected virtual void OnDialogueSystemPause()
		{
			SendEvent("OnDialogueSystemPause");
		}

		protected virtual void OnDialogueSystemUnpause()
		{
			SendEvent("OnDialogueSystemUnpause");
		}
	}
}
