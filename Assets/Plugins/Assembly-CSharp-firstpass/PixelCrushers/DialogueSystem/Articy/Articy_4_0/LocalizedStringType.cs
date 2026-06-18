using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy.Articy_4_0
{
	[Serializable]
	[GeneratedCode("xsd", "4.8.3928.0")]
	[DebuggerStepThrough]
	[XmlType(Namespace = "http://www.articy.com/schemas/articydraft/4.0/XmlContentExport_FullProject.xsd")]
	public class LocalizedStringType
	{
		private string langField;

		private TextStateType textStateField;

		private VoiceOverStateType voiceOverStateField;

		private bool voiceOverStateFieldSpecified;

		private string voiceOverReferenceField;

		private string valueField;

		[XmlAttribute(DataType = "token")]
		public string Lang
		{
			get
			{
				return langField;
			}
			set
			{
				langField = value;
			}
		}

		[XmlAttribute]
		public TextStateType TextState
		{
			get
			{
				return textStateField;
			}
			set
			{
				textStateField = value;
			}
		}

		[XmlAttribute]
		public VoiceOverStateType VoiceOverState
		{
			get
			{
				return voiceOverStateField;
			}
			set
			{
				voiceOverStateField = value;
			}
		}

		[XmlIgnore]
		public bool VoiceOverStateSpecified
		{
			get
			{
				return voiceOverStateFieldSpecified;
			}
			set
			{
				voiceOverStateFieldSpecified = value;
			}
		}

		[XmlAttribute]
		public string VoiceOverReference
		{
			get
			{
				return voiceOverReferenceField;
			}
			set
			{
				voiceOverReferenceField = value;
			}
		}

		[XmlText]
		public string Value
		{
			get
			{
				return valueField;
			}
			set
			{
				valueField = value;
			}
		}
	}
}
