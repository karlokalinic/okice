using System;
using System.Collections;
using UnityEngine;

namespace PixelCrushers
{
	public class Tweener : MonoBehaviour
	{
		public enum Easing
		{
			Linear = 0,
			EaseIn = 1,
			EaseOut = 2,
			EaseInOut = 3,
			EaseInElastic = 4,
			EaseOutElastic = 5,
			EaseInOutElastic = 6
		}

		private delegate T LerpFunction<T>(T from, T to, float current);

		private static Tweener instance;

		public static Tweener Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new GameObject("Tweener").AddComponent<Tweener>();
				}
				return instance;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void SubsystemRegistration()
		{
			instance = null;
		}

		private void Awake()
		{
			if (instance == null)
			{
				instance = this;
				UnityEngine.Object.DontDestroyOnLoad(this);
			}
			else
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		public static Coroutine Tween(float from, float to, float seconds, bool unscaledTime, Easing easing, Action onBegin, Action<float> onValue, Action onEnd)
		{
			return Instance.StartCoroutine(Instance.TweenCoroutine(from, to, seconds, unscaledTime, easing, (float f, float t, float c) => Mathf.Lerp(f, t, c), onBegin, onValue, onEnd));
		}

		public static Coroutine Tween(Vector2 from, Vector2 to, float seconds, bool unscaledTime, Easing easing, Action onBegin, Action<Vector2> onValue, Action onEnd)
		{
			return Instance.StartCoroutine(Instance.TweenCoroutine(from, to, seconds, unscaledTime, easing, (Vector2 f, Vector2 t, float c) => Vector2.Lerp(f, t, c), onBegin, onValue, onEnd));
		}

		public static Coroutine Tween(Vector3 from, Vector3 to, float seconds, bool unscaledTime, Easing easing, Action onBegin, Action<Vector3> onValue, Action onEnd)
		{
			return Instance.StartCoroutine(Instance.TweenCoroutine(from, to, seconds, unscaledTime, easing, (Vector3 f, Vector3 t, float c) => Vector3.Lerp(f, t, c), onBegin, onValue, onEnd));
		}

		public static Coroutine Tween(Quaternion from, Quaternion to, float seconds, bool unscaledTime, Easing easing, Action onBegin, Action<Quaternion> onValue, Action onEnd)
		{
			return Instance.StartCoroutine(Instance.TweenCoroutine(from, to, seconds, unscaledTime, easing, (Quaternion f, Quaternion t, float c) => Quaternion.Lerp(f, t, c), onBegin, onValue, onEnd));
		}

		public static Coroutine Tween(Color from, Color to, float seconds, bool unscaledTime, Easing easing, Action onBegin, Action<Color> onValue, Action onEnd)
		{
			return Instance.StartCoroutine(Instance.TweenCoroutine(from, to, seconds, unscaledTime, easing, (Color f, Color t, float c) => Color.Lerp(f, t, c), onBegin, onValue, onEnd));
		}

		private IEnumerator TweenCoroutine<T>(T from, T to, float seconds, bool unscaledTime, Easing easing, LerpFunction<T> lerpFunction, Action onBegin, Action<T> onValue, Action onEnd)
		{
			onBegin?.Invoke();
			if (onValue != null)
			{
				onValue(from);
				for (float elapsed = 0f; elapsed < seconds; elapsed += (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime))
				{
					float t = elapsed / seconds;
					float easingValue = GetEasingValue(easing, t);
					onValue(lerpFunction(from, to, easingValue));
					yield return null;
				}
				onValue(to);
			}
			onEnd?.Invoke();
		}

		private float GetEasingValue(Easing easing, float t)
		{
			switch (easing)
			{
			default:
				return t;
			case Easing.EaseIn:
				return t * t;
			case Easing.EaseOut:
				return t * (2f - t);
			case Easing.EaseInOut:
				if ((t *= 2f) < 1f)
				{
					return 0.5f * t * t;
				}
				return -0.5f * ((t -= 1f) * (t - 2f) - 1f);
			case Easing.EaseInElastic:
				if (t == 0f)
				{
					return 0f;
				}
				if (t == 1f)
				{
					return 1f;
				}
				return (0f - Mathf.Pow(2f, 10f * t - 10f)) * Mathf.Sin((t * 10f - 10.75f) * (MathF.PI * 2f / 3f));
			case Easing.EaseOutElastic:
				if (t == 0f)
				{
					return 0f;
				}
				if (t == 1f)
				{
					return 1f;
				}
				return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 10.75f) * (MathF.PI * 2f / 3f)) + 1f;
			case Easing.EaseInOutElastic:
				if (t == 0f)
				{
					return 0f;
				}
				if (t == 1f)
				{
					return 1f;
				}
				if (t < 0.5f)
				{
					return (0f - Mathf.Pow(2f, 20f * t - 10f) * Mathf.Sin((20f * t - 11.125f) * (MathF.PI * 4f / 9f))) / 2f;
				}
				return Mathf.Pow(2f, -20f * t + 10f) * Mathf.Sin((20f * t - 11.125f) * (MathF.PI * 4f / 9f)) / 2f + 1f;
			}
		}
	}
}
