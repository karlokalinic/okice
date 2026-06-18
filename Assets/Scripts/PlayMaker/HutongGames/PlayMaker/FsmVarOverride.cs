using System;

namespace HutongGames.PlayMaker
{
	[Serializable]
	public class FsmVarOverride
	{
		public NamedVariable variable;

		public FsmVar fsmVar;

		public bool isEdited;

		public string Name
		{
			get
			{
				if (variable == null)
				{
					return "";
				}
				return variable.Name;
			}
		}

		public FsmVarOverride(FsmVarOverride source)
		{
			variable = new NamedVariable(source.variable.Name);
			fsmVar = new FsmVar(source.fsmVar);
			isEdited = source.isEdited;
		}

		public FsmVarOverride(NamedVariable namedVar)
		{
			variable = namedVar;
			fsmVar = new FsmVar(variable);
			isEdited = false;
		}

		public FsmVarOverride(NamedVariable namedVar, string variableName)
		{
			variable = namedVar;
			fsmVar = new FsmVar(variable);
			fsmVar.variableName = variableName;
			isEdited = false;
		}

		public void Apply(FsmVariables variables)
		{
			variable = variables.GetVariable(variable.Name);
			fsmVar.ApplyValueTo(variable);
		}

		public void Update(FsmVariables fromVariables, FsmVariables toVariables)
		{
			variable = fromVariables.GetVariable(variable.Name);
			fsmVar.NamedVar = toVariables.GetVariable(fsmVar.NamedVar.Name);
			fsmVar.GetValueFrom(variable);
			fsmVar.ApplyValueTo(fsmVar.NamedVar);
		}
	}
}
