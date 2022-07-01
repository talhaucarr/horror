﻿using System.Collections.Generic;
using UnityEngine;

namespace razz
{
    //Holds prefab lists for InteractorTargetSpawner in ScriptableObject assets
    //TODO Effector parameters also can be recorded.
    [SerializeField]
    public class SaveData : ScriptableObject
    {
        [SerializeField] public List<GameObject> tabPrefabs;
        [SerializeField] public int[] listPointer;
        [SerializeField] public bool excludePlayerMask;
        [SerializeField] public int surfaceRotation;
        [SerializeField] public bool addComponentsOnParent;
        [SerializeField] public long rightClickTimer = 100;
    }
}
