using UnityEditor;
using UnityEngine;


[CustomPropertyDrawer(typeof(BHeader))]
public class BHeaderDrawer : DecoratorDrawer
{
    private const float k_HeaderSpacing = 10f;
    private const float xIndent = -4f;


    public override void OnGUI(Rect position)
    {
        var attr = (BHeader)attribute;

        var indentedRect = EditorGUI.IndentedRect(new Rect(position.x, position.y, position.width, position.height));

        Vector2 textSize = EditorStyles.boldLabel.CalcSize(new GUIContent(attr.Name));
        Color prevColor = GUI.color;
        Rect newRect = new Rect(indentedRect.x + xIndent, indentedRect.y + k_HeaderSpacing / 2, indentedRect.width + Mathf.Abs(xIndent * 2f), textSize.y * 1.3f);

        GUI.backgroundColor = EditorGUICustom.HighlightColor;
        GUI.Box(newRect, "");
        GUI.backgroundColor = prevColor;

        if (attr.IsTitle)
            GUI.Label(newRect, attr.Name, EditorGUICustom.CenteredBigLabel);
        else
        {
            newRect.x += 8f;
            GUI.Label(newRect, attr.Name, EditorGUICustom.MiddleLeftBoldMiniLabel);
        }

        GUI.color = prevColor;
    }

    public override float GetHeight() => base.GetHeight() + k_HeaderSpacing;
}
