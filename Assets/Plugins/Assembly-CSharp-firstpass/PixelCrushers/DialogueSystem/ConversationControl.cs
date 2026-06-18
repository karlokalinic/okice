using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	[AddComponentMenu("")]
	public class ConversationControl : MonoBehaviour
	{
		[Tooltip("Skip all subtitles until response menu or end of conversation is reached. Set by SkipAll().")]
		public bool skipAll;

		[Tooltip("Stop SkipAll() when unread subtitle is reached. You MUST tick Dialogue Manager's Include SimStatus checkbox to use this.")]
		public bool stopSkipAllOnUnreadSubtitle;

		[Tooltip("Stop SkipAll() when response menu is reached.")]
		public bool stopSkipAllOnResponseMenu = true;

		[Tooltip("Stop SkipAll() when end of conversation is reached.")]
		public bool stopSkipAllOnConversationEnd;

		[Tooltip("If Skip All is enabled, don't skip last conversation line.")]
		public bool dontSkipAllOnLastConversationLine;

		[Tooltip("Use this continue button mode when AutoPlay is on.")]
		public DisplaySettings.SubtitleSettings.ContinueButtonMode autoPlayOnContinueButton;

		[Tooltip("Use this continue button mode when AutoPlay is off.")]
		public DisplaySettings.SubtitleSettings.ContinueButtonMode autoPlayOffContinueButton = DisplaySettings.SubtitleSettings.ContinueButtonMode.Always;

		protected AbstractDialogueUI dialogueUI;

		protected bool mustStopAtCurrentUnreadEntry;

		protected bool hasStarted;

		protected virtual void Awake()
		{
			dialogueUI = GetComponent<AbstractDialogueUI>() ?? DialogueManager.standardDialogueUI ?? GameObjectUtility.FindFirstObjectByType<AbstractDialogueUI>();
		}

		protected virtual void Start()
		{
			if (stopSkipAllOnUnreadSubtitle)
			{
				if (!DialogueLua.includeSimStatus)
				{
					Debug.LogWarning("Dialogue System: Dialogue Manager's Include SimStatus isn't ticked but it requires for Stop Skip All On Unread Subtitle. Enabling SimStatus.");
					DialogueLua.includeSimStatus = true;
				}
				DialogueManager.instance.preparingConversationLine -= OnPreparingConversationLine;
				DialogueManager.instance.preparingConversationLine += OnPreparingConversationLine;
			}
			hasStarted = true;
		}

		protected virtual void OnEnable()
		{
			if (hasStarted)
			{
				DialogueManager.instance.preparingConversationLine -= OnPreparingConversationLine;
				DialogueManager.instance.preparingConversationLine += OnPreparingConversationLine;
			}
		}

		protected virtual void OnDisable()
		{
			DialogueManager.instance.preparingConversationLine -= OnPreparingConversationLine;
		}

		public virtual void ToggleAutoPlay()
		{
			DisplaySettings.SubtitleSettings.ContinueButtonMode continueButtonMode = ((DialogueManager.displaySettings.subtitleSettings.continueButton == autoPlayOnContinueButton) ? autoPlayOffContinueButton : autoPlayOnContinueButton);
			DialogueManager.displaySettings.subtitleSettings.continueButton = continueButtonMode;
			if (continueButtonMode == autoPlayOnContinueButton)
			{
				dialogueUI.OnContinueConversation();
				return;
			}
			DialogueManager.SetContinueMode(value: true);
			DialogueManager.displaySettings.subtitleSettings.continueButton = autoPlayOffContinueButton;
		}

		public virtual void SkipAll()
		{
			skipAll = true;
			if (dialogueUI != null)
			{
				dialogueUI.OnContinueConversation();
			}
		}

		public virtual void StopSkipAll()
		{
			skipAll = false;
		}

		protected virtual void OnPreparingConversationLine(DialogueEntry entry)
		{
			if (!stopSkipAllOnUnreadSubtitle)
			{
				mustStopAtCurrentUnreadEntry = false;
			}
			else
			{
				mustStopAtCurrentUnreadEntry = DialogueLua.GetSimStatus(entry) == "Untouched";
			}
		}

		public virtual void OnConversationLine(Subtitle subtitle)
		{
			if (skipAll && (!dontSkipAllOnLastConversationLine || DialogueManager.currentConversationState.hasAnyResponses) && !mustStopAtCurrentUnreadEntry)
			{
				subtitle.sequence = "Continue(); " + subtitle.sequence;
			}
		}

		public virtual void OnConversationResponseMenu(Response[] responses)
		{
			if (skipAll)
			{
				if (stopSkipAllOnResponseMenu)
				{
					skipAll = false;
				}
				if (dialogueUI != null)
				{
					dialogueUI.ShowSubtitle(DialogueManager.currentConversationState.subtitle);
				}
			}
		}

		public virtual void OnConversationEnd(Transform actor)
		{
			if (stopSkipAllOnConversationEnd)
			{
				skipAll = false;
			}
		}
	}
}
