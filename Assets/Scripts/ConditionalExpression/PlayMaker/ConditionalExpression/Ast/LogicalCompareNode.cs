using HutongGames.PlayMaker;

namespace PlayMaker.ConditionalExpression.Ast
{
	internal class LogicalCompareNode : LogicalNode
	{
		public ComparisonOperator Operator { get; private set; }

		public Node Right { get; private set; }

		public override VariableType Type => Utility.GetDominantType(Left.Type, Right.Type);

		public Node Left => base.Inner;

		public LogicalCompareNode(ComparisonOperator op, Node left, Node right)
			: base(left)
		{
			Operator = op;
			Right = right;
		}

		public override bool ToBoolean()
		{
			return Type switch
			{
				VariableType.Object => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToObject() == Right.ToObject(), 
					ComparisonOperator.CompareNotEqual => Left.ToObject() != Right.ToObject(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Object), 
				}, 
				VariableType.Bool => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToBoolean() == Right.ToBoolean(), 
					ComparisonOperator.CompareNotEqual => Left.ToBoolean() != Right.ToBoolean(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Bool), 
				}, 
				VariableType.Float => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToFloat() == Right.ToFloat(), 
					ComparisonOperator.CompareNotEqual => Left.ToFloat() != Right.ToFloat(), 
					ComparisonOperator.CompareGreater => Left.ToFloat() > Right.ToFloat(), 
					ComparisonOperator.CompareGreaterOrEqual => Left.ToFloat() >= Right.ToFloat(), 
					ComparisonOperator.CompareLess => Left.ToFloat() < Right.ToFloat(), 
					ComparisonOperator.CompareLessOrEqual => Left.ToFloat() <= Right.ToFloat(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Float), 
				}, 
				VariableType.Int => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToInt() == Right.ToInt(), 
					ComparisonOperator.CompareNotEqual => Left.ToInt() != Right.ToInt(), 
					ComparisonOperator.CompareGreater => Left.ToInt() > Right.ToInt(), 
					ComparisonOperator.CompareGreaterOrEqual => Left.ToInt() >= Right.ToInt(), 
					ComparisonOperator.CompareLess => Left.ToInt() < Right.ToInt(), 
					ComparisonOperator.CompareLessOrEqual => Left.ToInt() <= Right.ToInt(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Int), 
				}, 
				VariableType.String => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToString() == Right.ToString(), 
					ComparisonOperator.CompareNotEqual => Left.ToString() != Right.ToString(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.String), 
				}, 
				VariableType.Color => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToColor() == Right.ToColor(), 
					ComparisonOperator.CompareNotEqual => Left.ToColor() != Right.ToColor(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Color), 
				}, 
				VariableType.Quaternion => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToQuaternion() == Right.ToQuaternion(), 
					ComparisonOperator.CompareNotEqual => Left.ToQuaternion() != Right.ToQuaternion(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Quaternion), 
				}, 
				VariableType.Rect => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToRect() == Right.ToRect(), 
					ComparisonOperator.CompareNotEqual => Left.ToRect() != Right.ToRect(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Rect), 
				}, 
				VariableType.Vector2 => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToVector2() == Right.ToVector2(), 
					ComparisonOperator.CompareNotEqual => Left.ToVector2() != Right.ToVector2(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Vector2), 
				}, 
				VariableType.Vector3 => Operator switch
				{
					ComparisonOperator.CompareEqual => Left.ToVector3() == Right.ToVector3(), 
					ComparisonOperator.CompareNotEqual => Left.ToVector3() != Right.ToVector3(), 
					_ => throw new InvalidOperatorException(Operator, VariableType.Vector3), 
				}, 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}
	}
}
