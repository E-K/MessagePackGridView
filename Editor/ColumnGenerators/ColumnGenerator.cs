using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MessagePackGridView.ColumnGenerators
{
    public class ColumnGenerator
    {
        private static readonly Dictionary<Type, ColumnGenerator> GeneratorOverrides = new Dictionary<Type, ColumnGenerator>()
        {
            { typeof(KeyValuePair<,>), KeyValuePairColumnGenerator.Instance },
            { typeof(Tuple<>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,,,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,,,,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(Tuple<,,,,,,,>), BuiltinTypeColumnGenerator.Instance },
#if CSHARP_7_OR_LATER
            { typeof(ValueTuple<>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,,,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,,,,,,>), BuiltinTypeColumnGenerator.Instance },
            { typeof(ValueTuple<,,,,,,,>), BuiltinTypeColumnGenerator.Instance },
#endif
        };

        private static readonly ColumnGenerator DefaultColumnGenerator = new ColumnGenerator();

        public static ColumnGenerator GetColumnGenerator(Type elementType)
        {
            if (elementType.IsPrimitiveOrString())
                return PrimitiveColumnGenerator.Instance;

            //overrideがあるなら
            if (elementType.IsGenericType && GeneratorOverrides.TryGetValue(elementType.GetGenericTypeDefinition(), out ColumnGenerator generator))
                return generator;

            return DefaultColumnGenerator;
        }

        public virtual IEnumerable<MessagePackGridViewModel.IColumn> GenerateColumns(Type elementType)
        {
            var members = MessagePackTypeCache.GetMessagePackMembers(elementType);
            foreach(var member in members)
            {
                yield return new MessagePackGridViewModel.Column(member);
            }
        }

        public static bool IgnoreAttribute(Type type)
        {
            return type.IsGenericType && GeneratorOverrides.ContainsKey(type.GetGenericTypeDefinition());
        }
    }

    public class PrimitiveColumnGenerator : ColumnGenerator
    {
        public static readonly PrimitiveColumnGenerator Instance = new PrimitiveColumnGenerator();

        public override IEnumerable<MessagePackGridViewModel.IColumn> GenerateColumns(Type elementType)
        {
            yield return MessagePackGridViewModel.PrimitiveColumn.Instance;
        }
    }

    public class BuiltinTypeColumnGenerator : ColumnGenerator
    {
        public static readonly BuiltinTypeColumnGenerator Instance = new BuiltinTypeColumnGenerator();

        public override IEnumerable<MessagePackGridViewModel.IColumn> GenerateColumns(Type elementType)
        {
            var members = MessagePackTypeCache.GetMessagePackMembers(elementType, ignoretAttribute: true);
            foreach (var member in members)
            {
                yield return new MessagePackGridViewModel.Column(member);
            }
        }
    }

    public class KeyValuePairColumnGenerator : ColumnGenerator
    {
        public static readonly KeyValuePairColumnGenerator Instance = new KeyValuePairColumnGenerator();

        public override IEnumerable<MessagePackGridViewModel.IColumn> GenerateColumns(Type elementType)
        {
            var members = MessagePackTypeCache.GetMessagePackMembers(elementType, ignoretAttribute: true);

            var key = members.First(x => x.Name == "Key") as PropertyInfo;
            var value = members.First(x => x.Name == "Value") as PropertyInfo;
            
            bool keyIgonoreType = ColumnGenerator.IgnoreAttribute(key.PropertyType);
            bool valueIgonoreType = ColumnGenerator.IgnoreAttribute(value.PropertyType);

            return Enumerable.Concat(
                key.PropertyType.IsPrimitiveOrString() ? new[] { new MessagePackGridViewModel.KayValuePairColumn(key, isKey: true) }
                : MessagePackTypeCache.GetMessagePackMembers(key.PropertyType, ignoretAttribute: keyIgonoreType)
                    .Select(m => new MessagePackGridViewModel.KayValuePairColumn(key, isKey: true)),
                value.PropertyType.IsPrimitiveOrString() ? new[] { new MessagePackGridViewModel.KayValuePairColumn(value, isKey: false) }
                : MessagePackTypeCache.GetMessagePackMembers(value.PropertyType, ignoretAttribute: valueIgonoreType)
                    .Select(m => new MessagePackGridViewModel.KayValuePairColumn(m, isKey: false))
            );
        }
    }
}
