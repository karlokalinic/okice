using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Gets the actor name of a GameObject.")]
	public class GetActorName : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The GameObject for which to get the name")]
		public FsmOwnerDefault gameObject = new FsmOwnerDefault();

		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("Get the internal name used for the save system")]
		public FsmBool getInternalName = new FsmBool();

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("The actor name as a string")]
		public FsmString storeStringResult = new FsmString();

		public override void Reset()
		{
			gameObject = null;
			getInternalName = false;
			storeStringResult = null;
		}

		public override void OnEnter()
		{
			GameObject ownerDefaultTarget = base.Fsm.GetOwnerDefaultTarget(this.gameObject);
			if (storeStringResult != null)
			{
				if (ownerDefaultTarget == null || ownerDefaultTarget.gameObject == null)
				{
					storeStringResult.Value = null;
				}
				else
				{
					GameObject gameObject = ownerDefaultTarget.gameObject;
					if (getInternalName == null || !getInternalName.Value)
					{
						storeStringResult.Value = DialogueActor.GetActorName(gameObject.transform);
					}
					else
					{
						storeStringResult.Value = DialogueActor.GetPersistentDataName(gameObject.transform);
					}
				}
			}
			Finish();
		}
	}
}
