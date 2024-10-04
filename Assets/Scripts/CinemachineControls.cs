using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineControls : MonoBehaviour
{
    public static CinemachineControls Instance;

    [Header("Cinemachine Camera")]
    [SerializeField] private CinemachineFreeLook cinemachineFreeLookCam;

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleMouseMovement(bool disable)
    {
        if (disable)
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "";
        }
        else
        {
            cinemachineFreeLookCam.m_XAxis.m_InputAxisName = "Mouse X";
            cinemachineFreeLookCam.m_YAxis.m_InputAxisName = "Mouse Y";
        }
    }
}
