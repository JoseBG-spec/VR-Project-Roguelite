using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System;

public class TeleportToggle : MonoBehaviour
{
    [SerializeField] private InputActionReference teleportToggleBtn;

    public UnityEvent OnTeleportActivate;
    public UnityEvent OnTeleportDeactivate;

    #region Input action Listeners
    private void OnEnable()
    {
        teleportToggleBtn.action.performed += ActivateTeleport;
        teleportToggleBtn.action.canceled += DeactivateTeleport;
    }

    private void OnDisable()
    {
        teleportToggleBtn.action.performed -= ActivateTeleport;
        teleportToggleBtn.action.performed -= DeactivateTeleport;
    }
    #endregion

    private void ActivateTeleport(InputAction.CallbackContext obj)
    {
        OnTeleportActivate.Invoke();
    }

    private void DeactivateTeleport(InputAction.CallbackContext obj)
    {
        Invoke("TurnOffTeleport", .1f);
    }

    private void TurnOffTeleport()
    {
        OnTeleportDeactivate.Invoke();
    }
}
