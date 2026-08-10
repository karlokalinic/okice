using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Activates/deactivates a Game Object. Use this to hide/show areas, or enable/disable many Behaviours at once.")]
	public class ActivateGameObject : FsmStateAction
	{
		private const string PersistentLampGroupName = "two lamps";

		[RequiredField]
		[Tooltip("The Game Object to activate/deactivate.")]
		public FsmOwnerDefault gameObject;

		[RequiredField]
		[Tooltip("Check to activate, uncheck to deactivate Game Object.")]
		public FsmBool activate;

		[Tooltip("Recursively activate/deactivate all children.")]
		public FsmBool recursive;

		[Tooltip("Reset the Game Object's active state when exiting this state. Useful if you want an object to be active only while this state is active.\nNote: Only applies to the last Game Object activated/deactivated (won't work if Game Object changes).")]
		public bool resetOnExit;

		[Tooltip("Repeat this action every frame. Useful if using a variable for Activate that can change over time.")]
		public bool everyFrame;

		private GameObject activatedGameObject;

		private Light[] activatedLampLights;

		public override void Reset()
		{
			gameObject = null;
			activate = true;
			recursive = false;
			resetOnExit = false;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoActivateGameObject();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoActivateGameObject();
		}

		public override void OnExit()
		{
			if (activatedLampLights != null && resetOnExit)
			{
				SetLampLightsActive(!activate.Value);
			}
			else if (!(activatedGameObject == null) && resetOnExit)
			{
				if (recursive.Value)
				{
					SetActiveRecursively(activatedGameObject, !activate.Value);
				}
				else
				{
					activatedGameObject.SetActive(!activate.Value);
				}
			}
		}

		private void DoActivateGameObject()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(ownerDefaultTarget == null))
			{
				if (ownerDefaultTarget.name == PersistentLampGroupName)
				{
					ownerDefaultTarget.SetActive(true);
					activatedLampLights = ownerDefaultTarget.GetComponentsInChildren<Light>(true);
					if (activatedLampLights.Length != 0)
					{
						SetLampLightsActive(activate.Value);
						return;
					}
				}

				if (recursive.Value)
				{
					SetActiveRecursively(ownerDefaultTarget, activate.Value);
				}
				else
				{
					ownerDefaultTarget.SetActive(activate.Value);
				}
				activatedGameObject = ownerDefaultTarget;
			}
		}

		private void SetLampLightsActive(bool state)
		{
			foreach (Light lampLight in activatedLampLights)
			{
				if (lampLight != null)
				{
					lampLight.enabled = state;
				}
			}
		}

		public void SetActiveRecursively(GameObject go, bool state)
		{
			go.SetActive(state);
			foreach (Transform item in go.transform)
			{
				SetActiveRecursively(item.gameObject, state);
			}
		}
	}
}
