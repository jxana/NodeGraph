using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using System.IO;

namespace Plugin.NodeGraph.Editor {
    public class NodeGraphEditorWindow : EditorWindow {

        private LoadDataContainer loader;
        private static VisualElement ui;
        private int currentPickerWindow;
        private Action onDraw;


        [MenuItem("Window/Graph/Graph View")]
        static void InitWindow() {
            NodeGraphEditorWindow window = (NodeGraphEditorWindow)GetWindow(typeof(NodeGraphEditorWindow));
            window.titleContent.text = "Node Graph";
            window.Show();
        }

        private void OnEnable() {
            LoadTemplate();
            ConstructGraphView();
            DefineButtonActions();
        }
        private void OnDisable() {
            Helper.nodeGraphContainerSO = null;
        }

        private void OnGUI() {
            onDraw?.Invoke();
        }

        private void LoadTemplate() {
            VisualTreeAsset uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Helper.settings.UITemplate);
            ui = uiAsset.CloneTree();
            rootVisualElement.Add(ui);

        }

        private void ConstructGraphView() {
            Helper.nodeGraphView = new NodeGraphView(this);
            Helper.nodeGraphView.StretchToParentSize();
            var container = ui.Q<VisualElement>("GraphViewContainerElement");
            container.Add(Helper.nodeGraphView);
        }

        private void DefineButtonActions() {
            Button load = ui.Q<VisualElement>("InspectorElement").Q<Button>("Load");
            load.clicked += () => {
                currentPickerWindow = EditorGUIUtility.GetControlID(FocusType.Passive) + 100;
                EditorGUIUtility.ShowObjectPicker<NodeGraphContainerSO>(null, false, "", currentPickerWindow);
                onDraw = LoadPickedNodeGraphContainer;
            };

            Button create = ui.Q<VisualElement>("InspectorElement").Q<Button>("Create");
            create.clicked += () => {
                ShowCreationSection();
            };

            Button show = ui.Q<VisualElement>("InspectorElement").Q<Button>("ShowInspector");
            show.clicked += () => {

                if (Helper.nodeGraphContainerSO == null) {
                    return;
                }
                AssetDatabase.OpenAsset(Helper.nodeGraphContainerSO);

            };
        }
        private void LoadPickedNodeGraphContainer() {
            if (Event.current.commandName == "ObjectSelectorClosed" && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow) {
                Helper.nodeGraphContainerSO = EditorGUIUtility.GetObjectPickerObject() as NodeGraphContainerSO;
                Helper.nodeGraphContainer = new SerializedObject(Helper.nodeGraphContainerSO);
                ui.Q<VisualElement>("InspectorElement").Q<Label>("NameOfContainer").text = Helper.nodeGraphContainerSO.name;
                loader = new LoadDataContainer();
                loader.Load();
            }
        }
        private void ShowCreationSection() {

            VisualElement creationField = ui.Q<VisualElement>("InspectorElement").Q<VisualElement>("CreationField");
            TextField nameField = creationField.Q<TextField>("GraphNameField");
            VisualElement buttonContainer = creationField.Q<VisualElement>("ButtonHolder");
            Button submit = buttonContainer.Q<Button>("Submit");
            Button exit = buttonContainer.Q<Button>("Exit");

            nameField.visible = true;
            buttonContainer.visible = true;

            submit.clicked += () => {
                if (nameField.text == null) {
                    return;
                }

                ScriptableObject nodeContainer = ScriptableObject.CreateInstance<NodeGraphContainerSO>();
                AssetDatabase.CreateAsset(nodeContainer, Helper.settings.dataPath + nameField.text + ".asset");
                AssetDatabase.SaveAssets();
                nameField.visible = false;
                buttonContainer.visible = false;

            };
            exit.clicked += () => {
                nameField.visible = false;
                buttonContainer.visible = false;
                return;
            };
        }
    }
}