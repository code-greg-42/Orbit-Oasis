using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineControls : MonoBehaviour
{
    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineFreeLook cinemachineFreeLookCam;

    private void Start()
    {
        DisableCursor();
    }

    public void ToggleMouseMovement(bool disable)
    {
        if (disable)
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "";
            EnableCursor();
        }
        else
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "Mouse X";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "Mouse Y";
            DisableCursor();
        }
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
