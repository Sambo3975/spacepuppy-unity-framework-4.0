﻿using UnityEngine;
using System.Collections.Generic;

using com.spacepuppy.Geom;
using com.spacepuppy.Hooks;
using com.spacepuppy.Utils;

namespace com.spacepuppy.Motor
{

    /// <summary>
    /// Treats a CharacterController as an IMotor for a more uniform interface.
    /// </summary>
    [RequireComponentInEntity(typeof(CharacterController))]
    public class CharacterMotor : SPComponent, IMotor, IUpdateable, IMotorCollisionMessageDispatcher, IOnControllerColliderHitSubscriber
    {

        #region Fields

        [SerializeField]
        [DefaultFromSelf(Relativity = EntityRelativity.Entity)]
        private CharacterController _controller;

        [SerializeField]
        private float _mass = 1f;
        [SerializeField]
        [Tooltip("Automatically apply gravity when calling 'Move'.")]
        private bool _useGravity;
        [SerializeField]
        private bool _paused;

        [System.NonSerialized()]
        private Vector3 _vel;
        [System.NonSerialized()]
        private Vector3 _talliedVel;

        [System.NonSerialized()]
        private Vector3 _lastPos;
        [System.NonSerialized()]
        private Vector3 _lastVel;

        [System.NonSerialized]
        private Messaging.MessageToken<IMotorCollisionMessageHandler> _onCollisionMessage;
        [System.NonSerialized]
        private Messaging.ISubscribableMessageHook<IOnControllerColliderHitSubscriber> _collisionHook;

        #endregion

        #region CONSTRUCTOR

        protected override void OnEnable()
        {
            if (object.ReferenceEquals(_controller, null)) throw new System.InvalidOperationException("CharacterMotor must be initialized with an appropriate CharacterController.");

            base.OnEnable();

            _onCollisionMessage = Messaging.CreateBroadcastToken<IMotorCollisionMessageHandler>(this.gameObject);
            this.ValidateCollisionHandler();

            _lastPos = !object.ReferenceEquals(_controller, null) ? _controller.transform.position : Vector3.zero;
            _lastVel = Vector3.zero;
            _vel = Vector3.zero;
            _talliedVel = Vector3.zero;
            GameLoop.EarlyUpdatePump.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            GameLoop.EarlyUpdatePump.Remove(this);
        }

        #endregion

        #region Properties

        public CharacterController Controller
        {
            get { return _controller; }
            set { _controller = value; }
        }

        public bool UseGravity
        {
            get => _useGravity;
            set => _useGravity = value;
        }

        #endregion

        #region Methods

        public com.spacepuppy.Geom.Capsule GetGeom(bool ignoreSkin)
        {
            var cap = com.spacepuppy.Geom.Capsule.FromCollider(_controller);
            if (!ignoreSkin) cap.Radius += this.SkinWidth;
            return cap;
        }

        #endregion

        #region IMotor Interface

        public bool PrefersFixedUpdate
        {
            get { return false; }
        }

        public float Mass
        {
            get { return _mass; }
            set { _mass = Mathf.Max(0f, value); }
        }

        public float StepOffset
        {
            get
            {
                return !object.ReferenceEquals(_controller, null) ? _controller.stepOffset : 0f;
            }
            set
            {
                if (!object.ReferenceEquals(_controller, null)) _controller.stepOffset = value;
            }
        }

        public float SkinWidth
        {
            get
            {
                return !object.ReferenceEquals(_controller, null) ? _controller.skinWidth : 0f;
            }
            set
            {
                if (!object.ReferenceEquals(_controller, null)) _controller.skinWidth = value;
            }
        }

        public bool CollisionEnabled
        {
            get
            {
                return !object.ReferenceEquals(_controller, null) ? _controller.detectCollisions : false;
            }
            set
            {
                if (!object.ReferenceEquals(_controller, null)) _controller.detectCollisions = value;
            }
        }

        public bool Paused { get { return _paused; } set { _paused = value; } }

        public Vector3 Velocity
        {
            get { return _vel; }
            set
            {
                _vel = value;
                _talliedVel = _vel;
            }
        }

        public Vector3 Position
        {
            get
            {
                return !object.ReferenceEquals(_controller, null) ? _controller.transform.position : Vector3.zero;
            }
            set
            {
                if (object.ReferenceEquals(_controller, null)) throw new System.InvalidOperationException("CharacterMotor must be initialized with an appropriate CharacterController.");

                _controller.transform.position = value;
            }
        }

        public Vector3 LastPosition
        {
            get { return _lastPos; }
        }

        public Vector3 LastVelocity
        {
            get { return _lastVel; }
        }

        public void Move(Vector3 mv)
        {
            if (object.ReferenceEquals(_controller, null)) throw new System.InvalidOperationException("CharacterMotor must be initialized with an appropriate CharacterController.");
            if (_paused) return;

            if (_useGravity) mv += Physics.gravity * Time.deltaTime;

            if (_controller.detectCollisions)
            {
                _controller.Move(mv);
                //update velocity
                _talliedVel += _controller.velocity;
                _vel = _talliedVel;
            }
            else
            {
                _controller.transform.position += mv;
                //update velocity
                _talliedVel += mv / Time.deltaTime;
                _vel = _talliedVel;
            }
        }

