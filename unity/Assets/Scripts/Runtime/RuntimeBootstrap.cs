using UnityEngine;
namespace VectorTraffic3D.Runtime
{
    public static class RuntimeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Object.FindFirstObjectByType<VectorTrafficRuntime>() != null) return;
            var root=new GameObject("VectorTrafficRuntime");
            root.AddComponent<VectorTrafficRuntime>();
            Object.DontDestroyOnLoad(root);
        }
    }
}
