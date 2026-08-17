using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Continuously rotates the environment/reflection mapping used by a TextMeshPro material.
///
/// This does NOT rotate the text GameObject itself.
/// Instead, every frame it builds a rotation Matrix4x4 and writes that matrix into shader property `_EnvMatrix`.
/// The shader can then make highlights/reflections appear to move across otherwise stationary text.
///
///     Time.time × RotationSpeeds
///               ↓
///        Quaternion.Euler
///               ↓
///          Matrix4x4 TRS
///               ↓
///     material._EnvMatrix
///               ↓
///      shader reflection changes
/// </summary>
public class EnvMapAnimator : MonoBehaviour
{
	/// <summary>
	/// Rotation speed multiplier for X/Y/Z shader-environment rotation.
	/// Because each axis is `Time.time * speed`, values behave like degrees advanced per second.
	/// Example: Y=30 means about 30° of Y environment rotation after one second, 60° after two seconds.
	/// </summary>
	public Vector3 RotationSpeeds;

	/// <summary>TextMeshPro component on this same GameObject, resolved once in Awake().</summary>
	private TMP_Text m_textMeshPro;

	/// <summary>
	/// Shared font material used by the TMP component. IMPORTANT: fontSharedMaterial can be shared by multiple text
	/// objects, so changing `_EnvMatrix` here may visually affect every object using that same material instance.
	/// </summary>
	private Material m_material;

	/// <summary>
	/// Unity calls Awake during object initialization before Start. This caches dependencies needed by the coroutine.
	/// Current code assumes a TMP_Text exists on this GameObject; there is no null guard before fontSharedMaterial.
	/// </summary>
	private void Awake()
	{
		m_textMeshPro = GetComponent<TMP_Text>();
		m_material = m_textMeshPro.fontSharedMaterial;
	}

	/// <summary>
	/// Start is written as an IEnumerator, so Unity automatically runs it as a coroutine.
	/// It intentionally never finishes: `while (true)` updates the shader once per rendered frame forever while the
	/// component/coroutine remains active.
	/// </summary>
	private IEnumerator Start()
	{
		// One matrix variable is reused for every iteration instead of allocating a new logical container each frame.
		Matrix4x4 matrix = default(Matrix4x4);

		while (true)
		{
			// TRS = Translation, Rotation, Scale.
			// Translation is zero and scale is one, so this matrix represents ROTATION ONLY.
			matrix.SetTRS(
				Vector3.zero,
				Quaternion.Euler(
					Time.time * RotationSpeeds.x,
					Time.time * RotationSpeeds.y,
					Time.time * RotationSpeeds.z),
				Vector3.one);

			// Push new transform into material/shader. `_EnvMatrix` must be a property recognized by the shader in use.
			m_material.SetMatrix("_EnvMatrix", matrix);

			// Pause until next rendered frame. At ~60 FPS this repeats roughly every 16.7 ms.
			yield return null;
		}
	}
}
