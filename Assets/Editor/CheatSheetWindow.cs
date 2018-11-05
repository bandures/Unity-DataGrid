using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleEnums;
using System.Collections.Generic;

public class CheatSheetWindow : EditorWindow
{
    [MenuItem("UI/Cheat Sheet _%#C")]
    public static void ShowWindow()
    {
        var window = GetWindow<CheatSheetWindow>();
        window.titleContent = new GUIContent("Cheat Sheet");
        window.minSize = new Vector2(450, 200);
    }

    public void OnEnable()
    {
        var root = this.GetRootVisualContainer();
        root.style.flexDirection = FlexDirection.Row;

        var template = Resources.Load<VisualTreeAsset>("cheat-sheet");
        var cheatSheet = template.CloneTree(null);
        cheatSheet.AddStyleSheetPath("cheat-sheet-style");
        cheatSheet.style.flexGrow = 1;
        cheatSheet.style.flexBasis = 1;
        cheatSheet.style.marginLeft = 10;
        cheatSheet.style.marginRight = 10;
        cheatSheet.style.marginTop = 10;
        cheatSheet.style.marginBottom = 10;
        root.Add(cheatSheet);
    }
}
