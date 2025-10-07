﻿using com.spacepuppy.Utils;
using System;
using UnityEngine;

namespace com.spacepuppy
{

    /// <summary>
    /// General interface for custom numeric types like fixedpoint, discrete float, etc. 
    /// Numerics should usually be a struct type, but is not required.
    /// </summary>
    public interface INumeric : IConvertible
    {

        /// <summary>
        /// Returns the byte reprsentation of the numeric value.
        /// When implementing this methods, if you need to convert C# built-in numeric types make sure they're in big-endian. 
        /// Use the 'Numerics' static class as a helper tool to do this.
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();
        /// <summary>
        /// Sets the numeric value based on some byte array.
        /// When implementing this methods, if you need to convert C# built-in numeric types make sure they're in big-endian. 
        /// Use the 'Numerics' static class as a helper tool to do this.
        /// </summary>
        /// <param name="arr"></param>
        void FromByteArray(byte[] arr);

        /// <summary>
        /// Get the type code the underlying data can losslessly be converted to for easy storing.
        /// </summary>
        /// <returns></returns>
        TypeCode GetUnderlyingTypeCode();

        /// <summary>
        /// Set value based on a long.
        /// </summary>
        /// <param name="value"></param>
        void FromLong(long value);

        /// <summary>
        /// Set value based on a double.
        /// </summary>
        /// <param name="value"></param>
        void FromDouble(double value);

    }

    public static class Numerics
    {

        public static INumeric CreateNumeric<T>(byte[] data) where T : INumeric
        {
            var value = System.Activator.CreateInstance<T>();
            if (value != null) value.FromByteArray(data);
            return value;
        }

        public static INumeric CreateNumeric<T>(long data) where T : INumeric
        {
            var value = System.Activator.CreateInstance<T>();
            if (value != null) value.FromLong(data);
            return value;
        }

        public static INumeric CreateNumeric<T>(double data) where T : INumeric
        {
            var value = System.Activator.CreateInstance<T>();
            if (value != null) value.FromDouble(data);
            return value;
        }

        public static INumeric CreateNumeric(System.Type tp, byte[] data)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(INumeric).IsAssignableFrom(tp) && !tp.IsAbstract) throw new System.ArgumentException("Type must implement INumeric.");

