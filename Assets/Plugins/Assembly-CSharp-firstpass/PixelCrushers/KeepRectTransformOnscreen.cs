using UnityEngine;

namespace PixelCrushers
{
	[AddComponentMenu("")]
	[RequireComponent(typeof(RectTransform))]
	public class KeepRectTransformOnscreen : MonoBehaviour
	{
		private RectTransform rectTransform;

		private float originalX;

		private bool applied;

		private Camera mainCamera;

		private void Start()
		{
			rectTransform = GetComponent<RectTransform>();
			originalX = rectTransform.position.x;
			mainCamera = Camera.main;
		}

		private void OnEnable()
		{
			applied = false;
			RestoreOriginalPosition();
		}

		private void LateUpdate()
		{
			if (!(mainCamera == null) && !(rectTransform == null))
			{
				Vector3[] array = new Vector3[4];
				rectTransform.GetWorldCorners(array);
				Vector3 position = mainCamera.WorldToViewportPoint(rectTransform.position);
				Vector3 vector = mainCamera.WorldToViewportPoint(array[0]);
				Vector3 vector2 = mainCamera.WorldToViewportPoint(array[2]);
				float num = 0f;
				if (vector2.x > 1f)
				{
					num = vector2.x - 1f;
				}
				else if (vector.x < 0f)
				{
					num = vector.x;
				}
				if (num != 0f)
				{
					position.x = Mathf.Clamp(position.x - num, 0f, 1f);
					rectTransform.position = mainCamera.ViewportToWorldPoint(position);
					applied = true;
				}
				else if (!applied)
				{
					RestoreOriginalPosition();
				}
			}
		}

		private void RestoreOriginalPosition()
		{
			if (!(mainCamera == null) && !(rectTransform == null))
			{
				rectTransform.position = new Vector3(originalX, rectTransform.position.y, rectTransform.position.z);
				Vector3 position = mainCamera.WorldToViewportPoint(rectTransform.position);
				rectTransform.position = mainCamera.ViewportToWorldPoint(position);
			}
		}
	}
}
