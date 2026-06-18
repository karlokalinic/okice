namespace HutongGames.PlayMaker.ActionsInternal
{
	[ActionCategory(ActionCategory.PlayMakerInternal)]
	[Tooltip("Large header used to organize actions list. Double-click to edit.")]
	public class Header : FsmStateAction
	{
		[UIHint(UIHint.Comment)]
		public string comment;

		public int colorId;

		public override void Reset()
		{
			comment = "Double-click to edit comment.";
		}

		public override void Awake()
		{
			base.Enabled = false;
		}
	}
}
