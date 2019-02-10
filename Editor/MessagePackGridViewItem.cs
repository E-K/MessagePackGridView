using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.IMGUI.Controls;

namespace MessagePackGridView
{
    public class TreeViewItem<T> : TreeViewItem
    {
        public T Value { get; }
        public TreeViewItem(int id, int depth, string displayName, T value) : base(id, depth, displayName)
        {
            this.Value = value;
        }
    }
}
