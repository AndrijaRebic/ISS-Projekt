using UnityEngine;

public class SplitScreenSwitch : MonoBehaviour
{
    public Camera cam1, cam2;
    private bool isHorizontalSplit;

    void Start()
    {

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            isHorizontalSplit = !isHorizontalSplit;
            SetSplitScreen();
        }
    }

    void SetSplitScreen()
    {
        if (isHorizontalSplit)
        {
            cam1.rect = new Rect(0, 0.5f, 1, 0.5f);
            cam2.rect = new Rect(0, 0, 1, 0.5f);
        }
        else
        {
            cam1.rect = new Rect(0, 0, 0.5f, 1);
            cam2.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
    }
}
