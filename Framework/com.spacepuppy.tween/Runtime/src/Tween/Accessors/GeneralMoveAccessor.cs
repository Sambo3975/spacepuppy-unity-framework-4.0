﻿using UnityEngine;

using com.spacepuppy.Utils;
using com.spacepuppy.Dynamic.Accessors;

namespace com.spacepuppy.Tween.Accessors
{

    [CustomTweenMemberAccessor(typeof(GameObject), typeof(Vector3), "*Move")]
    [CustomTweenMemberAccessor(typeof(Component), typeof(Vector3), "*Move")]
    [CustomTweenMemberAccessor(typeof(IGameObjectSource), typeof(Vector3), "*Move")]
    [CustomTweenMemberAccessor(typeof(Rigidbody), typeof(Vector3), "*Move")]
    public class GeneralMoveAccessor : ITweenMemberAccessorProvider, IMemberAccessor<Vector3>
    {


        #region ImplicitCurve Interface

        string com.spacepuppy.Dynamic.Accessors.IMemberAccessor.GetMemberName() { return "*Move"; }

        public System.Type GetMemberType()
        {
            return typeof(Vector3);
        }

        public IMemberAccessor GetAccessor(object target, string propName, string args)
        {
            return this;
        }

        object IMemberAccessor.Get(object target)
        {
            return this.Get(target);
        }

        public void Set(object targ, object valueObj)
        {
            this.Set(targ, ConvertUtil.ToVector3(valueObj));
        }

        public Vector3 Get(object target)
        {
            var t = GameObjectUtil.GetTransformFromSource(target);
            if (t != null)
            {
                return t.position;
            }
            return Vector3.zero;
        }

        public void Set(object targ, Vector3 value)
        {
            if (targ is Rigidbody)
            {
                var rb = targ as Rigidbody;
                rb.MovePosition(value - rb.position);
            }
            else
            {
                var trans = GameObjectUtil.GetTransformFromSource(targ);
                if (trans == null) return;

                var rb = trans.GetComponent<Rigidbody>();
                if (rb != null && !rb.isKinematic)
                {
                    var dp = value - rb.position;
#if UNITY_2023_3_OR_NEWER
                    rb.linearVelocity = dp / Time.fixedDeltaTime;
#else
                    rb.velocity = dp / Time.fixedDeltaTime;
#endif
                    return;
                }

                //just update the position
                trans.position = value;
            }
        }

        #endregion


    }
}
