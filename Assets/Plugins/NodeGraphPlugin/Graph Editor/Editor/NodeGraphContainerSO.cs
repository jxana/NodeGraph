using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Plugin.NodeGraph.Editor {

    [CreateAssetMenu(menuName = "Container/Data Container")]
    [System.Serializable]
    public class NodeGraphContainerSO : ScriptableObject {

        public List<NodeLinkData> NodeLinkDatas = new List<NodeLinkData>();
     
        public List<StartNodeUI> StartData = new List<StartNodeUI>();
        public List<HappinessEffectRewardNodeUI> HappinessData = new List<HappinessEffectRewardNodeUI>();
        public List<EndNodeUI> EndData = new List<EndNodeUI>();
        public List<TestNodeUI> TestData = new List<TestNodeUI>();

        public List<BaseNodeUI> AllDatas {
            get {
                List<BaseNodeUI> tmp = new List<BaseNodeUI>();
                tmp.AddRange(TestData);
                tmp.AddRange(HappinessData);
                tmp.AddRange(EndData);
                tmp.AddRange(StartData);;
                
                return tmp;
            }
        }

    }

    [System.Serializable]
    public class NodeLinkData {
        public string BaseNodeGuid;
        public string BasePortName;
        public string DestinationNodeGuid;
        public string DestionationPortName;
    }
}