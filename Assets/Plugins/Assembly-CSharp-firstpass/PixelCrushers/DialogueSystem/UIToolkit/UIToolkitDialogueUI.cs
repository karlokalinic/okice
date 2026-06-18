using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PixelCrushers.DialogueSystem.UIToolkit
{
	public class UIToolkitDialogueUI : AbstractDialogueUI, IDialogueUI
	{
		[SerializeField]
		private UIToolkitRootElements rootElements;

		[SerializeField]
		private UIToolkitAlertElements alertElements;

		[SerializeField]
		private UIToolkitDialogueElements dialogueElements;

		[SerializeField]
		private UIToolkitQTEElements qteElements;

		public override AbstractUIRoot uiRootControls => rootElements;

		public override AbstractDialogueUIControls dialogueControls => dialogueElements;

		public override AbstractUIQTEControls qteControls => qteElements;

		public override AbstractUIAlertControls alertControls => alertElements;

		public override void Awake()
		{
			base.Awake();
			dialogueElements.Initialize(OnContinueConversation, OnClick);
		}

		public override void Update()
		{
			base.Update();
			(dialogueElements.responseMenuControls as UIToolkitResponseMenuElements).DoUpdate();
		}

		public override void Open()
		{
			base.Open();
			OpenSubtitlePanelsOnStart();
		}

		public override void ShowSubtitle(Subtitle subtitle)
		{
			UIToolkitSubtitleElements subtitlePanel = GetSubtitlePanel(subtitle);
			if (subtitlePanel != null)
			{
				HideOtherApplicablePanels(subtitlePanel);
				subtitlePanel.ShowSubtitle(subtitle);
			}
		}

		public override void HideSubtitle(Subtitle subtitle)
		{
			UIToolkitSubtitleElements subtitlePanel = GetSubtitlePanel(subtitle);
			if (subtitlePanel != null && !subtitlePanel.ShouldStayVisible)
			{
				subtitlePanel.Hide();
			}
		}

		protected virtual UIToolkitSubtitleElements GetSubtitlePanel(int index)
		{
			if (0 > index || index >= dialogueElements.SubtitlePanelElements.Count)
			{
				return null;
			}
			return dialogueElements.SubtitlePanelElements[index];
		}

		protected virtual UIToolkitSubtitleElements GetSubtitlePanel(Subtitle subtitle)
		{
			if (subtitle == null)
			{
				return null;
			}
			UIToolkitSubtitleElements subtitlePanel = GetSubtitlePanel(subtitle.formattedText.subtitlePanelNumber);
			if (subtitlePanel != null)
			{
				return subtitlePanel;
			}
			DialogueActor dialogueActorComponent = DialogueActor.GetDialogueActorComponent(subtitle.speakerInfo.transform);
			subtitlePanel = GetDialogueActorSubtitlePanel(dialogueActorComponent);
			if (subtitlePanel != null)
			{
				return subtitlePanel;
			}
			if (!subtitle.speakerInfo.isNPC)
			{
				return dialogueElements.pcSubtitleControls as UIToolkitSubtitleElements;
			}
			return dialogueElements.npcSubtitleControls as UIToolkitSubtitleElements;
		}

		protected virtual UIToolkitSubtitleElements GetDialogueActorSubtitlePanel(DialogueActor dialogueActor)
		{
			if (dialogueActor != null && dialogueActor.standardDialogueUISettings.subtitlePanelNumber != SubtitlePanelNumber.Default)
			{
				int subtitlePanelIndex = PanelNumberUtility.GetSubtitlePanelIndex(dialogueActor.standardDialogueUISettings.subtitlePanelNumber);
				return GetSubtitlePanel(subtitlePanelIndex);
			}
			return null;
		}

		protected virtual void OpenSubtitlePanelsOnStart()
		{
			Conversation conversation = DialogueManager.masterDatabase.GetConversation(DialogueManager.lastConversationStarted);
			if (conversation != null)
			{
				HashSet<UIToolkitSubtitleElements> checkedPanels = new HashSet<UIToolkitSubtitleElements>();
				HashSet<int> checkedActorIDs = new HashSet<int>();
				int actorID = conversation.ActorID;
				Actor actor = DialogueManager.masterDatabase.GetActor(DialogueActor.GetActorName(DialogueManager.currentActor));
				if (actor != null)
				{
					actorID = actor.id;
				}
				CheckActorIDOnStartConversation(actorID, checkedActorIDs, checkedPanels);
				CheckActorIDOnStartConversation(conversation.ConversantID, checkedActorIDs, checkedPanels);
				for (int i = 0; i < conversation.dialogueEntries.Count; i++)
				{
					int actorID2 = conversation.dialogueEntries[i].ActorID;
					CheckActorIDOnStartConversation(actorID2, checkedActorIDs, checkedPanels);
				}
			}
		}

		protected virtual void CheckActorIDOnStartConversation(int actorID, HashSet<int> checkedActorIDs, HashSet<UIToolkitSubtitleElements> checkedPanels)
		{
			if (checkedActorIDs.Contains(actorID))
			{
				return;
			}
			checkedActorIDs.Add(actorID);
			Actor actor = DialogueManager.MasterDatabase.GetActor(actorID);
			if (actor == null)
			{
				return;
			}
			Transform actorTransform = GetActorTransform(actor.Name);
			UIToolkitSubtitleElements defaultPanel = (actor.IsPlayer ? dialogueElements.PCSubtitleElements : dialogueElements.NPCSubtitleElements);
			DialogueActor dialogueActor;
			UIToolkitSubtitleElements actorTransformPanel = GetActorTransformPanel(actorTransform, defaultPanel, out dialogueActor);
			if (actorTransformPanel == null && actorTransform == null && Debug.isDebugBuild)
			{
				Debug.LogWarning("Dialogue System: Can't determine what subtitle panel to use for " + actor.Name, actorTransform);
			}
			if (actorTransformPanel != null && !checkedPanels.Contains(actorTransformPanel))
			{
				checkedPanels.Add(actorTransformPanel);
				if (actorTransformPanel.Visibility == UIVisibility.AlwaysFromStart)
				{
					Sprite portraitSprite = ((dialogueActor != null && dialogueActor.GetPortraitSprite() != null) ? dialogueActor.GetPortraitSprite() : actor.GetPortraitSprite());
					string localizedDisplayNameInDatabase = CharacterInfo.GetLocalizedDisplayNameInDatabase(actor.Name);
					actorTransformPanel.OpenOnStartConversation(portraitSprite, localizedDisplayNameInDatabase, dialogueActor);
				}
			}
		}

		protected virtual Transform GetActorTransform(string actorName)
		{
			Transform registeredActorTransform = CharacterInfo.GetRegisteredActorTransform(actorName);
			if (registeredActorTransform == null)
			{
				GameObject gameObject = GameObject.Find(actorName);
				if (gameObject != null)
				{
					registeredActorTransform = gameObject.transform;
				}
			}
			return registeredActorTransform;
		}

		public virtual UIToolkitSubtitleElements GetActorTransformPanel(Transform speakerTransform, UIToolkitSubtitleElements defaultPanel, out DialogueActor dialogueActor)
		{
			dialogueActor = null;
			if (speakerTransform == null)
			{
				return defaultPanel;
			}
			dialogueActor = DialogueActor.GetDialogueActorComponent(speakerTransform);
			if (dialogueActor != null && dialogueActor.standardDialogueUISettings.subtitlePanelNumber != SubtitlePanelNumber.Default)
			{
				UIToolkitSubtitleElements dialogueActorSubtitlePanel = GetDialogueActorSubtitlePanel(dialogueActor);
				if (dialogueActorSubtitlePanel != null)
				{
					return dialogueActorSubtitlePanel;
				}
			}
			return defaultPanel;
		}

		protected virtual void HideOtherApplicablePanels(UIToolkitSubtitleElements panel)
		{
			foreach (UIToolkitSubtitleElements subtitlePanelElement in dialogueElements.SubtitlePanelElements)
			{
				if (!subtitlePanelElement.IsSamePanel(panel) && !subtitlePanelElement.ShouldStayVisible)
				{
					subtitlePanelElement.Hide();
				}
			}
		}

		public override void ShowResponses(Subtitle subtitle, Response[] responses, float timeout)
		{
			base.ShowResponses(subtitle, responses, timeout);
		}

		public static void SetDisplay(VisualElement visualElement, bool value)
		{
			if (visualElement != null)
			{
				visualElement.style.display = ((!value) ? DisplayStyle.None : DisplayStyle.Flex);
			}
		}

		public static bool IsVisible(VisualElement visualElement)
		{
			if (visualElement == null)
			{
				return false;
			}
			return visualElement.style.display != DisplayStyle.None;
		}

		public static T GetVisualElement<T>(UIDocument document, string visualElementName) where T : VisualElement
		{
			if (document == null || document.rootVisualElement == null)
			{
				return null;
			}
			return document.rootVisualElement.Q<T>(visualElementName);
		}

		public static void SetInteractable(VisualElement rootVisualElement, bool value)
		{
			if (rootVisualElement != null)
			{
				rootVisualElement.pickingMode = ((!value) ? PickingMode.Ignore : PickingMode.Position);
			}
		}
	}
}
