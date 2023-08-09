using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Reflection;

namespace Plugin.NodeGraph.Editor {
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider {

        private Vector2 mousePosition;
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context) {

            Type[] types = Helper.GetDerivedTypes(typeof(BaseNode));

            List<SearchTreeEntry> tree = new List<SearchTreeEntry>{ new SearchTreeGroupEntry(new GUIContent("Nodes"),1) };

            foreach (Type type in types) {
                Type codeType = Assembly.GetExecutingAssembly().GetType("Plugin.NodeGraph.Editor." + type.Name);
                object obj = Activator.CreateInstance(codeType);
                tree.Add(CreateSearchTreeEntry(type.Name, obj as BaseNode));
            }

            return tree;
        }

        private SearchTreeEntry CreateSearchTreeEntry(string name, BaseNode baseNode) {
            SearchTreeEntry tmp = new SearchTreeEntry(new GUIContent(name)) {
                level = 2,
                userData = baseNode
            };
            return tmp;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context) {
            var editorWindow = Helper.nodeGraphEditorWindow;
            mousePosition = editorWindow
                .rootVisualElement
                .ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, context.screenMousePosition - editorWindow.position.position);

            Vector2 graphMousePosition = Helper.nodeGraphView.contentViewContainer.WorldToLocal(mousePosition);
            return CreateNodeInstance(searchTreeEntry, graphMousePosition);
        }

        private bool CreateNodeInstance(SearchTreeEntry searchTreeEntry, Vector2 position) {
            Type codeType = Assembly.GetExecutingAssembly().GetType("Plugin.NodeGraph.Editor." + searchTreeEntry.userData.GetType().Name);
            Activator.CreateInstance(codeType, position, null, null);
            return true;
        }
    }
}