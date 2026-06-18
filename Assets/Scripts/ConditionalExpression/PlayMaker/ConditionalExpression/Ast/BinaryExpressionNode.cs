using HutongGames.PlayMaker;
using UnityEngine;

namespace PlayMaker.ConditionalExpression.Ast
{
	internal class BinaryExpressionNode : ExpressionNode
	{
		public BinaryOperator Operator { get; private set; }

		public Node Right { get; private set; }

		public override VariableType Type => Utility.GetDominantType(Left.Type, Right.Type);

		public Node Left => base.Inner;

		public BinaryExpressionNode(BinaryOperator op, Node left, Node right)
			: base(left)
		{
			Operator = op;
			Right = right;
		}

		public override bool ToBoolean()
		{
			int num = Left.ToInt();
			int num2 = Right.ToInt();
			return Operator switch
			{
				BinaryOperator.Add => num + num2 != 0, 
				BinaryOperator.Subtract => num - num2 != 0, 
				BinaryOperator.Multiply => num * num2 != 0, 
				BinaryOperator.Divide => num / num2 != 0, 
				BinaryOperator.Modulo => num % num2 != 0, 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}

		public override float ToFloat()
		{
			float num = Left.ToFloat();
			float num2 = Right.ToFloat();
			return Operator switch
			{
				BinaryOperator.Add => num + num2, 
				BinaryOperator.Subtract => num - num2, 
				BinaryOperator.Multiply => num * num2, 
				BinaryOperator.Divide => num / num2, 
				BinaryOperator.Modulo => num % num2, 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}

		public override int ToInt()
		{
			int num = Left.ToInt();
			int num2 = Right.ToInt();
			return Operator switch
			{
				BinaryOperator.Add => num + num2, 
				BinaryOperator.Subtract => num - num2, 
				BinaryOperator.Multiply => num * num2, 
				BinaryOperator.Divide => num / num2, 
				BinaryOperator.Modulo => num % num2, 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}

		public override Object ToObject()
		{
			throw new InvalidOperatorException(Operator, VariableType.Object);
		}

		public override Color ToColor()
		{
			Color color = Left.ToColor();
			Color color2 = Right.ToColor();
			switch (Operator)
			{
			case BinaryOperator.Add:
				return color + color2;
			case BinaryOperator.Subtract:
				return color - color2;
			case BinaryOperator.Multiply:
				return color * color2;
			case BinaryOperator.Divide:
			case BinaryOperator.Modulo:
				throw new InvalidOperatorException(Operator, VariableType.Color);
			default:
				throw new InvalidOperatorException(Operator);
			}
		}

		public override Quaternion ToQuaternion()
		{
			Quaternion quaternion = Left.ToQuaternion();
			Quaternion quaternion2 = Right.ToQuaternion();
			switch (Operator)
			{
			case BinaryOperator.Multiply:
				return quaternion * quaternion2;
			case BinaryOperator.Add:
			case BinaryOperator.Subtract:
			case BinaryOperator.Divide:
			case BinaryOperator.Modulo:
				throw new InvalidOperatorException(Operator, VariableType.Quaternion);
			default:
				throw new InvalidOperatorException(Operator);
			}
		}

		public override Rect ToRect()
		{
			Rect rect = Left.ToRect();
			Rect rect2 = Right.ToRect();
			return Operator switch
			{
				BinaryOperator.Add => new Rect(rect.x + rect2.x, rect.y + rect2.y, rect.width + rect2.width, rect.height + rect2.height), 
				BinaryOperator.Subtract => new Rect(rect.x - rect2.x, rect.y - rect2.y, rect.width - rect2.width, rect.height - rect2.height), 
				BinaryOperator.Multiply => new Rect(rect.x * rect2.x, rect.y * rect2.y, rect.width * rect2.width, rect.height * rect2.height), 
				BinaryOperator.Divide => new Rect(rect.x / rect2.x, rect.y / rect2.y, rect.width / rect2.width, rect.height / rect2.height), 
				BinaryOperator.Modulo => new Rect(rect.x % rect2.x, rect.y % rect2.y, rect.width % rect2.width, rect.height % rect2.height), 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}

		public override Vector2 ToVector2()
		{
			Vector2 vector = Left.ToVector2();
			Vector2 vector2 = Right.ToVector2();
			return Operator switch
			{
				BinaryOperator.Add => new Vector2(vector.x + vector2.x, vector.y + vector2.y), 
				BinaryOperator.Subtract => new Vector2(vector.x - vector2.x, vector.y - vector2.y), 
				BinaryOperator.Multiply => new Vector2(vector.x * vector2.x, vector.y * vector2.y), 
				BinaryOperator.Divide => new Vector2(vector.x / vector2.x, vector.y / vector2.y), 
				BinaryOperator.Modulo => new Vector2(vector.x % vector2.x, vector.y % vector2.y), 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}

		public override Vector3 ToVector3()
		{
			Vector3 vector = Left.ToVector3();
			Vector3 vector2 = Right.ToVector3();
			return Operator switch
			{
				BinaryOperator.Add => new Vector3(vector.x + vector2.x, vector.y + vector2.y, vector.z + vector2.z), 
				BinaryOperator.Subtract => new Vector3(vector.x - vector2.x, vector.y - vector2.y, vector.z - vector2.z), 
				BinaryOperator.Multiply => new Vector3(vector.x * vector2.x, vector.y * vector2.y, vector.z * vector2.z), 
				BinaryOperator.Divide => new Vector3(vector.x / vector2.x, vector.y / vector2.y, vector.z / vector2.z), 
				BinaryOperator.Modulo => new Vector3(vector.x % vector2.x, vector.y % vector2.y, vector.z % vector2.z), 
				_ => throw new InvalidOperatorException(Operator), 
			};
		}

		public override string ToString()
		{
			string text = Left.ToString();
			string text2 = Right.ToString();
			switch (Operator)
			{
			case BinaryOperator.Add:
				return text + text2;
			case BinaryOperator.Subtract:
			case BinaryOperator.Multiply:
			case BinaryOperator.Divide:
			case BinaryOperator.Modulo:
				throw new InvalidOperatorException(Operator, VariableType.Quaternion);
			default:
				throw new InvalidOperatorException(Operator);
			}
		}
	}
}
