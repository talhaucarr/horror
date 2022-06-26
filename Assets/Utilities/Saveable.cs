using BayatGames.SaveGameFree;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Utilities
{
    public abstract class Saveable<T> where T : new()
    {
        private static T _data;
        public static T Data {
            get
            {
                if (_data == null) _data = LoadData();
                return _data;
            } 
        }

        public virtual void Save()
        {
            Debug.Log("Saved data of Type: " + typeof(T).FullName);
            SaveGame.Save<T>(typeof(T).FullName, _data, false);
        }

        public static void DeleteSave()
        {
            Debug.Log("Deleted data of Type: " + typeof(T).FullName);
            _data = new T();
            SaveGame.Delete(typeof(T).FullName);
        }

        private static T LoadData()
        {
            Debug.Log("Loading Data of Type: "+typeof(T).Name);
            T loadedData = SaveGame.Load<T>(typeof(T).FullName, default(T), false);
            if (loadedData == null) 
            {
                Debug.Log("No Data of Type: " + typeof(T).FullName + " found. Creating default Data");
                loadedData = new T(); 
            }
            return loadedData;
        }
    }
}