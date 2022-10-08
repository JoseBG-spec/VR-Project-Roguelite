using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class WeaponHandToggle : MonoBehaviour
{
    [SerializeField] private InputActionReference WeaponHandToggleBtn;

    public UnityEvent OnWeaponActivate;
    public UnityEvent OnWeaponDeactivate;

    #region Input action Listeners
    private void OnEnable()
    {
        WeaponHandToggleBtn.action.performed += ActivateWeapon;
        WeaponHandToggleBtn.action.canceled += DeactivateWeapon;
    }

    private void OnDisable()
    {
        WeaponHandToggleBtn.action.performed -= ActivateWeapon;
        WeaponHandToggleBtn.action.performed -= DeactivateWeapon;
    }
    #endregion

    private void ActivateWeapon(InputAction.CallbackContext obj)
    {
        OnWeaponActivate.Invoke();
    }

    private void DeactivateWeapon(InputAction.CallbackContext obj)
    {
        Invoke("TurnOffWeapon", .1f);
    }

    private void TurnOffWeapon()
    {
        OnWeaponDeactivate.Invoke();
    }
}
