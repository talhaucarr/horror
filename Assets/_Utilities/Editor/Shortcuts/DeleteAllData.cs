using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BayatGames.SaveGameFree;

public class DeleteAllData : EditorWindow
{
    [MenuItem("Tools/Delete Save Datas")]
    private static void Delete()
    {
        SaveGame.DeleteAll(SaveGamePath.PersistentDataPath);
    }
}
