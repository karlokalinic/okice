using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class Usable : MonoBehaviour
	{
		[Serializable]
		public class UsableEvents
		{
			public UnityEvent onSelect = new UnityEvent();

			public UnityEvent onDeselect = new UnityEvent();

			public UnityEvent onUse = new UnityEvent();
		}

		[SerializeField]
		[FormerlySerializedAs("overrideName")]
		private string m_overrideName;

		[SerializeField]
		[FormerlySerializedAs("overrideUseMessage")]
		private string m_overrideUseMessage;

		public float maxUseDistance = 5f;

		public UsableEvents events;

		public virtual string overrideName
		{
			get
			{
				return m_overrideName;
			}
			set
			{
				m_overrideName = value;
			}
		}

		public virtual string overrideUseMessage
		{
			get
			{
				return m_overrideUseMessage;
			}
			set
			{
				m_overrideUseMessage = value;
			}
		}

		public event UsableDelegate disabled = delegate
		{
		};

		protected virtual void OnDisable()
		{
			this.disabled(this);
		}

		public virtual void Start()
		{
		}

		public virtual string GetName()
		{
			if (string.IsNullOrEmpty(overrideName))
			{
				return DialogueActor.GetActorName(base.transform);
			}
			if (overrideName.Contains("[lua") || overrideName.Contains("[var"))
			{
				return DialogueManager.GetLocalizedText(FormattedText.Parse(overrideName, DialogueManager.masterDatabase.emphasisSettings).text);
			}
			return DialogueManager.GetLocalizedText(overrideName);
		}

		public virtual void OnSelectUsable()
		{
			if (events != null && events.onSelect != null)
			{
				events.onSelect.Invoke();
			}
		}

		public virtual void OnDeselectUsable()
		{
			if (events != null && events.onDeselect != null)
			{
				events.onDeselect.Invoke();
			}
		}

		public virtual void OnUseUsable()
		{
			if (events != null && events.onUse != null)
			{
				events.onUse.Invoke();
			}
		}
	}
}
