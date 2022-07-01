using UnityEngine;


    public class GroupAttribute : PropertyAttribute
    {
        public string PrefixText { get; private set; } = "";
        public bool OnlyShowPrefixIfNotExpanded { get; private set; } = true;


        public GroupAttribute()
        {
            PrefixText = "";
            OnlyShowPrefixIfNotExpanded = false;
        }

        public GroupAttribute(string appendText, bool showPrefixOnlyWhenNotExpanded = true)
        {
            PrefixText = appendText;
            OnlyShowPrefixIfNotExpanded = showPrefixOnlyWhenNotExpanded;
        }
    }
