using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {
    public class LoadDataContainer {
        private List<Edge> edges = new List<Edge>();
        public List<Port> ports => graphView.ports.ToList();
        private List<BaseNode> nodes => graphView.nodes.ToList().Where(node => node is BaseNode).Cast<BaseNode>().ToList();
        private NodeGraphView graphView;

        public LoadDataContainer() {
            graphView = Helper.nodeGraphView;
            this.edges = graphView.edges.ToList();
        }

        public void Load() {
            ClearGraph();
            GenerateNodes();
            ConnectNodes();
        }


        #region Load
        private void ClearGraph() {
            edges.ForEach(edge => graphView.RemoveElement(edge));
            foreach (BaseNode node in nodes) {
                graphView.RemoveElement(node);
            }
        }

        private void GenerateNodes() {

            //Start
            foreach (BaseNodeUI nodeUI in Helper.nodeGraphContainerSO.StartData) {
                new StartNode(nodeUI.NodePosition, nodeUI.NodeUIGuid, nodeUI.defaultInputPort.PortGuid);
            }

            //Happiness
            foreach (BaseNodeUI nodeUI in Helper.nodeGraphContainerSO.HappinessData) {
                new HappinessEffectRewardNode(nodeUI.NodePosition, nodeUI.NodeUIGuid, nodeUI.defaultInputPort.PortGuid);
            }

            //End
            foreach (BaseNodeUI nodeUI in Helper.nodeGraphContainerSO.EndData) {
                new EndNode(nodeUI.NodePosition, nodeUI.NodeUIGuid, nodeUI.defaultInputPort.PortGuid);
            }

            foreach (BaseNodeUI nodeUI in Helper.nodeGraphContainerSO.TestData) {
                new TestNode(nodeUI.NodePosition, nodeUI.NodeUIGuid, nodeUI.defaultInputPort.PortGuid);
            }

        }

        private void ConnectNodes() {
            Port output = null;
            Port input = null;
            
            foreach (NodeLinkData linkData in Helper.nodeGraphContainerSO.NodeLinkDatas.ToList()) {
                foreach (VisualElement inputPort in ports) {
                    Port port = inputPort as Port;
                    if (port.portName.Equals(linkData.DestionationPortName)) {
                        input = port;
                    }
                    if (port.portName.Equals(linkData.BasePortName)) {
                        output = port;
                    }

                }
                LinkNodesTogether(output, input);
            }
            
        }

        
        private void LinkNodesTogether(Port outputPort, Port inputPort) {

            if (outputPort == null || inputPort == null) {
                return;
            }

            Edge tempEdge = new Edge() {
                output = outputPort,
                input = inputPort
            };

            //check again if ports are compatible
            if (outputPort.node.Q<Label>("connectionTypeLabel").text.Equals(inputPort.node.GetType().Name)) {
                outputPort.ConnectTo(inputPort);
                outputPort.Connect(tempEdge);
                tempEdge.candidatePosition = outputPort.GetGlobalCenter();
                graphView.Add(tempEdge);
            } else {
                var elem = Helper.nodeGraphContainerSO.NodeLinkDatas.Find(link => link.BasePortName == outputPort.portName);
                Helper.nodeGraphContainerSO.NodeLinkDatas.Remove(elem);
                EditorUtility.SetDirty(Helper.nodeGraphContainerSO);
            }

        }

        #endregion
    }
}