using UnityEngine;

namespace TMPro.Examples
{
	public class ObjectSpin : MonoBehaviour
	{
		public enum MotionType
		{
			Rotation = 0,
			SearchLight = 1,
			Translation = 2
		}

		public MotionType Motion;

		public Vector3 TranslationDistance = new Vector3(5f, 0f, 0f);

		public float TranslationSpeed = 1f;

		public float SpinSpeed = 5f;

		public int RotationRange = 15;

		private Transform m_transform;

		private float m_time;

		private Vector3 m_prevPOS;

		private Vector3 m_initial_Rotation;

		private Vector3 m_initial_Position;

		private Color32 m_lightColor;

		private void Awake()
		{
			m_transform = base.transform;
			m_initial_Rotation = m_transform.rotation.eulerAngles;
			m_initial_Position = m_transform.position;
			Light component = GetComponent<Light>();
			m_lightColor = ((component != null) ? component.color : Color.black);
		}

		private void Update()
		{
			switch (Motion)
			{
			case MotionType.Rotation:
				m_transform.Rotate(0f, SpinSpeed * Time.deltaTime, 0f);
				break;
			case MotionType.SearchLight:
				m_time += SpinSpeed * Time.deltaTime;
				m_transform.rotation = Quaternion.Euler(m_initial_Rotation.x, Mathf.Sin(m_time) * (float)RotationRange + m_initial_Rotation.y, m_initial_Rotation.z);
				break;
			case MotionType.Translation:
			{
				m_time += TranslationSpeed * Time.deltaTime;
				float x = TranslationDistance.x * Mathf.Cos(m_time);
				float z = TranslationDistance.y * Mathf.Sin(m_time) * Mathf.Cos(m_time * 1f);
				float y = TranslationDistance.z * Mathf.Sin(m_time);
				m_transform.position = m_initial_Position + new Vector3(x, y, z);
				m_prevPOS = m_transform.position;
				break;
			}
			}
		}
	}
}
