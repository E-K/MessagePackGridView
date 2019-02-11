using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagePackGridView.ColumnGenerators
{
    public class ColumnGenerator
    {
        private static readonly Dictionary<Type, ColumnGenerator> GeneratorOverrides = new Dictionary<Type, ColumnGenerator>()
        {
            { typeof(KeyValuePair<,>), BuiltinTypeColumnGenerator.Instance },
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
}
