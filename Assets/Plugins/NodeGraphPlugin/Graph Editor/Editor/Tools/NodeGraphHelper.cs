using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {
    public class NodeGraphHelper {

        public NodeGraphContainerSO nodeGraphContainerSO;
        public NodeGraphView nodeGraphView;
        public NodeGraphEditorWindow nodeGraphEditorWindow;
        public SerializedObject nodeGraphContainer;
        public NodeGraphSettings settings = NodeGraphSettings.Instance;


        private static NodeGraphHelper _instance = null;
        public static NodeGraphHelper Instance {
            get {
                if (_instance == null) {
                    _instance = new NodeGraphHelper();
                }
                return _instance;
            }
        }

        /// <summary>
        /// Load a stylesheet in a given styleSheetSet
        /// </summary>
        /// <param name="path"></param>
        /// <param name="set"></param>
        public void StylesheetLoader(string path, VisualElementStyleSheetSet set) {
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
        public void Move<T>(List<T> list, int oldIndex, int newIndex) {
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
        public Type[] GetDerivedTypes(Type baseType) {
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
        /// Open the project brower at a given path 
        /// </summary>
        public void OpenProjectBrowser() {
            var obj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(settings.dataPath);

            var projectBrowserType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.ProjectBrowser");
            var projectBrowser = EditorWindow.GetWindow(projectBrowserType);

            //private void ShowFolderContents(int folderInstanceID, bool revealAndFrameInFolderTree)
            var ShowFolderContents = projectBrowserType.GetMethod("ShowFolderContents", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] { typeof(int), typeof(bool) }, null);
            ShowFolderContents.Invoke(projectBrowser, new object[] { obj.GetInstanceID(), true });
        }


    }
}