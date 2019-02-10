using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.IMGUI.Controls;

namespace MessagePackGridView
{
    public class MessagePackGridViewModel
    {
        public readonly MultiColumnHeaderState.Column[] columns;
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
                _data.AddRange(EnumerableReflectionHelper.GetValues(data).Select((x, i) => new TreeViewItem<object>(i + 1, 0, x.ToString(), x)));
            }
            else //not collection
            {
                _data.Add(new TreeViewItem<object>(1, 0, "", data));
            }

            //Header
            if(elementType.IsPrimitiveOrString())
            {
                columns = new MultiColumnHeaderState.Column[] { new MultiColumnHeaderState.Column
                {
                    headerContent = new UnityEngine.GUIContent("Value"),
                }};
                _columns.Add(PrimitiveColumn.Instance);
            }
            else
            {
                var members = MessagePackTypeCache.GetMessagePackMembers(elementType);
                columns = new MultiColumnHeaderState.Column[members.Length];
                for(int i = 0; i < members.Length; i++)
                {
                    var member = members[i];
                    columns[i] = new MultiColumnHeaderState.Column
                    {
                        headerContent = new UnityEngine.GUIContent(member.Name),
                    };
                    _columns.Add(new Column(member));
                }
            }
        }

        public interface IColumn
        {
            bool IsPrimitive { get; }
            object GetValue(object row);
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
        }
    }
}
