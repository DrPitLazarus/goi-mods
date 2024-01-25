using BepInEx;
using HarmonyLib;
using MuseBase.Multiplayer.Unity;
using System.Collections.Generic;
using System.Reflection;

namespace SkipConfirmExitGame
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    class Patch
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            var type = AccessTools.FirstInner(typeof(UIManager), t => t.Name.Contains("UINewMainMenuState"));
            var method = AccessTools.FirstMethod(type, method => method.Name.Contains("ExitGame"));
            yield return method;
            // Maybe exit confirmation is ok in match menu...
            // type = AccessTools.FirstInner(typeof(UIManager), t => t.Name.Contains("UIMatchMenuState"));
            // method = AccessTools.FirstMethod(type, method => method.Name.Contains("ExitGame"));
            // yield return method;
        }

        static bool Prefix()
        {
            // Who named this? I think it's safer than calling Application.Quit()... Maybe.
            UITextDrone.PumpAndDie();
            // Don't run the original method
            return false;
        }
    }

    [HarmonyPatch]
    class PatchConnectingScreen
    {
        static MethodBase TargetMethod()
        {
            var method = AccessTools.FirstMethod(typeof(UIConnectingScreen), method => method.Name.Contains("Awake"));
            return method;
        }

        static bool Prefix(UIConnectingScreen __instance)
        {
            // Remove modal
            __instance.exitGameButton.onClick.AddListener(delegate ()
            {
                // Who named this? I think it's safer than calling Application.Quit()... Maybe.
                UITextDrone.PumpAndDie();
            });
            // Keep original method
            __instance.returnToLobbyButton.onClick.AddListener(delegate ()
            {
                MuseWorldClient.Instance.ReconnectToLobby();
            });
            // Don't run the original method
            return false;
        }
    }
}
