using System;
using System.Reflection;
using System.Reflection.Emit;
using MonoMod.Utils;

namespace Vasi
{
    public static partial class Mirror
    {
        private static Delegate CreateGetStaticFieldDelegate<TField>(FieldInfo fi)
        {
            var dm = new DynamicMethodDefinition
            (
                "__FieldAccess" + fi.DeclaringType?.Name + fi.Name,
                typeof(TField),
                Type.EmptyTypes
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldsfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.Generate().CreateDelegate(typeof(Func<TField>));
        }

        private static Delegate CreateGetFieldDelegate<TType, TField>(FieldInfo fi)
        {
            var dm = new DynamicMethodDefinition
            (
                "__FieldAccess" + fi.DeclaringType?.Name + fi.Name,
                typeof(TField),
                new[] { typeof(TType) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.Generate().CreateDelegate(typeof(Func<TType, TField>));
        }

        private static Delegate CreateGetFieldRefDelegate<TClass, TField>(FieldInfo info)
        {
            var dm = new DynamicMethodDefinition
            (
                "__ReturnByRef" + info.DeclaringType?.Name + info.Name,
                typeof(TField).MakeByRefType(),
                new[] { typeof(TClass) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldflda, info);
            gen.Emit(OpCodes.Ret);

            return dm.Generate().CreateDelegate(typeof(FuncByRef<TClass, TField>));
        }

        private static Delegate CreateGetStaticFieldRefDelegate<TField>(FieldInfo fi)
        {
            var dm = new DynamicMethodDefinition
            (
                "__ReturnByRef" + fi.DeclaringType?.Name + fi.Name,
                fi.FieldType.MakeByRefType(),
                new Type[0]
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldsflda, fi);
            gen.Emit(OpCodes.Ret);

            return dm.Generate().CreateDelegate(typeof(FuncByRef<TField>));
        }

        private static Delegate CreateSetFieldDelegate<TType, TField>(FieldInfo fi)
        {
            var dm = new DynamicMethodDefinition
            (
                "__FieldSet" + fi.DeclaringType?.Name + fi.Name,
                typeof(void),
                new[] { typeof(TType), typeof(TField) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(OpCodes.Stfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.Generate().CreateDelegate(typeof(Action<TType, TField>));
        }

        private static Delegate CreateSetStaticFieldDelegate<TField>(FieldInfo fi)
        {
            var dm = new DynamicMethodDefinition
            (
                "__FieldSet" + fi.DeclaringType?.Name + fi.Name,
                typeof(void),
                new[] { typeof(TField) }
            );

            ILGenerator gen = dm.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Stsfld, fi);
            gen.Emit(OpCodes.Ret);

            return dm.Generate().CreateDelegate(typeof(Action<TField>));
        }
    }
}