using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ClusterRepairIndicators
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");

            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }

    [HarmonyPatch]
    class Patch
    {
        static MethodBase TargetMethod()
        {
            var method = AccessTools.Method(typeof(UIRepairComponentView), "DrawIndicators");
            return method;
        }

        static bool Prefix(IList<Repairable> repairables, UIRepairComponentView __instance, ref float ___lastHeight)
        {
            // Not sure if this slot logic is needed now that offscreen logic is gone...
            if (___lastHeight != UITransform.ScreenRect.height)
            {
                // Reflection kinda annoying...
                var CreateSlotList = AccessTools.Method(typeof(UIRepairComponentView), "CreateSlotList");
                CreateSlotList.Invoke(__instance, null);
                ___lastHeight = UITransform.ScreenRect.height;
            }
            // Much reflection
            var ClearSlots = AccessTools.Method(typeof(UIRepairComponentView), "ClearSlots");
            ClearSlots.Invoke(__instance, null);
            int i = 0;
            int j = 0;
            while (j < repairables.Count)
            {
                if (i >= __instance.indicatorCache.Length)
                {
                    MuseLog.Error("{0} >= {1}".F(new object[]
                    {
                    repairables.Count,
                    __instance.indicatorCache.Length
                    }), null);
                    break;
                }
                Repairable repairable = repairables[j];
                RepairIndicator repairIndicator = __instance.indicatorCache[i];
                int tickCount = 1;
                float normalizedHealth = repairable.NormalizedHealth;
                // Reflection overtime!!!
                var SetIndicatorBarTickCount = AccessTools.Method(typeof(UIRepairComponentView), "SetIndicatorBarTickCount");
                var SetIndicatorBarPercentage = AccessTools.Method(typeof(UIRepairComponentView), "SetIndicatorBarPercentage");
                var SetOrRotateIndicatorIcon = AccessTools.Method(typeof(UIRepairComponentView), "SetOrRotateIndicatorIcon");
                var GetDisplayOffset = AccessTools.Method(typeof(UIRepairComponentView), "GetDisplayOffset");
                SetIndicatorBarTickCount.Invoke(__instance, new object[] { repairIndicator, RepairComponentView.HEALTH_BAR_ONE, tickCount });
                SetIndicatorBarPercentage.Invoke(__instance, new object[] { repairIndicator, RepairComponentView.HEALTH_BAR_ONE, normalizedHealth });
                SetOrRotateIndicatorIcon.Invoke(__instance, new object[] { repairIndicator, repairable });
                Vector3 position = repairable.IndicatorPosition + (Vector3)GetDisplayOffset.Invoke(__instance, new object[] { repairable });
                Vector3 vector = CameraControl.Camera.WorldToScreenPoint(position) / UITransform.UIScale;
                float num = vector.x;
                float num2 = UITransform.ScreenRect.height - vector.y;
                // Not sure what this does
                if (vector.z < 0f)
                {
                    Vector3 vector2 = CameraControl.Transform.InverseTransformPoint(position) / UITransform.UIScale;
                    float x = Mathf.Sqrt(vector2.x * vector2.x + vector2.z * vector2.z);
                    float num3 = 57.29578f * Mathf.Atan2(vector2.x, vector2.z);
                    float num4 = -57.29578f * Mathf.Atan2(vector2.y, x);
                    num = UITransform.ScreenRect.width * (0.5f + num3 / CameraControl.HorizontalDegrees);
                    num2 = UITransform.ScreenRect.height * (0.5f + num4 / CameraControl.VerticalDegrees);
                }
                repairable.currentUIPosition.x = num;
                repairable.currentUIPosition.y = num2;
                repairable.UIVelocityX = 0f;
                repairable.UIVelocityY = 0f;
                __instance.indicatorCache[i].icon.SetPosition(repairable.currentUIPosition.x, repairable.currentUIPosition.y);
                j++;
                i++;
            }
            while (i < __instance.indicatorCache.Length)
            {
                if (__instance.indicatorCache[i].icon.Activated)
                {
                    __instance.indicatorCache[i].icon.Deactivate(0f);
                }
                i++;
            }

            // Don't run the original method
            return false;
        }
    }
}
