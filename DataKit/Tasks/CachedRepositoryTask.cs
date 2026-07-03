using LitJson;
using Muse.Goi2.Entity;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace DataKit.Tasks;

public class CachedRepositoryTask : MonoBehaviour
{
    public bool IsRunning { get; private set; } = false;
    public DateTime? LastRun { get; private set; } = null;
    public int TaskStartCount { get; private set; } = 0;
    public int TaskCompleteCount { get; private set; } = 0;
    //private static readonly TimeSpan _startAfter = TimeSpan.FromSeconds(2);
    //private static readonly TimeSpan _interval = TimeSpan.FromMinutes(2);

    private static JsonData _payloadJson;
    private static bool _requestDone = false;
    private static bool _uploadDone = false;

    private void Start()
    {
        //InvokeRepeating(nameof(BeginTask), (float)_startAfter.TotalSeconds, (float)_interval.TotalSeconds);
    }

    public void BeginTask()
    {
        //if (IsRunning && DateTime.UtcNow - LastRun > _interval)
        //{
        //    DataKit.Log($"Task timed out!");
        //    IsRunning = false;
        //}
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

        // Fetch.
        var cachedRepositoryJson = CachedRepository.Instance.ExportAllJson();
        _requestDone = true;
        _payloadJson["data"] = cachedRepositoryJson;
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
        using var request = UnityWebRequest.Put(DataKit.GetApiUrl("cached-repository"), data);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.Send();
        _uploadDone = true;
        var message = request.isError ? $"Error! {request.error}" : "OK!";
        DataKit.Log($"Upload! {message}");
    }
}