using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MessagePackGridView.ColumnGenerators;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MessagePackGridView
{
    public class MessagePackGridViewModel
    {
        private static readonly Type[] IgnoreAttibuteTypeDefinitions =
        {
            typeof(KeyValuePair<,>),
            typeof(Tuple<>),
            typeof(Tuple<,>),
            typeof(Tuple<,,>),
            typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>),
            typeof(Tuple<,,,,,>),
            typeof(Tuple<,,,,,,>),
            typeof(Tuple<,,,,,,,>),
#if CSHARP_7_OR_LATER
            typeof(ValueTuple<>),
            typeof(ValueTuple<,>),
            typeof(ValueTuple<,,>),
            typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>),
            typeof(ValueTuple<,,,,,>),
            typeof(ValueTuple<,,,,,,>),
            typeof(ValueTuple<,,,,,,,>),
#endif
        };

        private readonly List<IColumn> _columns = new List<IColumn>();
        public IReadOnlyList<IColumn> Columns => _columns;
        private readonly List<TreeViewItem<object>> _data = new List<TreeViewItem<object>>();
        public IReadOnlyList<TreeViewItem<object>> Data => _data;

        public MessagePackGridViewModel(object data)
        {
            var type = data.GetType();
            Type elementType = type;
            var (isEnumerable, interfaceType) = type.IsEnumerable();
            if(isEnumerable)
            {
                elementType = interfaceType.GetGenericArguments()[0];
                _data.AddRange(ReflectionHelper.GetEnumerableValues(data).Select((x, i) => new TreeViewItem<object>(i + 1, 0, x.ToString(), x)));
            }
            else //not collection
            {
                _data.Add(new TreeViewItem<object>(1, 0, "", data));
            }

            //Header
            _columns.AddRange(ColumnGenerator.GetColumnGenerator(elementType).GenerateColumns(elementType));
        }

        public MultiColumnHeaderState.Column[] CreateMultiColumnHeaderStateColumns()
        {
            return _columns.Select(x => x.CreateMultiColumnHeaderStateColumn()).ToArray();
        }

        public interface IColumn
        {
            bool IsPrimitive { get; }
            object GetValue(object row);
            MultiColumnHeaderState.Column CreateMultiColumnHeaderStateColumn();
        }

        public class Column : IColumn
        {
            private readonly MemberInfo _memberInfo = null;
            public bool IsPrimitive { get; }
            public Type Type { get; }
            public string Name { get; }

            public Column(MemberInfo memberInfo)
            {
                _memberInfo = memberInfo;
                Type = memberInfo.ValueType();
                Name = memberInfo.Name;
                IsPrimitive = Type.IsPrimitiveOrString();
            }

            public object GetValue(object row)
            {
                return _memberInfo.GetValue(row);
            }

            public MultiColumnHeaderState.Column CreateMultiColumnHeaderStateColumn()
            {
                return new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(_memberInfo.Name),
                    canSort = false
                };
            }
        }

        public class PrimitiveColumn : IColumn
        {
            public static readonly PrimitiveColumn Instance = new PrimitiveColumn();

            public bool IsPrimitive => true;

            private PrimitiveColumn() { }

            public object GetValue(object row)
            {
                return row;
            }

            public MultiColumnHeaderState.Column CreateMultiColumnHeaderStateColumn()
            {
                return new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Value"),
                    canSort = true,
                };
            }
        }

        public class KayValuePairColumn : IColumn
        {
            private readonly MemberInfo _memberInfo = null;
            private bool _isKey = false;
            public bool IsPrimitive { get; }
            private string Prefix { get; }

            public KayValuePairColumn(MemberInfo member, bool isKey = false)
            {
                _memberInfo = member;
                _isKey = isKey;
                IsPrimitive = member.ValueType().IsPrimitiveOrString();
                Prefix = isKey ? "K:" : "V:";
            }

            public object GetValue(object row)
            {
                object flatten = _isKey ? ReflectionHelper.GetKeyValuePairKey(row) : ReflectionHelper.GetKeyValuePairValue(row);
                if (flatten == null)
                    return null;

                return flatten.GetType().IsPrimitiveOrString() ? flatten : _memberInfo.GetValue(flatten);
            }

            public MultiColumnHeaderState.Column CreateMultiColumnHeaderStateColumn()
            {
                return new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent(Prefix + _memberInfo.Name),
                    canSort = IsPrimitive,
                };
            }
        }
    }
}
