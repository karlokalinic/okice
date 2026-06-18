using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIContinueButtonFastForward : MonoBehaviour
	{
		[Tooltip("Dialogue UI that the continue button affects.")]
		public StandardDialogueUI dialogueUI;

		[Tooltip("Typewriter effect to fast forward if it's not done playing.")]
		public AbstractTypewriterEffect typewriterEffect;

		[Tooltip("Hide the continue button when continuing.")]
		public bool hideContinueButtonOnContinue;

		[Tooltip("If subtitle is displaying, continue past it.")]
		public bool continueSubtitlePanel = true;

		[Tooltip("If alert is displaying, continue past it.")]
		public bool continueAlertPanel = true;

		protected Button continueButton;

		protected virtual AbstractDialogueUI runtimeDialogueUI
		{
			get
			{
				if (dialogueUI != null)
				{
					return dialogueUI;
				}
				StandardUISubtitlePanel componentInParent = GetComponentInParent<StandardUISubtitlePanel>();
				if (componentInParent != null)
				{
					return componentInParent.dialogueUI;
				}
				return GetComponentInParent<AbstractDialogueUI>() ?? (DialogueManager.dialogueUI as AbstractDialogueUI);
			}
		}

		public virtual void Awake()
		{
			if (typewriterEffect == null)
			{
				typewriterEffect = GetComponentInChildren<UnityUITypewriterEffect>();
			}
			continueButton = GetComponent<Button>();
		}

		public virtual void OnFastForward()
		{
			if (typewriterEffect != null && typewriterEffect.isPlaying)
			{
				typewriterEffect.Stop();
				return;
			}
			if (hideContinueButtonOnContinue && continueButton != null)
			{
				continueButton.gameObject.SetActive(value: false);
				EventSystem.current.SetSelectedGameObject(null);
			}
			if (runtimeDialogueUI != null)
			{
				if (continueSubtitlePanel && continueAlertPanel)
				{
					runtimeDialogueUI.OnContinue();
				}
				else if (continueSubtitlePanel)
				{
					runtimeDialogueUI.OnContinueConversation();
				}
				else if (continueAlertPanel)
				{
					runtimeDialogueUI.OnContinueAlert();
				}
			}
		}
	}
}
