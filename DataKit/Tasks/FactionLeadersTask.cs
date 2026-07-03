using LitJson;
using Muse.Icarus.Coop.WorldMap;
using Muse.Networking;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace DataKit.Tasks;

/// <summary>
/// Uploads current faction leaders for all factions. Manually triggered.
/// </summary>
public class FactionLeadersTask : MonoBehaviour
{
    public bool IsRunning { get; private set; } = false;
    public DateTime? LastRun { get; private set; } = null;

    private static JsonData _dataQueryJsonData;
    private static bool _uploadDone = false;

    public void BeginTask()
    {
        StartCoroutine(MainTask());
    }

    private IEnumerator MainTask()
    {
        if (NetworkedPlayer.Local is null) yield return null;
        DataKit.Log("Start!");
        IsRunning = true;
        LastRun = DateTime.UtcNow;
        // Fetch.
        WorldMapEntity.dataQueryDataCall.Invoke(null, (ExtensionResponse resp) =>
        {
            _dataQueryJsonData = resp.JsonData;
        });
        while (_dataQueryJsonData == null) yield return new WaitForSeconds(0.01f);
        // Upload.
        StartCoroutine(UploadTask(_dataQueryJsonData));
        while (!_uploadDone) yield return new WaitForSeconds(0.01f);
        // Reset.
        _dataQueryJsonData = null;
        _uploadDone = false;

        DataKit.Log("End!");
        IsRunning = false;
    }

    private IEnumerator UploadTask(JsonData jsonData)
    {
        DataKit.Log("Start!");
        using var request = UnityWebRequest.Put(DataKit.GetApiUrl("faction-leaders"), jsonData.ToJson());
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.Send();
        _uploadDone = true;
        var message = request.isError ? $"Error! {request.error}" : "OK!";
        DataKit.Log($"End! {message}");
    }
}