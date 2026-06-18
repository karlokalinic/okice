using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
	[AddComponentMenu("")]
	public class SequencerCommandCamera : SequencerCommand
	{
		protected const float SmoothMoveCutoff = 0.05f;

		protected Transform subject;

		protected Transform angleTransform;

		protected Transform cameraTransform;

		protected bool isLocalTransform;

		protected Quaternion targetRotation;

		protected Vector3 targetPosition;

		protected float duration;

		protected float startTime;

		protected float endTime;

		protected Quaternion originalRotation;

		protected Vector3 originalPosition;

		protected virtual void Start()
		{
			string text = GetParameter(0, "Closeup");
			subject = GetSubject(1);
			duration = GetParameterAsFloat(2);
			if (string.Equals(text, "default"))
			{
				text = SequencerTools.GetDefaultCameraAngle(subject);
			}
			bool flag = string.Equals(text, "original");
			angleTransform = ((!flag) ? ((base.sequencer.cameraAngles != null) ? base.sequencer.cameraAngles.transform.Find(text) : null) : ((Camera.main != null) ? Camera.main.transform : base.speaker));
			isLocalTransform = true;
			if (angleTransform == null)
			{
				isLocalTransform = false;
				angleTransform = SequencerTools.GetSubject(text, base.speaker, base.listener);
			}
			if (angleTransform == null && DialogueDebug.logWarnings)
			{
				Debug.LogWarning(string.Format("{0}: Sequencer: Camera({1}): Camera angle '{2}' wasn't found.", new object[3]
				{
					"Dialogue System",
					GetParameters(),
					text
				}));
			}
			else if (subject == null && !flag && (!isLocalTransform || !(angleTransform != null)) && DialogueDebug.logWarnings)
			{
				Debug.LogWarning(string.Format("{0}: Sequencer: Camera({1}): Camera subject '{2}' or GameObject named '{3}' wasn't found.", "Dialogue System", GetParameters(), GetParameter(1), GetParameter(0)));
			}
			else if (DialogueDebug.logInfo)
			{
				Debug.Log(string.Format("{0}: Sequencer: Camera({1}, {2}, {3}s)", "Dialogue System", text, Tools.GetGameObjectName(subject), duration));
			}
			base.sequencer.TakeCameraControl();
			if (flag || (angleTransform != null && (subject != null || isLocalTransform)))
			{
				cameraTransform = base.sequencer.sequencerCameraTransform;
				if (flag)
				{
					targetRotation = base.sequencer.originalCameraRotation;
					targetPosition = base.sequencer.originalCameraPosition;
				}
				else if (isLocalTransform)
				{
					targetRotation = subject.rotation * angleTransform.localRotation;
					targetPosition = subject.position + subject.rotation * angleTransform.localPosition;
				}
				else
				{
					targetRotation = angleTransform.rotation;
					targetPosition = angleTransform.position;
				}
				if (duration > 0.05f)
				{
					startTime = DialogueTime.time;
					endTime = startTime + duration;
					originalRotation = cameraTransform.rotation;
					originalPosition = cameraTransform.position;
					bool unscaledTime = DialogueTime.mode != DialogueTime.TimeMode.Gameplay;
					Tweener.Easing cameraEasing = DialogueManager.displaySettings.cameraSettings.cameraEasing;
					Tweener.Tween(originalPosition, targetPosition, duration, unscaledTime, cameraEasing, null, delegate(Vector3 x)
					{
						cameraTransform.position = x;
					}, base.Stop);
					Tweener.Tween(originalRotation, targetRotation, duration, unscaledTime, cameraEasing, null, delegate(Quaternion x)
					{
						cameraTransform.rotation = x;
					}, base.Stop);
				}
				else
				{
					Stop();
				}
			}
			else
			{
				Stop();
			}
		}

		protected virtual void Update()
		{
			if (DialogueTime.time > endTime)
			{
				Stop();
			}
		}

		protected virtual void OnDestroy()
		{
			if (angleTransform != null && subject != null)
			{
				cameraTransform.rotation = targetRotation;
				cameraTransform.position = targetPosition;
			}
		}
	}
}
