using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

        dataGrid.AddTextColumn("Name", 100, (object data) => { var go = data as GameObject; return go.name; });
//        dataGrid.AddPropertyColumn("Object", 200, (object data) => { var so = new SerializedObject(data as UnityEngine.Object); return so; });
        dataGrid.AddPropertyColumn("Position", 500, (object data) => {
            var so = new SerializedObject((data as GameObject).transform);
            return so.FindProperty("m_LocalPosition");
            });

        List<GameObject> roots = new List<GameObject>();
        SceneManager.GetActiveScene().GetRootGameObjects(roots);
        dataGrid.DataProvider = roots;

        /*
        m_Rows.Add(new DataRow() { Data = new List<string> { "Test", "LONG STRING WITH SEPARATION OF TEST", "SHORT STRING" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Lorem Ipsum Venom Destruction All Hail Britania", "LongStringWithoutBreaksAndSpaces", "555 666 888" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "123", "432", "3412" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Test", "LONG STRING WITH SEPARATION OF TEST", "SHORT STRING" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Lorem Ipsum Venom Destruction All Hail Britania", "LongStringWithoutBreaksAndSpaces", "555 666 888" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "123", "432", "3412" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Test", "LONG STRING WITH SEPARATION OF TEST", "SHORT STRING" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Lorem Ipsum Venom Destruction All Hail Britania", "LongStringWithoutBreaksAndSpaces", "555 666 888" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "123", "432", "3412" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Test", "LONG STRING WITH SEPARATION OF TEST", "SHORT STRING" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "Lorem Ipsum Venom Destruction All Hail Britania", "LongStringWithoutBreaksAndSpaces", "555 666 888" } });
        m_Rows.Add(new DataRow() { Data = new List<string> { "123", "432", "3412" } });
        */
    }
}
