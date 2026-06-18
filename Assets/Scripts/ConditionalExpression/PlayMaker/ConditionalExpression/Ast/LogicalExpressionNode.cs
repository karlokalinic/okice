using HutongGames.PlayMaker;

namespace PlayMaker.ConditionalExpression.Ast
{
	internal class LogicalExpressionNode : LogicalNode
	{
		public LogicalOperator Operator { get; private set; }

		public Node Right { get; private set; }

		public override VariableType Type => VariableType.Bool;

		public Node Left => base.Inner;

		public LogicalExpressionNode(LogicalOperator op, Node left, Node right)
			: base(left)
		{
			Operator = op;
			Right = right;
		}

		public override bool ToBoolean()
		{
			bool flag = Left.ToBoolean();
			bool flag2 = Right.ToBoolean();
			return Operator switch
			{
				LogicalOperator.And => flag && flag2, 
				LogicalOperator.Or => flag || flag2, 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}
	}
}
