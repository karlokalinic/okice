using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PixelCrushers.DialogueSystem.UIToolkit
{
	[Serializable]
	public class UIToolkitDialogueElements : AbstractDialogueUIControls
	{
		[SerializeField]
		private UIDocument document;

		[Tooltip("Name of document's root container.")]
		[SerializeField]
		private string rootContainerName;

		[SerializeField]
		private string dialoguePanelName;

		[Tooltip("Index (starting from 0) into Subtitle Panel Elements of the default NPC subtitle panel.")]
		[SerializeField]
		private int npcSubtitlePanelIndex;

		[Tooltip("Index (starting from 0) into Subtitle Panel Elements of the default PC subtitle panel.")]
		[SerializeField]
		private int pcSubtitlePanelIndex = 1;

		[SerializeField]
		private List<UIToolkitSubtitleElements> subtitlePanelElements;

		[SerializeField]
		private UIToolkitResponseMenuElements responseMenuElements;

		public List<UIToolkitSubtitleElements> SubtitlePanelElements => subtitlePanelElements;

		public UIToolkitSubtitleElements NPCSubtitleElements => GetSubtitleElements(npcSubtitlePanelIndex);

		public UIToolkitSubtitleElements PCSubtitleElements => GetSubtitleElements(pcSubtitlePanelIndex);

		protected UIDocument Document => document;

		protected VisualElement RootContainer => UIToolkitDialogueUI.GetVisualElement<VisualElement>(Document, rootContainerName);

		protected VisualElement DialoguePanel => UIToolkitDialogueUI.GetVisualElement<VisualElement>(Document, dialoguePanelName);

		public override AbstractUISubtitleControls npcSubtitleControls => NPCSubtitleElements;

		public override AbstractUISubtitleControls pcSubtitleControls => PCSubtitleElements;

		public override AbstractUIResponseMenuControls responseMenuControls => responseMenuElements;

		public UIToolkitSubtitleElements GetSubtitleElements(int index)
		{
			if (0 > index || index >= subtitlePanelElements.Count)
			{
				return null;
			}
			return subtitlePanelElements[index];
		}

		public void Initialize(Action clickedContinueAction, Action<object> clickedResponseAction)
		{
			responseMenuElements.Initialize(Document, clickedResponseAction);
			subtitlePanelElements.ForEach(delegate(UIToolkitSubtitleElements x)
			{
				x.Initialize(Document, clickedContinueAction);
			});
		}

		public override void ShowPanel()
		{
			UIToolkitDialogueUI.SetInteractable(RootContainer, value: false);
			UIToolkitDialogueUI.SetDisplay(DialoguePanel, value: true);
		}

		public override void SetActive(bool value)
		{
			UIToolkitDialogueUI.SetInteractable(RootContainer, value);
			UIToolkitDialogueUI.SetDisplay(DialoguePanel, value);
			base.SetActive(value);
		}
	}
}
