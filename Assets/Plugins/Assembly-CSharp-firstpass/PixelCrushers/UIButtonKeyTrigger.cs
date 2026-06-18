using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(Selectable))]
	public class UIButtonKeyTrigger : MonoBehaviour, IEventSystemUser
	{
		[Tooltip("Trigger the selectable when this key is pressed.")]
		public KeyCode key;

		[Tooltip("Trigger the selectable when this input button is pressed.")]
		public string buttonName = string.Empty;

		[Tooltip("Trigger the selectable when this input action is performed.")]
		public InputActionReference inputAction;

		[Tooltip("Disable input action after button is clicked.")]
		public bool disableInputActionAfterClick;

		[Tooltip("Trigger if any key, input button, or mouse button is pressed.")]
		public bool anyKeyOrButton;

		[Tooltip("Ignore trigger key/button if UI button is being clicked Event System's Submit input. Prevents unintentional double clicks. For this checkbox to work, you must set the Input Device Manager component's Submit input to the same inputs as the EventSystem's Submit.")]
		public bool skipIfBeingClickedBySubmit = true;

		[Tooltip("Visually show UI Button in pressed state when triggered.")]
		public bool simulateButtonClick = true;

		[Tooltip("Show pressed state for this duration in seconds.")]
		public float simulateButtonDownDuration = 0.1f;

		private Selectable m_selectable;

		private EventSystem m_eventSystem;

		public static bool monitorInput = true;

		protected Selectable selectable
		{
			get
			{
				return m_selectable;
			}
			set
			{
				m_selectable = value;
			}
		}

		public EventSystem eventSystem
		{
			get
			{
				if (m_eventSystem != null)
				{
					return m_eventSystem;
				}
				return EventSystem.current;
			}
			set
			{
				m_eventSystem = value;
			}
		}

		protected virtual void Awake()
		{
			m_selectable = GetComponent<Selectable>();
			if (m_selectable == null)
			{
				base.enabled = false;
			}
		}

		protected virtual void OnEnable()
		{
			if (inputAction != null)
			{
				inputAction.action.Enable();
				inputAction.action.performed += OnInputActionPerformed;
			}
		}

		protected virtual void OnDisable()
		{
			if (inputAction != null)
			{
				inputAction.action.performed -= OnInputActionPerformed;
				if (disableInputActionAfterClick)
				{
					inputAction.action.Disable();
				}
			}
		}

		private void OnInputActionPerformed(InputAction.CallbackContext context)
		{
			if (monitorInput && m_selectable.enabled && m_selectable.interactable && m_selectable.gameObject.activeInHierarchy && (!skipIfBeingClickedBySubmit || !IsBeingClickedBySubmit()))
			{
				Click();
			}
		}

		protected void Update()
		{
			if (monitorInput && m_selectable.enabled && m_selectable.interactable && m_selectable.gameObject.activeInHierarchy && (InputDeviceManager.IsKeyDown(key) || (!string.IsNullOrEmpty(buttonName) && InputDeviceManager.IsButtonDown(buttonName)) || (anyKeyOrButton && InputDeviceManager.IsAnyKeyDown())) && (!skipIfBeingClickedBySubmit || !IsBeingClickedBySubmit()))
			{
				Click();
			}
		}

		protected virtual bool IsBeingClickedBySubmit()
		{
			if (eventSystem != null && eventSystem.currentSelectedGameObject == m_selectable.gameObject && InputDeviceManager.instance != null)
			{
				return InputDeviceManager.IsButtonDown(InputDeviceManager.instance.submitButton);
			}
			return false;
		}

		protected virtual void Click()
		{
			if (simulateButtonClick)
			{
				StartCoroutine(SimulateButtonClick());
			}
			else
			{
				ExecuteEvents.Execute(m_selectable.gameObject, new PointerEventData(eventSystem), ExecuteEvents.submitHandler);
			}
		}

		protected IEnumerator SimulateButtonClick()
		{
			m_selectable.OnPointerDown(new PointerEventData(eventSystem));
			for (float timeLeft = simulateButtonDownDuration; timeLeft > 0f; timeLeft -= Time.unscaledDeltaTime)
			{
				yield return null;
			}
			m_selectable.OnPointerUp(new PointerEventData(eventSystem));
			m_selectable.OnDeselect(null);
			ExecuteEvents.Execute(m_selectable.gameObject, new PointerEventData(eventSystem), ExecuteEvents.submitHandler);
		}
	}
}