        public void AtypicalMove(Vector3 mv)
        {
            if (object.ReferenceEquals(_controller, null)) throw new System.InvalidOperationException("CharacterMotor must be initialized with an appropriate CharacterController.");
            if (_paused) return;

            if (_controller.detectCollisions)
            {
                _controller.Move(mv);
            }
            else
            {
                _controller.transform.position += mv;
            }
        }

        public void MovePosition(Vector3 pos, bool setVelocityByChangeInPosition = false)
        {
            if (object.ReferenceEquals(_controller, null)) throw new System.InvalidOperationException("CharacterMotor must be initialized with an appropriate CharacterController.");
            if (_paused) return;

            if (_controller.detectCollisions)
            {
                _controller.Move(pos - _controller.transform.position);
                if (setVelocityByChangeInPosition)
                {
                    //update velocity
                    _talliedVel += _controller.velocity;
                    _vel = _talliedVel;
                }
            }
            else
            {
                if (setVelocityByChangeInPosition)
                {
                    var mv = pos - _controller.transform.position;
                    //update velocity
                    _talliedVel += mv / Time.deltaTime;
                    _vel = _talliedVel;

                }
                _controller.transform.position = pos;
            }
        }

        #endregion

        #region IForceReceiver Interface

        public void AddForce(Vector3 f, ForceMode mode)
        {
            if (object.ReferenceEquals(_controller, null)) throw new System.InvalidOperationException("CharacterMotor must be initialized with an appropriate CharacterController.");
            if (_paused) return;

            switch (mode)
            {
                case ForceMode.Force:
                    //force = mass*distance/time^2
                    //distance = force * time^2 / mass
                    this.Move(f * Time.deltaTime * Time.deltaTime / _mass);
                    break;
                case ForceMode.Acceleration:
                    //acceleration = distance/time^2
                    //distance = acceleration * time^2
                    this.Move(f * (Time.deltaTime * Time.deltaTime));
                    break;
                case ForceMode.Impulse:
                    //impulse = mass*distance/time
                    //distance = impulse * time / mass
                    this.Move(f * Time.deltaTime / _mass);
                    break;
                case ForceMode.VelocityChange:
                    //velocity = distance/time
                    //distance = velocity * time
                    this.Move(f * Time.deltaTime);
                    break;
            }
        }

        public void AddForceAtPosition(Vector3 f, Vector3 pos, ForceMode mode)
        {
            this.AddForce(f, mode);
        }

        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Force)
        {
            if (_paused) return;

            var geom = this.GetGeom(true);
            var v = geom.Center - explosionPosition;
            var force = v.normalized * Mathf.Clamp01(v.magnitude / explosionRadius) * explosionForce;
            //TODO - apply upwards modifier

            this.AddForce(force, mode);
        }

        #endregion

        #region IPhysicsObject Interface

        public bool TestOverlap(int layerMask, QueryTriggerInteraction query)
        {
            return Capsule.FromCollider(_controller).TestOverlap(layerMask, query);
        }

        public int OverlapNonAlloc(Collider[] buffer, int layerMask, QueryTriggerInteraction query)
        {
            return Capsule.FromCollider(_controller).OverlapNonAlloc(buffer, layerMask, query);
        }

        public int Overlap(ICollection<Collider> results, int layerMask, QueryTriggerInteraction query)
        {
            return Capsule.FromCollider(_controller).Overlap(results, layerMask, query);
        }

        public bool Cast(Vector3 direction, out RaycastHit hitinfo, float distance, int layerMask, QueryTriggerInteraction query)
        {
            return Capsule.FromCollider(_controller).Cast(direction, out hitinfo, distance, layerMask, query);
        }

        public int CastAll(Vector3 direction, ICollection<RaycastHit> results, float distance, int layerMask, QueryTriggerInteraction query)
        {
            return Capsule.FromCollider(_controller).CastAll(direction, results, distance, layerMask, query);
        }

        bool IPhysicsObject.ContainsPoint(Vector3 point) => _controller.ContainsPoint(point);

        #endregion

        #region IUpdatable Interface

        void IUpdateable.Update()
        {
            _lastPos = _controller.transform.position;
            _lastVel = _vel;
            _talliedVel = Vector3.zero;
        }

        #endregion

        #region CollisionHandler Implementation

        private void ValidateCollisionHandler()
        {
            if (_collisionHook == null && _onCollisionMessage?.Count > 0)
            {
                this.gameObject.Subscribe<IOnControllerColliderHitSubscriber>(this, out _collisionHook);
            }
        }

        void IOnControllerColliderHitSubscriber.OnControllerColliderHit(GameObject sender, ControllerColliderHit hit)
        {
            if (_onCollisionMessage.Count > 0)
            {
                if (this.gameObject == sender)
                    _onCollisionMessage.Invoke(new MotorCollisionInfo(this, hit), (o,a) => o.OnCollision(a));
            }
            else if (_collisionHook != null)
            {
                _collisionHook.Unsubscribe(this);
                _collisionHook = null;
            }
        }

        void IMotorCollisionMessageDispatcher.SetDirty(IMotorCollisionMessageHandler handler)
        {
            _onCollisionMessage?.SetDirty();
            if (handler.enabled)
            {
                this.ValidateCollisionHandler();
            }
        }

        #endregion

    }

}
