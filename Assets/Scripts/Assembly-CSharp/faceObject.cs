using UnityEngine;

/// <summary>
/// Rotates this object so it faces a target, with optional per-axis locks.
///
/// The mental model is:
///
///     target.position - my.position
///                 ↓
///          direction vector
///                 ↓
///       Quaternion.LookRotation
///                 ↓
///        candidate X/Y/Z angles
///                 ↓
///     replace locked axes with START angles
///                 ↓
///        apply final world rotation
///
/// This is more flexible than a basic billboard because each Euler axis can independently remain fixed.
/// </summary>
public class faceObject : MonoBehaviour
{
	/// <summary>Transform to face. May be assigned manually or filled from Camera.main in Start().</summary>
	public Transform target;

	/// <summary>
	/// When true and target is null at startup, use the MainCamera Transform automatically if one exists.
	/// This lookup happens once in Start(); a MainCamera appearing later will not be auto-assigned by this class.
	/// </summary>
	public bool autoAssign = true;

	/// <summary>Keep world X Euler angle at its startup value instead of using LookRotation's X.</summary>
	public bool lockX;

	/// <summary>Keep world Y Euler angle at its startup value instead of using LookRotation's Y.</summary>
	public bool lockY;

	/// <summary>Keep world Z Euler angle at its startup value instead of using LookRotation's Z.</summary>
	public bool lockZ;

	/// <summary>
	/// World-space Euler rotation captured once at startup. Locked axes are restored from this snapshot every frame.
	/// </summary>
	private Vector3 initial;

	/// <summary>
	/// Cached reference to this component's Transform. The property lookup itself is cheap, but this makes repeated
	/// LateUpdate use explicit and avoids writing `transform` several times.
	/// </summary>
	private Transform cachedTransform;

	/// <summary>
	/// One-time setup: cache own Transform, optionally find MainCamera, then remember authored starting rotation.
	/// The `initial` snapshot happens AFTER target auto-assignment but before any facing rotation is applied.
	/// </summary>
	private void Start()
	{
		cachedTransform = transform;

		if (autoAssign && target == null && Camera.main != null)
		{
			target = Camera.main.transform;
		}

		initial = cachedTransform.eulerAngles;
	}

	/// <summary>
	/// Reorients after normal Update work for the frame. LateUpdate is useful for camera-facing objects because a
	/// moving camera/player has generally already updated its position before this calculation runs.
	///
	/// At 60 FPS this may recalculate roughly every 16.7 ms; at 120 FPS roughly every 8.3 ms.
	/// </summary>
	private void LateUpdate()
	{
		if (target == null)
		{
			return;
		}

		// Vector from this object toward the target.
		Vector3 direction = target.position - cachedTransform.position;

		// LookRotation is undefined/meaningless when both objects occupy effectively the same point.
		// sqrMagnitude avoids a square-root calculation because we only need a near-zero check.
		if (direction.sqrMagnitude <= Mathf.Epsilon)
		{
			return;
		}

		// Compute the full orientation that would point forward toward target, then expose it as editable Euler axes.
		Vector3 eulerAngles = Quaternion.LookRotation(direction).eulerAngles;

		// Axis locks do not "freeze current rotation"; they restore the angle captured once in Start().
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

		// Apply in WORLD Euler space because cachedTransform.eulerAngles (not localEulerAngles) is used.
		cachedTransform.eulerAngles = eulerAngles;
	}
}
