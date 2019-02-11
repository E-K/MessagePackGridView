using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace MessagePackGridView
{
    public class MessagePackGridView : TreeView
    {
        private readonly MessagePackGridViewModel _model = null;
        private List<TreeViewItem> _rows = new List<TreeViewItem>();

        public static MessagePackGridView Create(TreeViewState state, object data)
        {
            var model = new MessagePackGridViewModel(data);
            var header = new MultiColumnHeader(new MultiColumnHeaderState(model.CreateMultiColumnHeaderStateColumns()));
            return new MessagePackGridView(state, header, model);
        }

        public MessagePackGridView(TreeViewState state, MultiColumnHeader header, MessagePackGridViewModel model) : base(state, header)
        {
            _model = model;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1 };
            foreach(var item in _model.Data)
            {
                root.AddChild(item);
            }
            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            _rows.Clear();
            foreach (var item in _model.Data)
            {
                _rows.Add(item);
            }
            return _rows;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as TreeViewItem<object>;
            var numVisibleColumns = args.GetNumVisibleColumns();
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                var column = _model.Columns[i];
                CellGUI(args.GetCellRect(i), item, column, ref args);
            }
        }

        private void CellGUI(Rect cellRect, TreeViewItem<object> item, MessagePackGridViewModel.IColumn column, ref RowGUIArgs args)
        {
            var obj = column.GetValue(item.Value);
            if (obj is null)
                return;

            var str = obj.ToString();
            if(!column.IsPrimitive)
            {
                if(GUI.Button(cellRect, str))
                {
                    var w = EditorWindow.CreateInstance<MessagePackGridViewWindow>();
                    w.SetData(obj);
                    w.Show();
                }
            }
            else
            {
                EditorGUI.LabelField(cellRect, str);
            }
        }
    }
}
