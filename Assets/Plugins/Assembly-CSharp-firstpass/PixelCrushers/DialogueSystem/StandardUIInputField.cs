using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class StandardUIInputField : UIPanel, ITextFieldUI
	{
		[Tooltip("(Optional) Text field panel.")]
		public Graphic panel;

		[Tooltip("(Optional) Text element for prompt.")]
		public UITextField label;

		[Tooltip("Input field.")]
		public UIInputField inputField;

		[Tooltip("(Optional) Key code that accepts user's text input.")]
		public KeyCode acceptKey = KeyCode.Return;

		[Tooltip("(Optional) Input button that accepts user's text input.")]
		public string acceptButton = string.Empty;

		[Tooltip("(Optional) Key code that cancels user's text input.")]
		public KeyCode cancelKey = KeyCode.Escape;

		[Tooltip("(Optional) Input button that cancels user's text input.")]
		public string cancelButton = string.Empty;

		[Tooltip("Automatically open touchscreen keyboard.")]
		public bool showTouchScreenKeyboard;

		[Tooltip("Allow blank text input.")]
		public bool allowBlankInput = true;

		public UnityEvent onAccept = new UnityEvent();

		public UnityEvent onCancel = new UnityEvent();

		protected AcceptedTextDelegate m_acceptedText;

		protected bool m_isAwaitingInput;

		protected TouchScreenKeyboard m_touchScreenKeyboard;

		protected bool m_isQuitting;

		protected virtual void OnApplicationQuit()
		{
			m_isQuitting = true;
		}

		protected override void Start()
		{
			if (DialogueDebug.logWarnings && inputField == null)
			{
				Debug.LogWarning("Dialogue System: No InputField is assigned to the text field UI " + base.name + ". TextInput() sequencer commands or [var?=] won't work.");
			}
			SetActive(value: false);
		}

		public virtual void StartTextInput(string labelText, string text, int maxLength, AcceptedTextDelegate acceptedText)
		{
			if (label != null)
			{
				label.text = labelText;
			}
			if (inputField != null)
			{
				inputField.text = text;
				inputField.characterLimit = maxLength;
			}
			m_acceptedText = acceptedText;
			m_isAwaitingInput = true;
			Show();
		}

		protected override void Update()
		{
			if (m_isAwaitingInput && !DialogueManager.IsDialogueSystemInputDisabled())
			{
				if (InputDeviceManager.IsKeyDown(acceptKey) || InputDeviceManager.IsButtonDown(acceptButton) || IsTouchScreenDone())
				{
					AcceptTextInput();
				}
				else if (InputDeviceManager.IsKeyDown(cancelKey) || InputDeviceManager.IsButtonDown(cancelButton) || IsTouchScreenCancelled())
				{
					CancelTextInput();
				}
			}
		}

		protected virtual bool IsTouchScreenDone()
		{
			if (m_touchScreenKeyboard == null)
			{
				return false;
			}
			try
			{
				return m_touchScreenKeyboard.status == TouchScreenKeyboard.Status.Done;
			}
			catch (Exception)
			{
				return false;
			}
		}

		protected virtual bool IsTouchScreenCancelled()
		{
			if (m_touchScreenKeyboard == null)
			{
				return false;
			}
			try
			{
				return m_touchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled;
			}
			catch (Exception)
			{
				return false;
			}
		}

		protected virtual bool IsTouchScreenCanceled()
		{
			if (m_touchScreenKeyboard == null)
			{
				return false;
			}
			try
			{
				return m_touchScreenKeyboard.status == TouchScreenKeyboard.Status.Canceled;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public virtual void CancelTextInput()
		{
			m_isAwaitingInput = false;
			Hide();
			onCancel.Invoke();
		}

		public virtual void AcceptTextInput()
		{
			if (!CanAcceptInput())
			{
				return;
			}
			m_isAwaitingInput = false;
			if (m_acceptedText != null)
			{
				if (inputField != null)
				{
					m_acceptedText(inputField.text);
				}
				m_acceptedText = null;
			}
			Hide();
			onAccept.Invoke();
		}

		protected virtual bool CanAcceptInput()
		{
			if (!allowBlankInput)
			{
				return !string.IsNullOrWhiteSpace(inputField.text);
			}
			return true;
		}

		protected virtual void Show()
		{
			SetActive(value: true);
			Open();
			if (showTouchScreenKeyboard)
			{
				ShowTouchScreenKeyboard();
			}
			if (inputField != null)
			{
				inputField.ActivateInputField();
				if (base.eventSystem != null)
				{
					base.eventSystem.SetSelectedGameObject(inputField.gameObject);
				}
			}
		}

		protected virtual void ShowTouchScreenKeyboard()
		{
			m_touchScreenKeyboard = TouchScreenKeyboard.Open(inputField.text);
		}

		protected virtual void Hide()
		{
			if (m_isQuitting)
			{
				return;
			}
			Close();
			SetActive(value: false);
			if (m_touchScreenKeyboard != null)
			{
				try
				{
					m_touchScreenKeyboard.active = false;
				}
				catch (Exception)
				{
				}
				m_touchScreenKeyboard = null;
			}
		}

		protected virtual void SetActive(bool value)
		{
			if (panel != null)
			{
				panel.gameObject.SetActive(value);
			}
			if (panel == null || value)
			{
				if (label != null)
				{
					label.SetActive(value);
				}
				if (inputField != null)
				{
					inputField.SetActive(value);
				}
			}
		}
	}
}
