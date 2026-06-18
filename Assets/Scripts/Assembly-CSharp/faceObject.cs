using UnityEngine;

public class faceObject : MonoBehaviour
{
	public Transform target;

	public bool autoAssign = true;

	public bool lockX;

	public bool lockY;

	public bool lockZ;

	private Vector3 initial;

	private void Start()
	{
		if (autoAssign && target == null && Camera.main != null)
		{
			target = Camera.main.transform;
		}
		initial = base.transform.eulerAngles;
	}

	private void LateUpdate()
	{
		if (!(target == null))
		{
			Vector3 eulerAngles = Quaternion.LookRotation(target.position - base.transform.position).eulerAngles;
			if (lockX)
			{
				eulerAngles.x = initial.x;
			}
			if (lockY)
			{
				eulerAngles.y = initial.y;
			}
			if (lockZ)
			{
				eulerAngles.z = initial.z;
			}
			base.transform.eulerAngles = eulerAngles;
		}
	}
}
