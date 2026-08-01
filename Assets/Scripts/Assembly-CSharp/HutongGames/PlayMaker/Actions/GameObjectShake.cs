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

        [Tooltip("Only works if interpolation is to curve!")]
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

        private Transform targetTransform;
        private Vector3 originalPosition;
        private Vector3 targetPosition;
        private Quaternion originalRotation;
        private Quaternion targetRotation;
        private float remainingTime;
        private float interpolationFactor;
        private float rotationStrength;
        private bool useInterpolation;
        private bool useCurve;
        private bool rotate;
        private bool isCamera;

        public override void Reset()
        {
            gameObject = null;
            loop = false;
            exitOnFinish = false;
            setTime = 1f;
            shakeAmount = 0.02f;
            shakeSpeed = 1f;
            rotationAmount = 0.3f;
            interpolation = LerpInterpolationType.Linear;
        }

        public override void OnPreprocess()
        {
            Fsm.HandleLateUpdate = true;
        }

        public override void OnEnter()
        {
            GameObject targetObject = Fsm.GetOwnerDefaultTarget(gameObject);
            if (targetObject == null)
            {
                Finish();
                return;
            }

            targetTransform = targetObject.transform;
            isCamera = targetObject.GetComponent<Camera>() != null;
            originalPosition = targetTransform.localPosition;
            originalRotation = targetTransform.localRotation;
            remainingTime = Mathf.Max(0f, setTime.Value);
            rotationStrength = Mathf.Abs(rotationAmount.Value);
            rotate = rotationStrength > Mathf.Epsilon;
            interpolationFactor = GetInterpolation(Mathf.Abs(shakeSpeed.Value / 30f), interpolation);
            targetPosition = originalPosition + CreatePositionOffset();
            targetRotation = CreateRotationTarget();

            ApplyShake();
        }

        public override void OnUpdate()
        {
            if (!isCamera)
            {
                Tick();
            }
        }

        public override void OnLateUpdate()
        {
            if (isCamera)
            {
                Tick();
            }
        }

        public override void OnExit()
        {
            RestoreTransform();
        }

        private void Tick()
        {
            if (targetTransform == null)
            {
                Finish();
                return;
            }

            if (loop.Value)
            {
                ApplyShake();
                return;
            }

            if (remainingTime <= 0f)
            {
                RestoreTransform();

                if (exitOnFinish.Value)
                {
                    Fsm.Event(exit);
                    Finish();
                }

                return;
            }

            ApplyShake();
        }

        private void ApplyShake()
        {
            if (useInterpolation)
            {
                ApplyInterpolatedPosition();
            }
            else
            {
                targetTransform.localPosition = originalPosition + CreatePositionOffset();
                DecreaseRemainingTime();
            }

            if (rotate)
            {
                ApplyRotation();
            }
        }

        private void ApplyInterpolatedPosition()
        {
            float threshold = Mathf.Max(Mathf.Abs(shakeAmount.Value) / 30f, 0.0001f);
            if ((targetTransform.localPosition - targetPosition).sqrMagnitude <= threshold * threshold)
            {
                targetPosition = originalPosition + CreatePositionOffset();
            }

            targetTransform.localPosition = Vector3.Lerp(
                targetTransform.localPosition,
                targetPosition,
                GetCurrentBlend());

            DecreaseRemainingTime();
        }

        private void ApplyRotation()
        {
            if (Quaternion.Angle(targetTransform.localRotation, targetRotation) <= 0.05f)
            {
                targetRotation = CreateRotationTarget();
            }

            targetTransform.localRotation = Quaternion.Slerp(
                targetTransform.localRotation,
                targetRotation,
                GetCurrentBlend());

            if (!loop.Value)
            {
                rotationStrength = Mathf.MoveTowards(
                    rotationStrength,
                    0f,
                    Time.deltaTime * Mathf.Max(Mathf.Abs(interpolationFactor), 0.01f));
            }
        }

        private Vector3 CreatePositionOffset()
        {
            Vector3 offset = Random.insideUnitSphere * shakeAmount.Value;

            // Moving a camera forward/backward during shake can push nearby ceiling meshes
            // through the near clip plane. Camera shake stays in the image plane instead.
            if (isCamera)
            {
                offset.z = 0f;
            }

            return offset;
        }

        private Quaternion CreateRotationTarget()
        {
            if (!rotate)
            {
                return originalRotation;
            }

            Vector3 eulerOffset = new Vector3(
                Random.Range(-rotationStrength, rotationStrength),
                Random.Range(-rotationStrength, rotationStrength),
                Random.Range(-rotationStrength, rotationStrength));

            return originalRotation * Quaternion.Euler(eulerOffset);
        }

        private float GetCurrentBlend()
        {
            if (useCurve && lerpCurve != null && lerpCurve.curve != null)
            {
                float duration = Mathf.Max(setTime.Value, 0.0001f);
                float elapsed = 1f - Mathf.Clamp01(remainingTime / duration);
                return Mathf.Clamp01(lerpCurve.curve.Evaluate(elapsed));
            }

            return Mathf.Clamp01(Mathf.Abs(interpolationFactor));
        }

        private void DecreaseRemainingTime()
        {
            if (!loop.Value)
            {
                remainingTime -= Time.deltaTime;
            }
        }

        private void RestoreTransform()
        {
            if (targetTransform == null)
            {
                return;
            }

            targetTransform.localPosition = originalPosition;
            targetTransform.localRotation = originalRotation;
        }

        private float GetInterpolation(float value, LerpInterpolationType type)
        {
            useInterpolation = type != LerpInterpolationType.Off;
            useCurve = type == LerpInterpolationType.Curve;

            switch (type)
            {
                case LerpInterpolationType.Quadratic:
                    return Time.timeSinceLevelLoad * setTime.Value;
                case LerpInterpolationType.EaseIn:
                    return 1f - Mathf.Cos(value * Mathf.PI * 0.5f);
                case LerpInterpolationType.EaseOut:
                    return Mathf.Sin(value * Mathf.PI * 0.5f);
                case LerpInterpolationType.Smoothstep:
                    return value * value * (3f - 2f * value);
                case LerpInterpolationType.Smootherstep:
                    return value * value * value * (value * (6f * value - 15f) + 10f);
                case LerpInterpolationType.DeltaTime:
                    return Time.deltaTime * value;
                case LerpInterpolationType.SimpleSine:
                    return value * Mathf.Sin(Time.timeSinceLevelLoad);
                case LerpInterpolationType.DoubleSine:
                    return value * Mathf.Sin(Time.timeSinceLevelLoad / Mathf.Max(setTime.Value, 0.0001f));
                case LerpInterpolationType.DoubleByHalfSine:
                    return value * 1.5f * Mathf.Sin(Time.timeSinceLevelLoad * setTime.Value);
                default:
                    return value;
            }
        }

        public override string ErrorCheck()
        {
            return gameObject == null ? "Need GameObject" : string.Empty;
        }
    }
}
