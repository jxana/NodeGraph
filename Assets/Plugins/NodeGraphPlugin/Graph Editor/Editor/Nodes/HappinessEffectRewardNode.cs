using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {

    public class HappinessEffectRewardNode : BaseNode {

        private HappinessEffectRewardNodeUI ui = new HappinessEffectRewardNodeUI();
        public override string PropertyPathUIList { get => nameof(NodeGraphContainerSO.HappinessData); }
        public override string PropertyPathNodeUIGuid { get => nameof(HappinessEffectRewardNodeUI.NodeUIGuid); }
        public override BaseNodeUI NodeUI => ui;

        public HappinessEffectRewardNode() { }
        public HappinessEffectRewardNode(Vector2 position, string existingNodeGuid = null, string existingPortGUID = null) : base(position, existingNodeGuid, existingPortGUID) {

            title = "Happiness Effect Reward";

            if (existingNodeGuid == null && !isReload) {
                Helper.nodeGraphContainerSO.HappinessData.Add(NodeUI as HappinessEffectRewardNodeUI);
                Helper.nodeGraphContainer.Update();
                portFactory.AddPortList(typeof(EndNode).Name, 1);
            }

            NodeBuilder.DrawAndBindUI(PropertyPathUIList, PropertyPathNodeUIGuid, Helper.nodeGraphContainerSO, this);
           
        }
    }
}
