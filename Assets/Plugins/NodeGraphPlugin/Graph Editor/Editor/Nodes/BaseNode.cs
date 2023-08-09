using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {
    public abstract class BaseNode : Node, INode {

        protected PortFactory portFactory;
        protected string nodeGuid;
        protected string defaultPortGuid;
        protected bool isReload = true;
        
        public string NodeGuid { get => nodeGuid; set => nodeGuid = value; }
        public abstract string PropertyPathUIList { get; }
        public abstract string PropertyPathNodeUIGuid { get; }
        public abstract BaseNodeUI NodeUI { get; }

        public SerializedObject positionObject;  //wird einmal in der node ertsellt und dann wiederverwendet

        public BaseNode() {}

        public BaseNode(Vector2 position, string existingNodeGuid = null, string existingPortGUID = null) {
           
            Helper.StylesheetLoader(StyleSheets.nodeStyleSheet, styleSheets);
            portFactory = new PortFactory(this);

            SetPosition(new Rect(position, new Vector2(200, 400)));

            if (existingNodeGuid == null) {
                isReload = false;
                defaultPortGuid = Guid.NewGuid().ToString();
                nodeGuid = Guid.NewGuid().ToString();
                Helper.nodeGraphContainer.Update();

            } else {
                defaultPortGuid = existingPortGUID;
                nodeGuid = existingNodeGuid;
            }

            
            NodeUI.defaultInputPort.PortGuid = defaultPortGuid;
            NodeUI.NodeUIGuid = nodeGuid;
            NodeUI.NodePosition = position;

            GetPosition().Set(position.x, position.y, 0, 0);
            name = nodeGuid;

            Port port = portFactory.GetPortInstance(Direction.Input, Port.Capacity.Multi, NodeUI.defaultInputPort.PortGuid);
            inputContainer.Add(port);

            if (isReload) {
                ReloadPorts();
            }

            //warning
            EditorUtility.SetDirty(Helper.nodeGraphContainerSO);
            Helper.nodeGraphView.AddElement(this);
        }

        /// <summary>
        /// Find the matching ui.
        /// </summary>
        /// <param name="GUID"></param>
        /// <returns>null or an nodeUI</returns>
        public BaseNodeUI FindMatchingNodeUI(string GUID) {
            foreach (BaseNodeUI nodeUI in Helper.nodeGraphContainerSO.AllDatas) {
                if (nodeUI.NodeUIGuid.Equals(GUID)) {
                    return nodeUI;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the matching ui entry, clear the list and reload all port data entries from the matching ui
        /// </summary>
        public void ReloadPorts() {

            BaseNodeUI ui = FindMatchingNodeUI(nodeGuid);
            if (ui.portDatas.Count == 0 || ui.portDatas == null) {
                return;
            }

            portFactory.portList.Clear();
            portFactory.portListElements.Clear();

            foreach (PortData data_Port in ui.portDatas.ToArray()) {
                portFactory.ReloadPorts(data_Port.connectionType, data_Port.PortGuid);
            }

        }

    }

}