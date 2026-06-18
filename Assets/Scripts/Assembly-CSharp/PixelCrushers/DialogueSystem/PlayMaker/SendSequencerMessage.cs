using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Send a message to the sequencer.")]
	public class SendSequencerMessage : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The message to send to the sequencer")]
		public FsmString message;

		public override void Reset()
		{
			if (message != null)
			{
				message.Value = string.Empty;
			}
		}

		public override void OnEnter()
		{
			if (message != null)
			{
				Sequencer.Message(message.Value);
			}
			Finish();
		}
	}
}
