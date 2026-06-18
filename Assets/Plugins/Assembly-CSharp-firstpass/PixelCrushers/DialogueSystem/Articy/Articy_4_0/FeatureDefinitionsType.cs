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
	public class FeatureDefinitionsType
	{
		private FeatureDefinitionRefType[] featureDefinitionRefField;

		private int countField;

		private bool countFieldSpecified;

		[XmlElement("FeatureDefinitionRef")]
		public FeatureDefinitionRefType[] FeatureDefinitionRef
		{
			get
			{
				return featureDefinitionRefField;
			}
			set
			{
				featureDefinitionRefField = value;
			}
		}

		[XmlAttribute]
		public int Count
		{
			get
			{
				return countField;
			}
			set
			{
				countField = value;
			}
		}

		[XmlIgnore]
		public bool CountSpecified
		{
			get
			{
				return countFieldSpecified;
			}
			set
			{
				countFieldSpecified = value;
			}
		}
	}
}
