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
	public class ContentType
	{
		private object[] itemsField;

		private ItemsChoiceType[] itemsElementNameField;

		[XmlElement("Asset", typeof(AssetType))]
		[XmlElement("Assets", typeof(SystemFolderType))]
		[XmlElement("AssetsUserFolder", typeof(UserFolderType))]
		[XmlElement("BooleanPropertyDefinition", typeof(BooleanPropertyDefinitionType))]
		[XmlElement("Comment", typeof(CommentType))]
		[XmlElement("Condition", typeof(ConditionType))]
		[XmlElement("Connection", typeof(ConnectionType))]
		[XmlElement("Dialogue", typeof(DialogueType))]
		[XmlElement("DialogueFragment", typeof(DialogueFragmentType))]
		[XmlElement("Document", typeof(DocumentType))]
		[XmlElement("Documents", typeof(SystemFolderType))]
		[XmlElement("DocumentsUserFolder", typeof(UserFolderType))]
		[XmlElement("Entities", typeof(SystemFolderType))]
		[XmlElement("EntitiesUserFolder", typeof(UserFolderType))]
		[XmlElement("Entity", typeof(EntityType))]
		[XmlElement("EnumerationPropertyDefinition", typeof(EnumerationPropertyDefinitionType))]
		[XmlElement("FeatureDefinition", typeof(FeatureDefinitionType))]
		[XmlElement("Features", typeof(SystemFolderType))]
		[XmlElement("FeaturesUserFolder", typeof(UserFolderType))]
		[XmlElement("Flow", typeof(SystemFolderType))]
		[XmlElement("FlowFragment", typeof(FlowFragmentType))]
		[XmlElement("GlobalVariables", typeof(SystemFolderType))]
		[XmlElement("Hub", typeof(HubType))]
		[XmlElement("Instruction", typeof(InstructionType))]
		[XmlElement("Journey", typeof(JourneyType))]
		[XmlElement("Jump", typeof(JumpType))]
		[XmlElement("LayerFolder", typeof(LayerFolderType))]
		[XmlElement("Link", typeof(LinkType))]
		[XmlElement("Location", typeof(LocationType))]
		[XmlElement("LocationImage", typeof(LocationImageType))]
		[XmlElement("LocationText", typeof(LocationTextType))]
		[XmlElement("Locations", typeof(SystemFolderType))]
		[XmlElement("LocationsUserFolder", typeof(UserFolderType))]
		[XmlElement("NumberPropertyDefinition", typeof(NumberPropertyDefinitionType))]
		[XmlElement("ObjectCustomization", typeof(SystemFolderType))]
		[XmlElement("ObjectTemplateDefinition", typeof(ObjectTemplateDefinitionType))]
		[XmlElement("ObjectTemplates", typeof(SystemFolderType))]
		[XmlElement("ObjectTemplatesUserFolder", typeof(UserFolderType))]
		[XmlElement("Path", typeof(PathType))]
		[XmlElement("Project", typeof(ProjectType))]
		[XmlElement("ProjectSettings", typeof(ProjectSettingsType))]
		[XmlElement("PropertyTemplates", typeof(SystemFolderType))]
		[XmlElement("QueryReferenceStripPropertyDefinition", typeof(QueryReferenceStripPropertyDefinitionType))]
		[XmlElement("ReferenceSlotPropertyDefinition", typeof(ReferenceSlotPropertyDefinitionType))]
		[XmlElement("ReferenceStripPropertyDefinition", typeof(ReferenceStripPropertyDefinitionType))]
		[XmlElement("ScriptPropertyDefinition", typeof(ScriptPropertyDefinitionType))]
		[XmlElement("Spot", typeof(SpotType))]
		[XmlElement("TextObject", typeof(TextObjectType))]
		[XmlElement("TextPropertyDefinition", typeof(TextPropertyDefinitionType))]
		[XmlElement("TypedObjectTemplates", typeof(SystemFolderType))]
		[XmlElement("TypedPropertyTemplates", typeof(SystemFolderType))]
		[XmlElement("TypedPropertyTemplatesUserFolder", typeof(UserFolderType))]
		[XmlElement("VariableSet", typeof(VariableSetType))]
		[XmlElement("Zone", typeof(ZoneType))]
		[XmlChoiceIdentifier("ItemsElementName")]
		public object[] Items
		{
			get
			{
				return itemsField;
			}
			set
			{
				itemsField = value;
			}
		}

		[XmlElement("ItemsElementName")]
		[XmlIgnore]
		public ItemsChoiceType[] ItemsElementName
		{
			get
			{
				return itemsElementNameField;
			}
			set
			{
				itemsElementNameField = value;
			}
		}
	}
}
