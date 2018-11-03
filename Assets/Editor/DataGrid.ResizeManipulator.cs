using System;
using UnityEngine.Experimental.UIElements;

public partial class DataGrid
{
    public class ResizeManipulator : Manipulator
{
    bool hold = false;
    Action<float> applyDelegate;

    public ResizeManipulator(Action<float> _applyDelegate)
    {
        applyDelegate = _applyDelegate;
    }

    protected override void RegisterCallbacksOnTarget()
    {
        target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        target.RegisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);
    }

    protected override void UnregisterCallbacksFromTarget()
    {
        target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
        target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        target.UnregisterCallback<MouseCaptureOutEvent>(OnMouseCaptureOutEvent);
    }

    private void OnMouseDown(MouseDownEvent e)
    {
        if (hold)
        {
            e.StopImmediatePropagation();
            return;
        }

        if (target == null)
            return;

        hold = true;
        MouseCaptureController.TakeMouseCapture(target);
        e.StopPropagation();
    }

    private void OnMouseMove(MouseMoveEvent e)
    {
        if (hold)
        {
            applyDelegate(e.mouseDelta.x);
            e.StopPropagation();
        }
    }

    private void OnMouseUp(MouseUpEvent e)
    {
        hold = false;

        MouseCaptureController.ReleaseMouseCapture(target);
        e.StopPropagation();
    }

    void OnMouseCaptureOutEvent(MouseCaptureOutEvent e)
    {
        hold = false;
    }
}
}