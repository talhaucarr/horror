using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using SkiaSharp;
using System.Diagnostics;

public class CrunchFitter : EditorWindow
{
    private enum EditMode { Magnitude, Width, Height}

    private static EditMode mode;

    [MenuItem("Assets/Compress For Crunch/Compress To Size Of 4", false, 100)]
    public static void FitCrunch4()
    {
        mode = EditMode.Magnitude;
        EditorWindow window = EditorWindow.CreateInstance<CrunchFitter>();
        window.ShowPopup();
    }

    [MenuItem("Assets/Compress For Crunch/Compress To Size Of 4", true, 100)]
    public static bool FitCrunchControl()
    {
        return IsAllSelectonsTexture2D();
    }

    [MenuItem("Assets/Compress For Crunch/Add Width", false, 100)]
    public static void AddWidth()
    {
        mode = EditMode.Width;
        EditorWindow window = EditorWindow.CreateInstance<CrunchFitter>();
        window.ShowPopup();
    }

    [MenuItem("Assets/Compress For Crunch/Add Width", true, 100)]
    public static bool AddWidthControl()
    {
        return IsAllSelectonsTexture2D();
    }

    [MenuItem("Assets/Compress For Crunch/Add Height", false, 100)]
    public static void AddHeight()
    {
        mode = EditMode.Height;
        EditorWindow window = EditorWindow.CreateInstance<CrunchFitter>();
        window.ShowPopup();
    }

    [MenuItem("Assets/Compress For Crunch/Add Height", true, 100)]
    public static bool AddHeightControl()
    {
        return IsAllSelectonsTexture2D();
    }


    private int inputInt = 4;
    private Vector2Int size = new Vector2Int(2, 2);
    private bool _movedToMouse;

    void OnGUI()
    {
        ReColorBackGround();

        var e = Event.current;

        if (!_movedToMouse)
        {
            _movedToMouse = true;
            MoveToMouse(e);
        }

        // Close the window if ESC is pressed
        CloseWindowOnEscape(e);
        switch (mode)
        {
            case EditMode.Magnitude:
                EditorGUILayout.LabelField("Choose Magnitude", EditorStyles.wordWrappedLabel);
                inputInt = EditorGUILayout.IntField(inputInt);
                GUILayout.Space(15);
                if (GUILayout.Button("Resize & Compress")) { ResizeAll(inputInt); this.Close(); }
                break;
            case EditMode.Width:
                EditorGUILayout.LabelField("Choose Magnitude", EditorStyles.wordWrappedLabel);
                size = new Vector2Int(EditorGUILayout.IntField("Size Increase: ", size.x),0);
                GUILayout.Space(15);
                if (GUILayout.Button("Resize & Compress")) { ResizeAll(size); this.Close(); }
                break;
            case EditMode.Height:
                EditorGUILayout.LabelField("Choose Magnitude", EditorStyles.wordWrappedLabel);
                size = new Vector2Int(0, EditorGUILayout.IntField("Size Increase: ", size.y));
                GUILayout.Space(15);
                if (GUILayout.Button("Resize & Compress")) { ResizeAll(size); this.Close(); }
                break;
            default:
                break;
        }
    }

    private void CloseWindowOnEscape(Event e)
    {
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Escape) Close();
    }

    private void MoveToMouse(Event e)
    {
        Vector2 mousePos = e.mousePosition;
        position = new Rect(mousePos.x, mousePos.y, 250, 150);
    }

    private void ReColorBackGround()
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.grey);
        tex.Apply();
        GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), tex, ScaleMode.StretchToFill);
    }

    private static bool IsAllSelectonsTexture2D()
    {
        if (Selection.count == 0) return false;
        foreach (Object obj in Selection.objects)
        {
            if (!(obj is Texture2D)) return false;
        }
        return true;
    }

    private static void ResizeAll(int mod)
    {
        foreach (Object obj in Selection.objects)
        {
            Resize((Texture2D) obj, mod);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static void ResizeAll(Vector2Int sizeIncrease)
    {
        foreach (Object obj in Selection.objects)
        {
            Resize((Texture2D) obj, sizeIncrease);
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    private static void Resize(Texture2D texture, int mod)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        filePath = filePath.Replace("/", "\\");

        SKImage finalImage;
        SKBitmap bitMap = SKBitmap.Decode(filePath);

        SKImageInfo newInfo = bitMap.Info;

        UnityEngine.Debug.Log(newInfo.Width + " " + newInfo.Height);

        if (newInfo.Height%mod !=0)newInfo.Height += (mod-(newInfo.Height%mod));
        if(newInfo.Width%mod != 0)newInfo.Width += (mod - (newInfo.Width % mod));

        UnityEngine.Debug.Log(newInfo.Width + " " + newInfo.Height);

        using (SKSurface surface = SKSurface.Create(new SKImageInfo(newInfo.Width, newInfo.Height)))
        {
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            // set up drawing tools
            var paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Transparent,
                StrokeCap = SKStrokeCap.Round
            };
            canvas.DrawBitmap(bitMap, newInfo.Rect);
            finalImage = surface.Snapshot();
        }
        SKData data = finalImage.Encode(SKEncodedImageFormat.Png, 100);
        using (var output = File.OpenWrite(filePath))
        {
            data.SaveTo(output);
        }

        // Run OPTIPNG with level 7 compression.
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "optipng.exe";
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.Arguments = "\"" + filePath + "\" -o7";

        // Use Process for the application.
        using (Process exe = Process.Start(info))
        {
            exe.WaitForExit();
        }
    }

    private static void Resize(Texture2D texture, Vector2Int sizeIncrease)
    {
        string assetPath = AssetDatabase.GetAssetPath(texture);
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
        filePath = filePath.Replace("/", "\\");

        SKImage finalImage;
        SKBitmap bitMap = SKBitmap.Decode(filePath);

        SKImageInfo newInfo = bitMap.Info;

        UnityEngine.Debug.Log(newInfo.Width + " " + newInfo.Height);

        if (sizeIncrease.y > 0) newInfo.Height += sizeIncrease.y;
        if (sizeIncrease.x > 0) newInfo.Width += sizeIncrease.x;

        UnityEngine.Debug.Log(newInfo.Width + " " + newInfo.Height);

        using (SKSurface surface = SKSurface.Create(new SKImageInfo(newInfo.Width, newInfo.Height)))
        {
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            // set up drawing tools
            var paint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.Transparent,
                StrokeCap = SKStrokeCap.Round
            };
            canvas.DrawBitmap(bitMap, newInfo.Rect);
            finalImage = surface.Snapshot();
        }
        SKData data = finalImage.Encode(SKEncodedImageFormat.Png, 100);
        using (var output = File.OpenWrite(filePath))
        {
            data.SaveTo(output);
        }

        // Run OPTIPNG with level 7 compression.
        ProcessStartInfo info = new ProcessStartInfo();
        info.FileName = "optipng.exe";
        info.WindowStyle = ProcessWindowStyle.Hidden;
        info.Arguments = "\"" + filePath + "\" -o7";

        // Use Process for the application.
        using (Process exe = Process.Start(info))
        {
            exe.WaitForExit();
        }
    }
}
