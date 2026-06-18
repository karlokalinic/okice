using HutongGames.PlayMaker;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Makes an NPC bark.")]
	public class Bark : FsmStateAction
	{
		public enum BarkSource
		{
			Conversation = 0,
			Text = 1
		}

		public BarkSource barkSource;

		[HutongGames.PlayMaker.Tooltip("Bark this text")]
		public FsmString barkText;

		[HutongGames.PlayMaker.Tooltip("Play this sequence with the bark")]
		public FsmString barkSequence;

		[HutongGames.PlayMaker.Tooltip("The conversation containing the bark lines")]
		public FsmString conversation;

		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The character speaking the bark")]
		public FsmGameObject speaker;

		[HutongGames.PlayMaker.Tooltip("The character being barked at (optional)")]
		public FsmGameObject listener;

		public override void Reset()
		{
			barkSource = BarkSource.Conversation;
			if (barkText != null)
			{
				barkText.Value = string.Empty;
			}
			if (barkSequence != null)
			{
				barkSequence.Value = string.Empty;
			}
			if (conversation != null)
			{
				conversation.Value = string.Empty;
			}
			if (speaker != null)
			{
				speaker.Value = null;
			}
			if (listener != null)
			{
				listener.Value = null;
			}
		}

		public override void OnEnter()
		{
			Transform transform = ((speaker != null && speaker.Value != null) ? speaker.Value.transform : null);
			Transform transform2 = ((listener != null && listener.Value != null) ? listener.Value.transform : null);
			if (transform == null)
			{
				Debug.LogWarning(string.Format("{0}: PlayMaker Action Bark - speaker is null", "Dialogue System"));
			}
			switch (barkSource)
			{
			case BarkSource.Conversation:
			{
				string text = ((conversation != null) ? conversation.Value : string.Empty);
				if (string.IsNullOrEmpty(text))
				{
					Debug.LogWarning(string.Format("{0}: PlayMaker Action Bark - conversation title is blank", "Dialogue System"));
				}
				if (transform2 != null)
				{
					DialogueManager.Bark(text, transform, transform2);
				}
				else
				{
					DialogueManager.Bark(text, transform);
				}
				break;
			}
			case BarkSource.Text:
			{
				string value = ((barkText != null) ? barkText.Value : string.Empty);
				string sequence = ((barkSequence != null) ? barkSequence.Value : string.Empty);
				if (string.IsNullOrEmpty(value))
				{
					Debug.LogWarning(string.Format("{0}: PlayMaker Action Bark - Bark Text is blank", "Dialogue System"));
				}
				DialogueManager.BarkString(value, transform, transform2, sequence);
				break;
			}
			}
			Finish();
		}
	}
}
