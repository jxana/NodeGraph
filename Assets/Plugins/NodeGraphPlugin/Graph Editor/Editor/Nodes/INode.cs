namespace Plugin.NodeGraph.Editor {
    public interface INode {
        public void ReloadPorts();
        public BaseNodeUI FindMatchingNodeUI(string GUID);
    }
}