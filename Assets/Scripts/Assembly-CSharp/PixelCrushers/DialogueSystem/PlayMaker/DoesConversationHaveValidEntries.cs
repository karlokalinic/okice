using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Checks whether a conversation currently has any valid entries branching from the start entry.")]
	public class DoesConversationHaveValidEntries : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The conversation containing the bark lines")]
		public FsmString conversation;

		[HutongGames.PlayMaker.Tooltip("The primary participant in the conversation (e.g., the player)")]
		public FsmGameObject actor;

		[HutongGames.PlayMaker.Tooltip("The other participant in the conversation (e.g., the NPC)")]
		public FsmGameObject conversant;

		[UIHint(UIHint.Variable)]
		[HutongGames.PlayMaker.Tooltip("Store the result in a Bool variable")]
		public FsmBool storeResult;

		public FsmEvent validEvent;

		public FsmEvent notValidEvent;

		public override void Reset()
		{
			if (conversation != null)
			{
				conversation.Value = string.Empty;
			}
			if (actor != null)
			{
				actor.Value = null;
			}
			if (conversant != null)
			{
				conversant.Value = null;
			}
			if (storeResult != null)
			{
				storeResult.Value = false;
			}
		}

		public override void OnEnter()
		{
			string obj = ((conversation != null) ? conversation.Value : string.Empty);
			Transform transform = ((actor != null && actor.Value != null) ? actor.Value.transform : null);
			Transform transform2 = ((conversant != null && conversant.Value != null) ? conversant.Value.transform : null);
			if (string.IsNullOrEmpty(obj))
			{
				LogWarning(string.Format("{0}: PlayMaker Action Does Conversation Have Valid Entries - conversation title is blank", "Dialogue System"));
			}
			bool flag = DialogueManager.ConversationHasValidEntry(obj, transform, transform2);
			if (storeResult != null)
			{
				storeResult.Value = flag;
			}
			if (flag)
			{
				base.Fsm.Event(validEvent);
			}
			else
			{
				base.Fsm.Event(notValidEvent);
			}
			Finish();
		}
	}
}
