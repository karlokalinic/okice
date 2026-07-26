// HLADAN GRAD — Debug tools for fast testing.
// Adds Tools ▸ HLADAN GRAD Debug ▸ ... menu items that broadcast the game's OWN
// PlayMaker global events to jump between chapters / set resolution while playing.
//
// These use the exact global events the game itself uses (from PlayMakerGlobals),
// so nothing new is introduced — they must be fired while the GAMEPLAY scene is
// running (press Play, click "Igraj" to enter the game, then use these).
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HladanGrad.DebugTools
{
    public static class HladanGradDebugTools
    {
        // ---- Chapter / act jumps (game's own events) ----
        [MenuItem("Tools/HLADAN GRAD Debug/Start at Chapter 2 (Mirror Stage 2)", false, 10)]
        public static void Chapter2() => Broadcast("debug / mirror stage 2");

        [MenuItem("Tools/HLADAN GRAD Debug/Start at Chapter 3 (Act 2)", false, 11)]
        public static void Chapter3()
        {
            Broadcast("UNLOAD / ACT1");
            Broadcast("ACT 2 CHANGES");
        }

        // ---- Resolution helpers (game's own events) ----
        [MenuItem("Tools/HLADAN GRAD Debug/Resolution/Set 1440p", false, 30)]
        public static void Res1440() => Broadcast("1440p");

        [MenuItem("Tools/HLADAN GRAD Debug/Resolution/Set 2160p", false, 31)]
        public static void Res2160() => Broadcast("2160p");

        [MenuItem("Tools/HLADAN GRAD Debug/Resolution/Set 1080p", false, 32)]
        public static void Res1080() => Broadcast("1080p");

        // Menu items are only enabled while playing.
        [MenuItem("Tools/HLADAN GRAD Debug/Start at Chapter 2 (Mirror Stage 2)", true)]
        [MenuItem("Tools/HLADAN GRAD Debug/Start at Chapter 3 (Act 2)", true)]
        [MenuItem("Tools/HLADAN GRAD Debug/Resolution/Set 1440p", true)]
        [MenuItem("Tools/HLADAN GRAD Debug/Resolution/Set 2160p", true)]
        [MenuItem("Tools/HLADAN GRAD Debug/Resolution/Set 1080p", true)]
        public static bool ValidatePlaying() => Application.isPlaying;

        private static MethodInfo _broadcast;

        private static void Broadcast(string fsmEvent)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Enter Play mode first",
                    "These debug jumps fire the game's PlayMaker events, so the game " +
                    "must be RUNNING.\n\n1. Press Play\n2. Click \"Igraj\" to enter the " +
                    "gameplay scene\n3. Then use this menu.", "OK");
                return;
            }

            if (_broadcast == null)
            {
                Type t = Type.GetType("PlayMakerFSM, PlayMaker");
                if (t == null)
                {
                    t = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => { try { return a.GetTypes(); } catch { return Type.EmptyTypes; } })
                        .FirstOrDefault(x => x.Name == "PlayMakerFSM");
                }
                if (t != null)
                    _broadcast = t.GetMethod("BroadcastEvent", new[] { typeof(string) });
            }

            if (_broadcast == null)
            {
                Debug.LogError("[HladanGradDebug] Could not find PlayMakerFSM.BroadcastEvent.");
                return;
            }

            _broadcast.Invoke(null, new object[] { fsmEvent });
            Debug.Log("[HladanGradDebug] Broadcast PlayMaker event: '" + fsmEvent + "'");
        }
    }
}
