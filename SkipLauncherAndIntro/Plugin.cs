using BepInEx;
using HarmonyLib;
using System.Reflection;


namespace SkipLauncherAndIntro
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPatch(typeof(UILauncherMainPanel), "Activate")]
        [HarmonyPrefix]
        public static bool SkipLauncherAndIntro()
        {
            // Use reflection to call a private method
            var func = typeof(UILauncherMainPanel).GetMethod("Deactivate", BindingFlags.Instance | BindingFlags.NonPublic);
            func.Invoke(new UILauncherMainPanel(), null);

            // Don't run the original method
            return false;
        }
    }
}
