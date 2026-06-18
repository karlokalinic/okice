using UnityEngine;

[AddComponentMenu("PlayMaker/PlayMakerControls")]
[DisallowMultipleComponent]
[HelpURL("https://hutonggames.fogbugz.com/f/page?W1224")]
public class PlayMakerControls : MonoBehaviour
{
	[ContextMenu("Collapse PlayMakerFSM Inspectors")]
	public void CollapseFsmComponents()
	{
		PlayMakerFSM[] components = GetComponents<PlayMakerFSM>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].Fsm.NameIsExpanded = false;
		}
	}
}
