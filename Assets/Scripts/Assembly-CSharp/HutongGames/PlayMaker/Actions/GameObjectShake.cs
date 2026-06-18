using System;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Shake an object (GameObject: Camera, Canvas, Cube, etc)")]
	[HelpUrl("http://hutonggames.com/playmakerforum/index.php?topic=9928")]
	public class GameObjectShake : FsmStateAction
	{
		public enum LerpInterpolationType
		{
			Off = 0,
			Linear = 1,
			Quadratic = 2,
			EaseIn = 3,
			EaseOut = 4,
			Smoothstep = 5,
			Smootherstep = 6,
			DeltaTime = 7,
			SimpleSine = 8,
			DoubleSine = 9,
			DoubleByHalfSine = 10,
			Curve = 11
		}

		[ActionSection("Set GameObject")]
		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("GameObject to shake")]
		public FsmOwnerDefault gameObject;

		[ActionSection("Setup")]
		[Tooltip("Amount of Time for shake")]
		public FsmFloat setTime;

		[Tooltip("Shake strength. If interpolation is off, use low numbers such as 0,02 for subtle shake of camera for example.")]
		public FsmFloat shakeAmount;

		[ActionSection("Interpolation Setup")]
		[Tooltip("Select interpolation type. Off means interpolation + shake speed are disabled")]
		public LerpInterpolationType interpolation;

		[Tooltip("If interpolation is on, recommended shake speed above 1f (depends on selection!)")]
		public FsmFloat shakeSpeed;

		[Tooltip("Only works if interpolation is to curve!)")]
		public FsmAnimationCurve lerpCurve;

		[ActionSection("Rotation Setup")]
		[Tooltip("Set rotation strength. 0 = Off/disabled")]
		public FsmFloat rotationAmount;

		[ActionSection("Events")]
		[Tooltip("Leave state when finished shaking")]
		public FsmBool exitOnFinish;

		public FsmEvent exit;

		[ActionSection("")]
		[UIHint(UIHint.Description)]
		[Tooltip("Repeat this action every frame. To allow a loop force quit, set Fsm Bool (true = active) and change it to false in game")]
		public FsmBool loop;

		private Vector3 originalPos;

		private Vector3 newPos;

		private Quaternion newPosRot;

		private bool lerpOn = true;

		private bool animationCurvebool;

		private Transform objTransform;

		private GameObject gameObject2;

		private float shakeTime;

		private Quaternion OriginalRot;

		private float lerpFactor = 0.5f;

		private bool rotation;

		private float rotationIntent;

		private float rotationDecay;

		private bool isCamera;

		private float t;

		public override void Reset()
		{
			gameObject = null;
			loop = false;
			rotation = true;
			exitOnFinish = false;
			shakeTime = 0f;
			setTime = 1f;
			shakeAmount = 0.02f;
			lerpFactor = 1f;
			shakeSpeed = 1f;
			rotationAmount = 0.3f;
			animationCurvebool = false;
			isCamera = false;
		}

		public override void OnPreprocess()
		{
			if (isCamera)
			{
				base.Fsm.HandleLateUpdate = true;
			}
		}

		public override void OnEnter()
		{
			gameObject2 = base.Fsm.GetOwnerDefaultTarget(gameObject);
			objTransform = gameObject2.GetComponent(typeof(Transform)) as Transform;
			if (gameObject2.GetComponent<Camera>() != null)
			{
				isCamera = true;
			}
			originalPos = objTransform.localPosition;
			OriginalRot = objTransform.localRotation;
			shakeTime = setTime.Value;
			rotationIntent = rotationAmount.Value;
			t = shakeSpeed.Value / 30f;
			t = GetInterpolation(Mathf.Abs(t), interpolation);
			if (loop.Value)
			{
				lerpFactor = 0f;
				exitOnFinish = false;
			}
			else
			{
				lerpFactor = 1f;
			}
			if (rotationAmount.Value > 0f || rotationAmount.Value < 0f)
			{
				rotation = true;
			}
			DoObjShake();
		}

		public override void OnUpdate()
		{
			if (!isCamera)
			{
				doSetup();
			}
		}

		public override void OnLateUpdate()
		{
			if (isCamera)
			{
				doSetup();
			}
		}

		private void doSetup()
		{
			if (loop.Value)
			{
				DoObjShake();
			}
			else if (shakeTime <= 0f)
			{
				objTransform.localPosition = originalPos;
				objTransform.localRotation = OriginalRot;
				newPos = objTransform.localPosition;
				if (exitOnFinish.Value)
				{
					base.Fsm.Event(exit);
					if (shakeTime <= 0f)
					{
						Finish();
					}
				}
			}
			else
			{
				DoObjShake();
			}
		}

		private void DoObjShake()
		{
			if (shakeTime > 0f || loop.Value)
			{
				if (lerpOn)
				{
					DoObjShakeLerp();
				}
				else if (!lerpOn)
				{
					objTransform.localPosition = originalPos + UnityEngine.Random.insideUnitSphere * shakeAmount.Value;
					shakeTime -= Time.deltaTime * lerpFactor;
				}
				if (rotation)
				{
					DoObjShakerotation();
				}
			}
		}

		private void DoObjShakerotation()
		{
			newPosRot = objTransform.localRotation;
			objTransform.localRotation = new Quaternion(OriginalRot.x + UnityEngine.Random.Range(0f - rotationIntent, rotationIntent) * 0.2f, OriginalRot.y + UnityEngine.Random.Range(0f - rotationIntent, rotationIntent) * 0.2f, OriginalRot.z + UnityEngine.Random.Range(0f - rotationIntent, rotationIntent) * 0.2f, OriginalRot.w + UnityEngine.Random.Range(0f - rotationIntent, rotationIntent) * 0.2f);
			if (animationCurvebool)
			{
				objTransform.localRotation = Quaternion.Lerp(newPosRot, objTransform.localRotation, lerpCurve.curve.Evaluate(shakeTime));
			}
			else if (lerpOn)
			{
				objTransform.localRotation = Quaternion.Lerp(newPosRot, objTransform.localRotation, t);
			}
			rotationIntent -= Time.deltaTime * t;
		}

		private void DoObjShakeLerp()
		{
			newPos = objTransform.localPosition;
			if (Vector3.Distance(newPos, objTransform.localPosition) <= shakeAmount.Value / 30f)
			{
				newPos = originalPos + UnityEngine.Random.insideUnitSphere * shakeAmount.Value;
			}
			if (animationCurvebool)
			{
				objTransform.localPosition = Vector3.Lerp(objTransform.localPosition, newPos, lerpCurve.curve.Evaluate(shakeTime));
			}
			else if (!animationCurvebool)
			{
				objTransform.localPosition = Vector3.Lerp(objTransform.localPosition, newPos, t);
			}
			shakeTime -= Time.deltaTime;
		}

		private float GetInterpolation(float t, LerpInterpolationType type)
		{
			switch (type)
			{
			case LerpInterpolationType.Quadratic:
				lerpOn = true;
				animationCurvebool = false;
				return Time.timeSinceLevelLoad * setTime.Value;
			case LerpInterpolationType.EaseIn:
				lerpOn = true;
				animationCurvebool = false;
				return 1f - Mathf.Cos(t * MathF.PI * 0.5f);
			case LerpInterpolationType.EaseOut:
				lerpOn = true;
				animationCurvebool = false;
				return Mathf.Sin(t * MathF.PI * 0.5f);
			case LerpInterpolationType.Smoothstep:
				lerpOn = true;
				animationCurvebool = false;
				return t * t * (3f - 2f * t);
			case LerpInterpolationType.Smootherstep:
				animationCurvebool = false;
				lerpOn = true;
				return t * t * t * (t * (6f * t - 15f) + 10f);
			case LerpInterpolationType.DeltaTime:
				lerpOn = true;
				animationCurvebool = false;
				return Time.deltaTime * t;
			case LerpInterpolationType.SimpleSine:
				lerpOn = true;
				animationCurvebool = false;
				return t * Mathf.Sin(Time.timeSinceLevelLoad);
			case LerpInterpolationType.DoubleSine:
				lerpOn = true;
				animationCurvebool = false;
				return t * Mathf.Sin(Time.timeSinceLevelLoad / setTime.Value);
			case LerpInterpolationType.DoubleByHalfSine:
				lerpOn = true;
				animationCurvebool = false;
				return t * (1.5f * Mathf.Sin(Time.timeSinceLevelLoad * setTime.Value));
			case LerpInterpolationType.Curve:
				lerpOn = true;
				animationCurvebool = true;
				return t;
			case LerpInterpolationType.Off:
				animationCurvebool = false;
				lerpOn = false;
				return t;
			default:
				return t;
			}
		}

		public override string ErrorCheck()
		{
			if (gameObject == null)
			{
				return "Need GameObject";
			}
			return "";
		}
	}
}
