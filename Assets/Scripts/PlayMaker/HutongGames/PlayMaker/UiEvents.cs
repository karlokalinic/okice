using System;

namespace HutongGames.PlayMaker
{
	[Serializable]
	[Flags]
	public enum UiEvents
	{
		None = 0,
		Click = 1,
		BeginDrag = 2,
		Drag = 4,
		EndDrag = 8,
		Drop = 0x10,
		PointerClick = 0x20,
		PointerDown = 0x40,
		PointerEnter = 0x80,
		PointerExit = 0x100,
		PointerUp = 0x200,
		EndEdit = 0x400,
		BoolValueChanged = 0x800,
		FloatValueChanged = 0x1000,
		IntValueChanged = 0x2000,
		Vector2ValueChanged = 0x4000,
		DragEvents = 0xE,
		PointerEvents = 0x3E0
	}
}
