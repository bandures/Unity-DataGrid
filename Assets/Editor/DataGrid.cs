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

    public delegate VisualElement MakeCellDelegate(object data, int column, int row);

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

    public int AddIndexColumn(string name, float width, int afterColumn = -1)
    {
        MakeCellDelegate cellFunc = (object data, int column, int row) => {
            var label = new Label { text = string.Format("{0}", row) };
            label.AddToClassList("header");
            return label;
        };

        return AddColumn(name, width, typeof(string), cellFunc, afterColumn);
    }

    public int AddTextColumn(string name, float width, Func<object, string> accessor, int afterColumn = -1)
    {
        MakeCellDelegate cellFunc = (object data, int column, int row) => {
            return new Label { text = accessor(data) };
            };

        return AddColumn(name, width, typeof(string), cellFunc, afterColumn);
    }

    public int AddPropertyColumn(string name, float width, Func<object, SerializedProperty> accessor, int afterColumn = -1)
    {
        MakeCellDelegate cellFunc = (object data, int column, int row) => {
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
    }

    private void OnPostLayout(GeometryChangedEvent evt)
    {
        //if (evt.newRect.height == evt.oldRect.height)
            //return;

        // Calculate total height after all child layout have been updated and size calculated
        float totalHeight = 0;
        foreach (var row in m_RowsCache)
            totalHeight += row.Row.layout.height;

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

    /// 
    /// Content layout generation
    /// 
    protected void Refresh()
    {
        m_Dirty = false;

        var header = new RowCache();
        header.index = 0;
        header.Row = CreateRowElement(m_ScrollView.contentContainer, 16);
        header.RowElements = new List<VisualElement>();
        m_RowsCache.Add(header);

        CreateRowCells(header, null, 0, (_1, column, _3) => {
            var cellElem = new Label(m_Columns[column].Name);
            cellElem.AddToClassList("cell");
            cellElem.AddToClassList("header");
            return cellElem;
        });

        int index = 1;
        m_RowsCache = new List<RowCache>();
        foreach (var rowData in m_DataProvider)
        {
            var row = new RowCache();
            row.index = index;
            row.Row = CreateRowElement(m_ScrollView.contentContainer, 16);
            row.RowElements = new List<VisualElement>();
            m_RowsCache.Add(row);

            CreateRowCells(row, rowData, index);

            index++;
        }
    }

    private void CreateRowCells(RowCache row, object data, int rowIndex, MakeCellDelegate makeCellOverride = null)
    {
        // Cells use Flex because we know cell width, but we don't know cell height.
        // We let system calculate it, but it won't work with absolute positions

        int colIndex = 0;
        foreach (var col in m_Columns)
        {
            var makeCell = makeCellOverride == null ? col.MakeCell : makeCellOverride;

            var cellElem = makeCell(data, colIndex, rowIndex);
            cellElem.AddToClassList("cell");
            cellElem.style.width = col.Width;

            row.Row.Add(cellElem);
            row.RowElements.Add(cellElem);

            var grid = new VisualElement();
            grid.AddToClassList("grid");
            grid.style.width = 1;

            row.Row.Add(grid);

            colIndex++;
        }
    }

    private VisualElement CreateColumnElement(VisualElement container, float position, float width)
    {
        var elem = new VisualElement();
        elem.AddToClassList("col");
        elem.style.width = width;
        elem.style.positionLeft = position;
        container.Add(elem);
        return elem;
    }

    private VisualElement CreateRowElement(VisualElement container, float minHeight)
    {
        var elem = new VisualElement();
        elem.AddToClassList("row");
        elem.style.minHeight = minHeight;
        container.Add(elem);
        return elem;
    }
}