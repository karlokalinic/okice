using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{
	public class DialogueDatabaseLocalizationImporter
	{
		[Serializable]
		public class LocalizationLanguages
		{
			public List<string> languages = new List<string>();

			public List<string> extraEntryFields = new List<string>();

			public List<string> extraQuestFields = new List<string>();

			public List<string> extraItemFields = new List<string>();

			public int importMainTextIndex = -1;

			public string outputFolder;
		}

		private LocalizationLanguages localizationLanguages = new LocalizationLanguages();

		private string localizationKeyField = "Articy Id";

		private Dictionary<string, int> conversationIDCache = new Dictionary<string, int>();

		private Conversation lastCachedConversation;

		public void ImportLocalizationFilesFromFolder(DialogueDatabase database, string folderName, LocalizationLanguages localizationLanguages, bool exportLocalizationConversationTitle = false, bool exportLocalizationCreateNewFields = false, string localizationKeyField = null)
		{
			this.localizationLanguages = localizationLanguages;
			this.localizationKeyField = localizationKeyField;
			bool flag = !string.IsNullOrEmpty(localizationKeyField);
			conversationIDCache.Clear();
			lastCachedConversation = null;
			localizationLanguages.outputFolder = folderName;
			conversationIDCache.Clear();
			lastCachedConversation = null;
			int count = localizationLanguages.languages.Count;
			for (int i = 0; i < count; i++)
			{
				_ = (float)i / (float)count;
				string text = localizationLanguages.languages[i];
				bool flag2 = localizationLanguages.importMainTextIndex == i;
				string text2 = localizationLanguages.outputFolder + "/Actors_" + text + ".csv";
				List<string> list = ReadCSV(text2);
				CombineMultilineCSVSourceLines(list);
				for (int j = 2; j < list.Count; j++)
				{
					List<string> cSVColumnsFromLine = GetCSVColumnsFromLine(list[j]);
					if (cSVColumnsFromLine.Count < 3)
					{
						Debug.LogError(text2 + ":" + (j + 1) + " Invalid line: " + list[j]);
						continue;
					}
					string text3 = cSVColumnsFromLine[0].Trim();
					string value = cSVColumnsFromLine[1].Trim();
					string value2 = cSVColumnsFromLine[2].Trim();
					Actor actor = database.GetActor(text3);
					if (actor == null)
					{
						Debug.LogError(text2 + ": No actor in database is named '" + text3 + "'.");
						continue;
					}
					Field.SetValue(actor.fields, "Display Name " + text, value2);
					if (flag2 && !string.IsNullOrEmpty(value))
					{
						Field.SetValue(actor.fields, "Display Name", value);
					}
				}
				text2 = localizationLanguages.outputFolder + "/Dialogue_" + text + ".csv";
				list = ReadCSV(text2);
				CombineMultilineCSVSourceLines(list);
				for (int k = 2; k < list.Count; k++)
				{
					List<string> cSVColumnsFromLine2 = GetCSVColumnsFromLine(list[k]);
					if (cSVColumnsFromLine2.Count < 7)
					{
						Debug.LogError(text2 + ":" + (k + 1) + " Invalid line: " + list[k]);
						continue;
					}
					string keyFieldValue = null;
					if (flag)
					{
						keyFieldValue = cSVColumnsFromLine2[0];
						cSVColumnsFromLine2.RemoveAt(0);
					}
					int num = 0;
					if (exportLocalizationConversationTitle)
					{
						string key = cSVColumnsFromLine2[0];
						if (!conversationIDCache.ContainsKey(key))
						{
							Conversation conversation = database.GetConversation(cSVColumnsFromLine2[0]);
							if (conversation == null)
							{
								Debug.LogError(text2 + ":" + (k + 1) + " Database doesn't contain conversation '" + cSVColumnsFromLine2[0] + "'.");
								continue;
							}
							conversationIDCache[key] = conversation.id;
						}
						num = conversationIDCache[key];
					}
					else
					{
						num = Tools.StringToInt(cSVColumnsFromLine2[0]);
					}
					int dialogueEntryID = Tools.StringToInt(cSVColumnsFromLine2[1]);
					DialogueEntry dialogueEntry = null;
					if (flag)
					{
						if (lastCachedConversation == null || lastCachedConversation.id != num)
						{
							lastCachedConversation = database.GetConversation(num);
						}
						dialogueEntry = lastCachedConversation.dialogueEntries.Find((DialogueEntry x) => Field.LookupValue(x.fields, localizationKeyField) == keyFieldValue);
					}
					else
					{
						dialogueEntry = database.GetDialogueEntry(num, dialogueEntryID);
						if (dialogueEntry == null)
						{
							Debug.LogError(text2 + ":" + (k + 1) + " Database doesn't contain conversation " + num + " dialogue entry " + dialogueEntryID);
						}
					}
					if (dialogueEntry == null)
					{
						continue;
					}
					Field.SetValue(dialogueEntry.fields, text, cSVColumnsFromLine2[4], FieldType.Localization);
					Field.SetValue(dialogueEntry.fields, "Menu Text " + text, cSVColumnsFromLine2[6], FieldType.Localization);
					if (flag2)
					{
						dialogueEntry.DialogueText = cSVColumnsFromLine2[3];
						dialogueEntry.MenuText = cSVColumnsFromLine2[5];
					}
					for (int num2 = 0; num2 < localizationLanguages.extraEntryFields.Count; num2++)
					{
						string text4 = localizationLanguages.extraEntryFields[num2];
						int num3 = 8 + num2 * 2 + 1;
						if (!string.IsNullOrEmpty(text4) && (exportLocalizationCreateNewFields || Field.FieldExists(dialogueEntry.fields, text4) || !string.IsNullOrEmpty(cSVColumnsFromLine2[num3 - 1])))
						{
							Field.SetValue(dialogueEntry.fields, text4 + " " + text, cSVColumnsFromLine2[num3]);
							if (flag2)
							{
								Field.SetValue(dialogueEntry.fields, text4, cSVColumnsFromLine2[num3 - 1]);
							}
						}
					}
				}
				text2 = localizationLanguages.outputFolder + "/Quests_" + text + ".csv";
				if (File.Exists(text2))
				{
					list = ReadCSV(text2);
					CombineMultilineCSVSourceLines(list);
					for (int num4 = 2; num4 < list.Count; num4++)
					{
						List<string> cSVColumnsFromLine3 = GetCSVColumnsFromLine(list[num4]);
						if (cSVColumnsFromLine3.Count < 11)
						{
							Debug.LogError(text2 + ":" + (num4 + 1) + " Invalid line: " + list[num4]);
							continue;
						}
						Item item = database.GetItem(cSVColumnsFromLine3[0]);
						if (item == null)
						{
							continue;
						}
						string value3 = cSVColumnsFromLine3[1];
						string value4 = cSVColumnsFromLine3[2];
						if (!string.IsNullOrEmpty(value4))
						{
							if (!item.FieldExists("Display Name"))
							{
								Field.SetValue(item.fields, "Display Name", value3);
							}
							Field.SetValue(item.fields, "Display Name " + text, value4, FieldType.Localization);
						}
						string value5 = cSVColumnsFromLine3[3];
						string value6 = cSVColumnsFromLine3[4];
						bool flag3 = !item.FieldExists("Group") && (!string.IsNullOrEmpty(value5) || !string.IsNullOrEmpty(value6));
						if (item.FieldExists("Group") && string.IsNullOrEmpty(item.LookupValue("Group")) && !string.IsNullOrEmpty(value5))
						{
							flag3 = true;
						}
						if (flag3)
						{
							Field.SetValue(item.fields, "Group", value5);
						}
						if (item.FieldExists("Group"))
						{
							Field.SetValue(item.fields, "Group " + text, value6, FieldType.Localization);
						}
						Field.SetValue(item.fields, "Description " + text, cSVColumnsFromLine3[6], FieldType.Localization);
						Field.SetValue(item.fields, "Success Description " + text, cSVColumnsFromLine3[8], FieldType.Localization);
						Field.SetValue(item.fields, "Failure Description " + text, cSVColumnsFromLine3[10], FieldType.Localization);
						int num5 = 0;
						for (int num6 = 0; num6 < localizationLanguages.extraQuestFields.Count; num6++)
						{
							string text5 = localizationLanguages.extraQuestFields[num6];
							if (string.IsNullOrEmpty(text5))
							{
								continue;
							}
							int num7 = 11 + num6 * 2 + 1;
							num5++;
							if (exportLocalizationCreateNewFields || Field.FieldExists(item.fields, text5) || !string.IsNullOrEmpty(cSVColumnsFromLine3[num7 - 1]))
							{
								Field.SetValue(item.fields, text5 + " " + text, cSVColumnsFromLine3[num7]);
								if (flag2)
								{
									Field.SetValue(item.fields, text5, cSVColumnsFromLine3[num7 - 1]);
								}
							}
						}
						int num8 = item.LookupInt("Entry Count");
						for (int num9 = 0; num9 < num8; num9++)
						{
							int index = 12 + 2 * num5 + num9 * 2;
							Field.SetValue(item.fields, "Entry " + (num9 + 1) + " " + text, cSVColumnsFromLine3[index], FieldType.Localization);
						}
						if (flag2)
						{
							if (item.FieldExists("Display Name"))
							{
								Field.SetValue(item.fields, "Display Name", value3);
							}
							if (item.FieldExists("Group"))
							{
								Field.SetValue(item.fields, "Group", value5, FieldType.Text);
							}
							Field.SetValue(item.fields, "Description", cSVColumnsFromLine3[5], FieldType.Text);
							Field.SetValue(item.fields, "Success Description", cSVColumnsFromLine3[7], FieldType.Text);
							Field.SetValue(item.fields, "Failure Description", cSVColumnsFromLine3[9], FieldType.Text);
							for (int num10 = 0; num10 < num8; num10++)
							{
								Field.SetValue(item.fields, "Entry " + (num10 + 1), cSVColumnsFromLine3[11 + 2 * num10], FieldType.Text);
							}
						}
					}
				}
				text2 = localizationLanguages.outputFolder + "/Items_" + text + ".csv";
				if (!File.Exists(text2))
				{
					continue;
				}
				list = ReadCSV(text2);
				CombineMultilineCSVSourceLines(list);
				for (int num11 = 2; num11 < list.Count; num11++)
				{
					List<string> cSVColumnsFromLine4 = GetCSVColumnsFromLine(list[num11]);
					if (cSVColumnsFromLine4.Count < 5)
					{
						Debug.LogError(text2 + ":" + (num11 + 1) + " Invalid line: " + list[num11]);
						continue;
					}
					Item item2 = database.GetItem(cSVColumnsFromLine4[0]);
					if (item2 == null)
					{
						continue;
					}
					string value7 = cSVColumnsFromLine4[1];
					string value8 = cSVColumnsFromLine4[2];
					if (!string.IsNullOrEmpty(value8))
					{
						if (!item2.FieldExists("Display Name"))
						{
							Field.SetValue(item2.fields, "Display Name", value7);
						}
						Field.SetValue(item2.fields, "Display Name " + text, value8, FieldType.Localization);
					}
					Field.SetValue(item2.fields, "Description " + text, cSVColumnsFromLine4[4], FieldType.Localization);
					int num12 = 0;
					for (int num13 = 0; num13 < localizationLanguages.extraItemFields.Count; num13++)
					{
						string text6 = localizationLanguages.extraItemFields[num13];
						if (string.IsNullOrEmpty(text6))
						{
							continue;
						}
						int num14 = 4 + num13 * 2 + 1;
						num12++;
						if (exportLocalizationCreateNewFields || Field.FieldExists(item2.fields, text6) || !string.IsNullOrEmpty(cSVColumnsFromLine4[num14 - 1]))
						{
							Field.SetValue(item2.fields, text6 + " " + text, cSVColumnsFromLine4[num14]);
							if (flag2)
							{
								Field.SetValue(item2.fields, text6, cSVColumnsFromLine4[num14 - 1]);
							}
						}
					}
				}
			}
		}

		private List<string> GetCSVColumnsFromLine(string line)
		{
			Regex regex = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)");
			List<string> list = new List<string>();
			foreach (Match item in regex.Matches(line))
			{
				list.Add(UnwrapCSVValue(item.Value.TrimStart(',')));
			}
			return list;
		}

		private List<string> ReadCSV(string filename)
		{
			List<string> list = new List<string>();
			StreamReader streamReader = new StreamReader(filename, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
			string text;
			while ((text = streamReader.ReadLine()) != null)
			{
				list.Add(text.TrimEnd());
			}
			streamReader.Close();
			return list;
		}

		private void CombineMultilineCSVSourceLines(List<string> sourceLines)
		{
			int num = 0;
			int num2 = 0;
			int num3 = 999999;
			while (num < sourceLines.Count && num2 < num3)
			{
				num2++;
				string text = sourceLines[num];
				if (text == null)
				{
					sourceLines.RemoveAt(num);
					continue;
				}
				bool flag = true;
				char c = '\0';
				foreach (char num4 in text)
				{
					if (num4 == '"' && c != '\\')
					{
						flag = !flag;
					}
					c = num4;
				}
				if (flag || num + 1 >= sourceLines.Count)
				{
					if (!flag)
					{
						sourceLines[num] = text + "\"";
					}
					num++;
				}
				else
				{
					sourceLines[num] = text + "\\n" + sourceLines[num + 1];
					sourceLines.RemoveAt(num + 1);
				}
			}
		}

		private string UnwrapCSVValue(string s)
		{
			string text = s.Replace("\\n", "\n").Replace("\\r", "\r");
			if (text.StartsWith("\"") && text.EndsWith("\""))
			{
				text = text.Substring(1, text.Length - 2).Replace("\"\"", "\"");
			}
			return text;
		}
	}
}
