using System;
using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Gets the relationship value between two actors.")]
	public class GetRelationship : FsmStateAction
	{
		[RequiredField]
		[Tooltip("The name of actor1 in the Actor[] table")]
		public FsmString actor1Name;

		[RequiredField]
		[Tooltip("The name of actor2 in the Actor[] table")]
		public FsmString actor2Name;

		[RequiredField]
		[Tooltip("The relationship type")]
		public FsmString relationshipType;

		[RequiredField]
		[UIHint(UIHint.Variable)]
		[Tooltip("Store the result in a Float variable")]
		public FsmFloat storeResult;

		public override void Reset()
		{
			if (actor1Name != null)
			{
				actor1Name.Value = string.Empty;
			}
			if (actor2Name != null)
			{
				actor2Name.Value = string.Empty;
			}
			if (relationshipType != null)
			{
				relationshipType.Value = string.Empty;
			}
			storeResult = null;
		}

		public override void OnEnter()
		{
			if (actor1Name != null && actor2Name != null && relationshipType != null && storeResult != null)
			{
				try
				{
					storeResult.Value = Lua.Run($"return GetRelationship(Actor[\"{DialogueLua.StringToTableIndex(actor1Name.Value)}\"], Actor[\"{DialogueLua.StringToTableIndex(actor2Name.Value)}\"], \"{relationshipType.Value}\")", DialogueDebug.LogInfo).AsFloat;
				}
				catch (NullReferenceException)
				{
				}
			}
			Finish();
		}
	}
}
