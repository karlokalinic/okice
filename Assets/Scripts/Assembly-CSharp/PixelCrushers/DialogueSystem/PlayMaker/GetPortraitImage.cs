using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Gets an actor's portrait image.")]
	public class GetPortraitImage : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The actor's GameObject")]
		public FsmOwnerDefault gameObject = new FsmOwnerDefault();

		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("Portrait number to get, or zero for the current portrait")]
		public FsmInt portraitNumber = new FsmInt();

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("The portrait image")]
		public FsmTexture storeTextureResult = new FsmTexture();

		public override void Reset()
		{
			gameObject = null;
			portraitNumber = 0;
			storeTextureResult = null;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(gameObject);
			if (storeTextureResult != null)
			{
				storeTextureResult.Value = null;
				if (ownerDefaultTarget != null)
				{
					Actor actor = DialogueManager.MasterDatabase.GetActor(DialogueActor.GetActorName(ownerDefaultTarget.transform));
					if (actor != null)
					{
						Sprite sprite = null;
						sprite = ((portraitNumber.Value != 0) ? actor.GetPortraitSprite(portraitNumber.Value) : actor.GetPortraitSprite(1));
						storeTextureResult.Value = ((sprite != null) ? sprite.texture : null);
					}
				}
			}
			Finish();
		}
	}
}
