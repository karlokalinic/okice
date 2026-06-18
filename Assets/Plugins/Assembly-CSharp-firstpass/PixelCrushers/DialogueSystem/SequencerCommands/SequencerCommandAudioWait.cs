using System;
using System.Collections;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
	[AddComponentMenu("")]
	public class SequencerCommandAudioWait : SequencerCommand
	{
		protected float stopTime;

		protected AudioSource audioSource;

		protected int nextClipIndex = 2;

		protected string audioClipName;

		protected AudioClip currentClip;

		protected AudioClip originalClip;

		protected bool restoreOriginalClip;

		protected bool playedAudio;

		protected bool isLoadingAudio;

		public virtual IEnumerator Start()
		{
			audioClipName = GetParameter(0);
			Transform subject = GetSubject(1);
			nextClipIndex = 2;
			audioSource = GetAudioSource(subject);
			if (audioSource == null)
			{
				if (DialogueDebug.logWarnings)
				{
					if (subject == null)
					{
						Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait({1}) command: can't find or add AudioSource. Subject is null.", new object[2]
						{
							"Dialogue System",
							GetParameters()
						}));
					}
					else
					{
						Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait({1}) command: can't find or add AudioSource to {2}.", new object[3]
						{
							"Dialogue System",
							GetParameters(),
							subject.name
						}), subject);
					}
				}
				Stop();
			}
			else
			{
				originalClip = audioSource.clip;
				stopTime = DialogueTime.time + 1f;
				yield return null;
				originalClip = audioSource.clip;
				TryAudioClip(audioClipName);
			}
		}

		protected virtual AudioSource GetAudioSource(Transform subject)
		{
			return SequencerTools.GetAudioSource(subject);
		}

		protected virtual void TryAudioClip(string audioClipName)
		{
			try
			{
				if (string.IsNullOrEmpty(audioClipName))
				{
					if (DialogueDebug.logWarnings)
					{
						Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Audio clip name is blank.", new object[1] { "Dialogue System" }));
					}
					stopTime = 0f;
					if (nextClipIndex >= base.parameters.Length)
					{
						Stop();
					}
					return;
				}
				this.audioClipName = audioClipName;
				isLoadingAudio = true;
				DialogueManager.LoadAsset(audioClipName, typeof(AudioClip), delegate(UnityEngine.Object asset)
				{
					isLoadingAudio = false;
					AudioClip audioClip = asset as AudioClip;
					if (audioClip == null)
					{
						if (DialogueDebug.logWarnings && Sequencer.reportMissingAudioFiles)
						{
							Debug.LogWarning(string.Format("{0}: Sequencer: AudioWait() command: Clip '{1}' wasn't found.", new object[2] { "Dialogue System", audioClipName }));
						}
						stopTime = 0f;
						if (nextClipIndex >= base.parameters.Length)
						{
							Stop();
						}
					}
					else
					{
						if (IsAudioMuted())
						{
							if (DialogueDebug.logInfo)
							{
								Debug.Log(string.Format("{0}: Sequencer: AudioWait(): waiting but not playing '{1}'; audio is muted.", new object[2] { "Dialogue System", audioClipName }));
							}
						}
						else if (audioSource != null)
						{
							if (DialogueDebug.logInfo)
							{
								Debug.Log(string.Format("{0}: Sequencer: AudioWait(): playing '{1}' on {2}.", new object[3] { "Dialogue System", audioClipName, audioSource }), audioSource);
							}
							currentClip = audioClip;
							audioSource.clip = audioClip;
							audioSource.Play();
						}
						playedAudio = true;
						stopTime = DialogueTime.time + GetAudioClipLength(audioSource, audioClip);
					}
				});
			}
			catch (Exception)
			{
				stopTime = 0f;
			}
		}

		public static float GetAudioClipLength(AudioSource audioSource, AudioClip audioClip)
		{
			if (audioClip == null)
			{
				return 0f;
			}
			if (audioSource == null)
			{
				return audioClip.length;
			}
			float num = Mathf.Abs(audioSource.pitch);
			if (Time.timeScale > 0f)
			{
				if (num == 1f || Mathf.Approximately(0f, num))
				{
					return audioClip.length / Time.timeScale;
				}
				return audioClip.length / Time.timeScale / num;
			}
			if (num == 1f || Mathf.Approximately(0f, num))
			{
				return audioClip.length;
			}
			return audioClip.length / num;
		}

		public virtual void Update()
		{
			if (!(DialogueTime.time >= stopTime))
			{
				return;
			}
			if (currentClip != null)
			{
				DialogueManager.UnloadAsset(currentClip);
			}
			currentClip = null;
			if (!isLoadingAudio)
			{
				if (nextClipIndex < base.parameters.Length)
				{
					TryAudioClip(GetParameter(nextClipIndex));
					nextClipIndex++;
				}
				else
				{
					Stop();
				}
			}
		}

		public virtual void OnDialogueSystemPause()
		{
			if (!(audioSource == null))
			{
				audioSource.Pause();
			}
		}

		public virtual void OnDialogueSystemUnpause()
		{
			if (!(audioSource == null))
			{
				audioSource.Play();
			}
		}

		public virtual void OnDestroy()
		{
			if (audioSource != null)
			{
				if (audioSource.isPlaying && audioSource.clip == currentClip && audioSource.clip != null)
				{
					audioSource.Stop();
				}
				if (restoreOriginalClip)
				{
					audioSource.clip = originalClip;
				}
				DialogueManager.UnloadAsset(currentClip);
			}
		}
	}
}
