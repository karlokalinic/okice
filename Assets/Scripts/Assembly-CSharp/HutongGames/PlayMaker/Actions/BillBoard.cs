using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Align an object to always face the camera. You optionnally decide to keep a constant screensize.")]
	public class BillBoard : FsmStateAction
	{
		[CheckForComponent(typeof(Camera))]
		[Tooltip("Leave to none to use the main camera")]
		public FsmGameObject cameraTarget;

		[Tooltip("check this if you want constant screensize")]
		public bool ConstantScreenSize;

		[Tooltip("if 0, will use the current distance to keep constant screensize to be scaled to 1 at that distance")]
		public FsmFloat distanceBase;

		private GameObject _go;

		private Camera _cam;

		private Vector3 _originalScale;

		private float _originalDistance;

		public override void Reset()
		{
			cameraTarget = null;
			ConstantScreenSize = true;
			distanceBase = null;
		}

		public override void OnUpdate()
		{
			GameObject value = cameraTarget.Value;
			if (_cam == null || _go != value)
			{
				_go = value;
				if (value == null)
				{
					_cam = Camera.main;
				}
				else
				{
					Camera component = value.GetComponent<Camera>();
					if (component == null)
					{
						_cam = Camera.main;
						LogError("Missing Camera Component!");
						return;
					}
					_cam = component;
				}
				_originalDistance = (_cam.transform.position - base.Owner.transform.position).magnitude;
				_originalScale = base.Owner.transform.localScale;
			}
			base.Owner.transform.LookAt(base.Owner.transform.position + _cam.transform.rotation * Vector3.back, _cam.transform.rotation * Vector3.up);
			if (ConstantScreenSize)
			{
				float num = 0f;
				if (distanceBase.Value > 0f)
				{
					num = (_cam.transform.position - base.Owner.transform.position).magnitude;
					base.Owner.transform.localScale = Vector3.one * (num / distanceBase.Value);
				}
				else
				{
					num = (_cam.transform.position - base.Owner.transform.position).magnitude;
					base.Owner.transform.localScale = num / _originalDistance * _originalScale;
				}
			}
		}
	}
}
