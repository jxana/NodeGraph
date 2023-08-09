using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {
    public class NodeGraphView : GraphView {

        private NodeSearchWindow searchWindow;
        private ConcurrentBag<NodeLinkData> nodeLinkCollection = new ConcurrentBag<NodeLinkData>();
        public List<Port> compatiblePorts;
        private NodeGraphContainerSO dataContainerSO { get => Helper.nodeGraphContainerSO; }
        public NodeGraphView(NodeGraphEditorWindow editorWindow) {

            Helper.nodeGraphEditorWindow = editorWindow;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            Helper.StylesheetLoader(StyleSheets.graphViewStyleSheet, styleSheets);

            this.AddManipulator(new ContentDragger());      // The ability to drag nodes around.
            this.AddManipulator(new SelectionDragger());    // The ability to drag all selected nodes around.
            this.AddManipulator(new RectangleSelector());   // The ability to drag select a rectangle area.
            this.AddManipulator(new FreehandSelector());    // The ability to select a single node.

            GridBackground grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            compatiblePorts = new List<Port>();
            graphViewChanged = OnGraphChange;
            AddSearchWindow();

        }
        private void AddSearchWindow() {
            searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        /// <summary>
        /// Delete the binded NodeUI if a selected node going to be deleted 
        /// </summary>
        /// <returns></returns>
        public override EventPropagation DeleteSelection() {

            List<Edge> edgesToDelete = new List<Edge>();

            //Fill the node link collection 
            foreach (NodeLinkData linkdata in dataContainerSO.NodeLinkDatas.ToArray()) {
                nodeLinkCollection.Add(linkdata);
            }

            foreach (ISelectable obj in selection.ToArray()) {

                if (obj is Node) {
                    Helper.DeleteSelectedNode(obj as Node, nodeLinkCollection);
                    Helper.DisconnectEdges(obj as Node);
                    continue;
                }

                if (obj is Edge) {
                    Edge edge = obj as Edge;
                    Helper.DeleteSelectedEdge(edge, edgesToDelete, nodeLinkCollection);
                    continue;
                }
            }

            

            DeleteElements(edgesToDelete);
            //Warning
            EditorUtility.SetDirty(dataContainerSO);
            return base.DeleteSelection();


        }

        /// <summary>
        /// Override the handle event method to update node position if it was changed 
        /// </summary>
        /// <param name="evt"></param>
        protected override void ExecuteDefaultAction(EventBase evt) {

            if (evt is PointerUpEvent) {
                ChangeNodePosition(evt as PointerUpEvent);
            }

            base.ExecuteDefaultAction(evt);
        }

        private void ChangeNodePosition(PointerUpEvent evt) {
            foreach (ISelectable obj in selection.ToArray()) {
                if (obj is Node) {
                    BaseNode node = obj as BaseNode;
                    foreach (BaseNodeUI x in dataContainerSO.AllDatas.ToArray()) {
                        if (x.NodeUIGuid.Equals(node.name)) {
                            x.NodePosition.x = node.GetPosition().x;
                            x.NodePosition.y = node.GetPosition().y;
                            UnityEditor.EditorUtility.SetDirty(dataContainerSO);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Change listener
        /// </summary>
        /// <param name="change">The detected event</param>
        public GraphViewChange OnGraphChange(GraphViewChange change) {
            if (change.edgesToCreate != null) {
                //regitser a new edge drawing callback
                foreach (Edge edge in change.edgesToCreate.ToArray()) {

                    //generate a new node link data to store the new node connection => DONT NEED THAT; REMOVE!
                    NodeLinkData nodeLink = new NodeLinkData();
                    nodeLink.BaseNodeGuid = edge.output.node.name;
                    nodeLink.BasePortName = edge.output.portName;
                    nodeLink.DestinationNodeGuid = edge.input.parent.parent.parent.parent.parent.name;
                    nodeLink.DestionationPortName = edge.input.portName;

                    dataContainerSO.NodeLinkDatas.Add(nodeLink);
                    //find the corrosponding ui entry to bind the datas
                    foreach (BaseNodeUI x in dataContainerSO.AllDatas.ToArray()) {

                        if (x.NodeUIGuid.Equals(nodeLink.BaseNodeGuid)) {
                            PortData data_Port = x.portDatas.Find(port => port.PortGuid == nodeLink.BasePortName);
                            data_Port.DestionationPortGuid = nodeLink.DestionationPortName;
                        }
                        if (x.NodeUIGuid.Equals(nodeLink.DestinationNodeGuid)) {
                            x.defaultInputPort.SourcePortGuid = nodeLink.BasePortName;
                        }
                    }
                }
            }

            if(change.movedElements != null) {
                MarkDirtyRepaint();
            }
            
            return change;
        }

        /// <summary>
        /// This is a graph view method that we override.
        /// This is where we tell the graph view which nodes can connect to each other.
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            List<Port> compatiblePorts = new List<Port>();
            Port startPortView = startPort;

            ports.ForEach((port) => {
                Port portView = port;

                if (startPortView != portView && startPortView.node != portView.node && startPortView.direction != port.direction && startPortView.portColor == portView.portColor) {
                    if (startPort.node.Q<Label>("connectionTypeLabel") != null) {
                        if (startPort.node.Q<Label>("connectionTypeLabel").text.Equals(portView.node.GetType().Name)) {
                            compatiblePorts.Add(port);
                        }
                    }
                }
                port.RegisterCallback<MouseDownEvent>(evt => {

                });
            });

            return compatiblePorts;
        }
    }
}