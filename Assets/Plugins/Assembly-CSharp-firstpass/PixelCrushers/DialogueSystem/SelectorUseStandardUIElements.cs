using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class SelectorUseStandardUIElements : MonoBehaviour
	{
		[Serializable]
		public class TagInfo
		{
			[Tooltip("Use the UI elements below for usables with this tag. Tags take precedence over layers.")]
			public string tag;

			public string defaultUseMessage;

			public StandardUISelectorElements UIElements;
		}

		[Serializable]
		public class LayerInfo
		{
			[Tooltip("Use the UI elements below for usables in these layers.")]
			public LayerMask layerMask;

			public string defaultUseMessage;

			public StandardUISelectorElements UIElements;
		}

		public List<TagInfo> tagSpecificElements = new List<TagInfo>();

		public List<LayerInfo> layerSpecificElements = new List<LayerInfo>();

		protected Selector selector;

		protected ProximitySelector proximitySelector;

		protected string defaultUseMessage = string.Empty;

		protected Usable usable;

		protected bool lastInRange;

		protected AbstractUsableUI usableUI;

		protected bool started;

		protected string originalDefaultUseMessage;

		protected bool previousUseDefaultGUI;

		protected StandardUISelectorElements m_elements;

		protected float CurrentDistance
		{
			get
			{
				if (!(selector != null))
				{
					return 0f;
				}
				return selector.CurrentDistance;
			}
		}

		public StandardUISelectorElements elements
		{
			get
			{
				return m_elements;
			}
			protected set
			{
				m_elements = value;
			}
		}

		protected virtual void Start()
		{
			if (StandardUISelectorElements.instances.Count == 0)
			{
				if (DialogueDebug.logWarnings)
				{
					Debug.LogWarning("Dialogue System: SelectorUseStandardUIElements can't find a StandardUISelectorElements component in the scene.", this);
				}
				base.enabled = false;
				return;
			}
			started = true;
			ConnectDelegates();
			for (int num = StandardUISelectorElements.instances.Count - 1; num >= 0; num--)
			{
				elements = StandardUISelectorElements.instances[num];
				if (elements != null)
				{
					DeactivateControls();
				}
			}
		}

		protected virtual void OnEnable()
		{
			if (started)
			{
				ConnectDelegates();
			}
		}

		protected virtual void OnDisable()
		{
			DisconnectDelegates();
		}

		public virtual void ConnectDelegates()
		{
			DisconnectDelegates();
			selector = GetComponent<Selector>();
			if (selector != null)
			{
				previousUseDefaultGUI = selector.useDefaultGUI;
				selector.useDefaultGUI = false;
				selector.Enabled += OnSelectorEnabled;
				selector.Disabled += OnSelectorDisabled;
				selector.SelectedUsableObject += OnSelectedUsable;
				selector.DeselectedUsableObject += OnDeselectedUsable;
				defaultUseMessage = selector.defaultUseMessage;
			}
			proximitySelector = GetComponent<ProximitySelector>();
			if (proximitySelector != null)
			{
				previousUseDefaultGUI = proximitySelector.useDefaultGUI;
				proximitySelector.useDefaultGUI = false;
				proximitySelector.Enabled += OnSelectorEnabled;
				proximitySelector.Disabled += OnSelectorDisabled;
				proximitySelector.SelectedUsableObject += OnSelectedUsable;
				proximitySelector.DeselectedUsableObject += OnDeselectedUsable;
				defaultUseMessage = proximitySelector.defaultUseMessage;
			}
			originalDefaultUseMessage = defaultUseMessage;
		}

		public virtual void DisconnectDelegates()
		{
			selector = GetComponent<Selector>();
			if (selector != null)
			{
				selector.useDefaultGUI = previousUseDefaultGUI;
				selector.Enabled -= OnSelectorEnabled;
				selector.Disabled -= OnSelectorDisabled;
				selector.SelectedUsableObject -= OnSelectedUsable;
				selector.DeselectedUsableObject -= OnDeselectedUsable;
			}
			proximitySelector = GetComponent<ProximitySelector>();
			if (proximitySelector != null)
			{
				proximitySelector.useDefaultGUI = previousUseDefaultGUI;
				proximitySelector.Enabled -= OnSelectorEnabled;
				proximitySelector.Disabled -= OnSelectorDisabled;
				proximitySelector.SelectedUsableObject -= OnSelectedUsable;
				proximitySelector.DeselectedUsableObject -= OnDeselectedUsable;
			}
			HideControls();
		}

		protected virtual void SetElementsForUsable(Usable usable)
		{
			for (int i = 0; i < tagSpecificElements.Count; i++)
			{
				TagInfo tagInfo = tagSpecificElements[i];
				if (usable != null && usable.CompareTag(tagInfo.tag))
				{
					defaultUseMessage = tagInfo.defaultUseMessage;
					elements = tagInfo.UIElements ?? StandardUISelectorElements.instance;
					return;
				}
			}
			for (int j = 0; j < layerSpecificElements.Count; j++)
			{
				LayerInfo layerInfo = layerSpecificElements[j];
				if (usable != null && ((1 << usable.gameObject.layer) & layerInfo.layerMask.value) != 0)
				{
					defaultUseMessage = layerInfo.defaultUseMessage;
					elements = layerInfo.UIElements ?? StandardUISelectorElements.instance;
					return;
				}
			}
			defaultUseMessage = originalDefaultUseMessage;
			if (layerSpecificElements.Count > 0 || tagSpecificElements.Count > 0)
			{
				for (int k = 0; k < StandardUISelectorElements.instances.Count; k++)
				{
					StandardUISelectorElements instance = StandardUISelectorElements.instances[k];
					if (layerSpecificElements.Find((LayerInfo x) => x.UIElements == instance) == null && tagSpecificElements.Find((TagInfo x) => x.UIElements == instance) == null)
					{
						elements = instance;
						return;
					}
				}
			}
			elements = StandardUISelectorElements.instance;
		}

		protected virtual void OnSelectedUsable(Usable usable)
		{
			this.usable = usable;
			if (usableUI != null)
			{
				usableUI.Hide();
			}
			usableUI = ((usable != null) ? usable.GetComponentInChildren<AbstractUsableUI>() : null);
			if (usableUI != null)
			{
				usableUI.Show(GetUseMessage());
				HideControls();
			}
			else
			{
				StandardUISelectorElements standardUISelectorElements = elements;
				SetElementsForUsable(usable);
				if (standardUISelectorElements != elements)
				{
					StandardUISelectorElements standardUISelectorElements2 = elements;
					elements = standardUISelectorElements;
					HideControls();
					elements = standardUISelectorElements2;
				}
				ShowControls();
			}
			lastInRange = !IsUsableInRange();
			UpdateDisplay(!lastInRange);
		}

		protected virtual void OnDeselectedUsable(Usable usable)
		{
			if (usableUI != null)
			{
				usableUI.Hide();
				usableUI = null;
			}
			HideControls();
			this.usable = null;
		}

		protected virtual string GetUseMessage()
		{
			return DialogueManager.GetLocalizedText(string.IsNullOrEmpty(usable.overrideUseMessage) ? defaultUseMessage : usable.overrideUseMessage);
		}

		protected virtual void ShowControls()
		{
			if (!(usable == null) && !(elements == null))
			{
				Tools.SetGameObjectActive(elements.mainGraphic, value: true);
				elements.nameText.SetActive(value: true);
				elements.useMessageText.SetActive(value: true);
				elements.nameText.text = usable.GetName();
				elements.useMessageText.text = GetUseMessage();
				Tools.SetGameObjectActive(elements.reticleInRange, IsUsableInRange());
				Tools.SetGameObjectActive(elements.reticleOutOfRange, !IsUsableInRange());
				if (CanTriggerAnimations() && !string.IsNullOrEmpty(elements.animationTransitions.showTrigger))
				{
					elements.animator.ResetTrigger(elements.animationTransitions.hideTrigger);
					elements.animator.SetTrigger(elements.animationTransitions.showTrigger);
				}
			}
		}

		protected virtual void HideControls()
		{
			if (CanTriggerAnimations() && elements != null && !string.IsNullOrEmpty(elements.animationTransitions.hideTrigger))
			{
				elements.animator.ResetTrigger(elements.animationTransitions.showTrigger);
				elements.animator.SetTrigger(elements.animationTransitions.hideTrigger);
			}
			else
			{
				DeactivateControls();
			}
		}

		protected virtual void DeactivateControls()
		{
			if (!(elements == null))
			{
				elements.nameText.SetActive(value: false);
				elements.useMessageText.SetActive(value: false);
				Tools.SetGameObjectActive(elements.reticleInRange, value: false);
				Tools.SetGameObjectActive(elements.reticleOutOfRange, value: false);
				Tools.SetGameObjectActive(elements.mainGraphic, value: false);
			}
		}

		protected virtual bool IsUsableInRange()
		{
			if (usable != null)
			{
				return CurrentDistance <= usable.maxUseDistance;
			}
			return false;
		}

		public virtual void Update()
		{
			if (usable != null)
			{
				UpdateDisplay(IsUsableInRange());
			}
		}

		protected virtual void OnSelectorEnabled()
		{
			ShowControlsOrUsableUI();
		}

		protected virtual void OnSelectorDisabled()
		{
			HideControls();
		}

		public virtual void OnConversationStart(Transform actor)
		{
			HideControls();
		}

		public virtual void OnConversationEnd(Transform actor)
		{
			ShowControlsOrUsableUI();
		}

		protected virtual void ShowControlsOrUsableUI()
		{
			if (usableUI != null)
			{
				usableUI.Show(GetUseMessage());
			}
			else
			{
				ShowControls();
			}
		}

		protected virtual void UpdateDisplay(bool inRange)
		{
			if (usable != null && inRange != lastInRange)
			{
				lastInRange = inRange;
				if (usableUI != null)
				{
					usableUI.UpdateDisplay(inRange);
					return;
				}
				UpdateText(inRange);
				UpdateReticle(inRange);
			}
		}

		protected virtual void UpdateText(bool inRange)
		{
			if (!(elements == null) && elements.useRangeColors)
			{
				Color color = (inRange ? elements.inRangeColor : elements.outOfRangeColor);
				if (elements.nameText != null)
				{
					elements.nameText.color = color;
				}
				if (elements.useMessageText != null)
				{
					elements.useMessageText.color = color;
				}
			}
		}

		protected virtual void UpdateReticle(bool inRange)
		{
			if (!(elements == null))
			{
				Tools.SetGameObjectActive(elements.reticleInRange, inRange);
				Tools.SetGameObjectActive(elements.reticleOutOfRange, !inRange);
			}
		}

		protected virtual bool CanTriggerAnimations()
		{
			if (elements != null && elements.animator != null)
			{
				return elements.animationTransitions != null;
			}
			return false;
		}
	}
}
