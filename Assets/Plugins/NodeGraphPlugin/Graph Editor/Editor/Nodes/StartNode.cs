using UnityEditor;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {
    public class StartNode : BaseNode {

        private StartNodeUI ui = new StartNodeUI();
        public override string PropertyPathUIList { get => nameof(NodeGraphContainerSO.StartData); }
        public override string PropertyPathNodeUIGuid { get => nameof(StartNodeUI.NodeUIGuid); }

        public override BaseNodeUI NodeUI => ui;

        public StartNode() { }
        public StartNode(Vector2 position, string existingNodeGuid = null, string existingPortGUID = null) : base(position,existingNodeGuid, existingPortGUID) {

            title = "Start";

            if (existingNodeGuid == null && !isReload) {
                Helper.nodeGraphContainerSO.StartData.Add(NodeUI as StartNodeUI);
                Helper.nodeGraphContainer.Update();
                portFactory.AddPortList(typeof(HappinessEffectRewardNode).Name, 1);
            }

            NodeBuilder.DrawAndBindUI(PropertyPathUIList, PropertyPathNodeUIGuid, Helper.nodeGraphContainerSO, this);
        }
    }
}