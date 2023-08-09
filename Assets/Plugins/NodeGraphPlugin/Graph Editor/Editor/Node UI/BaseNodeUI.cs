using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {

    [System.Serializable]
    public class PortData {
        public string PortGuid;
        public string DestionationPortGuid;
        public string connectionType;
    }

    [System.Serializable]
    public class BaseNodeUI{
        public DefaultPortTempalte defaultInputPort = new DefaultPortTempalte();
        [HideInInspector] public List<PortData> portDatas = new List<PortData>();

        public string NodeUIGuid;
        public Vector2 NodePosition;
    }

    [System.Serializable]
    public class DefaultPortTempalte {
        public string PortGuid;
        [HideInInspector] public string SourcePortGuid; //not usefull if there is a multiple input port...
    }

}
