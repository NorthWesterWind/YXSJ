using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Utils
{
    public class ResourceLoader : SingletonBase<ResourceLoader>
    {
        private readonly Dictionary<string, AsyncOperationHandle> _cache = new ();
        private readonly object _cacheLock = new object();

        /// <summary>
        /// 通用加载并缓存 Addressables 资源
        /// </summary>
        public async Task<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
        {
            // 先看缓存
            AsyncOperationHandle cachedHandle = default;
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(address, out var h))
                {
                    if (h.IsValid())
                        cachedHandle = h;
                    else
                        _cache.Remove(address);
                }
            }

            // 如果有缓存的 handle，等它完成（可能已完成），然后返回结果（或在失败时移除缓存重载）
            if (cachedHandle.IsValid())
            {
                if (!cachedHandle.IsDone)
                    await cachedHandle.Task;

                if (cachedHandle.Status == AsyncOperationStatus.Succeeded)
                    return cachedHandle.Result as T;

                // 缓存的 handle 失败，移除它并继续重新加载
                lock (_cacheLock) { _cache.Remove(address); }
            }

            // 发起新的加载并缓存 handle（覆盖之前的）
            var handle = Addressables.LoadAssetAsync<T>(address);
            lock (_cacheLock) { _cache[address] = handle; }

            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
                return handle.Result;

            // 加载失败：如果缓存中依旧是这个失败的 handle，则移除
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(address, out var existing) && existing.Equals(handle))
                    _cache.Remove(address);
            }

            Debug.LogError($"Failed to load resource at {address}");
            throw new Exception($"Failed to load resource at {address}");
        }
        
        
        public async Task<T> LoadUIAsync<T>(string address) where T : Object
        {
            var asset = await LoadAssetAsync<T>(address);
            if (asset is GameObject go)
            {
                return GameObject.Instantiate(go) as T;
            }
            else
            {
                return asset;
            }
        }

        /// <summary>
        /// 释放单个缓存资源（并从缓存移除）
        /// </summary>
        public void ReleaseAsset(string address)
        {
            AsyncOperationHandle handle = default;
            lock (_cacheLock)
            {
                if (!_cache.TryGetValue(address, out handle)) return;
                _cache.Remove(address);
            }

            if (handle.IsValid())
                Addressables.Release(handle);
        }
        
        
    }
}
