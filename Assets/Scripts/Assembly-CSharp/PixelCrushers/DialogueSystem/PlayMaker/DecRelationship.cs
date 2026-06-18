using HutongGames.PlayMaker;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[Tooltip("Decrements a relationship value between two actors.")]
	public class DecRelationship : FsmStateAction
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
		[Tooltip("The amount to decrement the relationship value")]
		public FsmFloat decrementAmount;

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
			if (decrementAmount != null)
			{
				decrementAmount.Value = 0f;
			}
		}

		public override void OnEnter()
		{
			if (actor1Name != null && actor2Name != null && relationshipType != null && decrementAmount != null)
			{
				Lua.Run($"DecRelationship(Actor[\"{DialogueLua.StringToTableIndex(actor1Name.Value)}\"], Actor[\"{DialogueLua.StringToTableIndex(actor2Name.Value)}\"], \"{relationshipType.Value}\", {decrementAmount.Value})", DialogueDebug.LogInfo);
			}
			Finish();
		}
	}
}
