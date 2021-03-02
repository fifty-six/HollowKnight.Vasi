using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace Vasi
{
    /// <summary>
    ///     A class to aid in reflection while caching it.
    /// </summary>
    [PublicAPI]
    public static partial class Mirror
    {
        public delegate ref TField FuncByRef<in TClass, TField>(TClass instance);

        public delegate ref TField FuncByRef<TField>();

        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> Fields = new();

        private static readonly Dictionary<FieldInfo, Delegate> Getters = new();

        private static readonly Dictionary<FieldInfo, Delegate> RefGetters = new();

        private static readonly Dictionary<FieldInfo, Delegate> Setters = new();

        /// <summary>
        ///     Gets a field on a type
        /// </summary>
        /// <param name="t">Type</param>
        /// <param name="field">Field name</param>
        /// <param name="instance"></param>
        /// <returns>FieldInfo for field or null if field does not exist.</returns>
        public static FieldInfo GetFieldInfo(Type t, string field, bool instance = true)
        {
            if (!Fields.TryGetValue(t, out Dictionary<string, FieldInfo> typeFields))
            {
                Fields[t] = typeFields = new Dictionary<string, FieldInfo>();
            }

            if (typeFields.TryGetValue(field, out FieldInfo fi))
            {
                return fi;
            }

            fi = t.GetField
            (
                field,
                BindingFlags.NonPublic
                | BindingFlags.Public
                | (
                    instance
                        ? BindingFlags.Instance
                        : BindingFlags.Static
                )
            );

            typeFields[field] = fi ?? throw new MissingFieldException($"Field {field} does not exist on type {t.Name}");

            return fi;
        }

        /// <summary>
        ///     Gets delegate getting field on type
        /// </summary>
        /// <param name="fi">FieldInfo for field.</param>
        /// <returns>Function which gets value of field</returns>
        public static Delegate GetGetter<TType, TField>(FieldInfo fi)
        {
            if (Getters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral)
            {
                throw new ArgumentException("Field cannot be const", nameof(fi));
            }

            d = fi.IsStatic
                ? CreateGetStaticFieldDelegate<TField>(fi)
                : CreateGetFieldDelegate<TType, TField>(fi);

            return Getters[fi] = d;
        }

        /// <summary>
        ///     Gets delegate setting field on type
        /// </summary>
        /// <param name="fi">FieldInfo for field.</param>
        /// <returns>Function which sets field passed as FieldInfo</returns>
        public static Delegate GetSetter<TType, TField>(FieldInfo fi)
        {
            if (Setters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral || fi.IsInitOnly)
            {
                throw new ArgumentException("Field cannot be readonly or const", nameof(fi));
            }

            d = fi.IsStatic
                ? CreateSetStaticFieldDelegate<TField>(fi)
                : CreateSetFieldDelegate<TType, TField>(fi);

            return Setters[fi] = d;
        }

        public static Delegate GetRefGetter<TType, TField>(FieldInfo fi)
        {
            if (RefGetters.TryGetValue(fi, out Delegate d))
            {
                return d;
            }

            if (fi.IsLiteral || fi.IsInitOnly)
            {
                throw new ArgumentException("Field cannot be readonly or const", nameof(fi));
            }

            d = fi.IsStatic
                ? CreateGetStaticFieldRefDelegate<TField>(fi)
                : CreateGetFieldRefDelegate<TType, TField>(fi);

            return RefGetters[fi] = d;
        }

        public static ref TField GetFieldRef<TObject, TField>(TObject obj, string name)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name);

            return ref GetFieldRef<TObject, TField>(obj, fi);
        }

        public static ref TField GetFieldRef<TObject, TField>(TObject obj, FieldInfo fi)
        {
            if (fi.IsStatic) throw new InvalidOperationException("Field is static!");

            return ref ((FuncByRef<TObject, TField>) GetRefGetter<TObject, TField>(fi))(obj);
        }

        public static ref TField GetFieldRef<TObject, TField>(string name)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name);

            return ref GetFieldRef<TObject, TField>(fi);
        }

        public static ref TField GetFieldRef<TObject, TField>(FieldInfo fi)
        {
            if (!fi.IsStatic) throw new InvalidOperationException("Field is not static!");

            return ref ((FuncByRef<TField>) GetRefGetter<TObject, TField>(fi))();
        }

        /// <summary>
        ///     Get a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        public static TField GetField<TObject, TField>(TObject obj, string name)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name);

            return GetField<TObject, TField>(obj, fi);
        }

        public static TField GetField<TObject, TField>(TObject obj, FieldInfo fi)
        {
            if (fi.IsStatic) throw new InvalidOperationException("Field is static!");

            return ((Func<TObject, TField>) GetGetter<TObject, TField>(fi))(obj);
        }

        /// <summary>
        ///     Get a static field on an type using a string.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <typeparam name="TType">Type which static field resides upon</typeparam>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <returns>The value of a field on an object/type</returns>
        public static TField GetField<TType, TField>(string name)
        {
            FieldInfo fi = GetFieldInfo(typeof(TType), name, false);

            return GetField<TType, TField>(fi);
        }

        public static TField GetField<TType, TField>(FieldInfo fi)
        {
            if (!fi.IsStatic) throw new InvalidOperationException("Field is not static!");

            return ((Func<TField>) GetGetter<TType, TField>(fi))();
        }

        /// <summary>
        ///     Set a field on an object using a string.
        /// </summary>
        /// <param name="obj">Object/Object of type which the field is on</param>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TField">Type of field</typeparam>
        /// <typeparam name="TObject">Type of object being passed in</typeparam>
        public static void SetField<TObject, TField>(TObject obj, string name, TField value)
        {
            FieldInfo fi = GetFieldInfo(typeof(TObject), name);

            SetField(obj, fi, value);
        }

        public static void SetField<TObject, TField>(TObject obj, FieldInfo fi, TField value)
        {
            if (fi.IsStatic) throw new InvalidOperationException("Field is static!");

            ((Action<TObject, TField>) GetSetter<TObject, TField>(fi))(obj, value);
        }

        /// <summary>
        ///     Set a static field on an type using a string.
        /// </summary>
        /// <param name="name">Name of the field</param>
        /// <param name="value">Value to set the field to</param>
        /// <typeparam name="TType">Type which static field resides upon</typeparam>
        /// <typeparam name="TField">Type of field</typeparam>
        public static void SetField<TType, TField>(string name, TField value)
        {
            FieldInfo fi = GetFieldInfo(typeof(TType), name, false);

            SetField<TType, TField>(fi, value);
        }

        public static void SetField<TType, TField>(FieldInfo fi, TField value)
        {
            if (!fi.IsStatic) throw new InvalidOperationException("Field is not static!");

            ((Action<TField>) GetSetter<TType, TField>(fi))(value);
        }
    }
}