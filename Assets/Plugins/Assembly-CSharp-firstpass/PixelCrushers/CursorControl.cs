using UnityEngine;

namespace PixelCrushers
{
	public static class CursorControl
	{
		public static CursorLockMode cursorLockMode { get; set; } = CursorLockMode.Locked;

		public static bool isCursorActive
		{
			get
			{
				if (isCursorVisible)
				{
					return !isCursorLocked;
				}
				return false;
			}
		}

		public static bool isCursorVisible => Cursor.visible;

		public static bool isCursorLocked => Cursor.lockState != CursorLockMode.None;

		public static void SetCursorActive(bool value)
		{
			ShowCursor(value);
			LockCursor(!value);
		}

		public static void ShowCursor(bool value)
		{
			Cursor.visible = value;
		}

		public static void LockCursor(bool value)
		{
			if (!value && isCursorLocked)
			{
				cursorLockMode = Cursor.lockState;
			}
			Cursor.lockState = (value ? cursorLockMode : CursorLockMode.None);
		}
	}
}
