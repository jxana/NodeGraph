using System.Collections.Generic;
using UnityEngine;

namespace Plugin.NodeGraph.Editor {

    [System.Serializable]
    public class HappinessEffectRewardNodeData  {
        [Header("Happiness Data")]
        public string TargetSettlers;
        public string Stack;
       
        public EffectAction EffectAction = EffectAction.ThatAction;
        public HappinessEffect HappinessEffect = HappinessEffect.Decrease;
    }

}


