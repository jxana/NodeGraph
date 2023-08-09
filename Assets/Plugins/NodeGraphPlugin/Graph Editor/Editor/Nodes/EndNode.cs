using UnityEditor;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {
    public class EndNode : BaseNode {

        private EndNodeUI ui = new EndNodeUI();
        public override string PropertyPathUIList { get => nameof(NodeGraphContainerSO.EndData); }
        public override string PropertyPathNodeUIGuid { get => nameof(EndNodeUI.NodeUIGuid); }
        public override BaseNodeUI NodeUI => ui;

        public EndNode() { }
        public EndNode(Vector2 position, string existingNodeGuid = null, string existingPortGUID = null) : base(position, existingNodeGuid, existingPortGUID) {

            title = "End";

            if (existingNodeGuid == null && !isReload) {
                Helper.nodeGraphContainerSO.EndData.Add(NodeUI as EndNodeUI);
                Helper.nodeGraphContainer.Update();
            }
            NodeBuilder.DrawAndBindUI(PropertyPathUIList, PropertyPathNodeUIGuid, Helper.nodeGraphContainerSO, this);
        }


    }
}