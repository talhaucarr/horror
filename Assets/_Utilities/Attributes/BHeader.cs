using UnityEngine;


    public class BHeader : PropertyAttribute
    {
        public string Name { get; private set; }

        public bool IsTitle { get; private set; } 


        public BHeader(string name, bool isTitle)
        {
            Name = name;
            IsTitle = isTitle;
        }

        public BHeader(string name)
        {
            Name = name;
            IsTitle = true;
        }
    }
