using BepInEx;
using DataKit.Tasks;
using HarmonyLib;
using LitJson;
using MuseBase.Multiplayer;
using MuseBase.Multiplayer.Unity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace DataKit;

[HarmonyPatch]
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class DataKit : BaseUnityPlugin
{
    private Harmony _harmony;
    private static readonly string _baseApiUrl = "https://goi-library.drpitlazar.us/api/";
    private static readonly string _baseDevApiUrl = "http://localhost:5173/api/";
    private static readonly bool _isDevMode = false;

    private void Awake()
    {
        _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        _harmony.PatchAll();
        DataKit.Log($"{PluginInfo.PLUGIN_NAME} loaded!");
    }

    public static void Log(string message)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(1);
        var method = frame.GetMethod();
        var classFullName = method.DeclaringType.FullName;
        var methodName = method.Name;
        UnityEngine.Debug.Log($"{timestamp} [INFO] {classFullName} - {message}");
    }

    /// <summary>
    /// Add a red console message to the chat. Only visible to the player.
    /// </summary>
    /// <param name="message">Message string. Can use <c>\n</c> for new line.</param>
    public static void SendConsoleChatMessage(string message)
    {
        MuseWorldClient.Instance.ChatHandler.AddMessage(ChatMessage.Console(message));
    }

    public static string GetApiUrl(string endpoint)
    {
        if (_isDevMode)
        {
            return $"{_baseDevApiUrl}{endpoint}";
        }
        else
        {
            return $"{_baseApiUrl}{endpoint}";
        }
    }
}


[HarmonyPatch]
public class Manager : MonoBehaviour
{
    private static GameObject _gameObject;
    private static UpdateChecker _updateChecker;
    private static FactionLeadersTask _factionLeadersTask;
    private static TerritoryStatesTask _territoryStatesTask;
    private static TerritoryConflictsTask _territoryConflictsTask;
    private static CachedRepositoryTask _cachedRepositoryTask;
    private static WorldMapDataTask _worldMapDataTask;

    private static bool _firstMainMenuState = true;

    [HarmonyPatch(typeof(UIManager.UINewMainMenuState), nameof(UIManager.UINewMainMenuState.Enter))]
    [HarmonyPostfix]
    private static void Initialize()
    {
        if (!_firstMainMenuState) return;
        _firstMainMenuState = false;
        DataKit.Log("Initializing...");
        _gameObject = new GameObject(PluginInfo.PLUGIN_NAME);
        _gameObject.AddComponent<Manager>();
        _gameObject.AddComponent<TestMethods>();
        _updateChecker = _gameObject.AddComponent<UpdateChecker>();
        _factionLeadersTask = _gameObject.AddComponent<FactionLeadersTask>();
        _territoryStatesTask = _gameObject.AddComponent<TerritoryStatesTask>();
        _territoryConflictsTask = _gameObject.AddComponent<TerritoryConflictsTask>();
        _cachedRepositoryTask = _gameObject.AddComponent<CachedRepositoryTask>();
        _worldMapDataTask = _gameObject.AddComponent<WorldMapDataTask>();
    }

    private void Start()
    {
        DataKit.Log("GameObject created!");
    }
}


public class TestMethods : MonoBehaviour
{
    private static string GetJsonString(JsonData jsonData, bool prettyPrint = false)
    {
        if (prettyPrint)
        {
            return Muse.Goi2.Entity.CachedRepository.PrettyJson(jsonData);
        }
        else
        {
            return jsonData.ToJson();
        }
    }

    private static void SaveCachedRepository(bool prettyPrint = false)
    {
        var appendFile = false;
        using System.IO.StreamWriter outputFile = new(System.IO.Path.Combine(Application.dataPath, "0CachedRepository.json"), appendFile);
        outputFile.Write(GetJsonString(Muse.Goi2.Entity.CachedRepository.Instance.ExportAllJson(), prettyPrint));
    }

    private static void SaveWorldMapDataQuery(bool prettyPrint = true)
    {
        Muse.Icarus.Coop.WorldMap.WorldMapEntity.dataQueryDataCall.Invoke(null, delegate (Muse.Networking.ExtensionResponse resp)
        {
            var appendFile = false;
            using System.IO.StreamWriter outputFile = new(System.IO.Path.Combine(Application.dataPath, "0WorldMapDataQuery.json"), appendFile);
            outputFile.Write(GetJsonString(resp.JsonData, prettyPrint));
        });
    }

    private static void SaveWorldMapAllConflictsData(bool prettyPrint = true)
    {
        Muse.Icarus.Coop.WorldMap.WorldMapEntity.RequestAllConflictsData(delegate (Muse.Networking.ExtensionResponse resp)
        {
            var appendFile = false;
            using System.IO.StreamWriter outputFile = new(System.IO.Path.Combine(Application.dataPath, "0WorldMapRequestAllConflictsData.json"), appendFile);
            outputFile.Write(GetJsonString(resp.JsonData, prettyPrint));
        });
    }

    private static void SaveWorldMapAllTerritoryData()
    {
        var dictionary = new Dictionary<string, string>
        {
            { "refreshIndex", "0" }
        };
        Muse.Icarus.Coop.WorldMap.WorldMapEntity.requestAllTerritoryDataCall.Invoke(dictionary, delegate (Muse.Networking.ExtensionResponse resp)
        {
            var appendFile = false;
            using System.IO.StreamWriter outputFile = new(System.IO.Path.Combine(Application.dataPath, "0WorldMapRequestAllTerritoryData.txt"), appendFile);
            outputFile.Write(resp.Data);
        });
    }

    private static void SaveWorldMapMyFactionConflictsData(bool prettyPrint = true)
    {
        Muse.Icarus.Coop.WorldMap.WorldMapEntity.requestMyFactionConflictsDataCall.Invoke(null, delegate (Muse.Networking.ExtensionResponse resp)
        {
            var appendFile = false;
            using System.IO.StreamWriter outputFile = new(System.IO.Path.Combine(Application.dataPath, "0WorldMapRequestMyFactionConflictsData.json"), appendFile);
            outputFile.Write(GetJsonString(resp.JsonData, prettyPrint));
        });
    }

    private static void SaveFactionQuery(bool prettyPrint = true)
    {
        CharCustomActions.factionQuery.Invoke(null, (resp) =>
        {
            var appendFile = false;
            using System.IO.StreamWriter outputFile = new(System.IO.Path.Combine(Application.dataPath, "0FactionQuery.json"), appendFile);
            outputFile.Write(GetJsonString(resp.JsonData, prettyPrint));
        }, null);
    }
}