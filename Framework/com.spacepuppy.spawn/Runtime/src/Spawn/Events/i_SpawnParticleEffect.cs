﻿#pragma warning disable 0649 // variable declared but not used.

using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Events;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Spawn.Events
{

    public class i_SpawnParticleEffect : AutoTriggerable, IObservableTrigger, ISpawnPoint
    {

        public const string TRG_ONSPAWNED = "OnSpawned";

        public enum PositioningMode
        {
            AtSpawner = 0,
            AtSpawnedObjectParent = 1,
            FromPrefab = 2,
        }

        #region Fields

        [SerializeField()]
        [Tooltip("If left empty the default SpawnPool will be used instead.")]
        private SpawnPoolRef _spawnPool = new SpawnPoolRef();

        [SerializeField]
        [RespectsIProxy()]
        [TypeRestriction(typeof(Transform), AllowProxy = true, HideTypeDropDown = false)]
        private UnityEngine.Object _spawnedObjectParent;

        [SerializeField]
        private PositioningMode _positioning;

        [SerializeField()]
        [WeightedValueCollection("Weight", "Prefab", DrawElementAtBottom = true)]
        [Tooltip("ParticleEffects available for spawning. When spawn is called with no arguments a prefab is selected at random.")]
        private List<PrefabEntry> _prefabs;

        [SerializeField]
        private RandomRef _rng;

        [SerializeField()]
        [SPEvent.Config("spawned object (GameObject)")]
        private OnSpawnEvent _onSpawnedObject = new OnSpawnEvent();

        #endregion

        #region Properties

        public ISpawnPool SpawnPool
        {
            get { return _spawnPool.Value; }
            set { _spawnPool.Value = value; }
        }

        public UnityEngine.Object SpawnedObjectParent
        {
            get => _spawnedObjectParent;
            set => _spawnedObjectParent = value;
        }

        public PositioningMode Positioning
        {
            get => _positioning;
            set => _positioning = value;
        }

        public List<PrefabEntry> Prefabs
        {
            get { return _prefabs; }
        }

        public OnSpawnEvent OnSpawnedObject
        {
            get { return _onSpawnedObject; }
        }

        public IRandom RNG
        {
            get { return _rng.Value; }
            set { _rng.Value = value; }
        }

        #endregion

        #region Methods

        public GameObject Spawn()
        {
            if (!this.CanTrigger) return null;

            if (_prefabs == null || _prefabs.Count == 0) return null;

            if (_prefabs.Count == 1)
            {
                return this.Spawn(_prefabs[0]);
            }
            else
            {
                return this.Spawn(_prefabs.PickRandom((o) => o.Weight, _rng.Value));
            }
        }

        public GameObject Spawn(int index)
        {
            if (!this.enabled) return null;

            if (_prefabs == null || index < 0 || index >= _prefabs.Count) return null;
            return this.Spawn(_prefabs[index]);
        }

        public GameObject Spawn(string name)
        {
            if (!this.enabled) return null;

            if (_prefabs == null) return null;
            for (int i = 0; i < _prefabs.Count; i++)
            {
                if (_prefabs[i].Prefab != null && _prefabs[i].Prefab.CompareName(name)) return this.Spawn(_prefabs[i]);
            }
            return null;
        }

        private GameObject Spawn(PrefabEntry entry)
        {
            if (entry.Prefab == null) return null;

            var pool = _spawnPool.ValueOrDefault;
            var parent = ObjUtil.GetAsFromSource<Transform>(_spawnedObjectParent, true);
            GameObject go;
            switch (_positioning)
            {
                case PositioningMode.AtSpawnedObjectParent:
                    go = parent ? pool.Spawn(entry.Prefab.gameObject, parent.position, parent.rotation, parent) : pool.Spawn(entry.Prefab.gameObject);
                    break;
                case PositioningMode.FromPrefab:
                    go = pool.Spawn(entry.Prefab.gameObject, parent);
                    break;
                default:
                    go = pool.Spawn(entry.Prefab.gameObject, this.transform.position, this.transform.rotation, parent);
                    break;
            }

            var dur = (entry.Duration > 0f) ? entry.Duration : entry.Prefab.main.duration;
            var timeSupplier = (entry.Prefab.main.useUnscaledTime) ? SPTime.Real : SPTime.Normal;
            if (!float.IsNaN(dur) && !float.IsInfinity(dur) && dur >= 0f)
            {
                this.InvokeGuaranteed(() => go.Kill(), dur,
                                                       entry.Prefab.main.useUnscaledTime ? SPTime.Real : SPTime.Normal);
            }

            if (_onSpawnedObject?.HasReceivers ?? false)
            {
                _onSpawnedObject.ActivateTrigger(this, go);
            }

            return go;
        }

        #endregion


        #region ITriggerable Interface

        public override bool CanTrigger
        {
            get { return base.CanTrigger && _prefabs != null && _prefabs.Count > 0; }
        }

        public override bool Trigger(object sender, object arg)
        {
            if (!this.CanTrigger) return false;

            return this.Spawn() != null;
        }

        #endregion

        #region IObserverableTarget Interface

        BaseSPEvent[] IObservableTrigger.GetEvents()
        {
            return new BaseSPEvent[] { _onSpawnedObject };
        }

        #endregion

        #region ISpawnPoint Interface

        BaseSPEvent ISpawnPoint.OnSpawned
        {
            get { return _onSpawnedObject; }
        }

        void ISpawnPoint.Spawn()
        {
            this.Spawn();
        }

        #endregion

        #region Special Types

        [System.Serializable]
        public class OnSpawnEvent : SPDelegate<GameObject>
        {
            public OnSpawnEvent() : base(TRG_ONSPAWNED)
            {

            }
        }

        [System.Serializable]
        public struct PrefabEntry
        {
            public float Weight;
            public ParticleSystem Prefab;
            [TimeUnitsSelector()]
            [Tooltip("Delete particle effect after a duration. Leave 0 to use the 'duration' of the particle effect, or use negative value (-1) to never delete.")]
            public float Duration;
        }

        #endregion

    }

}
