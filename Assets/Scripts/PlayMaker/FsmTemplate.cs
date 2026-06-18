using System;
using HutongGames.PlayMaker;
using UnityEngine;

[Serializable]
public class FsmTemplate : ScriptableObject
{
	[Delayed]
	[SerializeField]
	private string category;

	public Fsm fsm;

	public string Description
	{
		get
		{
			if (fsm == null)
			{
				return "";
			}
			return fsm.Description;
		}
	}

	public string Category
	{
		get
		{
			return category;
		}
		set
		{
			category = value;
		}
	}

	public void OnEnable()
	{
		if (fsm != null)
		{
			fsm.UsedInTemplate = this;
		}
	}
}
