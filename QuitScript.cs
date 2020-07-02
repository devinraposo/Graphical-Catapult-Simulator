using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitScript : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) Quit();
    }
    public void Quit()
    {
#if (!UNITY_WEBGL && !UNITY_EDITOR)
        Application.Quit();
#endif
    }
}
