using UnityEngine;
namespace VectorTraffic3D.Runtime
{
    public sealed class RuntimeInputNotice : MonoBehaviour
    {
        private GUIStyle _style;
        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.label) { fontSize = 22, alignment = TextAnchor.UpperCenter };
            GUI.Label(new Rect(20,20,Screen.width-40,60),"Tap a vehicle to move its vector • Swipe to turn at junctions",_style);
        }
    }
}
