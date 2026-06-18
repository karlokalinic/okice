using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PixelCrushers.DialogueSystem.UIToolkit
{
	[Serializable]
	public class UIToolkitSubtitleElements : AbstractUISubtitleControls
	{
		[Tooltip("Container panel for subtitle.")]
		[SerializeField]
		private string subtitlePanelName;

		[Tooltip("Subtitle text.")]
		[SerializeField]
		private string subtitleLabelName;

		[Tooltip("Optional speaker portrait name.")]
		[SerializeField]
		private string portraitLabelName;

		[Tooltip("Optional speaker portrait image.")]
		[SerializeField]
		private string portraitImageName;

		[Tooltip("Continue button to advance conversation (if mode requires continue button click).")]
		[SerializeField]
		private string continueButtonName;

		[Tooltip("Specifies when panel should be visible/hidden.")]
		[SerializeField]
		private UIVisibility visibility;

		public string SubtitlePanelName => subtitlePanelName;

		public UIVisibility Visibility => visibility;

		protected UIDocument Document { get; set; }

		protected VisualElement SubtitlePanel => UIToolkitDialogueUI.GetVisualElement<VisualElement>(Document, subtitlePanelName);

		protected Label SubtitleLabel => UIToolkitDialogueUI.GetVisualElement<Label>(Document, subtitleLabelName);

		protected Label PortraitLabel => UIToolkitDialogueUI.GetVisualElement<Label>(Document, portraitLabelName);

		protected VisualElement PortraitImage => UIToolkitDialogueUI.GetVisualElement<VisualElement>(Document, portraitImageName);

		protected Button ContinueButton => UIToolkitDialogueUI.GetVisualElement<Button>(Document, continueButtonName);

		public bool ShouldStayVisible
		{
			get
			{
				if (Visibility != UIVisibility.AlwaysFromStart)
				{
					return Visibility == UIVisibility.AlwaysOnceShown;
				}
				return true;
			}
		}

		public override bool hasText => !string.IsNullOrEmpty(SubtitleLabel.text);

		public bool IsSamePanel(UIToolkitSubtitleElements panel)
		{
			return panel.subtitlePanelName == subtitlePanelName;
		}

		public virtual void Initialize(UIDocument document, Action clickedContinueAction)
		{
			Document = document;
			if (ContinueButton != null)
			{
				ContinueButton.clicked += clickedContinueAction;
			}
		}

		public override void SetActive(bool value)
		{
			UIToolkitDialogueUI.SetDisplay(SubtitlePanel, value);
			HideContinueButton();
		}

		public virtual void OpenOnStartConversation(Sprite portraitSprite, string portraitName, DialogueActor dialogueActor)
		{
			SetActive(value: true);
			string actorName = portraitName;
			Sprite sprite = portraitSprite;
			if (dialogueActor != null)
			{
				actorName = dialogueActor.GetActorName();
				Sprite portraitSprite2 = dialogueActor.GetPortraitSprite();
				if (portraitSprite2 != null)
				{
					sprite = portraitSprite2;
				}
			}
			SetActorPortraitSprite(actorName, sprite);
			if (SubtitleLabel != null)
			{
				SubtitleLabel.text = string.Empty;
			}
		}

		public override void ClearSubtitle()
		{
			if (SubtitleLabel != null)
			{
				SubtitleLabel.text = string.Empty;
			}
			HideContinueButton();
		}

		public override void SetSubtitle(Subtitle subtitle)
		{
			SetActive(value: true);
			if (SubtitleLabel != null)
			{
				SubtitleLabel.text = subtitle.formattedText.text;
			}
			SetActorPortraitSprite(subtitle.speakerInfo.Name, subtitle.GetSpeakerPortrait());
		}

		public override void SetActorPortraitSprite(string actorName, Sprite sprite)
		{
			if (PortraitLabel != null)
			{
				PortraitLabel.text = actorName;
			}
			if (PortraitImage != null)
			{
				bool flag = sprite != null;
				UIToolkitDialogueUI.SetDisplay(PortraitImage, flag);
				if (flag)
				{
					PortraitImage.style.backgroundImage = new StyleBackground(sprite);
				}
			}
		}

		public override void ShowContinueButton()
		{
			UIToolkitDialogueUI.SetDisplay(ContinueButton, value: true);
		}

		public override void HideContinueButton()
		{
			UIToolkitDialogueUI.SetDisplay(ContinueButton, value: false);
		}
	}
}
