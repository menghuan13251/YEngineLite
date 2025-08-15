using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class UnityWebRequestExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsTask(this UnityWebRequest www, MonoBehaviour runner)
    {
        var tcs = new TaskCompletionSource<UnityWebRequest>();
        runner.StartCoroutine(RequestCoroutine(www, tcs));
        return tcs.Task;
    }

    private static IEnumerator RequestCoroutine(UnityWebRequest www, TaskCompletionSource<UnityWebRequest> tcs)
    {
        using (www)
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                tcs.SetException(new Exception($"Request failed for {www.url} with error: {www.error}"));
            }
            else
            {
                tcs.SetResult(www);
            }
        }
    }
}