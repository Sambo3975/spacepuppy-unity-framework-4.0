#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using com.spacepuppy.Async;
using com.spacepuppy.Events;

#if SP_UNITASK
using Cysharp.Threading.Tasks;
#endif

namespace com.spacepuppy.Scenes.Events
{

    public class i_LoadSceneByOptions : AutoTriggerable
    {

        #region Fields

        [SerializeReference, SerializeRefPicker(typeof(LoadSceneOptions), AllowNull = false, AlwaysExpanded = true, DisplayBox = true)]
        private LoadSceneOptions _options;

        [Infobox("If the targets of this complete event get destroyed during the load they will not activate.")]
        [SerializeField]
        private SPEvent _onComplete = new SPEvent("OnComplete");

        #endregion

        #region Properties

        public LoadSceneOptions Options
        {
            get => _options;
            set => _options = value;
        }

        public SPEvent OnComplete => _onComplete;

        #endregion

        #region Methods

        //public override bool CanTrigger => base.CanTrigger && _options != null;

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("i_LoadSceneByOptions->" + (_options?.ToString() ?? "NULL"));
#endif
            if (_options == null) return false;

            var handle = _options.Clone();
            if (handle == null) return false;

            if (_onComplete.HasReceivers && handle != null)
            {
                handle.Complete += (s, e) =>
                {
                    _onComplete.ActivateTrigger(this, null);
                };
            };
            SceneManagerUtils.LoadScene(handle);

            return true;
        }

        #endregion

        #region Special Types

        /// <summary>
        /// Represents a simple load situation (no asyncawait). This primarily exists to facilitate i_LoadSceneByOptions.
        /// </summary>
        [System.Serializable]
        public sealed class SimpleLoadSceneOptions : LoadSceneOptions
        {

            #region Fields

            [SerializeField]
            [Tooltip("Prefix with # to load by index.")]
            private SceneRef _scene;
            [SerializeField]
            private LoadSceneMode _mode;
            [SerializeField]
            [EnumPopupExcluding((int)LoadSceneBehaviour.AsyncAndWait)]
            private LoadSceneBehaviour _behaviour;

            [SerializeField]
            [Tooltip("A token used to persist data across scenes.")]
            VariantReference _persistentToken = new VariantReference();

            #endregion

            #region Properties

            public SceneRef ConfiguredScene
            {
                get => _scene;
                set => _scene = value;
            }

            public LoadSceneMode ConfiguredMode
            {
                get => _mode;
                set => _mode = value;
            }

            public LoadSceneBehaviour ConfiguredBehaviour
            {
                get => _behaviour;
                set => _behaviour = value.RestrictAsyncAndAwait();
            }

            public VariantReference ConfiguredPersistentToken => _persistentToken;

            #endregion

            #region Methods

            public override string ToString()
            {
                return $"{base.ToString()}({_scene.SceneName})";
            }

            #endregion

            #region LoadSceneOptions Interface

            public override LoadSceneMode Mode => _mode;

#if SP_UNITASK
            protected override void DoBegin(ISceneManager manager)
            {
                _ = this.DoBeginUniTask(manager);
            }

            private async UniTaskVoid DoBeginUniTask(ISceneManager manager)
            {
#else
            protected override async void DoBegin(ISceneManager manager)
            {
#endif
                try
                {
                    this.PersistentToken = IProxyExtensions.ReduceIfProxy(_persistentToken.Value);

                    LoadSceneInternalResult handle = this.LoadScene(_scene, _mode, _behaviour.RestrictAsyncAndAwait());

#if SP_UNITASK
                    await handle.AsUniTask();
#else
                    await handle.AsTask();
#endif
                    this.SignalComplete();
                }
                catch (System.Exception ex)
                {
                    this.SignalError();
                    throw ex;
                }
            }

            #endregion

            #region ICloneable Interface

            public override LoadSceneOptions Clone()
            {
                var result = base.Clone() as SimpleLoadSceneOptions;
                result._persistentToken = _persistentToken.Clone();
                return result;
            }

            #endregion

        }

        #endregion

    }
}
