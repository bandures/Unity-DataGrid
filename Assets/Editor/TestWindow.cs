using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using System.Collections.Generic;

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

        dataGrid.AddIndexColumn("#", 30);
        dataGrid.AddTextColumn("Name", 100, (object data) => { var go = data as GameObject; return go.name; });
//        dataGrid.AddPropertyColumn("Object", 200, (object data) => { var so = new SerializedObject(data as UnityEngine.Object); return so; });

        dataGrid.AddPropertyColumn("Local Position", 250, (object data) => {
            var so = new SerializedObject((data as GameObject).transform);
            return so.FindProperty("m_LocalPosition");
            });
        dataGrid.AddPropertyColumn("X", 100, (object data) => {
            var so = new SerializedObject((data as GameObject).transform);
            return so.FindProperty("m_LocalPosition.x");
        });


        List<GameObject> roots = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(roots);
        dataGrid.DataProvider = roots;
    }
}
