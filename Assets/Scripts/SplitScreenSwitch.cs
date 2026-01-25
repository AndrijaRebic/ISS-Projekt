using UnityEngine;

public class SplitScreenSwitch : MonoBehaviour
{
    public Camera cam1, cam2;
    private bool isHorizontal;

    void Start()
    {
        
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            isHorizontal = !isHorizontal;
            UpdateCameraViews();
        }
    }

    void UpdateCameraViews()
    {
        if (isHorizontal)
        {
            cam1.rect = new Rect(0, 0, 0.5f, 1);
            cam2.rect = new Rect(0.5f, 0, 0.5f, 1);
        }
        else
        {
            cam1.rect = new Rect(0, 0.5f, 1, 0.5f);
            cam2.rect = new Rect(0, 0, 1, 0.5f);
        }
    }
}
