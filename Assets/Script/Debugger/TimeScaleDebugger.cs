using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleDebugger : MonoBehaviour
{

    GUIStyle style;
    private void Start()
    {
        style = new GUIStyle();
        style.fontSize = 40;
        style.normal.textColor = Color.blue;

    }

    public void Update()
    {
    }

    //@@@
    private void OnGUI()
    {
        GUI.Label(new Rect(Screen.width / 2*0.5f, Screen.height / 2 * 0.5f, 300, 50), "TimeScale: " + Time.timeScale, style);
        GUI.Label(new Rect(Screen.width / 2 * 0.5f, Screen.height / 2 * 0.7f, 300, 50), "FixedDelta: " + Time.fixedDeltaTime, style);
    }
}
