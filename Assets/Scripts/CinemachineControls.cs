using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineControls : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineFreeLook cinemachineFreeLookCam;

    [Header("Custom Cursor")]
    [SerializeField] private Texture2D customCursorTexture;
    private Vector2 cursorHotspot = new(10, 4);

    public CinemachineFreeLook CameraReference => cinemachineFreeLookCam;

    private void Start()
    {
        // set cursor to custom cursor
        Cursor.SetCursor(customCursorTexture, cursorHotspot, CursorMode.Auto);
        DisableCursor();
    }

    public void ToggleMouseMovement(bool disable, bool adjustCursor = true)
    {
        if (disable)
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "";

            // Stop any ongoing movement
            cinemachineFreeLookCam.m_XAxis.m_InputAxisValue = 0f;
            cinemachineFreeLookCam.m_YAxis.m_InputAxisValue = 0f;

            if (adjustCursor)
            {
                EnableCursor();
            }
        }
        else
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "Mouse X";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "Mouse Y";

            if (adjustCursor)
            {
                DisableCursor();
            }
        }
    }

    public void LoadCameraPos(float camX, float camY)
    {
        if (camX != 0 || camY != 0)
        {
            cinemachineFreeLookCam.m_XAxis.Value = camX;
            cinemachineFreeLookCam.m_YAxis.Value = camY;
        }
    }

    public void DisableCam()
    {
        cinemachineFreeLookCam.enabled = false;
    }

    public void EnableCam()
    {
        cinemachineFreeLookCam.enabled = true;
    }

    private void EnableCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void DisableCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
