using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
using System.Collections.Generic;
using System.Collections;
using System;

public class DataGrid : VisualElement
{
    public new class UxmlFactory : UxmlFactory<DataGrid, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        UxmlIntAttributeDescription m_ItemHeight = new UxmlIntAttributeDescription { name = "item-height", defaultValue = 100 };

        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
            ((DataGrid)ve).itemHeight = m_ItemHeight.GetValueFromBag(bag, cc);
        }
    }


    private StyleValue<int> m_ItemHeight;
    public StyleValue<int> itemHeight
    {
        get { return m_ItemHeight; }
        set
        {
            m_ItemHeight = value;
            //Refresh();
        }
    }

    [SerializeField]
    private float m_ScrollOffset;

    private ScrollView m_ScrollView;

    public DataGrid()
    {
        m_ScrollOffset = 0.0f;

        AddStyleSheetPath("datagrid-style");
        AddToClassList("datagrid");

        m_ScrollView = new ScrollView();
        m_ScrollView.StretchToParentSize();
        m_ScrollView.stretchContentWidth = true;
        m_ScrollView.horizontalScroller.valueChanged += OnScroll;
        m_ScrollView.verticalScroller.valueChanged += OnScroll;
        shadow.Add(m_ScrollView);

        // Parent size changed, need update ScrollView area
        RegisterCallback<GeometryChangedEvent>(OnSizeChanged);

        m_ScrollView.contentContainer.AddToClassList("content");
        m_ScrollView.contentContainer.RegisterCallback<MouseDownEvent>(OnClick);
        m_ScrollView.contentContainer.RegisterCallback<KeyDownEvent>(OnKeyDown);
        // Content content changed, need update ScrollView area
        m_ScrollView.contentContainer.RegisterCallback<GeometryChangedEvent>(OnPostLayout);

        AddTextColumn("Test 1", 100);
        AddTextColumn("Test 2", 10);
        AddTextColumn("Test 3", 50);

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
        Refresh();
    }

    class Column
    {
        public string Header;
        public float MinWidth;
    }

    class DataRow
    {
        public IList Data;
        public VisualElement Row;
    }

    private List<Column> m_Columns = new List<Column>();
    private List<DataRow> m_Rows = new List<DataRow>();

    public void AddTextColumn(string header, float minWidth)
    {
        m_Columns.Add(new Column() { Header = header, MinWidth = minWidth });
        //Refresh();
    }

    /// 
    /// Input
    /// 
    public void OnKeyDown(KeyDownEvent evt)
    {
        Debug.Log("KeyDown!");
    }
    private void OnClick(MouseDownEvent evt)
    {
        Debug.Log("OnClick!");
    }
    private void OnScroll(float offset)
    {
        Debug.Log("OnScroll!");
    }

    private void OnSizeChanged(GeometryChangedEvent evt)
    {
        if (evt.newRect.height == evt.oldRect.height)
            return;

        Debug.Log("OnSizeChanged!");
        //ResizeHeight(evt.newRect.height);
    }

    private void OnPostLayout(GeometryChangedEvent evt)
    {
        //if (evt.newRect.height == evt.oldRect.height)
            //return;

        float totalHeight = 0;
        foreach (var row in m_Rows)
        {
            totalHeight += row.Row.layout.height;
        }

        m_ScrollView.contentContainer.style.height = Math.Max(totalHeight, layout.height);
    }

    protected override void OnStyleResolved(ICustomStyle elementStyle)
    {
        base.OnStyleResolved(elementStyle);
        elementStyle.ApplyCustomProperty("-unity-item-height", ref m_ItemHeight);
    }

    protected void Refresh()
    {
        m_ScrollView.contentContainer.style.height = 100;

        float position = 0;
        foreach (var col in m_Columns)
        {
            var elem = new VisualElement();
            elem.AddToClassList("col");
            elem.style.width = 100;
            elem.style.positionLeft = position;
            m_ScrollView.contentContainer.Add(elem);

            position += 100;
        }

        foreach (var row in m_Rows)
        {
            row.Row = new VisualElement();
            row.Row.AddToClassList("row");
            row.Row.style.minHeight = 16;
            m_ScrollView.contentContainer.Add(row.Row);

            foreach (var cell in row.Data)
            {
                var cellElem = new Label();
                cellElem.text = cell as string;
                cellElem.AddToClassList("cell");
                cellElem.style.width = 100;
                row.Row.Add(cellElem);
            }
        }
    }
}
