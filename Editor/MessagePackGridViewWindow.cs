using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace MessagePackGridView
{
    public class MessagePackGridViewWindow : EditorWindow
    {
        private object _data = null;
        private MessagePackGridView _gridView = null;
        
        public void SetData(object data)
        {
            _data = data;

            if(_gridView == null)
            {
                var state = new TreeViewState();
                _gridView = MessagePackGridView.Create(state, _data);
                _gridView.Reload(); //OnGUIより先に呼ぶ必要がある
            }
        }

        private void OnGUI()
        {
            if(_gridView != null)
                _gridView.OnGUI(new UnityEngine.Rect(Vector2.zero, position.size));
        }
    }
}
