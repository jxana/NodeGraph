using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {
    public static class Helper {

        public static NodeGraphContainerSO nodeGraphContainerSO;
        public static NodeGraphView nodeGraphView;
        public static NodeGraphEditorWindow nodeGraphEditorWindow;
        public static SerializedObject nodeGraphContainer;
        public static NodeGraphSettings settings = NodeGraphSettings.Instance;

        /// <summary>
        /// Find all Graphs in asset database
        /// </summary>
        /// <returns></returns>
        public static List<NodeGraphContainerSO> FindAllNodeGraphContainerSO() {
            string[] guids = AssetDatabase.FindAssets("t:NodeGraphContainerSO");
            NodeGraphContainerSO[] items = new NodeGraphContainerSO[guids.Length];

            for (int i = 0; i < guids.Length; i++){
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);                
                items[i] = AssetDatabase.LoadAssetAtPath<NodeGraphContainerSO>(path);
            }
            return items.ToList();
        }

        /// <summary>
        /// Load a stylesheet in a given styleSheetSet
        /// </summary>
        /// <param name="path"></param>
        /// <param name="set"></param>
        public static void StylesheetLoader(string path, VisualElementStyleSheetSet set) {
            StyleSheet styleSheet = Resources.Load<StyleSheet>(path);
            set.Add(styleSheet);
        }

        /// <summary>
        /// Move one list item at given index to an new index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="oldIndex"></param>
        /// <param name="newIndex"></param>
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex) {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex) newIndex--;
            list.Insert(newIndex, item);
        }

        /// <summary>
        /// Get all derived types from a base type 
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static Type[] GetDerivedTypes(Type baseType) {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies) {
                try {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).ToArray());
                } catch (ReflectionTypeLoadException) { }
            }
            return types.ToArray();
        }

        /// <summary>
        /// Open the project browser at a given path 
        /// </summary>
        public static void OpenProjectBrowser(string path) {;

            var obj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);

            var projectBrowserType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var projectBrowser = EditorWindow.GetWindow(projectBrowserType);

            //private void ShowFolderContents(int folderInstanceID, bool revealAndFrameInFolderTree)
            var ShowFolderContents = projectBrowserType.GetMethod("ShowFolderContents", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new System.Type[] { typeof(int), typeof(bool) }, null);
            ShowFolderContents.Invoke(projectBrowser, new object[] { obj.GetInstanceID(), true });
        }

        /// <summary>
        /// Disconnect all ports of a given node
        /// </summary>
        /// <param name="node"></param>
        public static void DisconnectEdges(Node node) {
            BaseNode baseNode = node as BaseNode;

            foreach (Port port in baseNode.inputContainer.Children()) {
                port.DisconnectAll();
                nodeGraphView.DeleteElements(port.connections);
            }

            foreach (VisualElement container in baseNode.mainContainer.Children()) {
                if (container.Q<VisualElement>("elementContainer") != null) {
                    VisualElement portContainer = container.Q<VisualElement>("elementContainer");
                    if (portContainer.Q<Port>() != null && portContainer.Q<Port>().connected is true) {
                        Port port = portContainer.Q<Port>();
                        nodeGraphView.DeleteElements(port.connections);
                    }
                }
            }
        }


        /// <summary>
        /// Delete the node with reflection approach. Removes the NodeUIData entry from according list in the SO 
        /// </summary>
        /// <param name="node"></param>
        public static void DeleteSelectedNode(Node node, ConcurrentBag<NodeLinkData> nodeLinkCollection) {

            foreach (BaseNodeUI x in nodeGraphContainerSO.AllDatas.ToArray()) {

                BaseNode baseNode = node as BaseNode;

                if (x.NodeUIGuid.Equals(baseNode.NodeGuid)) {

                    nodeGraphContainerSO.AllDatas.Remove(x);

                    SerializedObject nodeContainer = Helper.nodeGraphContainer;
                    SerializedProperty nodeData = nodeContainer.FindProperty(baseNode.PropertyPathUIList);
                    SerializedProperty uiList = nodeContainer.FindProperty(nodeData.name);

                    for (int i = 0; i < uiList.arraySize; i++) {
                        SerializedProperty item = uiList.GetArrayElementAtIndex(i);
                        string guid = item.FindPropertyRelative("NodeUIGuid").stringValue;
                        if (guid.Equals(x.NodeUIGuid)) {
                            uiList.GetArrayElementAtIndex(i).DeleteCommand();
                            nodeContainer.ApplyModifiedProperties();
                        }
                    }
                }
            }

            foreach (NodeLinkData nodeLink in nodeLinkCollection.ToArray()) {
                BaseNode baseNode = node as BaseNode;
                if (nodeLink.DestinationNodeGuid.Equals(baseNode.NodeGuid) || nodeLink.BaseNodeGuid.Equals(baseNode.NodeGuid)) {
                    nodeGraphContainerSO.NodeLinkDatas.Remove(nodeLink);
                    EditorUtility.SetDirty(nodeGraphContainerSO);
                }
            }


        }

        /// <summary>
        /// Remove the edge from graph view.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="edgesToDelete"></param>
        public static void DeleteSelectedEdge(Edge edge, List<Edge> edgesToDelete, ConcurrentBag<NodeLinkData> nodeLinkCollection) {
            edgesToDelete.Add(edge);
            foreach (NodeLinkData linkdata in nodeLinkCollection.ToArray()) {
                if (linkdata.BasePortName.Equals(edge.output.portName) && linkdata.DestionationPortName.Equals(edge.input.portName)) {
                    nodeGraphContainerSO.NodeLinkDatas.Remove(linkdata);
                }
            }

        }
    }
}