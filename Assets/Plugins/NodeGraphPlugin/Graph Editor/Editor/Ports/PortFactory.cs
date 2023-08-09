using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {
    public class PortFactory : Node {

        private NodeGraphContainerSO nodeGraphContainerSO;
        private BaseNode parentNode;
        private VisualElement root;
        private Label connectionTypeLabel;
        private int startIndex;
        private Button add;
        private string connectionType;

        public List<Port> portList;
        public List<VisualElement> portListElements;
        public List<VisualElement> slots;

        public int portCount;
       

        public PortFactory(BaseNode parentNode) {

            portListElements = new List<VisualElement>();
            portList = new List<Port>();
            slots = new List<VisualElement>();
            connectionTypeLabel = new Label();
            connectionTypeLabel.name = "connectionTypeLabel";
            connectionTypeLabel.AddToClassList("connectionTypeLabel");;

            this.parentNode = parentNode;
            root = parentNode.mainContainer;
            nodeGraphContainerSO = Helper.nodeGraphContainerSO;
            portCount = 0;

        }

        /// <summary>
        /// Add a list of output ports to the node
        /// </summary>
        /// <param name="count">Amount of ports</param>
        /// <param name="startIndex">Start index of the port list</param>
        public void AddPortList(string connectionType, int count = 1, int startIndex = 0) {

            this.startIndex = startIndex;
            this.connectionType = connectionType;
            
            connectionTypeLabel.text = connectionType;
            root.Insert(startIndex + 2, connectionTypeLabel); 
            root.Insert(startIndex + 3, AddButton());

            for (int i = 0; i < count; i++) {
                AddPort(Guid.NewGuid().ToString());
            }
        }
        public void AddPort(string portGuid) {

            if (portGuid == null || portGuid == string.Empty) {
                Debug.LogWarning("Try to add port with empty or null guid");
                return;
            }

            Port port = GetPortInstance(Direction.Output);
            port.AddToClassList("list_port");
            port.portName = portGuid;
            
            portList.Add(port);

            VisualElement newElementContainer = GetElementContainer(port, portCount);
            portListElements.Add(newElementContainer);
            portCount++;

            AddPortData(port);

            //WARNING 
            UnityEditor.EditorUtility.SetDirty(nodeGraphContainerSO);
        }
        public void ReloadPorts(string connectionType, string portGuid) {

            connectionTypeLabel.text = connectionType;
            this.connectionType = connectionType;

            if (portCount == 0 ) {
                root.Insert(startIndex + 2, connectionTypeLabel);
                root.Insert(startIndex + 3, AddButton());
            }

            Port port = GetPortInstance(Direction.Output);
            port.AddToClassList("list_port");
            port.portName = portGuid;

            portList.Add(port);

            VisualElement newElementContainer = GetElementContainer(port, portCount);
            portListElements.Add(newElementContainer);
            portCount++;

        }
        public Port GetPortInstance(Direction nodeDirection, Port.Capacity capacity = Port.Capacity.Single, string portGuid = null) {
            Port port = InstantiatePort(Orientation.Horizontal, nodeDirection, capacity, typeof(float));

            if (portGuid != null) {
                port.portName = portGuid;
            }
            return port;
        }
        public VisualElement GetElementContainer(Port port, int portListIndex) {

            //Create the slot for the list elemenet
            VisualElement slot = new VisualElement();
            slot.name = portListIndex.ToString();
           
            VisualElement elementContainer = new VisualElement();

            // 1: title container , 2: input container, 3: compatibility label 4: add button
            
            root.Insert(startIndex + portListIndex + 4,slot); 

            AddDeleteButton(port, elementContainer, slot);
            //AddReorderUpAndDown(elementContainer, portListIndex);

            slot.AddToClassList("slot");
            slot.Add(new Label("slot-" + portListIndex));

            elementContainer.AddToClassList("listElem");

            elementContainer.Add(port);
            elementContainer.AddToClassList("listViewElem");

            DragAndDropManipulator manipulator = new DragAndDropManipulator(elementContainer, root, parentNode.FindMatchingNodeUI(parentNode.NodeGuid), parentNode);

            elementContainer.name = "elementContainer";
            slot.Add(elementContainer);

            //return elementContainer;
            return slot;
        }

        /// <summary>
        /// Generate a new portData object and add it to portDatas
        /// </summary>
        /// <param name="port">the port of the portData to be created</param>
        /// <returns></returns>
        private PortData AddPortData(Port port) {
            PortData data = new PortData();
            data.PortGuid = port.portName;
            data.connectionType =  connectionType;
            parentNode.FindMatchingNodeUI(parentNode.NodeGuid).portDatas.Add(data);
            return data;
        }

        #region Reorder
        /**private void AddReorderUpAndDown(VisualElement elementContainer, int portListIndex) {
            BaseNodeUI ui = parentNode.FindMatchingNodeUI(parentNode.NodeGuid);
            int listStartIndex = startIndex + 3;

            Button up = new Button() { text = ('\u21D1').ToString()};
            Button down = new Button() { text = ('\u21D3').ToString() };

            up.AddToClassList("reorder_button");
            down.AddToClassList("reorder_button");

            up.clicked += () => {UpAction(portListIndex, elementContainer, ui);};
            down.clicked += () => { DownAction(portListIndex, elementContainer, ui); };

            elementContainer.Add(up);
            elementContainer.Add(down);
        }

        private void UpAction(int portListIndex, VisualElement elementContainer, BaseNodeUI ui) {
            if (portListIndex == 0) {
                Debug.LogWarning("Element is first list element. Cant push it up");
            } else {
                int elementIndex = root.IndexOf(elementContainer);
                elementContainer.RemoveFromHierarchy();
                root.Insert(elementIndex - 1, elementContainer);
                Helper.Move(ui.portDatas, portListIndex, portListIndex - 1);
                portListIndex -= 1;
            }
        }

        private void DownAction(int portListIndex, VisualElement elementContainer, BaseNodeUI ui) {
            if (portListIndex == ui.portDatas.Count - 1) {
                Debug.LogWarning("Element is last element. Cant pull it down");
            } else {
                int elementIndex = root.IndexOf(elementContainer);
                elementContainer.RemoveFromHierarchy();
                root.Insert(elementIndex + 1, elementContainer);
                Helper.Move(ui.portDatas, portListIndex + 1, portListIndex);
                portListIndex += 1;
            }
        }**/
        #endregion

        #region Add Action
        public Button AddButton() {
            add = new Button() { text = "+" };
            add.AddToClassList("add_port_button");
            add.clicked += () => { AddAction(); };
            return add;
        }
        public void AddAction() {
            AddPort(Guid.NewGuid().ToString());
        }
        #endregion

        #region Delete Action
        private void AddDeleteButton(Port port, VisualElement elementContainer, VisualElement slot) {
            Button delete = new Button() { text = ('\u2717').ToString() };
            delete.AddToClassList("delete_port_button");
            delete.clicked += () => { DeleteAction(port, elementContainer, slot); };
            elementContainer.Add(delete);
        }

        /// <summary>
        /// Remove the port ant its parent container element from hierarchy,
        /// remove the portListElement from portListElement list
        /// remove the portData entry from portList, 
        /// disconnect all edges, 
        /// remove the nodelink entries according to this port, decrease port count 
        /// </summary>
        /// <param name="port">deleted port</param>
        /// <param name="elementContainer">container of deleted port</param>
        private void DeleteAction(Port port, VisualElement elementContainer, VisualElement slot) {

            slot.RemoveFromHierarchy();
            slots.Remove(slot);

            elementContainer.RemoveFromHierarchy();
            portListElements.Remove(elementContainer);
            portList.Remove(elementContainer.Q<Port>());
            port.DisconnectAll();

            foreach (BaseNodeUI ui in nodeGraphContainerSO.AllDatas.ToArray()) {
                for (int i = 0; i < ui.portDatas.Count; i++) {
                    if (ui.portDatas[i].PortGuid == port.portName) {
                        ui.portDatas.Remove(ui.portDatas[i]);
                    }
                }
            }
            for (int i = 0; i < nodeGraphContainerSO.NodeLinkDatas.Count; i++) {
                if (nodeGraphContainerSO.NodeLinkDatas[i].BasePortName.Equals(port.portName) || nodeGraphContainerSO.NodeLinkDatas[i].DestionationPortName.Equals(port.portName)) {
                    nodeGraphContainerSO.NodeLinkDatas.Remove(nodeGraphContainerSO.NodeLinkDatas[i]);
                }
            }
            portCount--;
            Debug.Log(portCount);
            //WARNING
            UnityEditor.EditorUtility.SetDirty(nodeGraphContainerSO);
        }
        #endregion

       
    }

}
