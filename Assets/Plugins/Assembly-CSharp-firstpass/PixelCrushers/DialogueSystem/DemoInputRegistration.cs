using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	public class DemoInputRegistration : MonoBehaviour
	{
		private DemoInputControls controls;

		protected static bool isRegistered;

		private bool didIRegister;

		private void Awake()
		{
			controls = new DemoInputControls();
		}

		private void OnEnable()
		{
			if (!isRegistered)
			{
				isRegistered = true;
				didIRegister = true;
				controls.Enable();
				InputDeviceManager.RegisterInputAction("Horizontal", controls.DemoActionMap.Horizontal);
				InputDeviceManager.RegisterInputAction("Vertical", controls.DemoActionMap.Vertical);
				InputDeviceManager.RegisterInputAction("Fire1", controls.DemoActionMap.Fire1);
			}
		}

		private void OnDisable()
		{
			if (didIRegister)
			{
				isRegistered = false;
				didIRegister = false;
				controls.Disable();
				InputDeviceManager.UnregisterInputAction("Horizontal");
				InputDeviceManager.UnregisterInputAction("Vertical");
				InputDeviceManager.UnregisterInputAction("Fire1");
			}
		}
	}
}
