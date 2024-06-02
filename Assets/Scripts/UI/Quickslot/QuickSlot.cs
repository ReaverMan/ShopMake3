using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSlot : MonoBehaviour
{
    [SerializeField]
    private Player owner;
    public Player Owner => owner;

    private PlayerMove UIinputActions;

    public Action<Equipment> onWeaponChange;
    public Action<Equipment> onGranadeChange;
    public Action<Equipment> onETCChange;

    private void Awake()
    {
        UIinputActions = new PlayerMove();
    }

    private void OnEnable()
    {
        EnableInputActions();
    }

    private void OnDisable()
    {
        DisableInputActions();
    }

    private void EnableInputActions()
    {
        UIinputActions.Player.Enable();
        UIinputActions.Player.QuickSlot1.performed += MainWeapon1;
        UIinputActions.Player.QuickSlot2.performed += ThrowWeapon;
        UIinputActions.Player.QuickSlot3.performed += ETCSlot;
    }

    private void DisableInputActions()
    {
        UIinputActions.Player.QuickSlot1.performed -= MainWeapon1;
        UIinputActions.Player.QuickSlot2.performed -= ThrowWeapon;
        UIinputActions.Player.QuickSlot3.performed -= ETCSlot;
        UIinputActions.Player.Disable();
    }

    private void MainWeapon1(InputAction.CallbackContext context)
    {
        onWeaponChange?.Invoke(Equipment.Gun);
    }

    private void ThrowWeapon(InputAction.CallbackContext context)
    {
        onGranadeChange?.Invoke(Equipment.Throw);
    }

    private void ETCSlot(InputAction.CallbackContext context)
    {
        onETCChange?.Invoke(Equipment.ETC);
    }

    public void SetOwner(Player newOwner)
    {
        owner = newOwner;
    }
}