            var value = System.Activator.CreateInstance(tp) as INumeric;
            if (value != null) value.FromByteArray(data);
            return value;
        }

        public static INumeric CreateNumeric(System.Type tp, long data)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(INumeric).IsAssignableFrom(tp) && !tp.IsAbstract) throw new System.ArgumentException("Type must implement INumeric.");

            var value = System.Activator.CreateInstance(tp) as INumeric;
            if (value != null) value.FromLong(data);
            return value;
        }

        public static INumeric CreateNumeric(System.Type tp, double data)
        {
            if (tp == null) throw new System.ArgumentNullException("tp");
            if (!typeof(INumeric).IsAssignableFrom(tp) && !tp.IsAbstract) throw new System.ArgumentException("Type must implement INumeric.");

            var value = System.Activator.CreateInstance(tp) as INumeric;
            if (value != null) value.FromDouble(data);
            return value;
        }


        #region Bit Helpers

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(float value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(double value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int16 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int32 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(Int64 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt16 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt32 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Returns a byte representation in big-endian order.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] GetBytes(UInt64 value)
        {
            var result = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian) System.Array.Reverse(result);
            return result;
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static float ToSingle(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToSingle(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static double ToDouble(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToDouble(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Int16 ToInt16(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToInt16(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Int32 ToInt32(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToInt32(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static Int64 ToInt64(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToInt64(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static UInt16 ToUInt16(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToUInt16(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static UInt32 ToUInt32(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToUInt32(arr, 0);
        }

        /// <summary>
        /// Converts a byte array in big-endian form to a numeric value.
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static UInt64 ToUInt64(byte[] arr)
        {
            if (BitConverter.IsLittleEndian)
                System.Array.Reverse(arr);
            return BitConverter.ToUInt64(arr, 0);
        }

        #endregion

    }

    /// <summary>
    /// Stores a whole number as a floating point value. You get the range of a float, as well as infinity representations. 
    /// Implicit conversion between float and int is defined.
    /// </summary>
    [System.Serializable]
    public struct DiscreteFloat : INumeric, IConvertible, IComparable, IComparable<float>, IComparable<DiscreteFloat>, IEquatable<float>, IEquatable<DiscreteFloat>, IFormattable
    {

        [SerializeField()]
        private float _value;

        #region CONSTRUCTOR

        public DiscreteFloat(float f)
        {
            _value = Mathf.Round(f);
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        /// <summary>
        /// Returns the value as a positive value int where infinite is represented as -1.
        /// </summary>
        /// <returns></returns>
        public int ToStandardMetricInt(int valueForInfinity = -1)
        {
            if (float.IsInfinity(_value)) return valueForInfinity;
            else if (float.IsNaN(_value)) return 0;
            else return (int)Mathf.Abs(_value);
        }

        public override bool Equals(object obj)
        {
            if (obj is DiscreteFloat)
            {
                return _value == ((DiscreteFloat)obj)._value;
            }
            else if (obj is System.IConvertible)
            {
                try
                {
                    var f = System.Convert.ToSingle(obj);
                    return _value == f;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        #endregion

        #region Static/Constants

        public static DiscreteFloat Zero
        {
            get
            {
                return new DiscreteFloat()
                {
                    _value = 0f
                };
            }
        }

        public static DiscreteFloat PositiveInfinity { get { return new DiscreteFloat() { _value = float.PositiveInfinity }; } }
        public static DiscreteFloat NegativeInfinity { get { return new DiscreteFloat() { _value = float.NegativeInfinity }; } }

        public static bool IsNaN(DiscreteFloat value)
        {
            return float.IsNaN(value._value);
        }

        public static bool IsInfinity(DiscreteFloat value)
        {
            return float.IsInfinity(value._value);
        }

        public static bool IsPositiveInfinity(DiscreteFloat value)
        {
            return float.IsPositiveInfinity(value._value);
        }

        public static bool IsNegativeInfinity(DiscreteFloat value)
        {
            return float.IsNegativeInfinity(value._value);
        }

        public static bool IsReal(DiscreteFloat value)
        {
            return !(float.IsNaN(value._value) || float.IsInfinity(value._value));
        }



        #endregion

        #region Operators

        public static DiscreteFloat operator ++(DiscreteFloat df)
        {
            df._value++;
            return df;
        }

        public static DiscreteFloat operator +(DiscreteFloat df)
        {
            return df;
        }

        public static DiscreteFloat operator +(DiscreteFloat a, DiscreteFloat b)
        {
            a._value = Mathf.Floor(a._value + b._value);
            return a;
        }

        public static DiscreteFloat operator --(DiscreteFloat df)
        {
            df._value--;
            return df;
        }

        public static DiscreteFloat operator -(DiscreteFloat df)
        {
            df._value = -df._value;
            return df;
        }

        public static DiscreteFloat operator -(DiscreteFloat a, DiscreteFloat b)
        {
            a._value = Mathf.Floor(a._value - b._value);
            return a;
        }

        public static DiscreteFloat operator *(DiscreteFloat a, DiscreteFloat b)
        {
            a._value = Mathf.Floor(a._value * b._value);
            return a;
        }

        public static DiscreteFloat operator /(DiscreteFloat a, DiscreteFloat b)
        {
            a._value = Mathf.Floor(a._value / b._value);
            return a;
        }

        public static bool operator >(DiscreteFloat a, DiscreteFloat b)
        {
            return a._value > b._value;
        }

        public static bool operator >=(DiscreteFloat a, DiscreteFloat b)
        {
            return a._value >= b._value;
        }

        public static bool operator <(DiscreteFloat a, DiscreteFloat b)
        {
            return a._value < b._value;
        }

        public static bool operator <=(DiscreteFloat a, DiscreteFloat b)
        {
            return a._value <= b._value;
        }

        public static bool operator ==(DiscreteFloat a, DiscreteFloat b)
        {
            return a._value == b._value;
        }

        public static bool operator !=(DiscreteFloat a, DiscreteFloat b)
        {
            return a._value != b._value;
        }

        #endregion

        #region Conversions

        public static implicit operator DiscreteFloat(int f)
        {
            return new DiscreteFloat((float)f);
        }

        public static explicit operator int(DiscreteFloat df)
        {
            return (int)df._value;
        }

        public static implicit operator DiscreteFloat(float f)
        {
            return new DiscreteFloat(f);
        }

        public static implicit operator float(DiscreteFloat df)
        {
            return df._value;
        }

        public static explicit operator DiscreteFloat(double d)
        {
            return new DiscreteFloat((float)d);
        }

        public static implicit operator double(DiscreteFloat df)
        {
            return (double)df._value;
        }

        public static explicit operator DiscreteFloat(decimal d)
        {
            return new DiscreteFloat((float)d);
        }

        public static explicit operator decimal(DiscreteFloat df)
        {
            return (decimal)df._value;
        }

        #endregion

        #region INumeric Interface

        TypeCode INumeric.GetUnderlyingTypeCode()
        {
            return TypeCode.Single;
        }

        public byte[] ToByteArray()
        {
            return Numerics.GetBytes(_value);
        }

        void INumeric.FromByteArray(byte[] data)
        {
            _value = Mathf.Round(Numerics.ToSingle(data));
        }

        void INumeric.FromLong(long value)
        {
            _value = Convert.ToSingle(value);
        }

        void INumeric.FromDouble(double value)
        {
            _value = (float)Math.Round(value);
        }

        public static DiscreteFloat FromByteArray(byte[] data)
        {
            var result = new DiscreteFloat();
            result._value = Mathf.Round(Numerics.ToSingle(data));
            return result;
        }

        #endregion

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Single;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return _value != 0f;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(_value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(_value);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(_value);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_value);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(_value);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_value);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return _value;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(_value);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(_value);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return _value.ToString(provider);
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return (_value as IConvertible).ToType(conversionType, provider);
        }

        #endregion

        #region IComparable Interface

        public int CompareTo(object obj)
        {
            return _value.CompareTo(obj);
        }

        public int CompareTo(float other)
        {
            return _value.CompareTo(other);
        }

        public int CompareTo(DiscreteFloat other)
        {
            return _value.CompareTo(other._value);
        }

        #endregion

        #region IEquatable Interface

        public bool Equals(float other)
        {
            return _value.Equals(other);
        }

        public bool Equals(DiscreteFloat other)
        {
            return _value.Equals(other._value);
        }

        #endregion

        #region IFormattable Interface

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return _value.ToString(format, formatProvider);
        }

        #endregion

        #region Special Types

        public class HideInfinityCheckbox : System.Attribute { }

        public abstract class ConfigAttribute : System.Attribute
        {

            public abstract float Normalize(float value);

        }

        public class NonNegative : ConfigAttribute
        {

            public override float Normalize(float value)
            {
                if (value < 0f) return 0f;
                else return value;
            }

        }

        public class Positive : ConfigAttribute
        {
            public override float Normalize(float value)
            {
                if (value <= 0f) return 1f;
                else return value;
            }
        }

        #endregion

    }

    /// <summary>
    /// Very similar to float?, but is serializable. Unfortunately it doesn't behave 100% like float? and should be cast to float? as early as possible 
    /// since C# relies on a lot of syntax sugar to get things like coallescing and boxing correct.
    /// </summary>
    [System.Serializable]
    public struct NullableFloat : INumeric, IConvertible
    {

        #region Fields

        [SerializeField]
        private float _value;
        [SerializeField, HideInInspector]
        private bool _hasValue;

        #endregion

        #region CONSTRUCTOR

        public NullableFloat(float value)
        {
            this._value = value;
            this._hasValue = true;
        }

        #endregion

        #region Properties

        public float Value
        {
            get
            {
                if (!_hasValue) throw new System.InvalidOperationException("NullableFloat has no value.");
                return _value;
            }
        }

        public bool HasValue => _hasValue;

        #endregion

        #region Methods

        public float GetValueOrDefault()
        {
            return _value;
        }

        public float GetValueOrDefault(float defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        public override bool Equals(object other)
        {
            if (!_hasValue) return other == null;
            if (other == null) return false;
            return _value.Equals(other);
        }

        public override int GetHashCode()
        {
            return _hasValue ? _value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return _hasValue ? _value.ToString() : string.Empty;
        }

        public Nullable<float> ToNullable()
        {
            return _hasValue ? new Nullable<float>(_value) : null;
        }

        #endregion

        #region Conversions

        public static implicit operator NullableFloat(float value)
        {
            return new NullableFloat(value);
        }

        public static explicit operator float(NullableFloat value)
        {
            return value.Value;
        }

        public static implicit operator Nullable<float>(NullableFloat value)
        {
            return value._hasValue ? new Nullable<float>(value._value) : null;
        }

        public static implicit operator NullableFloat(Nullable<float> value)
        {
            return value.HasValue ? new NullableFloat(value.Value) : default;
        }

        #endregion

        #region INumeric Interface

        TypeCode INumeric.GetUnderlyingTypeCode()
        {
            return TypeCode.Single;
        }

        public byte[] ToByteArray()
        {
            return _hasValue ? Numerics.GetBytes(_value) : ArrayUtil.Empty<byte>();
        }

        void INumeric.FromByteArray(byte[] data)
        {
            if (data.Length > 0)
            {
                _value = Numerics.ToSingle(data);
                _hasValue = true;
            }
            else
            {
                _value = default;
                _hasValue = false;
            }
        }

        void INumeric.FromLong(long value)
        {
            _value = Convert.ToSingle(value);
            _hasValue = true;
        }

        void INumeric.FromDouble(double value)
        {
            _value = (float)value;
            _hasValue = true;
        }

        public static NullableFloat FromByteArray(byte[] data)
        {
            if (data.Length > 0)
            {
                var result = new NullableFloat();
                result._value = Numerics.ToSingle(data);
                result._hasValue = true;
                return result;
            }
            else
            {
                return default;
            }
        }

        #endregion

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Single;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return _value != 0f;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(_value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(_value);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(_value);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_value);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return Convert.ToInt32(_value);
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_value);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return _value;
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(_value);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(_value);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return _hasValue ? _value.ToString(provider) : string.Empty;
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return (_value as IConvertible).ToType(conversionType, provider);
        }

        #endregion

        #region Equality Interface

        public static bool operator ==(NullableFloat a, NullableFloat b)
        {
            return a._hasValue == b._hasValue && a._value == b._value;
        }
        public static bool operator !=(NullableFloat a, NullableFloat b)
        {
            return a._hasValue != b._hasValue || a._value != b._value;
        }

        public static bool operator ==(NullableFloat a, float b)
        {
            return a._hasValue && a._value == b;
        }
        public static bool operator !=(NullableFloat a, float b)
        {
            return !a._hasValue || a._value != b;
        }

        public static bool operator ==(float a, NullableFloat b)
        {
            return b._hasValue && a == b._value;
        }
        public static bool operator !=(float a, NullableFloat b)
        {
            return !b._hasValue || a != b._value;
        }

        #endregion

        #region Special Types

        public class LabelAttribute : System.Attribute
        {
            public string ShortLabel;
            public string LongLabel;
        }

        public abstract class ConfigAttribute : System.Attribute
        {

            public abstract float Normalize(float value);

        }

        public class NonNegative : ConfigAttribute
        {

            public override float Normalize(float value)
            {
                if (value < 0f) return 0f;
                else return value;
            }

        }

        public class Positive : ConfigAttribute
        {
            public override float Normalize(float value)
            {
                if (value <= 0f) return 1f;
                else return value;
            }
        }

        #endregion

    }

    /// <summary>
    /// Very similar to int?, but is serializable. Unfortunately it doesn't behave 100% like float? and should be cast to float? as early as possible 
    /// since C# relies on a lot of syntax sugar to get things like coallescing and boxing correct.
    /// </summary>
    [System.Serializable]
    public struct NullableInt : INumeric, IConvertible
    {

        #region Fields

        [SerializeField]
        private int _value;
        [SerializeField, HideInInspector]
        private bool _hasValue;

        #endregion

        #region CONSTRUCTOR

        public NullableInt(int value)
        {
            this._value = value;
            this._hasValue = true;
        }

        #endregion

        #region Properties

        public int Value
        {
            get
            {
                if (!_hasValue) throw new System.InvalidOperationException("NullableInt has no value.");
                return _value;
            }
        }

        public bool HasValue => _hasValue;

        #endregion

        #region Methods

        public int GetValueOrDefault()
        {
            return _value;
        }

        public int GetValueOrDefault(int defaultValue)
        {
            return _hasValue ? _value : defaultValue;
        }

        public override bool Equals(object other)
        {
            if (!_hasValue) return other == null;
            if (other == null) return false;
            return _value.Equals(other);
        }

        public override int GetHashCode()
        {
            return _hasValue ? _value.GetHashCode() : 0;
        }

        public override string ToString()
        {
            return _hasValue ? _value.ToString() : string.Empty;
        }

        public Nullable<float> ToNullable()
        {
            return _hasValue ? new Nullable<float>(_value) : null;
        }

        #endregion

        #region Conversions

        public static implicit operator NullableInt(int value)
        {
            return new NullableInt(value);
        }

        public static explicit operator int(NullableInt value)
        {
            return value.Value;
        }

        public static implicit operator Nullable<int>(NullableInt value)
        {
            return value._hasValue ? new Nullable<int>(value._value) : null;
        }

        public static implicit operator NullableInt(Nullable<int> value)
        {
            return value.HasValue ? new NullableInt(value.Value) : default;
        }

        #endregion

        #region INumeric Interface

        TypeCode INumeric.GetUnderlyingTypeCode()
        {
            return TypeCode.Int32;
        }

        public byte[] ToByteArray()
        {
            return _hasValue ? Numerics.GetBytes(_value) : ArrayUtil.Empty<byte>();
        }

        void INumeric.FromByteArray(byte[] data)
        {
            if (data.Length > 0)
            {
                _value = Numerics.ToInt32(data);
                _hasValue = true;
            }
            else
            {
                _value = default;
                _hasValue = false;
            }
        }

        void INumeric.FromLong(long value)
        {
            _value = Convert.ToInt32(value);
            _hasValue = true;
        }

        void INumeric.FromDouble(double value)
        {
            _value = (int)Math.Round(value);
            _hasValue = true;
        }

        public static NullableInt FromByteArray(byte[] data)
        {
            if (data.Length > 0)
            {
                var result = new NullableInt();
                result._value = Numerics.ToInt32(data);
                result._hasValue = true;
                return result;
            }
            else
            {
                return default;
            }
        }

        #endregion

        #region IConvertible

        public TypeCode GetTypeCode()
        {
            return TypeCode.Int32;
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            return _value != 0;
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            return Convert.ToChar(_value);
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            return Convert.ToSByte(_value);
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            return Convert.ToByte(_value);
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            return Convert.ToInt16(_value);
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            return Convert.ToUInt16(_value);
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            return _value;
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            return Convert.ToUInt32(_value);
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            return Convert.ToInt64(_value);
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            return Convert.ToUInt64(_value);
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            return Convert.ToSingle(_value);
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            return Convert.ToDouble(_value);
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            return Convert.ToDecimal(_value);
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            return Convert.ToDateTime(_value);
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            return _hasValue ? _value.ToString(provider) : string.Empty;
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            return (_value as IConvertible).ToType(conversionType, provider);
        }

        #endregion

        #region Equality Interface

        public static bool operator ==(NullableInt a, NullableInt b)
        {
            return a._hasValue == b._hasValue && a._value == b._value;
        }
        public static bool operator !=(NullableInt a, NullableInt b)
        {
            return a._hasValue != b._hasValue || a._value != b._value;
        }

        public static bool operator ==(NullableInt a, float b)
        {
            return a._hasValue && a._value == b;
        }
        public static bool operator !=(NullableInt a, float b)
        {
            return !a._hasValue || a._value != b;
        }

        public static bool operator ==(float a, NullableInt b)
        {
            return b._hasValue && a == b._value;
        }
        public static bool operator !=(float a, NullableInt b)
        {
            return !b._hasValue || a != b._value;
        }

        #endregion

        #region Special Types

        public class LabelAttribute : System.Attribute
        {
            public string ShortLabel;
            public string LongLabel;
        }

        public abstract class ConfigAttribute : System.Attribute
        {

            public abstract int Normalize(int value);

        }

        public class NonNegative : ConfigAttribute
        {

            public override int Normalize(int value)
            {
                if (value < 0f) return 0;
                else return value;
            }

        }

        public class Positive : ConfigAttribute
        {
            public override int Normalize(int value)
            {
                if (value <= 0f) return 1;
                else return value;
            }
        }

        #endregion

    }
}
