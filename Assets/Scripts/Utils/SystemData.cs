using UnityEngine;
using Constant;

namespace Utils
{
    public class SystemData
    {
        public static SystemData Instance;
        public SceneMode sceneMode { get; private set; }

        public void SetSceneMode(SceneMode mode)
        {
            //Debug.Log("SetSceneMode(" + mode + ")");
            sceneMode = mode;
        }
    }
}
