using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {
    public class TestNode : BaseNode {

        private TestNodeUI ui = new TestNodeUI();
        public override string PropertyPathUIList { get => nameof(NodeGraphContainerSO.TestData); }
        public override string PropertyPathNodeUIGuid { get => nameof(TestNodeUI.NodeUIGuid); }
        public override BaseNodeUI NodeUI => ui;

        public TestNode() { }
        public TestNode(Vector2 position, string existingNodeGuid = null, string existingPortGUID = null) : base(position, existingNodeGuid, existingPortGUID) {

            title = "Test";

            if (existingNodeGuid == null && !isReload) {
                Helper.nodeGraphContainerSO.TestData.Add(NodeUI as TestNodeUI);
                Helper.nodeGraphContainer.Update();
            }
            NodeBuilder.DrawAndBindUI(PropertyPathUIList, PropertyPathNodeUIGuid, Helper.nodeGraphContainerSO, this);
        }


    }
}