using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

public partial class DataGrid
{
    public class DragAndDropManipulator : Manipulator
    {
        private bool m_Active = false;
        private bool m_Dragging = false;

        public DragAndDropManipulator()
        {
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
            target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);

            target.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            target.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
            target.RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
            target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);

            target.UnregisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            target.UnregisterCallback<DragPerformEvent>(OnDragPerformEvent);
            target.UnregisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (m_Active || m_Dragging)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (target == null)
                return;


            m_Active = true;
            MouseCaptureController.TakeMouseCapture(target);
            e.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (m_Active && !m_Dragging)
            {
                m_Dragging = true;

                DragAndDrop.PrepareStartDrag();
                /// this IS required for dragging to work
                //DragAndDrop.objectReferences = new UnityEngine.Object[] { };
                /// if you want Hierarchy view to accept object, it needs to be transform
                DragAndDrop.objectReferences = new UnityEngine.Object[] { /*FindObjectOfType<Camera>().transform*/ };
                /// if you want to drag serialized asset, it needs to be path
                //DragAndDrop.paths = "";
                /// You can supply property/value attached to dragged object
                DragAndDrop.SetGenericData("DragSelection", "Test");

                /// You can provide name
                if (DragAndDrop.objectReferences.Length > 1)
                    DragAndDrop.StartDrag("<Multiple>");
                else
                    DragAndDrop.StartDrag("Object Title");

                DragAndDrop.visualMode = DragAndDropVisualMode.Move;

                MouseCaptureController.ReleaseMouseCapture(target);
                e.StopPropagation();
            }
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            m_Active = false;
            m_Dragging = false;

            MouseCaptureController.ReleaseMouseCapture(target);
            e.StopPropagation();
        }

        void OnMouseCaptureOutEvent(MouseCaptureOutEvent e)
        {
            m_Active = false;
            m_Dragging = false;
        }

        private void OnDragUpdatedEvent(DragUpdatedEvent evt)
        {
            Debug.Log("Drag update!");

            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            evt.StopPropagation();
        }

        private void OnDragPerformEvent(DragPerformEvent evt)
        {
            Debug.Log("Drag perform!");
            DragAndDrop.AcceptDrag();
            evt.StopPropagation();
        }

        private void OnDragLeaveEvent(DragLeaveEvent evt)
        {
            Debug.Log("Drag leave!");
        }
    }
}