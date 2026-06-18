using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class DemoInputControls : IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct DemoActionMapActions
	{
		private DemoInputControls m_Wrapper;

		public InputAction Horizontal => m_Wrapper.m_DemoActionMap_Horizontal;

		public InputAction Vertical => m_Wrapper.m_DemoActionMap_Vertical;

		public InputAction Fire1 => m_Wrapper.m_DemoActionMap_Fire1;

		public bool enabled => Get().enabled;

		public DemoActionMapActions(DemoInputControls wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_DemoActionMap;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(DemoActionMapActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IDemoActionMapActions instance)
		{
			if (m_Wrapper.m_DemoActionMapActionsCallbackInterface != null)
			{
				Horizontal.started -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnHorizontal;
				Horizontal.performed -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnHorizontal;
				Horizontal.canceled -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnHorizontal;
				Vertical.started -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnVertical;
				Vertical.performed -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnVertical;
				Vertical.canceled -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnVertical;
				Fire1.started -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnFire1;
				Fire1.performed -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnFire1;
				Fire1.canceled -= m_Wrapper.m_DemoActionMapActionsCallbackInterface.OnFire1;
			}
			m_Wrapper.m_DemoActionMapActionsCallbackInterface = instance;
			if (instance != null)
			{
				Horizontal.started += instance.OnHorizontal;
				Horizontal.performed += instance.OnHorizontal;
				Horizontal.canceled += instance.OnHorizontal;
				Vertical.started += instance.OnVertical;
				Vertical.performed += instance.OnVertical;
				Vertical.canceled += instance.OnVertical;
				Fire1.started += instance.OnFire1;
				Fire1.performed += instance.OnFire1;
				Fire1.canceled += instance.OnFire1;
			}
		}
	}

	public interface IDemoActionMapActions
	{
		void OnHorizontal(InputAction.CallbackContext context);

		void OnVertical(InputAction.CallbackContext context);

		void OnFire1(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_DemoActionMap;

	private IDemoActionMapActions m_DemoActionMapActionsCallbackInterface;

	private readonly InputAction m_DemoActionMap_Horizontal;

	private readonly InputAction m_DemoActionMap_Vertical;

	private readonly InputAction m_DemoActionMap_Fire1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public DemoActionMapActions DemoActionMap => new DemoActionMapActions(this);

	public DemoInputControls()
	{
		asset = InputActionAsset.FromJson("{\n    \"name\": \"DemoInputControls\",\n    \"maps\": [\n        {\n            \"name\": \"DemoActionMap\",\n            \"id\": \"41649a10-fe04-42dc-b834-7b0e6b8f6f8e\",\n            \"actions\": [\n                {\n                    \"name\": \"Horizontal\",\n                    \"type\": \"Button\",\n                    \"id\": \"ef3929c6-b315-4851-8f3e-ae170992d312\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Vertical\",\n                    \"type\": \"Button\",\n                    \"id\": \"74bfe387-c2ec-4a2e-9b81-cd1c81ee069b\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                },\n                {\n                    \"name\": \"Fire1\",\n                    \"type\": \"Button\",\n                    \"id\": \"804b48fe-6035-4b70-a3b4-877f04982d7d\",\n                    \"expectedControlType\": \"Button\",\n                    \"processors\": \"\",\n                    \"interactions\": \"\"\n                }\n            ],\n            \"bindings\": [\n                {\n                    \"name\": \"A-D\",\n                    \"id\": \"988324e0-d947-4fa7-825f-8c22a3d5a9cd\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Horizontal\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"096967ca-ee92-45be-9f93-fc5e3a4f109d\",\n                    \"path\": \"<Keyboard>/a\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Horizontal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"fbfca6ac-a78f-40e1-b53a-27a6570672c4\",\n                    \"path\": \"<Keyboard>/d\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Horizontal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Left-Right\",\n                    \"id\": \"80a8ea42-1404-4111-b927-3d3e018469dd\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Horizontal\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"bd31e001-3b16-4b7e-865c-dc188bb61918\",\n                    \"path\": \"<Keyboard>/leftArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Horizontal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"98dc1df3-8219-4687-a480-47c71a1953df\",\n                    \"path\": \"<Keyboard>/rightArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Horizontal\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"S-W\",\n                    \"id\": \"5fe719fc-bbc5-4091-b418-91c9a8699b54\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Vertical\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"5d7cca19-57a8-4a09-ab88-7dcac3570e64\",\n                    \"path\": \"<Keyboard>/s\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Vertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"6a6517d3-50c7-4c54-b86e-4dec04733436\",\n                    \"path\": \"<Keyboard>/w\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Vertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"Down-Up\",\n                    \"id\": \"8ae87a3c-1197-4725-baf0-9be8746497fb\",\n                    \"path\": \"1DAxis\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Vertical\",\n                    \"isComposite\": true,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"negative\",\n                    \"id\": \"5f943101-e079-4c5e-93f1-1602b61d2418\",\n                    \"path\": \"<Keyboard>/downArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Vertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"positive\",\n                    \"id\": \"8322cbd3-368e-43ed-b41e-f7ce51f1f189\",\n                    \"path\": \"<Keyboard>/upArrow\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Vertical\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": true\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"6de4a513-0301-4138-972a-db7bccc7e316\",\n                    \"path\": \"<Keyboard>/space\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Fire1\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                },\n                {\n                    \"name\": \"\",\n                    \"id\": \"e5a158a9-f419-43dc-912d-97f01d68c681\",\n                    \"path\": \"<Mouse>/leftButton\",\n                    \"interactions\": \"\",\n                    \"processors\": \"\",\n                    \"groups\": \"\",\n                    \"action\": \"Fire1\",\n                    \"isComposite\": false,\n                    \"isPartOfComposite\": false\n                }\n            ]\n        }\n    ],\n    \"controlSchemes\": []\n}");
		m_DemoActionMap = asset.FindActionMap("DemoActionMap", throwIfNotFound: true);
		m_DemoActionMap_Horizontal = m_DemoActionMap.FindAction("Horizontal", throwIfNotFound: true);
		m_DemoActionMap_Vertical = m_DemoActionMap.FindAction("Vertical", throwIfNotFound: true);
		m_DemoActionMap_Fire1 = m_DemoActionMap.FindAction("Fire1", throwIfNotFound: true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}
}
