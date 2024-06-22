using Constant;

namespace Utils
{
    public class SystemData
    {
        public static SystemData Instance;
        public SceneMode sceneMode { get; private set; }
        public float width1 { get; private set; }
        public float width2 { get; private set; }

        public void SetSceneMode(SceneMode mode)
        {
            //Debug.Log("SetSceneMode(" + mode + ")");
            sceneMode = mode;
        }

        private void SetWidth(float w1, float w2)
        {
            //Debug.Log(w1 + " : " + w2);
            width1 = w1;
            width2 = w2;
        }
    }
}
