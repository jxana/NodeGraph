using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Plugin.NodeGraph {
    //[CreateAssetMenu(fileName = nameof(NodeGraphSettings), menuName = nameof(NodeGraphSettings))]
    public class NodeGraphSettings : ScriptableObject {
        private const string guid = "9e643d501ee71994b9a1006a16af16c9";

        public string dataPath;
        public string UITemplate;

        private static NodeGraphSettings _instance = null;
        public static NodeGraphSettings Instance {
            get {
                if (_instance == null) {
                    _instance = AssetDatabase.LoadAssetAtPath<NodeGraphSettings>(AssetDatabase.GUIDToAssetPath(guid));
                }
                return _instance;
            }
        }

    }
}
