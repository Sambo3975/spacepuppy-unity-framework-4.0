﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using com.spacepuppy.Utils;

namespace com.spacepuppy
{

    public abstract class SPScriptableObject : ScriptableObject, ISPDisposable, INameable
    {

        #region Fields

        #endregion

        #region ISPDisposable Interface

        bool ISPDisposable.IsDisposed
        {
            get
            {
                return !ObjUtil.IsObjectAlive(this);
            }
        }

        void System.IDisposable.Dispose()
        {
            ObjUtil.SmartDestroy(this);
        }

        #endregion

        #region INameable Interface

        private NameCache.UnityObjectNameCache _nameCache;
        public new string name
        {
            get { return (_nameCache ??= new NameCache.UnityObjectNameCache(this)).Name; }
            set { (_nameCache ??= new NameCache.UnityObjectNameCache(this)).Name = value; }
        }
        string INameable.Name
        {
            get { return (_nameCache ??= new NameCache.UnityObjectNameCache(this)).Name; }
            set { (_nameCache ??= new NameCache.UnityObjectNameCache(this)).Name = value; }
        }
        public bool CompareName(string nm)
        {
            return (_nameCache ??= new NameCache.UnityObjectNameCache(this)).CompareName(nm);
        }
        void INameable.SetDirty()
        {
            (_nameCache ??= new NameCache.UnityObjectNameCache(this)).SetDirty();
        }

        #endregion

    }

    /// <summary>
    /// Represents a SPComponent without the eventful nature, use this only if you specifically need a component that doesn't use 'enable'. 
    /// In most cases you should be inheriting from SPComponent. If inheriting from this you likely should be sealing the class.
    /// </summary>
    public abstract class SPMonoBehaviour : MonoBehaviour, IComponent, ISPDisposable, INameable
    {

        #region Fields

        [System.NonSerialized]
        private List<IMixin> _mixins;

        #endregion

        #region CONSTRUCTOR

        protected virtual void Awake()
        {
            if (this is IAutoMixinDecorator amxd) this.RegisterMixins(MixinUtil.CreateAutoMixins(amxd));
        }

        #endregion

        #region Methods

        protected void RegisterMixins(IEnumerable<IMixin> mixins)
        {
            if (mixins == null) throw new System.ArgumentNullException(nameof(mixins));
            foreach (var mixin in mixins)
            {
                if (mixin.Awake(this))
                {
                    (_mixins = _mixins ?? new List<IMixin>()).Add(mixin);
                }
            }
        }

        protected void RegisterMixin(IMixin mixin)
        {
            if (mixin == null) throw new System.ArgumentNullException(nameof(mixin));

            if (mixin.Awake(this))
            {
                (_mixins = _mixins ?? new List<IMixin>()).Add(mixin);
            }
        }

        public T GetMixinState<T>() where T : class, IMixin
        {
            if (_mixins != null)
            {
                for (int i = 0; i < _mixins.Count; i++)
                {
                    if (_mixins[i] is T mixin) return mixin;
                }
            }
            return null;
        }

        /// <summary>
        /// This should only be used if you're not using RadicalCoroutine. If you are, use StopAllRadicalCoroutines instead.
        /// </summary>
        public new void StopAllCoroutines()
        {
            //this is an attempt to capture this method, it's not guaranteed and honestly you should avoid calling StopAllCoroutines all together and instead call StopAllRadicalCoroutines.
            this.SendMessage("RadicalCoroutineManager_InternalHook_StopAllCoroutinesCalled", this, SendMessageOptions.DontRequireReceiver);
            base.StopAllCoroutines();
        }

        #endregion

        #region IComponent Interface

        bool IComponent.enabled
        {
            get { return this.enabled; }
            set { this.enabled = value; }
        }

        Component IComponent.component
        {
            get { return this; }
        }

        //implemented implicitly
        /*
        GameObject IComponent.gameObject { get { return this.gameObject; } }
        Transform IComponent.transform { get { return this.transform; } }
        */

