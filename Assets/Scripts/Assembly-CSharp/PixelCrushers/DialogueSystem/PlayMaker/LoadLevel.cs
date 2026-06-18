using System.Collections;
using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelCrushers.DialogueSystem.PlayMaker
{
	[ActionCategory("Dialogue System")]
	[HutongGames.PlayMaker.Tooltip("Loads a level with Dialogue System persistent data.")]
	public class LoadLevel : FsmStateAction
	{
		[RequiredField]
		[HutongGames.PlayMaker.Tooltip("The name of the scene to load. NOTE: Must be in the list of levels defined in File->Build Settings... ")]
		public FsmString levelName;

		[HutongGames.PlayMaker.Tooltip("A GameObject (typically empty) where the player should be positioned in the new scene.")]
		public FsmString spawnpoint;

		[HutongGames.PlayMaker.Tooltip("Load the scene additively, keeping the current scene. NOTE: Not used if the scene has a LevelManager.")]
		public bool additive;

		[HutongGames.PlayMaker.Tooltip("Load the scene asynchronously in the background. NOTE: Not used if the scene has a LevelManager.")]
		public bool async;

		[HutongGames.PlayMaker.Tooltip("Reset the Dialogue System state before loading.")]
		public bool resetDialogueDatabase;

		[HutongGames.PlayMaker.Tooltip("If Reset Dialogue Database is ticked, tick this to reset to the initial database or untick to keep all loaded databases.")]
		public bool resetToInitialDatabase;

		[HutongGames.PlayMaker.Tooltip("After loading the scene, apply persistent data saved in the Dialogue System's Lua environment.")]
		public bool applyPersistentData = true;

		[HutongGames.PlayMaker.Tooltip("Delay this many frames before applying persistent data to the newly-loaded scene. Allows GameObjects to run their Start methods first.")]
		public int framesToWaitBeforeApplyData;

		[HutongGames.PlayMaker.Tooltip("Event to send when the scene has loaded. NOTE: This only makes sense if the FSM is still in the scene! Not used if the scene has a LevelManager.")]
		public FsmEvent loadedEvent;

		[HutongGames.PlayMaker.Tooltip("Keep this GameObject in the new scene. NOTE: The GameObject and components is disabled then enabled on load; uncheck Reset On Disable to keep the active state.")]
		public FsmBool dontDestroyOnLoad;

		private AsyncOperation asyncOperation;

		public override void Reset()
		{
			if (levelName != null)
			{
				levelName.Value = string.Empty;
			}
			additive = false;
			async = false;
			loadedEvent = null;
			dontDestroyOnLoad = false;
		}

		public override void OnEnter()
		{
			string text = ((levelName == null) ? null : levelName.Value);
			if (string.IsNullOrEmpty(text))
			{
				LogError("Level name is an empty string");
			}
			else
			{
				if (dontDestroyOnLoad.Value)
				{
					Object.DontDestroyOnLoad(base.Owner.transform.root.gameObject);
				}
				DialogueLua.SetActorField("Player", "Spawnpoint", spawnpoint);
				if (Object.FindObjectOfType<SaveSystem>() != null)
				{
					PersistentDataManager.LevelWillBeUnloaded();
					string text2 = (string.IsNullOrEmpty(spawnpoint.Value) ? levelName.Value : (levelName.Value + "@" + spawnpoint.Value));
					if (additive)
					{
						SaveSystem.LoadAdditiveScene(text2);
					}
					else
					{
						SaveSystem.LoadScene(text2);
					}
				}
				else
				{
					LevelManager levelManager = Object.FindObjectOfType<LevelManager>();
					if (levelManager != null && !resetDialogueDatabase)
					{
						levelManager.LoadLevel(text);
					}
					else
					{
						PersistentDataManager.LevelWillBeUnloaded();
						if (resetDialogueDatabase)
						{
							DialogueManager.ResetDatabase(resetToInitialDatabase ? DatabaseResetOptions.RevertToDefault : DatabaseResetOptions.KeepAllLoaded);
						}
						else
						{
							if (resetToInitialDatabase)
							{
								LogWarning("Reset To Initial Database is ticked, but it has no effect because Reset Dialogue Database is unticked.");
							}
							PersistentDataManager.Record();
						}
						if (async)
						{
							if (additive)
							{
								asyncOperation = SceneManager.LoadSceneAsync(text, LoadSceneMode.Additive);
							}
							else
							{
								asyncOperation = SceneManager.LoadSceneAsync(text);
							}
							return;
						}
						if (additive)
						{
							SceneManager.LoadScene(text, LoadSceneMode.Additive);
						}
						else
						{
							SceneManager.LoadScene(text);
						}
					}
				}
			}
			DoneLoadingLevel();
		}

		public override void OnUpdate()
		{
			if (asyncOperation != null && asyncOperation.isDone)
			{
				DoneLoadingLevel();
			}
		}

		private void DoneLoadingLevel()
		{
			DialogueManager.Instance.StartCoroutine(DoneLoadingLevelCoroutine());
		}

		private IEnumerator DoneLoadingLevelCoroutine()
		{
			base.Fsm.Event(loadedEvent);
			for (int i = 0; i < framesToWaitBeforeApplyData; i++)
			{
				yield return null;
			}
			if (applyPersistentData)
			{
				PersistentDataManager.Apply();
			}
			Finish();
		}
	}
}
