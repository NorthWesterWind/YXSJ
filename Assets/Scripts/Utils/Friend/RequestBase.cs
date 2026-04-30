using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public abstract class RequestBase<T> where T : class
{
    protected string Url;

    public RequestBase(string url)
    {
        Url = FactoryUrl.Factory(url);
    }

    protected void SentPost(WWWForm form, Action<T> callback, CancellationToken ct = default)
    {
        RequestPosAsync(form, callback, ct).Forget();
    }

    private async UniTaskVoid RequestPosAsync(WWWForm form, Action<T> callback, CancellationToken ct)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(Url, form))
        {
            request.timeout = 30;
            try
            {
                await request.SendWebRequest().WithCancellation(ct);
#if UNITY_2020_3_OR_NEWER
                if (request.result == UnityWebRequest.Result.Success)
#else
                if (!request.isNetworkError && !request.isHttpError)
#endif
                {
                    if (callback != null)
                    {
                        T result = ParseResponse(request.downloadHandler.text);
                        callback.Invoke(result);
                    }
                }
                else
                {
                    Debug.LogWarning($"Request Error: {request.error}");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing response: {e.Message}");
            }
        }
    }

    protected virtual T ParseResponse(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        if (typeof(T) == typeof(string)) return json as T;
        return JsonConvert.DeserializeObject<T>(json);
    }
}
