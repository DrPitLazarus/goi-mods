using HarmonyLib;
using LitJson;
using MuseBase.Multiplayer;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Version = System.Version;

namespace DataKit;

[HarmonyPatch]
public class UpdateChecker : MonoBehaviour
{
    private static readonly string _chatCommandToOpenDownloadPage = "/datakit update";
    private static readonly string _gitHubReleasesUrl = "https://api.github.com/repos/drpitlazarus/goi-mods/releases";
    private static readonly string _chatCommandToGitHub = "/datakit github";
    private static readonly string _gitHubUrl = "https://github.com/DrPitLazarus/goi-mods/tree/master/DataKit";
    private static UpdateChecker _instance;
    private static string _latestReleasePageUrl;


    private void Start()
    {
        _instance = this;
        StartCoroutine(CheckForUpdates());
    }

    /// <summary>
    /// Log current version in chat and check for updates. If outdated, log an update available message in chat.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckForUpdates()
    {
        var currentVersion = PluginInfo.PLUGIN_VERSION;
        DataKit.Log($"Current {PluginInfo.PLUGIN_NAME} version: {currentVersion}.");
        DataKit.SendConsoleChatMessage($"{PluginInfo.PLUGIN_NAME} {currentVersion} loaded.");

        var request = UnityWebRequest.Get(_gitHubReleasesUrl);
        yield return request.Send(); // Wait for the request to complete without blocking.
        if (request.isError || request.responseCode != 200)
        {
            DataKit.Log($"Failed to check for update. Error: {request.error}");
            yield break; // Early return.
        }

        try
        {
            var releasesJson = JsonMapper.ToObject(request.downloadHandler.text);
            var foundLatestRelease = false;
            var versionFromServer = currentVersion;
            foreach (JsonData release in releasesJson)
            {
                if (release["tag_name"].ToString().Contains("DataKit@"))
                {
                    foundLatestRelease = true;
                    versionFromServer = release["tag_name"].ToString().Split('@')[1];
                    DataKit.Log($"Latest {PluginInfo.PLUGIN_NAME} version: {versionFromServer}.");
                    _latestReleasePageUrl = release["html_url"].ToString();
                    break;
                }
            }

            if (!foundLatestRelease)
            {
                throw new Exception("Did not find a release with the expected tag format (DataKit@<version>).");
            }

            var isOutdated = new Version(currentVersion).CompareTo(new Version(versionFromServer)) < 0; // -1 means the server version is newer.
            if (!isOutdated)
            {
                DataKit.Log("No update available.");
                yield break; // Early return.
            }
            var message = $"{PluginInfo.PLUGIN_NAME} {versionFromServer} is available. Type \"{_chatCommandToOpenDownloadPage}\" to open download page.";
            DataKit.SendConsoleChatMessage(message);
        }
        catch (Exception ex)
        {
            DataKit.Log($"Failed to check for update. Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Prefix patch to prevent sending chat command as a message and run actions.
    /// </summary>
    [HarmonyPatch(typeof(MessageClient), nameof(MessageClient.TrySendMessage))]
    [HarmonyPrefix]
    private static bool HandleChatCommand(string msg)
    {
        var preparedMsg = msg.ToLower().Trim();
        if (preparedMsg == _chatCommandToOpenDownloadPage)
        {
            Application.OpenURL(_latestReleasePageUrl);
            return false; // Prevent the message from being sent to the server.
        }
        if (preparedMsg == _chatCommandToGitHub)
        {
            Application.OpenURL(_gitHubUrl);
            return false; // Prevent the message from being sent to the server.
        }
        return true; // Allow other messages to be sent normally.
    }
}