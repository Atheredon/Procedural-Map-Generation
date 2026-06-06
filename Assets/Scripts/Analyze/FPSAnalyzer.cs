using UnityEngine;
using UnityEngine.InputSystem;

public class FPSAnalyzer : MonoBehaviour
{
    private int totalFrames = 0;
    private float totalTime = 0f;
    private float currentFps = 0f;

    private bool isTesting = false;

    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            isTesting = !isTesting;

            if (isTesting)
            {
                Debug.Log("FPS Test Started!");
                ResetTest();
            }
            else
            {
                PrintReport();
            }
        }

        if (isTesting)
        {
            float deltaTime = Time.unscaledDeltaTime;
            currentFps = 1.0f / deltaTime;

            totalFrames++;
            totalTime += deltaTime;
        }
    }

    private void OnGUI()
    {
        if (isTesting)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(20, 20, 250, 100), "FPS: " + Mathf.Round(currentFps));
        }
    }

    void ResetTest()
    {
        totalFrames = 0;
        totalTime = 0f;
    }

    void PrintReport()
    {
        float averageFps = totalFrames / totalTime;

        Debug.Log("<color=green><b>--- FPS TEST RESULT ---</b></color>");
        Debug.Log("Test Time: " + totalTime.ToString("F2") + " seconds");
        Debug.Log("Total Frame Count: " + totalFrames);
        Debug.Log("<color=yellow><b>AVARAGE FPS: " + averageFps.ToString("F1") + "</b></color>");
        Debug.Log("---------------------------");
    }
}
