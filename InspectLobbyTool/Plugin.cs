using BepInEx;
using HarmonyLib;
using Muse.Goi2.Entity;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace InspectLobbyTool
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static Harmony harmony;

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
            // For ScriptEngine reloading; press F6 while on the match lobby screen.
            Destroy(GameObject.Find("UI Match Lobby")?.GetComponent<InspectLobbyToolGui>());
        }
    }

    [HarmonyPatch]
    public class PatchGui
    {
        private static MethodBase TargetMethod()
        {
            var type = AccessTools.FirstInner(typeof(UIManager), t => t.Name.Contains("UINewMatchLobbyState"));
            return AccessTools.Method(type, "Enter");
        }

        private static void Postfix()
        {
            GameObject uiMatchLobby = GameObject.Find("UI Match Lobby");
            if (uiMatchLobby.GetComponent<InspectLobbyToolGui>() == null) uiMatchLobby.AddComponent<InspectLobbyToolGui>();
        }
    }

    public class InspectLobbyToolGui : MonoBehaviour
    {
        private static readonly float WINDOW_COLLAPSE_WIDTH = 175f;
        private Rect windowRect = new Rect(350, 100, WINDOW_COLLAPSE_WIDTH, 0);
        private bool expanded = false;
        private bool showEmptySlots = false;
        private bool sampleEmptySlots = false;
        private readonly Texture2D textureWindow = MakeBackgroundTexture(1, 1, new Color(0, 0, 0, .8f));
        private readonly Texture2D textureShipRed = MakeBackgroundTexture(1, 1, new Color32(116, 53, 53, 255));
        private readonly Texture2D textureShipBlue = MakeBackgroundTexture(1, 1, new Color32(47, 97, 127, 255));
        private readonly Texture2D textureShipYellow = MakeBackgroundTexture(1, 1, new Color32(216, 152, 64, 255));
        private readonly Texture2D textureShipPurple = MakeBackgroundTexture(1, 1, new Color32(93, 43, 112, 255));
        private GUIStyle styleWindow;
        private GUIStyle styleShipRed;
        private GUIStyle styleShipBlue;
        private GUIStyle styleShipYellow;
        private GUIStyle styleShipPurple;

        private void OnGUI()
        {
            GUI.skin.box.alignment = TextAnchor.UpperLeft;
            GUI.skin.label.padding = GUI.skin.box.padding;
            GUI.skin.label.wordWrap = false;
            styleWindow = new GUIStyle(GUI.skin.window);
            styleWindow.onNormal.background = styleWindow.normal.background = textureWindow;
            styleWindow.padding.top = 30; // +5 height from Box with plugin name
            styleShipRed = new GUIStyle(GUI.skin.box);
            styleShipRed.normal.background = textureShipRed;
            styleShipBlue = new GUIStyle(GUI.skin.box);
            styleShipBlue.normal.background = textureShipBlue;
            styleShipYellow = new GUIStyle(GUI.skin.box);
            styleShipYellow.normal.background = textureShipYellow;
            styleShipYellow.normal.textColor = Color.black;
            styleShipPurple = new GUIStyle(GUI.skin.box);
            styleShipPurple.normal.background = textureShipPurple;

            windowRect = GUILayout.Window(0, windowRect, MainWindow, "", styleWindow);
        }

        private void MainWindow(int windowId)
        {
            // Main window toolbar.
            GUI.Box(new Rect(0, 0, 10000, 25), PluginInfo.PLUGIN_NAME + " v" + PluginInfo.PLUGIN_VERSION, styleShipRed);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(expanded ? "Collapse ▲" : "Expand ▼"))
            {
                expanded = !expanded;
                if (!expanded) ShrinkWindow();
            }
            if (expanded)
            {
                bool showEmptySlotsOld = showEmptySlots;
                showEmptySlots = GUILayout.Toggle(showEmptySlots, "Show empty slots");
                if (!showEmptySlots && showEmptySlotsOld) ShrinkWindow();

                bool sampleEmptySlotsOld = sampleEmptySlots;
                sampleEmptySlots = GUILayout.Toggle(sampleEmptySlots, "Sample empty slots");
                if (!sampleEmptySlots && sampleEmptySlotsOld) ShrinkWindow();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Content starts here!
            if (expanded)
            {
                DrawShipCrews();
            }

            // Make entire window draggable, must be at the bottom.
            GUI.DragWindow();
        }

        private void ShrinkWindow()
        {
            // The window automatically grows to fit content, but does not shrink automatically.
            // Call this after an action that reduces the content on the window.
            windowRect.width = WINDOW_COLLAPSE_WIDTH;
            windowRect.height = 0;
        }

        private void DrawShipCrews()
        {
            MatchLobbyView mlv = MatchLobbyView.Instance;

            // Wrap the entire thing in a horizontal group so we can have another column for ships 5+.
            GUILayout.BeginHorizontal();
            // Start a column.
            GUILayout.BeginVertical();

            int crewNum = 0;
            foreach (var crew in mlv.FlatCrews)
            {
                crewNum++;
                // 3v3 start new column on the 4th ship, 4v4 on 5th ship.
                if ((crewNum == 4 && mlv.FlatCrews.Count == 6) || (crewNum == 5 && mlv.FlatCrews.Count == 8))
                {
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                }

                // Begin ship data
                GUILayout.BeginHorizontal();

                GUIStyle[] teamStyles = { styleShipRed, styleShipBlue, styleShipYellow, styleShipPurple };
                string text = String.Format("{0} <{1}>",
                    crew.Captain != null ? crew.ShipName : "No Pilot",
                    crew.Captain != null ? crew.ShipClass : "No Ship");
                // In PvE lobbies, Team 0 is blue.
                GUILayout.Box(text, mlv.DifficultyLevel != null ? teamStyles[1] : teamStyles[crew.Team]);

                // Get equiped guns if there is a pilot.
                if (crew.Captain != null)
                {
                    foreach (var ship in mlv.CrewShips)
                    {
                        // Skip ship until the crew ID matches.
                        if (ship.CrewId != crew.CrewId) continue;
                        int gunNum = 0;
                        foreach (var gun in ship.Equipments)
                        {
                            // Add 2 guns to the row and start another.
                            if (gunNum == 2)
                            {
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                GUILayout.FlexibleSpace();
                            }
                            GUILayout.Label(CachedRepository.Instance.Get<Item>(gun.SlotItemId).NameText.En);
                            gunNum++;
                        }
                    }
                }
                else if (showEmptySlots && sampleEmptySlots)
                {
                    GUILayout.Label("Whirlwind Light Gatling Gun");
                    GUILayout.Label("Scylla Double-Barreled Mortar");
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Whirlwind Light Gatling Gun");
                    GUILayout.Label("Echidna Light Flak Cannon");
                    GUILayout.Label("Whirlwind Light Gatling Gun");
                    GUILayout.Label("Echidna Light Flak Cannon");
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // Begin crew member data.
                foreach (var crewMember in crew.CrewMembers)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(crewMember.CurrentClass.ToString());
                    GUILayout.Label(crewMember.Name);
                    foreach (var equipment in (from id in crewMember.CurrentSkills
                                               select CachedRepository.Instance.Get<SkillConfig>(id) into s
                                               select s).ToList<SkillConfig>())
                    {
                        GUILayout.Label(equipment.NameText.En);
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
                if (showEmptySlots)
                {
                    for (int slot = 0; slot < crew.OpenSlotCount; slot++)
                    {
                        GUILayout.BeginHorizontal();
                        if (sampleEmptySlots) GUILayout.Label("Pilot");
                        GUILayout.Label("Empty Slot");
                        if (sampleEmptySlots)
                        {
                            GUILayout.Label("Pipe Wrench");
                            GUILayout.Label("Kerosene");
                            GUILayout.Label("Phoenix Claw");
                            GUILayout.Label("Impact Bumpers");
                            GUILayout.Label("Extended Magazine");
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                    }
                }

            }
            // end column.
            GUILayout.EndVertical();
            // main.
            GUILayout.EndHorizontal();
        }

        private void DrawShipCrewSample()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Box("not the enemy <Pyramidion>", styleShipRed);
            GUILayout.Label("Whirlwind Light Gatling Gun");
            GUILayout.Label("Scylla Double-Barreled Mortar");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label("Whirlwind Light Gatling Gun");
            GUILayout.Label("Echidna Light Flak Cannon");
            GUILayout.Label("Whirlwind Light Gatling Gun");
            GUILayout.Label("Echidna Light Flak Cannon");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            DrawCrewSlotSample();
            DrawCrewSlotSample();
            DrawCrewSlotSample();
            DrawCrewSlotSample();
        }

        private void DrawCrewSlotSample()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pilot");
            GUILayout.Label("Dr. Pit Lazarus [PC]");
            GUILayout.Label("Pipe Wrench");
            GUILayout.Label("Kerosene");
            GUILayout.Label("Phoenix Claw");
            GUILayout.Label("Impact Bumpers");
            GUILayout.Label("Extended Magazine");
            GUILayout.Label("Engine Stabilization Abilty");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private static Texture2D MakeBackgroundTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D backgroundTexture = new Texture2D(width, height);

            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();

            return backgroundTexture;
        }

    }
}

