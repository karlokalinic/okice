using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Logic)]
	[Tooltip("Check to see if a gameobject is visible within the camera's frustrum. Because dynamic shadows break isVisible, it's better to use this check.")]
	public class IsVisibleInCameraFrustrum : FsmStateAction
	{
		[RequiredField]
		[CheckForComponent(typeof(Renderer))]
		[Tooltip("The GameObject to test.")]
		public FsmOwnerDefault gameObject;

		[CheckForComponent(typeof(Camera))]
		[Tooltip("The Camera to test with. leave to none or empty to use the main")]
		public FsmOwnerDefault camera;

		[Tooltip("Event to send if the GameObject is visible.")]
		public FsmEvent trueEvent;

		[Tooltip("Event to send if the GameObject is NOT visible.")]
		public FsmEvent falseEvent;

		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a bool variable.")]
		public FsmBool storeResult;

		public bool everyFrame;

		private GameObject go;

		private Camera _cam;

		private bool isVisible;

		private Plane[] planes = new Plane[6];

		public override void Reset()
		{
			gameObject = null;
			camera = new FsmOwnerDefault();
			camera.OwnerOption = OwnerDefaultOption.SpecifyGameObject;
			camera.GameObject = new FsmGameObject
			{
				UseVariable = true
			};
			trueEvent = null;
			falseEvent = null;
			storeResult = null;
			everyFrame = false;
		}

		public override void OnEnter()
		{
			DoIsVisible();
			if (!everyFrame)
			{
				Finish();
			}
		}

		public override void OnUpdate()
		{
			DoIsVisible();
		}

		private void DoIsVisible()
		{
			go = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (!(go == null) && !(go.GetComponent<Renderer>() == null))
			{
				GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(camera);
				if (ownerDefaultTarget != null)
				{
					_cam = ownerDefaultTarget.GetComponent<Camera>();
				}
				if (_cam == null)
				{
					_cam = Camera.main;
				}
				isVisible = IsVisibleFrom(go.GetComponent<Renderer>(), _cam);
				storeResult.Value = isVisible;
				base.Fsm.Event(isVisible ? trueEvent : falseEvent);
			}
		}

		public bool IsVisibleFrom(Renderer renderer, Camera camera)
		{
			GeometryUtility.CalculateFrustumPlanes(camera, planes);
			return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
		}
	}
}
