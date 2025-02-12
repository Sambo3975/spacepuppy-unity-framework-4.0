﻿using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Collections;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Events
{

    public struct SPEventTrackedListenerToken : System.IDisposable
    {
        private BaseSPEvent _spevent;
        private System.EventHandler<TempEventArgs> _handler;

        public BaseSPEvent SPEvent => _spevent;

        public void Dispose()
        {
            if (_spevent != null && _handler != null)
            {
                _spevent.TriggerActivated -= _handler;
            }
            _spevent = null;
            _handler = null;
        }

        internal static SPEventTrackedListenerToken Create(BaseSPEvent spevent, System.EventHandler<TempEventArgs> handler)
        {
            if (spevent == null || handler == null) return default;
            spevent.TriggerActivated += handler;
            return new SPEventTrackedListenerToken()
            {
                _spevent = spevent,
                _handler = handler
            };
        }

    }

    public static class SPEventExtensions
    {
        public static IEnumerable<SPEventTrackedListenerToken> AddTrackedListeners(this IEnumerable<SPEvent> targets, System.EventHandler<TempEventArgs> handler)
        {
            if (handler == null) throw new System.ArgumentNullException(nameof(handler));
            if (targets == null) yield break;

            foreach (var t in targets)
            {
                if (t == null) continue;
                yield return t.AddTrackedListener(handler);
            }
        }

        public static void Dispose(this IEnumerable<SPEventTrackedListenerToken> hooks)
        {
            if (hooks == null) return;

            foreach (var h in hooks)
            {
                h.Dispose();
            }
        }

    }


    [System.Serializable()]
    public abstract class BaseSPEvent
    {

        public const string ID_DEFAULT = "Trigger";

        #region Events

        private System.EventHandler<TempEventArgs> _triggerActivated;
        public event System.EventHandler<TempEventArgs> TriggerActivated
        {
            add
            {
                _triggerActivated += value;
            }
            remove
            {
                _triggerActivated -= value;
            }
        }
        protected virtual void OnTriggerActivated(object sender, object arg)
        {
            try
            {
                _activationToken++;
                if (_triggerActivated != null)
                {
                    var e = TempEventArgs.Create(arg);
                    var d = _triggerActivated;
                    d(sender, e);
                    TempEventArgs.Release(e);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        protected bool HasTriggerActivatedListeners => _triggerActivated != null;

        #endregion

        #region Fields

        [SerializeField()]
        private List<EventTriggerTarget> _targets = new List<EventTriggerTarget>();
        [System.NonSerialized]
        private TargetList _targetsWrapper;

        [System.NonSerialized()]
        private string _id;

        [System.NonSerialized]
        private HashSet<object> _hijackTokens;

        [System.NonSerialized]
        private int _activationToken;

        #endregion

        #region CONSTRUCTOR

        public BaseSPEvent()
        {
            _id = ID_DEFAULT;
        }

        public BaseSPEvent(string id)
        {
            _id = id;
        }

        #endregion

        #region Properties

        public string ObservableTriggerId
        {
            get { return _id; }
            set { _id = value; }
        }

        public IList<EventTriggerTarget> Targets
        {
            get { return _targetsWrapper ?? (_targetsWrapper = new TargetList(this)); }
        }

        /// <summary>
        /// Number of registered targets, same as Targets.Count. This may return 0 even if there are event listeners.
        /// </summary>
        public int TargetCount
        {
            get { return _targets.Count; }
        }

        /// <summary>
        /// Returns true if TargetCount > 0 or there are event receivers attached. 
        /// </summary>
        public virtual bool HasReceivers
        {
            get { return _triggerActivated != null || _targets.Count > 0; }
        }

        public bool CurrentlyHijacked
        {
            get { return _hijackTokens != null && _hijackTokens.Count > 0; }
        }

        /// <summary>
        /// This value changes every time the event is triggered.
        /// </summary>
        public int ActivationToken => _activationToken;

        #endregion

        #region Methods

        public EventTriggerTarget AddNew()
        {
            var targ = new EventTriggerTarget();
            _targets.Add(targ);
            return targ;
        }

        /// <summary>
        /// Begins a hijack, when a trigger is hijacked none of its targets are triggered, but its TriggerActivated event still fires. 
        /// If tokens are passed in it allows compounded hijacking so that just because one caller ends the hijack, another can still continue hijacking.
        /// </summary>
        /// <param name="token"></param>
        public void BeginHijack(object token = null)
        {
            if (token == null) token = "*DEFAULT*";

            if (_hijackTokens == null) _hijackTokens = new HashSet<object>(EqualityComparer<object>.Default);
            _hijackTokens.Add(token);
        }

        /// <summary>
        /// Attempts to stop a hijack, but if more than one token has been used to hijack the event it may continue hijacking.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>Returns true if the trigger is no longer hijacked after calling this</returns>
        public bool EndHijack(object token = null)
        {
            if (_hijackTokens == null) return true;

            _hijackTokens.Remove(token ?? "*DEFAULT*");
            return _hijackTokens.Count == 0;
        }

        /// <summary>
        /// Forces the end of a hijack.
        /// </summary>
        public void ForceEndHijack()
        {
            if (_hijackTokens != null) _hijackTokens.Clear();
        }


        protected void ActivateTrigger(object sender, object arg)
        {
            if (_targets.Count > 0 && !this.CurrentlyHijacked)
            {
                var e = _targets.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current?.Trigger(sender, arg);
                }
            }

            this.OnTriggerActivated(sender, arg);
        }

        protected void ActivateTriggerAt(int index, object sender, object arg)
        {
            if (index >= 0 && index < _targets.Count && !this.CurrentlyHijacked)
            {
                _targets[index]?.Trigger(sender, arg);
            }

            this.OnTriggerActivated(sender, arg);
        }

        public SPEventTrackedListenerToken AddTrackedListener(System.EventHandler<TempEventArgs> handler) => SPEventTrackedListenerToken.Create(this, handler);

        #endregion

        #region Special Types

        public struct Enumerator : IEnumerator<EventTriggerTarget>
        {

            private List<EventTriggerTarget>.Enumerator _e;

            public Enumerator(BaseSPEvent t)
            {
                _e = t._targets.GetEnumerator();
            }

            public EventTriggerTarget Current
            {
                get { return _e.Current; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return _e.Current; }
            }

            public bool MoveNext()
            {
                return _e.MoveNext();
            }

            void System.Collections.IEnumerator.Reset()
            {
                (_e as IEnumerator<EventTriggerTarget>).Reset();
            }

            public void Dispose()
            {
                _e.Dispose();
            }

        }

        private class TargetList : IList<EventTriggerTarget>
        {

            private BaseSPEvent _owner;

            public TargetList(BaseSPEvent owner)
            {
                _owner = owner;
            }

            public EventTriggerTarget this[int index] { get { return _owner._targets[index]; } set { throw new System.NotSupportedException(); } }

            public int Count { get { return _owner._targets.Count; } }

            public bool IsReadOnly { get { return false; } }

            public void Add(EventTriggerTarget item)
            {
                if (_owner._targets.Contains(item)) return;
                _owner._targets.Add(item);
            }

            public void Clear()
            {
                _owner._targets.Clear();
            }

            public bool Contains(EventTriggerTarget item)
            {
                return _owner._targets.Contains(item);
            }

            public void CopyTo(EventTriggerTarget[] array, int arrayIndex)
            {
                _owner._targets.CopyTo(array, arrayIndex);
            }

            public int IndexOf(EventTriggerTarget item)
            {
                return _owner._targets.IndexOf(item);
            }

            public void Insert(int index, EventTriggerTarget item)
            {
                int i = _owner._targets.IndexOf(item);
                if (i >= 0)
                {
                    if (i < index)
                    {
                        _owner._targets.RemoveAt(i);
                        index--; //slide back a slot to make up for the removal
                    }
                    else if (i == index)
                    {
                        //it already exists at index, do nothing
                        return;
                    }
                    else
                    {
                        _owner._targets.RemoveAt(index);
                    }
                }

                _owner._targets.Insert(index, item);
            }

            public bool Remove(EventTriggerTarget item)
            {
                bool result = _owner._targets.Remove(item);
                return result;
            }

            public void RemoveAt(int index)
            {
                _owner._targets.RemoveAt(index);
            }

            public BaseSPEvent.Enumerator GetEnumerator()
            {
                return new BaseSPEvent.Enumerator(_owner);
            }

            IEnumerator<EventTriggerTarget> IEnumerable<EventTriggerTarget>.GetEnumerator()
            {
                return new BaseSPEvent.Enumerator(_owner);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return new BaseSPEvent.Enumerator(_owner);
            }
        }

        #endregion

    }

    [System.Serializable]
    public abstract class BaseSPDelegate<TDelegate> : BaseSPEvent where TDelegate : System.Delegate
    {

        #region Fields

        private TDelegate _handle = null;

        #endregion

        #region CONSTRUCTOR

        public BaseSPDelegate()
        {
        }

        public BaseSPDelegate(string id) : base(id)
        {
        }

        #endregion

        #region Properties

        public override bool HasReceivers => _handle != null || base.HasReceivers;

        #endregion

        #region Methods

        public virtual void Register(TDelegate callback)
        {
            _handle = System.Delegate.Combine(_handle, callback) as TDelegate;
        }

        public virtual void Unregister(TDelegate callback)
        {
            _handle = System.Delegate.Remove(_handle, callback) as TDelegate;
        }

        protected TDelegate Trigger
        {
            get
            {
                return _handle;
            }
        }

        #endregion

    }



    [System.Serializable]
    public class SPDelegate : BaseSPDelegate<System.Action>
    {

        #region CONSTRUCTOR

        public SPDelegate()
        {
        }

        public SPDelegate(string id) : base(id)
        {
        }

        #endregion

        public virtual void ActivateTrigger(object sender)
        {
            if (this.TargetCount > 0 || this.HasTriggerActivatedListeners) base.ActivateTrigger(sender, null);
            this.Trigger?.Invoke();
        }

    }

    [System.Serializable]
    public class SPDelegate<T> : BaseSPDelegate<System.Action<T>>
    {

        #region CONSTRUCTOR

        public SPDelegate()
        {
        }

        public SPDelegate(string id) : base(id)
        {
        }

        #endregion

        public virtual void ActivateTrigger(object sender, T arg)
        {
            if (this.TargetCount > 0 || this.HasTriggerActivatedListeners) base.ActivateTrigger(sender, arg);
            this.Trigger?.Invoke(arg);
        }

    }

    [System.Serializable]
    public class SPDelegate<T1, T2> : BaseSPDelegate<System.Action<T1, T2>>
    {

        #region CONSTRUCTOR

        public SPDelegate()
        {
        }

        public SPDelegate(string id) : base(id)
        {
        }

        #endregion

        public virtual void ActivateTrigger(object sender, T1 arg1, T2 arg2)
        {
            int targcnt = this.TargetCount;
            if (targcnt > 0)
            {
                object boxedArg1 = arg1;
                for (int i = 0; i < targcnt; i++)
                {
                    var targ = this.Targets[i];
                    if (targ.IsMultiArgMethod())
                    {
                        targ.TryTriggerAsMultiArgMethod(sender, arg1, arg2);
                    }
                    else
                    {
                        targ.Trigger(sender, boxedArg1);
                    }
                }
            }
            if (this.HasTriggerActivatedListeners) this.OnTriggerActivated(sender, arg1);
            this.Trigger?.Invoke(arg1, arg2);
        }

    }

    [System.Serializable]
    public class SPDelegate<T1, T2, T3> : BaseSPDelegate<System.Action<T1, T2, T3>>
    {

        #region CONSTRUCTOR

        public SPDelegate()
        {
        }

        public SPDelegate(string id) : base(id)
        {
        }

        #endregion

        public virtual void ActivateTrigger(object sender, T1 arg1, T2 arg2, T3 arg3)
        {
            int targcnt = this.TargetCount;
            if (targcnt > 0)
            {
                object boxedArg1 = arg1;
                for (int i = 0; i < targcnt; i++)
                {
                    var targ = this.Targets[i];
                    if (targ.IsMultiArgMethod())
                    {
                        targ.TryTriggerAsMultiArgMethod(sender, arg1, arg2, arg3);
                    }
                    else
                    {
                        targ.Trigger(sender, boxedArg1);
                    }
                }
            }
            if (this.HasTriggerActivatedListeners) this.OnTriggerActivated(sender, arg1);
            this.Trigger?.Invoke(arg1, arg2, arg3);
        }

    }

    [System.Serializable]
    public class SPDelegate<T1, T2, T3, T4> : BaseSPDelegate<System.Action<T1, T2, T3, T4>>
    {

        #region CONSTRUCTOR

        public SPDelegate()
        {
        }

        public SPDelegate(string id) : base(id)
        {
        }

        #endregion

        public virtual void ActivateTrigger(object sender, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            int targcnt = this.TargetCount;
            if (targcnt > 0)
            {
                object boxedArg1 = arg1;
                for (int i = 0; i < targcnt; i++)
                {
                    var targ = this.Targets[i];
                    if (targ.IsMultiArgMethod())
                    {
                        targ.TryTriggerAsMultiArgMethod(sender, arg1, arg2, arg3, arg4);
                    }
                    else
                    {
                        targ.Trigger(sender, boxedArg1);
                    }
                }
            }
            if (this.HasTriggerActivatedListeners) this.OnTriggerActivated(sender, arg1);
            this.Trigger?.Invoke(arg1, arg2, arg3, arg4);
        }

    }



    [System.Serializable()]
    public class SPEvent : BaseSPEvent
    {

        #region CONSTRUCTOR

        public SPEvent()
        {
        }

        public SPEvent(string id) : base(id)
        {
        }

        #endregion

        #region Methods

        public new virtual void ActivateTrigger(object sender, object arg)
        {
            base.ActivateTrigger(sender, arg);
        }

        public new virtual void ActivateTriggerAt(int index, object sender, object arg)
        {
            base.ActivateTriggerAt(index, sender, arg);
        }

        public virtual bool ActivateRandomTrigger(object sender, object arg, bool considerWeights, bool selectOnlyIfActive, IRandom rng = null)
        {
            try
            {
                bool result = false;
                if (this.Targets.Count > 0 && !this.CurrentlyHijacked)
                {
                    EventTriggerTarget trig;
                    if (selectOnlyIfActive)
                    {
                        using (var lst = TempCollection.GetList<EventTriggerTarget>())
                        {
                            for (int i = 0; i < this.Targets.Count; i++)
                            {
                                var go = GameObjectUtil.GetGameObjectFromSource(this.Targets[i].CalculateTarget(arg));
                                if (object.ReferenceEquals(go, null) || go.IsAliveAndActive()) lst.Add(this.Targets[i]);
                            }
                            trig = (considerWeights) ? lst.PickRandom((t) => { return t.Weight; }, rng) : lst.PickRandom(rng);
                        }
                    }
                    else
                    {
                        trig = (considerWeights) ? this.Targets.PickRandom((t) => { return t.Weight; }, rng) : this.Targets.PickRandom(rng);
                    }

                    if (trig != null)
                    {
                        result = true;
                        trig.Trigger(sender, arg);
                    }
                }

                this.OnTriggerActivated(sender, arg);
                return result;
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        #endregion

        #region Special Types

        /*
         * This may be defined here, it is still usable on all types inheriting from BaseSPEvent. It's only here for namespace purposes to be consistent across the framework.
         */
        public class ConfigAttribute : System.Attribute
        {
            public bool Weighted;
            public bool AlwaysExpanded;
            public string ArgDescription;

            public ConfigAttribute()
            {

            }

            public ConfigAttribute(string argdesc)
            {
                this.ArgDescription = argdesc;
            }

        }

        #endregion

    }

    [System.Serializable()]
    public class SPEvent<T> : BaseSPDelegate<System.EventHandler<T>> where T : System.EventArgs
    {

        #region Events

        public new event System.EventHandler<T> TriggerActivated
        {
            add
            {
                this.Register(value);
            }
            remove
            {
                this.Unregister(value);
            }
        }

        #endregion

        #region CONSTRUCTOR

        public SPEvent()
        {
        }

        public SPEvent(string id) : base(id)
        {
        }

        #endregion

        #region Methods

        public virtual void ActivateTrigger(object sender, T e)
        {
            base.ActivateTrigger(sender, e);
            this.Trigger?.Invoke(sender, e);
        }

        #endregion

    }

}
