using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace TEngine
{
    public static class EmitHelper
    {
        private static readonly Dictionary<Type, Delegate> _defaultCache = new Dictionary<Type, Delegate>();

        private static readonly Dictionary<CacheKey, Delegate> _argCache = new Dictionary<CacheKey, Delegate>();

        public static T CreateInstance<T>() where T : new()
        {
            var type = typeof(T);

            if (!_defaultCache.TryGetValue(type, out var factory))
            {
                if (!_defaultCache.TryGetValue(type, out factory))
                {
                    factory = CreateFactory<T>();
                    _defaultCache.TryAdd(type, factory);
                }
            }

            return ((Func<T>)factory)();
        }

        public static object CreateInstance(Type type)
        {
            if (!_defaultCache.TryGetValue(type, out var factory))
            {
                lock (_defaultCache)
                {
                    if (!_defaultCache.TryGetValue(type, out factory))
                    {
                        var constructor = type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null, Type.EmptyTypes, null);

                        if (constructor == null)
                        {
                            throw new MissingMethodException($"No parameterless constructor defined for type '{type.FullName}'");
                        }

                        // 创建动态方法（关键优化点）
                        var dynamicMethod = new DynamicMethod(
                            name: $"CreateInstance_{type.FullName}",
                            returnType: typeof(object),
                            parameterTypes: Type.EmptyTypes,
                            owner: typeof(object),
                            skipVisibility: true);

                        // 生成IL指令（核心逻辑）
                        var il = dynamicMethod.GetILGenerator();

                        // 处理值类型和引用类型的差异
                        if (type.IsValueType)
                        {
                            il.DeclareLocal(type);
                            il.Emit(OpCodes.Ldloca_S, 0);
                            il.Emit(OpCodes.Initobj, type);
                            il.Emit(OpCodes.Ldloc_0);
                            il.Emit(OpCodes.Box, type); // 值类型需要装箱
                        }
                        else
                        {
                            il.Emit(OpCodes.Newobj, constructor);
                        }

                        il.Emit(OpCodes.Ret);

                        factory = dynamicMethod.CreateDelegate(typeof(Func<object>));
                        _defaultCache.TryAdd(type, factory);
                    }
                }
            }

            return ((Func<object>)factory)();
        }

        private static Func<T> CreateFactory<T>() where T : new()
        {
            var constructor = typeof(T).GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);

            var dynamicMethod = new DynamicMethod(
                name: $"CreateInstance_{typeof(T).FullName}",
                returnType: typeof(T),
                parameterTypes: Type.EmptyTypes,
                owner: typeof(object), // 关键优化：使用object类型模块
                skipVisibility: true);

            var il = dynamicMethod.GetILGenerator();
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);

            return (Func<T>)dynamicMethod.CreateDelegate(typeof(Func<T>));
        }


        public static object CreateInstance(Type type, params object[] args)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            args ??= Array.Empty<object>();

            // 预转换参数类型数组
            var argTypes = Array.ConvertAll(args, a => a?.GetType() ?? typeof(object));
            var cacheKey = new CacheKey(type, argTypes);

            if (!_argCache.TryGetValue(cacheKey, out var factory))
            {
                if (!_argCache.TryGetValue(cacheKey, out factory))
                {
                    // 预热动态方法生成
                    factory = CreateDynamicFactory(type, argTypes);
                    _argCache.TryAdd(cacheKey, factory);

                    // 预热编译器
                    ((Func<object[], object>)factory)(args);
                }
            }

            return ((Func<object[], object>)factory)(args);
        }

        private static Func<object[], object> CreateDynamicFactory(Type type, Type[] argTypes)
        {
            // 精确匹配构造函数
            var constructor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, argTypes, null) ?? throw new MissingMethodException(type.Name);

            // 创建动态方法
            var dynamicMethod = new DynamicMethod(
                name: $"CreateInstance_{type.Name}",
                returnType: typeof(object),
                parameterTypes: new[] { typeof(object[]) },
                owner: typeof(object),
                skipVisibility: true);

            var il = dynamicMethod.GetILGenerator();

            for (int i = 0; i < argTypes.Length; i++)
            {
                // 高效参数加载序列（网页9 IL优化）
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldelem_Ref);

                // 类型转换合并优化
                if (argTypes[i].IsValueType)
                {
                    il.Emit(OpCodes.Unbox_Any, argTypes[i]);
                }
                else if (argTypes[i] != typeof(object))
                {
                    il.Emit(OpCodes.Castclass, argTypes[i]);
                }
            }

            il.Emit(OpCodes.Newobj, constructor);

            // 统一装箱处理
            if (type.IsValueType)
            {
                il.Emit(OpCodes.Box, type);
            }

            il.Emit(OpCodes.Ret);

            return (Func<object[], object>)dynamicMethod.CreateDelegate(typeof(Func<object[], object>));
        }

        private readonly struct CacheKey
        {
            public readonly Type Type;

            public readonly Type[] ArgTypes;

            public CacheKey(Type type, Type[] argTypes)
            {
                Type = type;
                ArgTypes = argTypes;
            }

            public override int GetHashCode() => Type.GetHashCode() ^ ArgTypes.Aggregate(0, (h, t) => h ^ t.GetHashCode());
        }
    }
}