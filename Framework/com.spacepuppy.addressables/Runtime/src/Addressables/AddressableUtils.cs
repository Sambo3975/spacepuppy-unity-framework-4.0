using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using com.spacepuppy.Utils;
using com.spacepuppy.Async;
using System.Threading.Tasks;

namespace com.spacepuppy.Addressables
{

    public static class AddressableUtils
    {

        public static AsyncWaitHandle AsAsyncWaitHandle(this AsyncOperationHandle handle)
        {
            return new AsyncWaitHandle(AsyncOperationHandleProvider.Default, handle);
        }

        public static AsyncWaitHandle<TObject> AsAsyncWaitHandle<TObject>(this AsyncOperationHandle<TObject> handle)
        {
            return new AsyncWaitHandle<TObject>(AsyncOperationHandleProvider<TObject>.Default, handle);
        }

        /// <summary>
        /// Returns true if the AssetReference has a configured target. That target may not necessarily be valid, but one is configured.
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static bool HasTargetGuid(this AssetReference asset)
        {
            return !string.IsNullOrEmpty(asset?.AssetGUID);
        }

        public static AsyncOperationHandle<TObject> LoadAssetSPManagedAsync<TObject>(this AssetReference reference)
        {
            var handle = reference.LoadAssetAsync<TObject>();
            handle.Completed += (h) =>
            {
                if(h.Status == AsyncOperationStatus.Succeeded)
                {
                    var go = GameObjectUtil.GetGameObjectFromSource(h.Result);
                    if (go != null)
                    {
                        go.AddOrGetComponent<AddressableKillHandle>();
                    }
                }
            };
            return handle;
        }

        public static AsyncOperationHandle<GameObject> InstantiateSPManagedAsync(this AssetReference reference, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var handle = reference.InstantiateAsync(position, rotation, parent);
            handle.Completed += (h) =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    h.Result.AddOrGetComponent<AddressableKillHandle>();
                }
            };
            return handle;
        }

        public static AsyncOperationHandle<GameObject> InstantiateSPManagedAsync<TObject>(this AssetReference reference, Transform parent = null, bool instantiateInWorldSpace = false)
        {
            var handle = reference.InstantiateAsync(parent, instantiateInWorldSpace);
            handle.Completed += (h) =>
            {
                if (h.Status == AsyncOperationStatus.Succeeded)
                {
                    h.Result.AddOrGetComponent<AddressableKillHandle>();
                }
            };
            return handle;
        }


        #region Special Typers

        private class AsyncOperationHandleProvider : IAsyncWaitHandleProvider
        {
            public static readonly AsyncOperationHandleProvider Default = new AsyncOperationHandleProvider();

            public float GetProgress(object token)
            {
                if (token is AsyncOperationHandle h)
                {
                    return h.IsDone ? 1f : h.PercentComplete;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public System.Threading.Tasks.Task GetTask(object token)
            {
                if(token is AsyncOperationHandle h)
                {
                    if(GameLoop.InvokeRequired)
                    {
                        Task result = null;
                        GameLoop.UpdateHandle.Invoke(() => result = h.IsDone ? System.Threading.Tasks.Task.CompletedTask : h.Task);
                        return result ?? Task.CompletedTask;
                    }
                    else
                    {
                        return h.IsDone ? System.Threading.Tasks.Task.CompletedTask : h.Task;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public object GetYieldInstruction(object token)
            {
                if (token is AsyncOperationHandle)
                {
                    return token;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public bool IsComplete(object token)
            {
                if (token is AsyncOperationHandle h)
                {
                    return h.IsDone;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle> callback)
            {
                if (token is AsyncOperationHandle h)
                {
                    if(GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsDone)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.Completed += (aoh) =>
                                {
                                    callback(aoh.AsAsyncWaitHandle());
                                };
                            }
                        });
                    }
                    else
                    {
                        if(h.IsDone)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.Completed += (aoh) =>
                            {
                                callback(aoh.AsAsyncWaitHandle());
                            };
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

            public object GetResult(object token)
            {
                if (token is AsyncOperationHandle h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider was associated with a token that was not an AsyncOperationHandle.");
                }
            }

        }

        private class AsyncOperationHandleProvider<TObject> : IAsyncWaitHandleProvider<TObject>
        {
            public static readonly AsyncOperationHandleProvider<TObject> Default = new AsyncOperationHandleProvider<TObject>();

            public float GetProgress(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.IsDone ? 1f : h.PercentComplete;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public System.Threading.Tasks.Task<TObject> GetTask(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        Task<TObject> result = null;
                        GameLoop.UpdateHandle.Invoke(() => result = h.IsDone ? Task.FromResult(h.Result) : h.Task);
                        return result ?? Task.FromResult(h.Result);
                    }
                    else
                    {
                        return h.IsDone ? Task.FromResult(h.Result) : h.Task;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            System.Threading.Tasks.Task IAsyncWaitHandleProvider.GetTask(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        Task<TObject> result = null;
                        GameLoop.UpdateHandle.Invoke(() => result = h.IsDone ? Task.FromResult(h.Result) : h.Task);
                        return result ?? Task.FromResult(h.Result);
                    }
                    else
                    {
                        return h.IsDone ? Task.FromResult(h.Result) : h.Task;
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public object GetYieldInstruction(object token)
            {
                if (token is AsyncOperationHandle<TObject>)
                {
                    return token;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public bool IsComplete(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.IsDone;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle> callback)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    if(GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if(h.IsDone)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.Completed += (aoh) =>
                                {
                                    callback(aoh.AsAsyncWaitHandle());
                                };
                            }
                        });
                    }
                    else
                    {
                        if (h.IsDone)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.Completed += (aoh) =>
                            {
                                callback(aoh.AsAsyncWaitHandle());
                            };
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public void OnComplete(object token, System.Action<AsyncWaitHandle<TObject>> callback)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    if (GameLoop.InvokeRequired)
                    {
                        GameLoop.UpdateHandle.BeginInvoke(() =>
                        {
                            if (h.IsDone)
                            {
                                callback(h.AsAsyncWaitHandle());
                            }
                            else
                            {
                                h.Completed += (aoh) =>
                                {
                                    callback(aoh.AsAsyncWaitHandle());
                                };
                            }
                        });
                    }
                    else
                    {
                        if(h.IsDone)
                        {
                            callback(h.AsAsyncWaitHandle());
                        }
                        else
                        {
                            h.Completed += (aoh) =>
                            {
                                callback(aoh.AsAsyncWaitHandle());
                            };
                        }
                    }
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            public TObject GetResult(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }

            object IAsyncWaitHandleProvider.GetResult(object token)
            {
                if (token is AsyncOperationHandle<TObject> h)
                {
                    return h.Result;
                }
                else
                {
                    throw new System.InvalidOperationException("An instance of AsyncOperationHandleProvider<TObject> was associated with a token that was not an AsyncOperationHandle<TObject>.");
                }
            }
        }

        #endregion

    }

}
