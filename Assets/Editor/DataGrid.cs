using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using UnityEngine.Experimental.UIElements.StyleSheets;
using System.Linq;

public partial class DataGrid : VisualElement
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

    private class TableCell
    {
        public object Data;
        public Vector2Int Position;
    }

    private class TableColumn
    {
        public string Name;
        public float Width;

        public Type DataType;
        public MakeCellDelegate MakeCell;

        public List<VisualElement> Elements;
    }

    private class TableRow
    {
        public int index;
        public VisualElement Row;

        public List<VisualElement> Elements;
    }

    private bool m_Dirty = false;

    private StyleValue<int> m_ItemHeight;
    [SerializeField] private IList m_DataProvider;
    [SerializeField] private Vector2Int m_SelectionPos = new Vector2Int(0, 0);

    // TODO: Keep cache of used and freed rows for reuse
    private VisualElement m_Header;
    private ScrollView m_ScrollView;
    private VisualElement m_Selection;
    private List<TableRow> m_Rows = new List<TableRow>();
    private List<TableColumn> m_Columns = new List<TableColumn>();


    public DataGrid()
    {
        AddStyleSheetPath("datagrid-style");
        AddToClassList("datagrid");

        var template = Resources.Load<VisualTreeAsset>("datagrid-template");
        template.CloneTree(this, null);

        m_Header = this.Q("header");
        m_ScrollView = this.Q<ScrollView>("content");
        m_Selection = this.Q("selection");

        m_ScrollView.verticalScroller.valueChanged += OnScroll;
        m_ScrollView.horizontalScroller.valueChanged += OnScroll;
        m_ScrollView.RegisterCallback<KeyDownEvent>(OnKeyDown);

        m_Selection.visible = false;
        m_Selection.RemoveFromHierarchy();

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

        var data = new TableColumn() { Name = name, Width = width, DataType = dataType, MakeCell = MakeCell, Elements = new List<VisualElement>() };
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
            return CreatePropertyCell(prop);
            };

        return AddColumn(name, width, typeof(SerializedProperty), cellFunc, afterColumn);
    }

    private VisualElement CreatePropertyCell(SerializedProperty property)
    {
        var propertyType = property.propertyType;

        switch (propertyType)
        {
            case SerializedPropertyType.Integer:
                return BindPropertyCell(new IntegerField(), property);
            case SerializedPropertyType.Boolean:
                return BindPropertyCell(new Toggle(), property);
            case SerializedPropertyType.Float:
                return BindPropertyCell(new FloatField(), property);
            case SerializedPropertyType.String:
                return BindPropertyCell(new TextField(), property);
            case SerializedPropertyType.Color:
                return BindPropertyCell(new ColorField(), property);
            case SerializedPropertyType.LayerMask:
                return BindPropertyCell(new LayerMaskField(), property);
            case SerializedPropertyType.Enum:
                {
                    var field = new PopupField<string>(property.enumDisplayNames.ToList(), property.enumValueIndex);
                    field.index = property.enumValueIndex;
                    return BindPropertyCell(field, property);
                }
            case SerializedPropertyType.Vector2:
                return BindPropertyCell(new Vector2Field(), property);
            case SerializedPropertyType.Vector3:
                return BindPropertyCell(new Vector3Field(), property);
            case SerializedPropertyType.Vector4:
                return BindPropertyCell(new Vector4Field(), property);
            case SerializedPropertyType.Rect:
                return BindPropertyCell(new RectField(), property);
            case SerializedPropertyType.Character:
                {
                    var field = new TextField();
                    field.maxLength = 1;
                    return BindPropertyCell(field, property);
                }
            case SerializedPropertyType.AnimationCurve:
                return BindPropertyCell(new CurveField(), property);
            case SerializedPropertyType.Bounds:
                return BindPropertyCell(new BoundsField(), property);
            case SerializedPropertyType.Gradient:
                return BindPropertyCell(new GradientField(), property);
            case SerializedPropertyType.Quaternion:
                return null;
            case SerializedPropertyType.ExposedReference:
                return null;
            case SerializedPropertyType.FixedBufferSize:
                return null;
            case SerializedPropertyType.Vector2Int:
                return BindPropertyCell(new Vector2IntField(), property);
            case SerializedPropertyType.Vector3Int:
                return BindPropertyCell(new Vector3IntField(), property);
            case SerializedPropertyType.RectInt:
                return BindPropertyCell(new RectIntField(), property);
            case SerializedPropertyType.BoundsInt:
                return BindPropertyCell(new BoundsIntField(), property);
            case SerializedPropertyType.Generic:
            default:
                return null;
        }
    }

    private VisualElement BindPropertyCell<T>(T input, SerializedProperty property) where T : VisualElement, IBindable
    {
        input.name = "Input:" + property.propertyPath;
        input.BindProperty(property);
        return input;
    }

    /// 
    /// Input
    /// 
    public void OnKeyDown(KeyDownEvent evt)
    {
        //Debug.Log("OnKeyDown!");

        switch (evt.keyCode)
        {
            case KeyCode.LeftArrow:
                if (m_SelectionPos.x > 0)
                    m_SelectionPos.x -= 1;
                break;
            case KeyCode.RightArrow:
                if (m_SelectionPos.x + 1 < m_Columns.Count)
                    m_SelectionPos.x += 1;
                break;
            case KeyCode.UpArrow:
                if (m_SelectionPos.y > 0)
                    m_SelectionPos.y -= 1;
                break;
            case KeyCode.DownArrow:
                if (m_SelectionPos.y + 1 < m_DataProvider.Count)
                    m_SelectionPos.y += 1;
                break;
            case KeyCode.Home:
                m_SelectionPos.x = 0;
                break;
            case KeyCode.End:
                m_SelectionPos.x = m_Columns.Count - 1;
                break;
        }

        RefreshSelection();
    }

    private bool IsTableCell(IEventHandler input)
    {
        var target = input as VisualElement;
        if (target == null)
            return false;

        if (!target.ClassListContains("cell"))
            return false;

        var cellData = target.userData as TableCell;
        if (cellData == null)
            return false;

        if ((cellData.Position.x == 0) || (cellData.Position.y == 0))
            return false;

        return true;
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (!IsTableCell(evt.currentTarget))
            return;

        var clickPos = ((evt.currentTarget as VisualElement).userData as TableCell).Position;  // CRAP
        if (m_SelectionPos != clickPos)
        {
            m_SelectionPos = clickPos;
            RefreshSelection();
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void OnMouseDown(MouseDownEvent evt)
    {
        if (!IsTableCell(evt.currentTarget))
            return;

        var clickPos = ((evt.currentTarget as VisualElement).userData as TableCell).Position;  // CRAP
        if (m_SelectionPos != clickPos)
        {
            evt.StopPropagation();
            evt.PreventDefault();
        }
    }

    private void OnScroll(float offset)
    {
        //Debug.Log("OnScroll!");
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
        if (m_DataProvider == null)
            return;

        // clear old state
        m_Dirty = false;
        m_Rows = new List<TableRow>();
        m_ScrollView.contentContainer.Clear();

        // create header
        TableRow header = new TableRow();
        header.index = 0;
        header.Row = CreateRowElement(m_Header, 16);
        header.Elements = new List<VisualElement>();
        m_Rows.Add(header);

        CreateRowCells(m_Rows[0], null, 0, (_1, column, _3) => {
            var cellElem = new Label(m_Columns[column].Name);
            cellElem.AddToClassList("header");

            var resizeControl = new VisualElement();
            resizeControl.AddToClassList("resize");
            resizeControl.AddManipulator(new ResizeManipulator((float delta) => {
                m_Columns[column].Width += delta;
                foreach(var elem in m_Columns[column].Elements)
                    elem.style.width = m_Columns[column].Width;
                } ));
            cellElem.Add(resizeControl);
            return cellElem;
        });

        // create rows
        int index = 1;
        foreach (var rowData in m_DataProvider)
        {
            var row = new TableRow();
            row.index = index;
            row.Row = CreateRowElement(m_ScrollView.contentContainer, 16);
            row.Elements = new List<VisualElement>();
            m_Rows.Add(row);

            CreateRowCells(row, rowData, index);

            index++;
        }
    }

    private void RefreshSelection()
    {
        if (m_DataProvider == null)
            return;
        if ((m_SelectionPos.x >= m_Columns.Count) || (m_SelectionPos.y >= m_Rows.Count))
            return;

        if (m_Selection.parent != null)
            m_Selection.parent.Remove(m_Selection);

        m_Selection.style.positionType = PositionType.Absolute;
        m_Selection.style.positionLeft = 0;
        m_Selection.style.positionRight = 0;
        m_Selection.style.positionTop = 0;
        m_Selection.style.positionBottom = 0;
        m_Selection.pickingMode = PickingMode.Ignore;
        m_Selection.visible = true;

        var refElem = m_Rows[m_SelectionPos.y].Elements[m_SelectionPos.x];
        refElem.Add(m_Selection);
    }

    private void CreateRowCells(TableRow row, object data, int rowIndex, MakeCellDelegate makeCellOverride = null)
    {
        // Cells use Flex because we know cell width, but we don't know cell height.
        // We let system calculate it, but it won't work with absolute positions

        int colIndex = 0;
        foreach (var col in m_Columns)
        {
            var makeCell = makeCellOverride == null ? col.MakeCell : makeCellOverride;

            var cellContent = makeCell(data, colIndex, rowIndex);
            cellContent.AddToClassList("cellcontent");

            var cellFrame = new VisualElement();
            cellFrame.name = string.Format("cell {0}-{1}", colIndex, rowIndex);
            cellFrame.AddToClassList("cell");
            cellFrame.style.width = col.Width;
            cellFrame.userData = new TableCell() { Data = data, Position = new Vector2Int(colIndex, rowIndex)};
            cellFrame.RegisterCallback<MouseUpEvent>(OnMouseUp, TrickleDown.TrickleDown);
            cellFrame.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
            cellFrame.Add(cellContent);

            row.Row.Add(cellFrame);
            row.Elements.Add(cellFrame);
            col.Elements.Add(cellFrame);

            var grid = new VisualElement();
            grid.AddToClassList("grid");
            grid.style.width = 1;

            row.Row.Add(grid);

            colIndex++;
        }

        var expand = new VisualElement();
        expand.AddToClassList("cell");
        expand.AddToClassList("expand");
        expand.style.width = 1;
        row.Row.Add(expand);
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