        #endregion

        #region ISPDisposable Interface

        bool ISPDisposable.IsDisposed
        {
            get
            {
                return !ObjUtil.IsObjectAlive(this);
            }
        }

        void System.IDisposable.Dispose()
        {
            ObjUtil.SmartDestroy(this);
        }

        #endregion

        #region INameable Interface

        public new string name
        {
            get => NameCache.GetCachedName(this.gameObject);
            set => NameCache.SetCachedName(this.gameObject, value);
        }
        string INameable.Name
        {
            get => NameCache.GetCachedName(this.gameObject);
            set => NameCache.SetCachedName(this.gameObject, value);
        }
        public bool CompareName(string nm) => this.gameObject.CompareName(nm);
        void INameable.SetDirty() => NameCache.SetDirty(this.gameObject);

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Move To Top")]
        void ComponentEditor_MoveToTop()
        {
            int steps = this.gameObject.GetComponentIndex(this) - 1;
            for (int i = 0; i < steps; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentUp(this);
            }
        }

        [ContextMenu("Move To Bottom")]
        void ComponentEditor_MoveToBottom()
        {
            int lastindex = this.gameObject.GetComponentCount() - 1;
            int steps = lastindex - this.gameObject.GetComponentIndex(this);
            for (int i = 0; i < steps; i++)
            {
                UnityEditorInternal.ComponentUtility.MoveComponentDown(this);
            }
        }
#endif

    }

    /// <summary>
    /// A base implementation of components used by most of Spacepuppy Framework. It expands on the functionality of MonoBehaviour as well as implements various interfaces from the Spacepuppy framework. 
    /// 
    /// All scripts that are intended to work in tandem with Spacepuppy Unity Framework should inherit from this instead of MonoBehaviour.
    /// </summary>
    public abstract class SPComponent : SPMonoBehaviour, IEventfulComponent
    {

        #region Events

        public event System.EventHandler OnEnabled;
        public event System.EventHandler OnStarted;
        public event System.EventHandler OnDisabled;
        public event System.EventHandler ComponentDestroyed;

        #endregion

        #region Fields

        #endregion

        #region CONSTRUCTOR

        protected virtual void Start()
        {
            this.started = true;
            try
            {
                this.OnStarted?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected virtual void OnEnable()
        {
            try
            {
                this.OnEnabled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected virtual void OnDisable()
        {
            try
            {
                this.OnDisabled?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected virtual void OnDestroy()
        {
            try
            {
                this.ComponentDestroyed?.Invoke(this, System.EventArgs.Empty);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Start has been called on this component.
        /// </summary>
        public bool started { get; private set; }

        #endregion

    }

    /// <summary>
    /// Represents a component that should always exist as a member of an entity.
    /// 
    /// Such a component should not change parents frequently as it would be expensive.
    /// </summary>
    [RequireComponentInEntity(typeof(SPEntity))]
    public abstract class SPEntityComponent : SPComponent
    {

        #region Fields

        [System.NonSerialized]
        private SPEntity _entity;
        [System.NonSerialized]
        private GameObject _entityRoot;
        [System.NonSerialized]
        private bool _synced;

        #endregion

        #region Properties

        public SPEntity Entity
        {
            get
            {
                if (!_synced) this.SyncRoot();
                return _entity;
            }
        }

        public GameObject entityRoot
        {
            get
            {
                if (!_synced) this.SyncRoot();
                return _entityRoot;
            }
        }

        #endregion

        #region Methods

        protected virtual void OnTransformParentChanged()
        {
            _synced = false;
            _entity = null;
            _entityRoot = null;
        }

        protected void SyncRoot()
        {
            _synced = true;
            _entity = SPEntity.Pool.GetFromSource(this);
            _entityRoot = (_entity != null) ? _entity.gameObject : this.gameObject;
        }

        #endregion

    }

}
