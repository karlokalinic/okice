using System;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	public class CharacterInfo
	{
		public int id;

		public string nameInDatabase;

		public CharacterType characterType;

		public Transform transform;

		public Sprite portrait;

		private static Dictionary<string, Transform> registeredActorTransforms = new Dictionary<string, Transform>();

		public bool isPlayer => characterType == CharacterType.PC;

		public bool isNPC => characterType == CharacterType.NPC;

		public string Name { get; set; }

		public bool IsPlayer => isPlayer;

		public bool IsNPC => isNPC;

		public CharacterInfo(int id, string nameInDatabase, Transform transform, CharacterType characterType, Sprite portrait)
		{
			this.id = id;
			this.nameInDatabase = nameInDatabase;
			this.characterType = characterType;
			this.portrait = portrait;
			this.transform = transform;
			if (transform == null && !string.IsNullOrEmpty(nameInDatabase))
			{
				GameObject gameObject = SequencerTools.FindSpecifier(nameInDatabase, onlyActiveInScene: true);
				if (gameObject != null)
				{
					this.transform = gameObject.transform;
				}
			}
			DialogueActor dialogueActorComponent = DialogueActor.GetDialogueActorComponent(transform);
			if (dialogueActorComponent == null)
			{
				Name = GetLocalizedDisplayNameInDatabase(nameInDatabase);
				return;
			}
			Name = dialogueActorComponent.GetActorName();
			Actor actor = DialogueManager.masterDatabase.GetActor(dialogueActorComponent.actor);
			Sprite portraitSprite = dialogueActorComponent.GetPortraitSprite();
			if (portraitSprite != null)
			{
				this.portrait = portraitSprite;
			}
			else if (actor != null && portrait == null)
			{
				this.portrait = actor.GetPortraitSprite();
			}
		}

		public static string GetLocalizedDisplayNameInDatabase(string nameInDatabase)
		{
			string text = DialogueLua.GetLocalizedActorField(nameInDatabase, "Display Name").asString;
			if (string.IsNullOrEmpty(text) || string.Equals(text, "nil"))
			{
				text = DialogueLua.GetLocalizedActorField(nameInDatabase, "Name").asString;
			}
			if (string.IsNullOrEmpty(text) || string.Equals(text, "nil"))
			{
				text = nameInDatabase;
			}
			return FormattedText.ParseCode(text);
		}

		public Sprite GetPicOverride(int picNum)
		{
			if (picNum < 2)
			{
				return portrait;
			}
			int num = picNum - 2;
			Actor actor = DialogueManager.masterDatabase.GetActor(id);
			if (actor == null || num >= actor.alternatePortraits.Count)
			{
				if (actor == null || num >= actor.spritePortraits.Count)
				{
					return portrait;
				}
				return actor.spritePortraits[num];
			}
			return UITools.CreateSprite(actor.alternatePortraits[num]);
		}

		public Field GetField(string title)
		{
			return DialogueManager.masterDatabase.GetActor(id)?.fields.Find((Field field) => field.title == title);
		}

		public string GetFieldText(string title)
		{
			Field field = GetField(title);
			if (field == null)
			{
				return string.Empty;
			}
			return field.value;
		}

		public bool GetFieldBool(string title)
		{
			Field field = GetField(title);
			if (field == null)
			{
				return false;
			}
			return string.Equals(field.value, "true", StringComparison.OrdinalIgnoreCase);
		}

		public int GetFieldInt(string title)
		{
			Field field = GetField(title);
			if (field == null)
			{
				return 0;
			}
			return SafeConvert.ToInt(field.value);
		}

		public float GetFieldFloat(string title)
		{
			Field field = GetField(title);
			if (field == null)
			{
				return 0f;
			}
			return SafeConvert.ToFloat(field.value);
		}

		public static void RegisterActorTransform(string actorName, Transform actorTransform)
		{
			if (string.IsNullOrEmpty(actorName) || actorTransform == null)
			{
				return;
			}
			if (registeredActorTransforms.ContainsKey(actorName))
			{
				if (DialogueDebug.logInfo)
				{
					Debug.LogWarning("Dialogue System: Registering transform " + actorTransform.name + " as actor '" + actorName + "' but another transform is already registered. Overwriting with new transform.", actorTransform);
				}
				registeredActorTransforms[actorName] = actorTransform;
			}
			else
			{
				if (DialogueDebug.logInfo)
				{
					Debug.Log("Dialogue System: Registering transform " + actorTransform.name + " as actor '" + actorName + "'.", actorTransform);
				}
				registeredActorTransforms.Add(actorName, actorTransform);
			}
			if (!DialogueManager.hasInstance)
			{
				if (DialogueDebug.logWarnings)
				{
					Debug.LogWarning("Dialogue System: CharacterInfo.RegisterActorTransform(" + actorName + ") can't update active conversations' caches because there no Dialogue Manager is present.");
				}
				return;
			}
			Actor actor = (DialogueManager.hasInstance ? DialogueManager.masterDatabase.GetActor(actorName) : null);
			if (actor == null)
			{
				return;
			}
			foreach (ActiveConversationRecord activeConversation in DialogueManager.instance.activeConversations)
			{
				activeConversation.conversationModel.OverrideCharacterInfo(actor.id, actorTransform);
			}
		}

		public static void UnregisterActorTransform(string actorName, Transform actorTransform)
		{
			if (!string.IsNullOrEmpty(actorName) && !(actorTransform == null) && registeredActorTransforms.ContainsKey(actorName))
			{
				if (DialogueDebug.logInfo)
				{
					Debug.Log("Dialogue System: Unregistering transform " + actorTransform.name + " from actor '" + actorName + "'.", actorTransform);
				}
				registeredActorTransforms.Remove(actorName);
			}
		}

		public static Transform GetRegisteredActorTransform(string actorName)
		{
			if (!registeredActorTransforms.ContainsKey(actorName))
			{
				return null;
			}
			return registeredActorTransforms[actorName];
		}

		public static List<Transform> GetAllRegisteredActorTransforms()
		{
			return new List<Transform>(registeredActorTransforms.Values);
		}
	}
}
