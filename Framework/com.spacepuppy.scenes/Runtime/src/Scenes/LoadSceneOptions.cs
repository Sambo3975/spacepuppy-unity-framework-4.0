﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Collections.Generic;
using System.Linq;
using com.spacepuppy.Dynamic;
using com.spacepuppy.Async;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Scenes
{

    public enum LoadSceneOptionsStatus
    {
        Unused = 0,
        Running = 1,
        Complete = 2,
        Error = 3,
    }

    /// <summary>
    /// Base abstract class for all LoadSceneOptions (it's not an interface since it has to be an EventArgs and there is a lot of helper methods to facilitate functionality). 
    /// 
    /// When implmenting:
    /// Implement all the abstract methods, as well as the Clone method. It is important that when cloning your LoadSceneOptions that any state info used during loading are reset. 
    /// It is best to call base.Clone from your override as it cleans up the inherited state as well. Things to clean up include registered event handlers, status's, wait handles, etc. 
    /// 
    /// Then when implementing 'DoBegin' make sure to call SignalComplete when finished, or SignalError on failure, otherwise the wait handle will block forever.
    /// 
    /// Other methods remain virtual for more advanced implementations of LoadSceneOptions. 
    /// </summary>
    public abstract class LoadSceneOptions : System.EventArgs, IProgressingYieldInstruction, IRadicalWaitHandle, IRadicalEnumerator, ISPDisposable, System.ICloneable
    {

        private static readonly System.Action<ISceneLoadedGlobalHandler, LoadSceneOptions> OnSceneLoadedFunctor = (o, d) => o.OnSceneLoaded(d);

        public event System.EventHandler<LoadSceneOptions> BeforeLoadBegins;
        public event System.EventHandler<LoadSceneOptions> BeforeSceneLoadCalled;
        public event System.EventHandler<LoadSceneOptions> Complete;

        #region Fields

        private UnityLoadResult _primaryResult;
        private List<UnityLoadResult> _additiveResults;

        #endregion

        #region Properties

        public ISceneManager SceneManager
        {
            get;
            private set;
        }

        public LoadSceneOptionsStatus Status
        {
            get;
            private set;
        }

        /// <summary>
        /// Use this to pass some token between scenes. 
        /// Anything that handles the ISceneLoadedMessageReceiver will receiver a reference to this handle, and therefore this token.
        /// The token should not be something that is destroyed by the load process.
        /// </summary>
        public object PersistentToken
        {
            get;
            set;
        }

        /// <summary>
        /// The primary scene being loaded by these options. If the options load more than 1 scene, this should be the dominant scene.
        /// </summary>
        public virtual Scene Scene => _primaryResult.Scene;

        /// <summary>
        /// How the primary scene returned by the 'Scene' property was loaded.
        /// </summary>
        public virtual LoadSceneMode Mode => _primaryResult.Mode;

        public virtual float Progress
        {
            get
            {
                switch (this.Status)
                {
                    case LoadSceneOptionsStatus.Unused:
                        return 0f;
                    case LoadSceneOptionsStatus.Complete:
                        return 1f;
                    default:
                        {
                            float p = _primaryResult.Progress;
                            int cnt = 1;
                            if (_additiveResults != null)
                            {
                                for (int i = 0; i < _additiveResults.Count; i++)
                                {
                                    if (_additiveResults[i].Scene.handle == _primaryResult.Scene.handle) continue;

                                    p += _additiveResults[i].Progress;
                                    cnt++;
                                }
                            }
                            return p / cnt;
                        }
                }
            }
        }

        protected UnityLoadResult PrimaryLoadResult => _primaryResult;

        #endregion

        #region Methods

        /// <summary>
        /// Begins loading the scene on the main thread. If called on another thread, will block until the next frame on mainthread.
        /// </summary>
        /// <param name="manager"></param>
        public void Begin(ISceneManager manager)
        {
            if (this.Status != LoadSceneOptionsStatus.Unused)
            {
                throw new System.InvalidOperationException("LoadSceneOptions should be ran only once. Clone if you need a copy.");
            }

            if (GameLoop.InvokeRequired)
            {
                GameLoop.UpdateHandle.Invoke(() => this.Begin(manager));
            }
            else
            {
                this.SceneManager = manager;
                this.Status = LoadSceneOptionsStatus.Running;
                this.OnBeforeLoadBegins();
                this.DoBegin(manager);
            }
        }

        protected UnityLoadResult LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async)
        {
            switch (behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    {
                        this.BeforeSceneLoadCalled?.Invoke(this, this);
                        UnitySceneManager.LoadScene(sceneName, mode);
                        var scene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                        return this.RegisterHandlesScene(null, scene, mode);
                    }
                case LoadSceneBehaviour.Async:
                    {
                        this.BeforeSceneLoadCalled?.Invoke(this, this);
                        var op = UnitySceneManager.LoadSceneAsync(sceneName, mode);
                        var scene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                        return this.RegisterHandlesScene(op, scene, mode);
                    }
                case LoadSceneBehaviour.AsyncAndWait:
                    {
                        this.BeforeSceneLoadCalled?.Invoke(this, this);
                        var op = UnitySceneManager.LoadSceneAsync(sceneName, mode);
                        op.allowSceneActivation = false;
                        var scene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                        return this.RegisterHandlesScene(op, scene, mode);
                    }
                default:
                    throw new System.InvalidOperationException("Unsupported LoadSceneBehaviour.");
            }
        }

        protected UnityLoadResult LoadScene(int index, LoadSceneMode mode = LoadSceneMode.Single, LoadSceneBehaviour behaviour = LoadSceneBehaviour.Async)
        {
            switch (behaviour)
            {
                case LoadSceneBehaviour.Standard:
                    {
                        this.BeforeSceneLoadCalled?.Invoke(this, this);
                        UnitySceneManager.LoadScene(index, mode);
                        var scene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                        return this.RegisterHandlesScene(null, scene, mode);
                    }
                case LoadSceneBehaviour.Async:
                    {
                        this.BeforeSceneLoadCalled?.Invoke(this, this);
                        var op = UnitySceneManager.LoadSceneAsync(index, mode);
                        var scene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                        return this.RegisterHandlesScene(op, scene, mode);
                    }
                case LoadSceneBehaviour.AsyncAndWait:
                    {
                        this.BeforeSceneLoadCalled?.Invoke(this, this);
                        var op = UnitySceneManager.LoadSceneAsync(index, mode);
                        op.allowSceneActivation = false;
                        var scene = UnitySceneManager.GetSceneAt(UnitySceneManager.sceneCount - 1);
                        return this.RegisterHandlesScene(op, scene, mode);
                    }
                default:
                    throw new System.InvalidOperationException("Unsupported LoadSceneBehaviour.");
            }
        }

        protected virtual void OnBeforeLoadBegins()
        {
            this.BeforeLoadBegins?.Invoke(this, this);
        }

        protected void SignalComplete()
        {
            this.Status = LoadSceneOptionsStatus.Complete;

            var d = this.Complete;
            this.Complete = null;
            try
            {
                d?.Invoke(this, this);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                com.spacepuppy.Utils.Messaging.Broadcast(this, OnSceneLoadedFunctor);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected void SignalError()
        {
            this.Status = LoadSceneOptionsStatus.Error;

            var d = this.Complete;
            this.Complete = null;
            try
            {
                d?.Invoke(this, this);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            try
            {
                com.spacepuppy.Utils.Messaging.Broadcast(this, OnSceneLoadedFunctor);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private UnityLoadResult RegisterHandlesScene(AsyncOperation op, Scene scene, LoadSceneMode mode)
        {
            var result = new UnityLoadResult()
            {
                Op = op,
                Scene = scene,
                Mode = mode
            };
            switch (mode)
            {
                case LoadSceneMode.Single:
                    _primaryResult = result;
                    _additiveResults?.Clear();
                    break;
                case LoadSceneMode.Additive:
                default:
                    if (!_primaryResult.IsValid)
                    {
                        _primaryResult = result;
                    }
                    if (_additiveResults == null) _additiveResults = new List<UnityLoadResult>();
                    _additiveResults.Add(result);
                    break;
            }
            return result;
        }


        public virtual bool HandlesScene(Scene scene)
        {
            if (!scene.IsValid()) return false;

            if (_primaryResult.Scene == scene)
            {
                return true;
            }
            else if (_additiveResults != null)
            {
                for (int i = 0; i < _additiveResults.Count; i++)
                {
                    if (_additiveResults[i].Scene.handle == scene.handle) return true;
                }
            }
            return false;
        }

        public virtual IEnumerable<Scene> GetHandledScenes()
        {
            if (_primaryResult.IsValid) yield return _primaryResult.Scene;

            if (_additiveResults != null)
            {
                for (int i = 0; i < _additiveResults.Count; i++)
                {
                    if (_additiveResults[i].Scene.handle == _primaryResult.Scene.handle) continue;

                    yield return _additiveResults[i].Scene;
                }
            }
        }

        protected virtual IEnumerable<UnityLoadResult> GetHandledLoadResults()
        {
            if (_primaryResult.IsValid) yield return _primaryResult;

            if (_additiveResults != null)
            {
                for (int i = 0; i < _additiveResults.Count; i++)
                {
                    if (_additiveResults[i].Scene.handle == _primaryResult.Scene.handle) continue;

                    yield return _additiveResults[i];
                }
            }
        }

        #endregion

        #region Abstract Interface

        protected abstract void DoBegin(ISceneManager manager);

        #endregion

        #region ICloneable Interface

        object System.ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual LoadSceneOptions Clone()
        {
            var result = this.MemberwiseClone() as LoadSceneOptions;
            result._primaryResult = default;
            result._additiveResults = null;
            result.SceneManager = null;
            result.Status = LoadSceneOptionsStatus.Unused;
            result.BeforeLoadBegins = null;
            result.BeforeSceneLoadCalled = null;
            result.Complete = null;
            return result;
        }

        #endregion

        #region IProgressingAsyncOperation Interface

        public virtual bool IsComplete
        {
            get => this.Status >= LoadSceneOptionsStatus.Complete;
        }

        bool IRadicalYieldInstruction.Tick(out object yieldObject)
        {
            return this.Tick(out yieldObject);
        }
        protected virtual bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return !this.IsComplete;
        }

        #endregion

        #region IRadicalWaitHandle Interface

        bool IRadicalWaitHandle.Cancelled
        {
            get { return !_disposed; }
        }

        void IRadicalWaitHandle.OnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            this.RegisterWaitHandleOnComplete(callback);
        }

        protected virtual void RegisterWaitHandleOnComplete(System.Action<IRadicalWaitHandle> callback)
        {
            if (callback == null) return;
            this.Complete += (s, e) => callback(e);
        }

        #endregion

        #region IEnumerator Interface

        object System.Collections.IEnumerator.Current => null;

        bool System.Collections.IEnumerator.MoveNext()
        {
            object inst;
            return this.Tick(out inst);
        }

        void System.Collections.IEnumerator.Reset()
        {
            //do nothing
        }

        #endregion

        #region IDisposable Interface

        private bool _disposed;
        public bool IsDisposed
        {
            get { return _disposed; }
        }

        public virtual void Dispose()
        {
            _disposed = true;
        }

        ~LoadSceneOptions()
        {
            //make sure we clean ourselves up
            this.Dispose();
        }

        #endregion

        #region Special Types

        protected struct UnityLoadResult
        {
            public AsyncOperation Op;
            public Scene Scene;
            public LoadSceneMode Mode;

            public float Progress => Op?.progress ?? 0f;

            public bool IsComplete => Op?.isDone ?? false;

            public bool IsValid => Scene.IsValid();

            public async System.Threading.Tasks.Task<Scene> GetTask()
            {
                await Op.AsAsyncWaitHandle().AsTask();
                return this.Scene;
            }

#if SP_UNITASK
            public async UniTask<Scene> GetUniTask()
            {
                await Op;
                return this.Scene;
            }
#endif

            public object GetYieldInstruction()
            {
                return Op;
            }

            public void OnComplete(System.Action<UnityLoadResult> callback)
            {
                var r = this;
                Op.AsAsyncWaitHandle().OnComplete((h) => callback(r));
            }

            public static readonly UnityLoadResult Empty = new UnityLoadResult();
        }

        #endregion

    }

    public class UnmanagedSceneLoadedEventArgs : LoadSceneOptions
    {

        private Scene _scene;
        private LoadSceneMode _mode;

        internal UnmanagedSceneLoadedEventArgs(Scene scene, LoadSceneMode mode)
        {
            _scene = scene;
        }

        public override Scene Scene { get { return _scene; } }

        public override LoadSceneMode Mode { get { return _mode; } }

        public override float Progress { get { return 1f; } }

        public override bool IsComplete { get { return true; } }

        protected override void DoBegin(ISceneManager manager)
        {
            throw new System.NotSupportedException();
        }

        public override bool HandlesScene(Scene scene)
        {
            return _scene == scene;
        }

        protected override bool Tick(out object yieldObject)
        {
            yieldObject = null;
            return false;
        }
    }

}
