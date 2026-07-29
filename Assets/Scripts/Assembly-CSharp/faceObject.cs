using UnityEngine;

public class faceObject : MonoBehaviour
{
	public Transform target;

	public bool autoAssign = true;

	public bool lockX;

	public bool lockY;

	public bool lockZ;

	private Vector3 initial;

	private Transform cachedTransform;

	private void Start()
	{
		cachedTransform = transform;
		if (autoAssign && target == null && Camera.main != null)
		{
			target = Camera.main.transform;
		}
		initial = cachedTransform.eulerAngles;
	}

	private void LateUpdate()
	{
		if (target == null)
		{
			return;
		}

		Vector3 direction = target.position - cachedTransform.position;
		if (direction.sqrMagnitude <= Mathf.Epsilon)
		{
			return;
		}

		Vector3 eulerAngles = Quaternion.LookRotation(direction).eulerAngles;
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
		cachedTransform.eulerAngles = eulerAngles;
	}
}
