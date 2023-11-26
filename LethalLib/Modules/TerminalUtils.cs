using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLib.Modules
{
    public class TerminalUtils
    {
        /*     
        public string word;

        public bool isVerb;

        public CompatibleNoun[] compatibleNouns;

        public TerminalNode specialKeywordResult;

        [Space(5f)]
        public TerminalKeyword defaultVerb;

        [Space(3f)]
        public bool accessTerminalObjects;
        */
        public static TerminalKeyword CreateTerminalKeyword(string word, bool isVerb = false, CompatibleNoun[] compatibleNouns = null, TerminalNode specialKeywordResult = null, TerminalKeyword defaultVerb = null, bool accessTerminalObjects = false)
        {
            
            TerminalKeyword keyword = ScriptableObject.CreateInstance<TerminalKeyword>();
            keyword.name = word;
            keyword.word = word;
            keyword.isVerb = isVerb;
            keyword.compatibleNouns = compatibleNouns;
            keyword.specialKeywordResult = specialKeywordResult;
            keyword.defaultVerb = defaultVerb;
            keyword.accessTerminalObjects = accessTerminalObjects;
            return keyword;
        }
    }
}
