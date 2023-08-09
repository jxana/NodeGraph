using UnityEngine;

namespace Plugin.NodeGraph {
    [System.Serializable]
    public class EndNodeData {
        [Header("End Data")]
        public string EndNode = "I am a EndNode";
        public EndNodeType endNodeType = EndNodeType.End;
    }
}
