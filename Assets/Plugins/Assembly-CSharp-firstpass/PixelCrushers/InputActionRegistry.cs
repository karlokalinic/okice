using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	public class InputActionRegistry : MonoBehaviour
	{
		[SerializeField]
		private List<InputActionReference> inputActions;

		public List<InputActionReference> InputActions => inputActions;

		private void Start()
		{
			RegisterInputActions();
		}

		private void RegisterInputActions()
		{
			foreach (InputActionReference inputAction in InputActions)
			{
				if (!(inputAction == null))
				{
					InputDeviceManager.RegisterInputAction(inputAction.action.name, inputAction.action);
				}
			}
		}
	}
}
