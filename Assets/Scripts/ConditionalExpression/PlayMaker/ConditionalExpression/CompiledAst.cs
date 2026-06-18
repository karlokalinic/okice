using PlayMaker.ConditionalExpression.Ast;

namespace PlayMaker.ConditionalExpression
{
	public sealed class CompiledAst
	{
		public IEvaluatorContext Context { get; private set; }

		private Node Ast { get; set; }

		public static CompiledAst Compile(string expression, IEvaluatorContext context)
		{
			return new CompiledAst(new Parser(expression).Parse(), context);
		}

		private CompiledAst(Node ast, IEvaluatorContext context)
		{
			Ast = ast;
			Context = context;
		}

		public bool Evaluate()
		{
			try
			{
				Parser.EvaluatorContext = Context;
				return Ast.ToBoolean();
			}
			finally
			{
				Parser.EvaluatorContext = null;
			}
		}
	}
}
