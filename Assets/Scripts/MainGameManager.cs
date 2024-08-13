using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    public static MainGameManager Instance;

    // centralized references
    [SerializeField] CinemachineFreeLook cinemachineCam;
    [SerializeField] PlayerMovement playerMovement;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        playerMovement.LoadPlayerPosition();
        LoadCameraPos();
    }

    public void SetPlayerAndCameraPos()
    {
        DataManager.Instance.SetPlayerPosition(playerMovement.PlayerPosition, playerMovement.PlayerRotation);
        DataManager.Instance.SetCameraValues(cinemachineCam.m_XAxis.Value, cinemachineCam.m_YAxis.Value);
    }

    private void LoadCameraPos()
    {
        float camX = DataManager.Instance.PlayerStats.CameraX;
        float camY = DataManager.Instance.PlayerStats.CameraY;

        if (camX != 0 || camY != 0)
        {
            cinemachineCam.m_XAxis.Value = camX;
            cinemachineCam.m_YAxis.Value = camY;
        }
    }
}
