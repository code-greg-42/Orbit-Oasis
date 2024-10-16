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

    private void Start()
    {
        // set cursor to custom cursor
        Cursor.SetCursor(customCursorTexture, cursorHotspot, CursorMode.Auto);
        DisableCursor();
    }

    public void ToggleMouseMovement(bool disable)
    {
        if (disable)
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "";

            // Stop any ongoing movement
            cinemachineFreeLookCam.m_XAxis.m_InputAxisValue = 0f;
            cinemachineFreeLookCam.m_YAxis.m_InputAxisValue = 0f;

            EnableCursor();
        }
        else
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "Mouse X";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "Mouse Y";
            DisableCursor();
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
