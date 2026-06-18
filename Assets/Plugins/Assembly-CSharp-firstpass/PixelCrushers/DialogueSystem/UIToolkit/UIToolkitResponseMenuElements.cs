using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PixelCrushers.DialogueSystem.UIToolkit
{
	[Serializable]
	public class UIToolkitResponseMenuElements : AbstractUIResponseMenuControls
	{
		[Tooltip("Container panel for response menu.")]
		[SerializeField]
		private string responseMenuPanelName;

		[Tooltip("Progress bar for optional timer. Value range should be 0-1.")]
		[SerializeField]
		private string timerProgressBarName;

		[Tooltip("Optional player portrait name.")]
		[SerializeField]
		private string portraitLabelName;

		[Tooltip("Optional player portrait image.")]
		[SerializeField]
		private string portraitImageName;

		[Tooltip("List of all available response buttons. The dialogue UI will use these to fill out the menu.")]
		[SerializeField]
		private List<string> responseButtonNames;

		protected Dictionary<int, Response> ResponsesByButtonIndex = new Dictionary<int, Response>();

		protected UIDocument Document { get; set; }

		public override AbstractUISubtitleControls subtitleReminderControls => null;

		protected VisualElement ResponseMenuPanel => UIToolkitDialogueUI.GetVisualElement<VisualElement>(Document, responseMenuPanelName);

		protected ProgressBar TimerProgressBar => UIToolkitDialogueUI.GetVisualElement<ProgressBar>(Document, timerProgressBarName);

		protected Label PortraitLabel => UIToolkitDialogueUI.GetVisualElement<Label>(Document, portraitLabelName);

		protected VisualElement PortraitImage => UIToolkitDialogueUI.GetVisualElement<VisualElement>(Document, portraitImageName);

		protected float TimerSecondsMax { get; set; }

		protected float TimerSecondsLeft { get; set; }

		protected Action<object> ClickedResponseAction { get; set; }

		protected virtual Button GetResponseButton(int index)
		{
			return UIToolkitDialogueUI.GetVisualElement<Button>(Document, responseButtonNames[index]);
		}

		public virtual void Initialize(UIDocument document, Action<object> clickedResponseAction)
		{
			Document = document;
			ClickedResponseAction = clickedResponseAction;
			UIToolkitDialogueUI.SetDisplay(ResponseMenuPanel, value: false);
			for (int i = 0; i < responseButtonNames.Count; i++)
			{
				int index = i;
				GetResponseButton(i).clicked += delegate
				{
					OnClickResponse(index);
				};
			}
		}

		public virtual void DoUpdate()
		{
			UpdateTimer();
		}

		public override void SetActive(bool value)
		{
			UIToolkitDialogueUI.SetDisplay(ResponseMenuPanel, value);
			UIToolkitDialogueUI.SetDisplay(TimerProgressBar, value: false);
		}

		public override void SetPCPortrait(Sprite sprite, string portraitName)
		{
			if (PortraitLabel != null)
			{
				PortraitLabel.text = portraitName;
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

		protected override void ClearResponseButtons()
		{
			ResponsesByButtonIndex.Clear();
			for (int i = 0; i < responseButtonNames.Count; i++)
			{
				UIToolkitDialogueUI.SetDisplay(GetResponseButton(i), value: false);
			}
		}

		public override void ShowResponses(Subtitle subtitle, Response[] responses, Transform target)
		{
			if (responses != null && responses.Length != 0)
			{
				ClearResponseButtons();
				SetResponseButtons(responses, target);
				Show();
			}
			else
			{
				Hide();
			}
		}

		protected override void SetResponseButtons(Response[] responses, Transform target)
		{
			int num = Mathf.Min(responses.Length, responseButtonNames.Count);
			int num2 = responseButtonNames.Count - num;
			for (int i = 0; i < responses.Length; i++)
			{
				Response response = responses[i];
				int num3 = ((response.formattedText.position != -1) ? response.formattedText.position : ((buttonAlignment == ResponseButtonAlignment.ToFirst) ? i : (num2 + i)));
				ResponsesByButtonIndex[num3] = response;
				Button responseButton = GetResponseButton(num3);
				if (responseButton != null)
				{
					responseButton.text = response.formattedText.text;
					UIToolkitDialogueUI.SetDisplay(responseButton, value: true);
				}
			}
			if (!showUnusedButtons)
			{
				return;
			}
			int num4 = ((buttonAlignment == ResponseButtonAlignment.ToFirst) ? num : 0);
			for (int j = num4; j < num4 + num2; j++)
			{
				Button responseButton2 = GetResponseButton(j);
				if (responseButton2 != null)
				{
					responseButton2.text = string.Empty;
					UIToolkitDialogueUI.SetDisplay(responseButton2, value: true);
				}
			}
		}

		protected virtual void OnClickResponse(int index)
		{
			if (ResponsesByButtonIndex.TryGetValue(index, out var value))
			{
				Hide();
				ClickedResponseAction(value);
			}
		}

		public override void StartTimer(float timeout)
		{
			if (TimerProgressBar != null)
			{
				UIToolkitDialogueUI.SetDisplay(TimerProgressBar, value: true);
				float timerSecondsLeft = (TimerSecondsMax = timeout);
				TimerSecondsLeft = timerSecondsLeft;
				TimerProgressBar.value = 1f;
			}
		}

		protected virtual void UpdateTimer()
		{
			if (!(TimerSecondsMax <= 0f))
			{
				TimerSecondsLeft -= DialogueTime.deltaTime;
				TimerProgressBar.value = Mathf.Clamp01(TimerSecondsLeft / TimerSecondsMax);
				Debug.Log($"{TimerSecondsLeft} --> {TimerProgressBar.value}");
				if (TimerSecondsLeft <= 0f)
				{
					TimerSecondsMax = 0f;
					OnTimedOut();
				}
			}
		}

		private void OnTimedOut()
		{
			DialogueManager.instance.SendMessage("OnConversationTimeout");
		}
	}
}
