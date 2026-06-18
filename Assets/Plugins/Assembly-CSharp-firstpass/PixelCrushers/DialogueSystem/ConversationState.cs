using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	public class ConversationState
	{
		public Subtitle subtitle;

		public Response[] npcResponses;

		public Response[] pcResponses;

		private static Dictionary<DialogueEntry, DialogueEntry> s_lastRandomlyChosenEntry;

		public bool hasNPCResponse
		{
			get
			{
				if (npcResponses != null)
				{
					return npcResponses.Length != 0;
				}
				return false;
			}
		}

		public Response firstNPCResponse
		{
			get
			{
				if (!hasNPCResponse)
				{
					return null;
				}
				return npcResponses[0];
			}
		}

		public bool hasPCResponses
		{
			get
			{
				if (pcResponses != null)
				{
					return pcResponses.Length != 0;
				}
				return false;
			}
		}

		public bool hasPCAutoResponse
		{
			get
			{
				if (pcResponses == null || pcResponses.Length == 0)
				{
					return false;
				}
				bool flag = false;
				for (int i = 0; i < pcResponses.Length; i++)
				{
					if (pcResponses[i].formattedText.forceMenu)
					{
						return false;
					}
					if (pcResponses[i].formattedText.forceAuto && pcResponses[i].enabled)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					return pcResponses.Length == 1;
				}
				return true;
			}
		}

		public bool hasForceAutoResponse
		{
			get
			{
				if (pcResponses == null || pcResponses.Length == 0)
				{
					return false;
				}
				for (int i = 0; i < pcResponses.Length; i++)
				{
					if (pcResponses[i].enabled && pcResponses[i].formattedText.forceAuto)
					{
						return true;
					}
				}
				return false;
			}
		}

		public Response pcAutoResponse
		{
			get
			{
				if (pcResponses == null || pcResponses.Length == 0)
				{
					return null;
				}
				for (int i = 0; i < pcResponses.Length; i++)
				{
					if (pcResponses[i].enabled && pcResponses[i].formattedText.forceAuto)
					{
						return pcResponses[i];
					}
				}
				return pcResponses[0];
			}
		}

		public bool hasAnyResponses
		{
			get
			{
				if (!hasNPCResponse)
				{
					return hasPCResponses;
				}
				return true;
			}
		}

		public bool isGroup { get; set; }

		public bool HasNPCResponse => hasNPCResponse;

		public Response FirstNPCResponse => firstNPCResponse;

		public bool HasPCResponses => hasPCResponses;

		public bool HasPCAutoResponse => hasPCAutoResponse;

		public Response PCAutoResponse => pcAutoResponse;

		public bool HasAnyResponses => hasAnyResponses;

		public bool IsGroup
		{
			get
			{
				return isGroup;
			}
			set
			{
				isGroup = value;
			}
		}

		public bool HasValidNPCResponse()
		{
			return AreAnyResponsesValid(npcResponses);
		}

		public bool HasValidPCResponses()
		{
			return AreAnyResponsesValid(pcResponses);
		}

		public ConversationState(Subtitle subtitle, Response[] npcResponses, Response[] pcResponses, bool isGroup = false)
		{
			this.subtitle = subtitle;
			this.npcResponses = npcResponses;
			this.pcResponses = pcResponses;
			this.isGroup = isGroup;
		}

		private bool AreAnyResponsesValid(Response[] responses)
		{
			if (responses == null)
			{
				return false;
			}
			foreach (Response response in responses)
			{
				if (response != null && response.enabled)
				{
					return true;
				}
			}
			return false;
		}

		public DialogueEntry GetRandomNPCEntry(bool noDuplicate = false)
		{
			if (!hasNPCResponse)
			{
				return null;
			}
			if (noDuplicate)
			{
				if (s_lastRandomlyChosenEntry == null)
				{
					s_lastRandomlyChosenEntry = new Dictionary<DialogueEntry, DialogueEntry>();
				}
				DialogueEntry destinationEntry;
				if (s_lastRandomlyChosenEntry.TryGetValue(subtitle.dialogueEntry, out var lastEntry))
				{
					List<Response> list = new List<Response>(npcResponses);
					list.RemoveAll((Response x) => x.destinationEntry == lastEntry);
					if (list.Count == 0)
					{
						return lastEntry;
					}
					destinationEntry = list[Random.Range(0, list.Count)].destinationEntry;
				}
				else
				{
					destinationEntry = npcResponses[Random.Range(0, npcResponses.Length)].destinationEntry;
				}
				s_lastRandomlyChosenEntry[subtitle.dialogueEntry] = destinationEntry;
				return destinationEntry;
			}
			return npcResponses[Random.Range(0, npcResponses.Length)].destinationEntry;
		}
	}
}
