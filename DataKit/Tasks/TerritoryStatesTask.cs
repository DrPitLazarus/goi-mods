using LitJson;
using Muse.Icarus.Coop.WorldMap;
using Muse.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace DataKit.Tasks;

/// <summary>
/// Uploads faction/alliance ownership of territories.
/// </summary>
public class TerritoryStatesTask : MonoBehaviour
{
    public bool IsRunning { get; private set; } = false;
    public DateTime? LastRun { get; private set; } = null;
    public int TaskStartCount { get; private set; } = 0;
    public int TaskCompleteCount { get; private set; } = 0;
    private static readonly TimeSpan _startAfter = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

    private static JsonData _payloadJson;
    private static bool _requestDone = false;
    private static bool _uploadDone = false;

    private void Start()
    {
        InvokeRepeating(nameof(BeginTask), (float)_startAfter.TotalSeconds, (float)_interval.TotalSeconds);
    }

    public void BeginTask()
    {
        if (IsRunning && DateTime.UtcNow - LastRun > _interval)
        {
            DataKit.Log($"Task timed out!");
            IsRunning = false;
        }
        if (IsRunning) return;
        if (NetworkedPlayer.Local is null) return;
        StartCoroutine(MainTask());
    }

    private IEnumerator MainTask()
    {
        if (NetworkedPlayer.Local is null) yield break;
        IsRunning = true;
        DataKit.Log($"Start! StartCount: {TaskStartCount} CompleteCount: {TaskCompleteCount}.");
        TaskStartCount++;
        LastRun = DateTime.UtcNow;

        // Reset.
        _requestDone = false;
        _uploadDone = false;
        _payloadJson = [];
        _payloadJson["submitterId"] = NetworkedPlayer.Local.UserId;
        _payloadJson["dataString"] = String.Empty;

        // Fetch.
        var dictionary = new Dictionary<string, string>
        {
            { "refreshIndex", "0" }
        };
        WorldMapEntity.requestAllTerritoryDataCall.Invoke(dictionary, (ExtensionResponse resp) =>
        {
            _requestDone = true;
            _payloadJson["dataString"] = resp.Data;
        });
        while (!_requestDone) yield return new WaitForSeconds(0.01f);

        // Upload.
        StartCoroutine(UploadTask(_payloadJson.ToJson()));
        while (!_uploadDone) yield return new WaitForSeconds(0.01f);

        TaskCompleteCount++;
        DataKit.Log($"End! StartCount: {TaskStartCount} CompleteCount: {TaskCompleteCount}.");
        IsRunning = false;
    }

    private IEnumerator UploadTask(string data)
    {
        using var request = UnityWebRequest.Put(DataKit.GetApiUrl("territory-states"), data);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.Send();
        _uploadDone = true;
        var message = request.isError ? $"Error! {request.error}" : "OK!";
    }
}