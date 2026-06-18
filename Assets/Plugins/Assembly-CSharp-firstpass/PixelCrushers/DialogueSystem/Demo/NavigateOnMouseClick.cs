using UnityEngine;

namespace PixelCrushers.DialogueSystem.Demo
{
	[AddComponentMenu("")]
	public class NavigateOnMouseClick : MonoBehaviour
	{
		public enum MouseButtonType
		{
			Left = 0,
			Right = 1,
			Middle = 2
		}

		public string animatorSpeedParameter = "Speed";

		public float stoppingDistance = 0.5f;

		public MouseButtonType mouseButton;

		public bool ignoreClicksOnUI = true;
	}
}
