using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MessagePack;
using UnityEditor;

namespace MessagePackGridView
{
    public static class MessagePackTypeCache
    {
        static void MessagePackTypeCacheXX()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.FullName.StartsWith("mscorlib,"))
                .Where(asm => !asm.FullName.StartsWith("System,"))
                .Where(asm => !asm.FullName.StartsWith("System."))
                .Where(asm => !asm.FullName.StartsWith("nunit.framework"))
                .Where(asm => !asm.FullName.StartsWith("ICSharpCode.NRefactory,"))
                .Where(asm => !asm.FullName.StartsWith("ExCSS.Unity,"))
                .Where(asm => !asm.FullName.StartsWith("Unity"))
                .Where(asm => !asm.FullName.StartsWith("SyntaxTree"))
                .Where(asm => !asm.FullName.StartsWith("Mono.Security,"))
#if (NET_4_6 || NET_STANDARD_2_0)
                .Where(asm => !asm.IsDynamic)
#endif
                .SelectMany(asm => asm.GetExportedTypes())
                .ToArray();

            foreach (var t in types)
            {
                //cache union
                var unions = t.GetCustomAttributes(typeof(UnionAttribute), true)
                    .Cast<UnionAttribute>()
                    .OrderBy(attr => attr.Key)
                    .ToArray();

                if (unions != null && unions.Length > 0)
                {
                    UnionCache.Add(t, unions);
                }

                //next cache [Key] property
                //type should be concrete
                if (t.IsInterface || t.IsAbstract)
                    continue;

                var mpo = t.GetCustomAttributes(typeof(MessagePackObjectAttribute), true);
                if (mpo == null || mpo.Length == 0)
                    continue;

                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(prop => prop.CanRead && prop.CanWrite) //requires get; set;
                    .Where(prop =>
                    {
                        var attr = prop.GetCustomAttributes<KeyAttribute>(true);
                        return attr != null;
                    })
                    .ToArray();

                MemberCache.Add(t, props);
            }
        }

        private static readonly object s_gate = new object(); //lock
        private static readonly Dictionary<Type, UnionAttribute[]> UnionCache = new Dictionary<Type, UnionAttribute[]>();

        /// <summary>
        /// KeyAttributeがついたメンバー(Field or Property)のリフレクション情報
        /// </summary>
        private static readonly Dictionary<Type, MemberInfo[]> MemberCache = new Dictionary<Type, MemberInfo[]>();
        
        public static bool IsCached(Type t)
        {
            return UnionCache.ContainsKey(t) || MemberCache.ContainsKey(t);
        }

        public static UnionAttribute[] GetUnions(Type type)
        {
            UnionAttribute[] unions = null;
            UnionCache.TryGetValue(type, out unions);
            return unions;
        }

        public static MemberInfo[] GetMessagePackMembers(Type type, int unionKey = 0)
        {
            lock(s_gate)
            {
                if(MemberCache.TryGetValue(type, out MemberInfo[] result))
                    return result;

                var m = GetMembersInfo(type);
                MemberCache.Add(type, m);
                return m;
            }
        }

        private static MemberInfo[] GetMembersInfo(Type type)
        {
            var messagePackObjectAttribute = type.GetCustomAttribute<MessagePackObjectAttribute>(inherit: true);
            if (messagePackObjectAttribute is null)
                throw new ArgumentException($"type {type.FullName} shoud have MessagePackObject Attribute");
            
            var props = type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                    .Where(m => m.MemberType == MemberTypes.Field || (m as PropertyInfo).CanRead)
                    .Where(prop =>
                    {
                        var attr = prop.GetCustomAttributes<KeyAttribute>(true);
                        return attr != null;
                    })
                    .ToArray();

            return props;
        }

        #region extensions

        public static (bool isEnumerable, Type interfaceType) IsEnumerable(this Type type)
        {
            if (type.IsInterface)
                return (type.IsGenericInterfaceOf(typeof(IEnumerable<>)), type);

            var interfaces = type.GetInterfaces();
            if (interfaces == null || interfaces.Length <= 0)
                return (false, null);

            foreach(var t in interfaces)
            {
                if (t.IsGenericInterfaceOf(typeof(IEnumerable<>)))
                    return (true, t);
            }
            return (false, null);
        }
        
        public static bool IsDictionary(this Type type)
        {
            if (type.IsInterface)
                return type.IsGenericInterfaceOf(typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>));

            var interfaces = type.GetInterfaces();
            if (interfaces == null || interfaces.Length <= 0)
                return false;

            return type.GetInterfaces().Any(
                t => t.IsGenericInterfaceOf(typeof(IDictionary<,>), typeof(IReadOnlyDictionary<,>))
            );
        }

        private static bool IsGenericInterfaceOf(this Type type, Type openGenericType)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType;
        }

        private static bool IsGenericInterfaceOf(this Type type, Type openGenericType0, Type openGenericType1)
        {
            if (!type.IsGenericType)
                return false;

            var definition = type.GetGenericTypeDefinition();
            return definition == openGenericType0 || definition == openGenericType1;
        }

        /// <summary>
        /// PropertyInfoならPropertyTypeを、FieldInfoならFieldTypeを返します
        /// </summary>
        public static Type ValueType(this MemberInfo member)
        {
            if (member is PropertyInfo prop)
                return prop.PropertyType;

            var field = member as FieldInfo;
            return field.FieldType;
        }

        public static object GetValue(this MemberInfo member, object obj)
        {
            if (member is PropertyInfo prop)
                return prop.GetValue(obj);

            var field = member as FieldInfo;
            return field.GetValue(obj);
        }

        public static bool IsPrimitiveOrString(this Type type)
        {
            return type.IsPrimitive || type == typeof(string);
        }

        #endregion
    }
}