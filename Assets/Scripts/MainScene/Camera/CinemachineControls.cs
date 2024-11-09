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

    private const float baseLookSensitivityX = 200.0f;
    private const float baseLookSensitivityY = 1.8f;

    private float[] baseOrbitRadius = { 3, 7, 5 };
    private float[] farOrbitRadius = { 7, 11, 9 };

    private bool isZoomedOut = false;
    private bool isChangingOrbits = false;

    private const float orbitChangeDuration = 1.0f;
    private Coroutine orbitChangeCoroutine;

    private KeyCode altZoomInKey = KeyCode.UpArrow;
    private KeyCode altZoomOutKey = KeyCode.DownArrow;

    public CinemachineFreeLook CameraReference => cinemachineFreeLookCam;

    private void Start()
    {
        // set cursor to custom cursor
        Cursor.SetCursor(customCursorTexture, cursorHotspot, CursorMode.Auto);
        DisableCursor();

        if (cinemachineFreeLookCam != null )
        {
            cinemachineFreeLookCam.m_XAxis.m_MaxSpeed = baseLookSensitivityX * DataManager.Instance.PlayerStats.LookSensitivity;
            cinemachineFreeLookCam.m_YAxis.m_MaxSpeed = baseLookSensitivityY * DataManager.Instance.PlayerStats.LookSensitivity;
        }
    }

    private void Update()
    {
        if (!isChangingOrbits && !SpaceshipSelection.Instance.IsMenuActive)
        {
            // get inputs
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            bool altZoomInPressed = Input.GetKeyDown(altZoomInKey);
            bool altZoomOutPressed = Input.GetKeyDown(altZoomOutKey);
            bool changeZoomToIn = scrollInput > 0 || altZoomInPressed;
            bool changeZoomToOut = scrollInput < 0 || altZoomOutPressed;

            // apply inputs if applicable
            if (changeZoomToIn && isZoomedOut)
            {
                isZoomedOut = false;
                StartOrbitChange(baseOrbitRadius);
            }
            else if (changeZoomToOut && !isZoomedOut)
            {
                isZoomedOut = true;
                StartOrbitChange(farOrbitRadius);
            }
        }
    }

    private void SetOrbitRadius(float[] settings)
    {
        for (int i = 0; i < 3; i++)
        {
            cinemachineFreeLookCam.m_Orbits[i].m_Radius = settings[i];
        }
    }

    private void StartOrbitChange(float[] targetSettings)
    {
        orbitChangeCoroutine ??= StartCoroutine(OrbitChangeCoroutine(targetSettings));
    }

    private IEnumerator OrbitChangeCoroutine(float[] targetSettings)
    {
        isChangingOrbits = true;

        // get current radius values
        float[] startRadius = new float[3];
        for (int i = 0; i < 3; i++)
        {
            startRadius[i] = cinemachineFreeLookCam.m_Orbits[i].m_Radius;
        }

        // smoothly transition
        float elapsed = 0f;
        while (elapsed < orbitChangeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / orbitChangeDuration);

            // loop through and change each radius
            for (int i = 0; i < 3; i++)
            {
                cinemachineFreeLookCam.m_Orbits[i].m_Radius = Mathf.Lerp(startRadius[i], targetSettings[i], t);
            }

            yield return null;
        }

        // finalize change
        SetOrbitRadius(targetSettings);

        isChangingOrbits = false;
        orbitChangeCoroutine = null;
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
