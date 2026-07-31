using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Karlolegend.Gradomraz
{
    /// <summary>
    /// Removes broken serialized UnityEvent callbacks from the restored main-menu buttons.
    /// Button.onClick.RemoveAllListeners() does not remove persistent Inspector callbacks, so the
    /// complete event object must be replaced before ProductionRuntimeRecovery adds safe actions.
    /// </summary>
    public static class ProductionMenuPersistentEventStripper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "MainMenu")
            {
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
            {
                if (root == null)
                {
                    continue;
                }

                foreach (var button in root.GetComponentsInChildren<Button>(true))
                {
                    if (button == null)
                    {
                        continue;
                    }

                    var descriptor = Describe(button);
                    if (!IsRecoveredMenuButton(descriptor))
                    {
                        continue;
                    }

                    button.onClick = new Button.ButtonClickedEvent();
                    button.interactable = true;
                }
            }
        }

        private static bool IsRecoveredMenuButton(string descriptor)
        {
            var isNewGame =
                (descriptor.Contains("NEW") && descriptor.Contains("GAME")) ||
                (descriptor.Contains("NOVA") && descriptor.Contains("IGRA")) ||
                descriptor.Contains("START GAME") ||
                descriptor.Contains("POKRENI IGRU");

            var isOptions =
                descriptor.Contains("OPTIONS") ||
                descriptor.Contains("SETTINGS") ||
                descriptor.Contains("OPCIJE") ||
                descriptor.Contains("POSTAVKE");

            var isQuit =
                descriptor.Contains("QUIT") ||
                descriptor.Contains("EXIT") ||
                descriptor.Contains("IZLAZ") ||
                descriptor.Contains("IZADI");

            return isNewGame || isOptions || isQuit;
        }

        private static string Describe(Button button)
        {
            var builder = new StringBuilder(button.gameObject.name);

            var tmpText = button.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
            {
                builder.Append(' ').Append(tmpText.text);
            }

            var legacyText = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
            if (legacyText != null)
            {
                builder.Append(' ').Append(legacyText.text);
            }

            var decomposed = builder.ToString().Normalize(NormalizationForm.FormD);
            builder.Clear();

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
