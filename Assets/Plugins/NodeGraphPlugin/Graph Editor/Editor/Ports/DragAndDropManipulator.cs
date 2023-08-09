using Plugin.NodeGraph.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class DragAndDropManipulator : PointerManipulator {
    public DragAndDropManipulator(VisualElement target, VisualElement rootContainer, BaseNodeUI ui, BaseNode node) {
        this.target = target;
        this.ui = ui;
        this.node = node;
        root = rootContainer;
    }
    protected override void RegisterCallbacksOnTarget() {
        target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
        target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
    }

    protected override void UnregisterCallbacksFromTarget() {
        target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
        target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
        target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
    }
    private BaseNodeUI ui;

    private BaseNode node;
    private Vector2 targetStartPosition { get; set; }

    private Vector3 pointerStartPosition { get; set; }

    private bool enabled { get; set; }

    private VisualElement root { get; }

    private UQueryBuilder<VisualElement> allSlots = new UQueryBuilder<VisualElement>();

    private UQueryBuilder<VisualElement> allListElems = new UQueryBuilder<VisualElement>();

    private int NewHomeSlotNumber = 0;

    private int targetSlot = 0;

    private void PointerDownHandler(PointerDownEvent evt) {
        targetStartPosition = target.transform.position;
        pointerStartPosition = evt.position;
        target.CapturePointer(evt.pointerId);
        target.BringToFront();

        //slot from the target listContainerObject
        targetSlot = int.Parse(target.parent.Q<VisualElement>(className: "slot").name);
        enabled = true;
    }
    private void PointerMoveHandler(PointerMoveEvent evt) {
        this.allSlots = root.Query<VisualElement>(className: "slot");
        this.allListElems = root.Query<VisualElement>(className: "listElem");

        if (enabled && target.HasPointerCapture(evt.pointerId)) {

            Vector2 pointerDelta = evt.position - pointerStartPosition;
            target.transform.position = new Vector2(.0f, pointerDelta.y);

            if (OverlapsTarget(target)) {

                UQueryBuilder<VisualElement> overlappingSlots = allSlots.Where(OverlapsTarget);
                VisualElement closestOverlappingSlot = FindClosestSlot(overlappingSlots);
                Vector3 closestStartPosition = RootSpaceOfSlot(closestOverlappingSlot.Q<VisualElement>());

                if (closestOverlappingSlot.name != "listContainer") {
                    OverlappAction(closestOverlappingSlot);
                    EditorUtility.SetDirty(Helper.nodeGraphContainerSO);
                }

            }
        }
    }

    public void OverlappAction(VisualElement closestOverlappingSlot) {

        //slot from the closest overlapping element (depending of moving up or down the above or below slot)
        int destinantionSlotNumber = int.Parse(closestOverlappingSlot.name);
        NewHomeSlotNumber = destinantionSlotNumber;

        // If the target element have changed positon, the numbers will be different from each other
        if (targetSlot != destinantionSlotNumber) {

            VisualElement listElem = new VisualElement(); //the list element that is gonna change the slot 
            VisualElement newHomeSlot = new VisualElement();//the new slot for the element
            NewHomeSlotNumber = destinantionSlotNumber;

            //Debug.Log(targetSlot + " : " + destinantionSlotNumber);

            if (destinantionSlotNumber > targetSlot) {
                destinantionSlotNumber -= 1;
                targetSlot += 1;
                Helper.Move(ui.portDatas, targetSlot, destinantionSlotNumber);
            } else {
                destinantionSlotNumber += 1;
                targetSlot -= 1;
                Helper.Move(ui.portDatas,destinantionSlotNumber, targetSlot);
            }
            newHomeSlot = root.Q<VisualElement>((destinantionSlotNumber).ToString());
            listElem = allListElems.ToList().Find(elem => elem.parent.name.Equals(closestOverlappingSlot.name));
            newHomeSlot.Add(listElem);

            Port port = listElem.Q<Port>();
            port.UpdatePresenterPosition();
            EditorUtility.SetDirty(Helper.nodeGraphContainerSO);
        }
    }

    private void ReconnectPorts(VisualElement slot) {
        string destinationPortGuid = "";
        string targetPortGuid = slot.Q<Port>().Q<Label>().text;
        Port targetPort = slot.Q<Port>();
        BaseNode node = targetPort.node as BaseNode;
        List<NodeLinkData> nodelinks = Helper.nodeGraphContainerSO.NodeLinkDatas;
        foreach(NodeLinkData link in nodelinks) {
            if(link.BaseNodeGuid == node.NodeGuid){
                if(link.BasePortName == targetPortGuid) {
                    destinationPortGuid = link.DestionationPortName;
                }
            }
        }
        Port port = Helper.nodeGraphView.ports.ToList().Find(port => port.portName == destinationPortGuid);
        if(port != null) {
            Debug.Log("help");
            targetPort.DisconnectAll();
            targetPort.ConnectTo(port);
        }
        EditorUtility.SetDirty(Helper.nodeGraphContainerSO);
    }

    private void PointerUpHandler(PointerUpEvent evt) {
        if (enabled && target.HasPointerCapture(evt.pointerId)) {

            VisualElement listElem = target.Q<VisualElement>(className: "listElem");
            VisualElement slot = allSlots.ToList().Find(slot => slot.name.Equals((NewHomeSlotNumber).ToString()));

            slot.Add(listElem);
            listElem.BringToFront();
            listElem.transform.position = new Vector3(0, 0, 0);

            target.ReleasePointer(evt.pointerId);

            GraphViewChange graphViewChange = new GraphViewChange();
            graphViewChange.movedElements = new List<GraphElement>();
            graphViewChange.movedElements.AddRange(Helper.nodeGraphView.edges.ToList());
            graphViewChange.moveDelta = new Vector2(1f,3f);
            Helper.nodeGraphView.graphViewChanged(graphViewChange);
           
            ReconnectPorts(listElem);
        }
    }

    private bool OverlapsTarget(VisualElement slot) {
        return target.worldBound.Overlaps(slot.worldBound);
    }

    private VisualElement FindClosestSlot(UQueryBuilder<VisualElement> slots) {
        List<VisualElement> slotsList = slots.ToList();
        float bestDistanceSq = float.MaxValue;
        VisualElement closest = null;
        foreach (VisualElement slot in slotsList) {
            Vector3 displacement =
                RootSpaceOfSlot(slot) - target.transform.position;
            float distanceSq = displacement.sqrMagnitude;
            if (distanceSq < bestDistanceSq) {
                bestDistanceSq = distanceSq;
                closest = slot;
            }
        }
        return closest;
    }

    private Vector3 RootSpaceOfSlot(VisualElement slot) {
        Vector2 slotWorldSpace = slot.parent.LocalToWorld(slot.layout.position);
        return root.WorldToLocal(slotWorldSpace);
    }

}