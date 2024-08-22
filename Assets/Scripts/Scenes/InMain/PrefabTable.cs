using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Constant;

namespace Scenes.InMain
{
    [CreateAssetMenu]
    public class PrefabTable : ScriptableObject
    {
        public List<GameObject> prefabs = new List<GameObject>();
        public List<int> student = new List<int>();
        public List<int> people = new List<int>();
        public List<int> addition = new List<int>();
    }
}
