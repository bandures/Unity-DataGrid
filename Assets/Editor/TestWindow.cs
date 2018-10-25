using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;


public class TestWindow : EditorWindow
{
    [MenuItem("Test/DataGrid _%#D")]
    public static void ShowWindow()
    {
        var window = GetWindow<TestWindow>();
        window.titleContent = new GUIContent("DataGrid");
        window.minSize = new Vector2(450, 200);
    }

    public void OnEnable()
    {
        var root = this.GetRootVisualContainer();
        root.style.flexDirection = FlexDirection.Row;

        var dataGrid = new DataGrid();
        dataGrid.StretchToParentSize();
        root.Add(dataGrid);
    }
}
