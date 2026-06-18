using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Sets a Dialogue Actor's subtitle panel number.")]
	public class SetDialogueActorPanel : FsmStateAction
	{
		[RequiredField]
		[Tooltip("GameObject with a Dialogue Actor component")]
		public FsmGameObject dialogueActor;

		[Tooltip("Subtitle panel number")]
		public SubtitlePanelNumber subtitlePanelNumber;

		[Tooltip("If Subtitle Panel Number is Custom, the custom panel to use")]
		public FsmGameObject customPanel;

		public override void Reset()
		{
			dialogueActor = null;
			subtitlePanelNumber = SubtitlePanelNumber.Default;
			customPanel = null;
		}

		public override void OnEnter()
		{
			if (dialogueActor == null || !(dialogueActor.Value != null))
			{
				LogError(string.Format("{0}: You must assign the Dialogue Actor and a panel number or custom panel.", "Dialogue System"));
			}
			else
			{
				DialogueActor dialogueActorComponent = DialogueActor.GetDialogueActorComponent(dialogueActor.Value.transform);
				if (dialogueActorComponent == null)
				{
					LogError(string.Format("{0}: The Dialogue Actor GameObject doesn't have a Dialogue Actor component.", "Dialogue System"));
				}
				else
				{
					dialogueActorComponent.standardDialogueUISettings.subtitlePanelNumber = subtitlePanelNumber;
					if (subtitlePanelNumber == SubtitlePanelNumber.Custom)
					{
						if (customPanel.Value == null)
						{
							LogError(string.Format("{0}: You must assign Custom Panel.", "Dialogue System"));
						}
						else
						{
							StandardUISubtitlePanel component = customPanel.Value.GetComponent<StandardUISubtitlePanel>();
							if (component == null)
							{
								LogError(string.Format("{0}: The Custom Panel GameObject doesn't have a Standard UI Subtitle Panel component.", "Dialogue System"));
							}
							else
							{
								dialogueActorComponent.standardDialogueUISettings.customSubtitlePanel = component;
								dialogueActorComponent.standardDialogueUISettings.subtitlePanelNumber = SubtitlePanelNumber.Custom;
							}
						}
					}
				}
			}
			Finish();
		}
	}
}
