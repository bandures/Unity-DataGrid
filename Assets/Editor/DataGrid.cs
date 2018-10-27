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
            ((DataGrid)ve).ItemHeight = m_ItemHeight.GetValueFromBag(bag, cc);
        }
    }

    public delegate VisualElement MakeCellDelegate(object data);

    [Serializable]
    private class Column
    {
        public string Name;
        public float Width;

        public Type DataType;
        public MakeCellDelegate MakeCell;
    }

    class RowCache
    {
        public int index;
        public VisualElement Row;
        public List<VisualElement> RowElements;
    }

    private bool m_Dirty = false;

    private StyleValue<int> m_ItemHeight;
    [SerializeField] private ScrollView m_ScrollView;
    [SerializeField] private List<Column> m_Columns = new List<Column>();
    [SerializeField] private IList m_DataProvider;

    // TODO: Keep cache of used and freed rows for reuse
    private List<RowCache> m_RowsCache = new List<RowCache>();


    public DataGrid()
    {
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

        MarkDirty();
    }

    public IList DataProvider
    {
        get { return m_DataProvider; }
        set
        {
            m_DataProvider = value;
            MarkDirty();
        }
    }

    public StyleValue<int> ItemHeight
    {
        get { return m_ItemHeight; }
        set
        {
            m_ItemHeight = value;
            MarkDirty();
        }
    }

    public int AddColumn(string name, float width, Type dataType, MakeCellDelegate MakeCell, int afterColumn = -1)
    {
        if ((afterColumn != -1) && (afterColumn >= m_Columns.Count))
        {
            Debug.LogError($"Invalid parameter afterColumn = {afterColumn}");
            return -1;
        }

        MarkDirty();

        var data = new Column() { Name = name, Width = width, DataType = dataType, MakeCell = MakeCell };
        if (afterColumn == -1)
        {
            m_Columns.Add(data);
            return m_Columns.Count - 1;
        }
        else
        {
            m_Columns.Insert(afterColumn, data);
            return afterColumn + 1;
        }
    }

    public int AddTextColumn(string name, float width, Func<object, string> accessor, int afterColumn = -1)
    {
        MakeCellDelegate cellFunc = (object data) => { return new Label { text = accessor(data) }; };
        return AddColumn(name, width, typeof(string), cellFunc, afterColumn);
    }

    public int AddPropertyColumn(string name, float width, Func<object, SerializedProperty> accessor, int afterColumn = -1)
    {
        MakeCellDelegate cellFunc = (object data) => {
            var prop = accessor(data);
            var field = new PropertyField();
            field.BindProperty(prop);
            return field;
            };
        return AddColumn(name, width, typeof(SerializedProperty), cellFunc, afterColumn);
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

        // Calculate total height after all child layout have been updated and size calculated
        float totalHeight = 0;
        foreach (var row in m_RowsCache)
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

    protected void MarkDirty()
    {
        if (m_Dirty)
            return;

        m_Dirty = true;
        schedule.Execute(() => Refresh());
    }

    protected void Refresh()
    {
        m_Dirty = false;

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

        int index = 0;
        m_RowsCache = new List<RowCache>();
        foreach (var rowData in m_DataProvider)
        {
            var row = new RowCache();
            row.index = index;
            row.Row = new VisualElement();
            row.Row.AddToClassList("row");
            row.Row.style.minHeight = 16;
            row.RowElements = new List<VisualElement>();

            m_RowsCache.Add(row);
            m_ScrollView.contentContainer.Add(row.Row);

            foreach (var col in m_Columns)
            {
                var cellElem = col.MakeCell(rowData);
                cellElem.AddToClassList("cell");
                cellElem.style.width = 100;
                row.Row.Add(cellElem);
                row.RowElements.Add(cellElem);
            }

            index++;
        }
    }
}
