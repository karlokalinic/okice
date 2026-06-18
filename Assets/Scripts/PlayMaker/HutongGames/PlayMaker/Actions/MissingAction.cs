namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.Debug)]
	[Tooltip("Placeholder for a missing action.\nThis action is usually generated when the editor can't load an action, e.g., if the script was deleted.\nYou could also use this action as a TODO note to yourself.\nTIP: The realtime error checker lists Missing Actions, making them very easy to find.\nNOTE: The original action data is NOT preserved after you save your project! To fix a missing action, restore the action script, then reload your project.")]
	public class MissingAction : FsmStateAction
	{
		[Tooltip("The name of the missing action.")]
		public string actionName;
	}
}
