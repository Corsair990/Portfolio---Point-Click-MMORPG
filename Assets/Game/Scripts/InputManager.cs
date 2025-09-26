using UnityEngine;
using Mirror;
using System;
using System.Collections.Generic;

[Flags]
public enum PlayerCommand : ushort
{ 
    leftMouseClicked    = (1 << 1),
    rightMouseClicked  = (1 << 2),
    inventoryButtonPressed  = (1 << 3),
    interactButtonPressed = (1 << 4)
}

public class InputManager : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;

    Queue<ushort> playerCommands = new Queue<ushort>();

    Vector2 clickedPosition;

    bool leftMouseClicked;
    bool rightMouseClicked;
    bool inventoryButtonPressed;
    bool interactButtonPressed;

    private void Update()
    {
        if (isOwned)
        { 
            CollectInput();
            PackInput();
        }
    }

    private void FixedUpdate()
    {
        if (isOwned) 
        {
            SendPlayerCommands(playerCommands.ToArray());
            playerCommands.Clear();
        }
    }

    private void CollectInput()
    {
        leftMouseClicked = Input.GetMouseButtonDown(0);
        rightMouseClicked = Input.GetMouseButtonDown(1);
        inventoryButtonPressed = Input.GetKeyDown(KeyCode.I);
        interactButtonPressed = Input.GetKeyDown(KeyCode.E);
    }

    private void PackInput()
    {
        ushort packedInputs = 0;
        packedInputs |= (ushort)PlayerCommand.leftMouseClicked;
        packedInputs |= (ushort)PlayerCommand.rightMouseClicked;
        packedInputs |= (ushort)PlayerCommand.inventoryButtonPressed;
        packedInputs |= (ushort)PlayerCommand.interactButtonPressed;

        if (packedInputs > 0)
        {
            playerCommands.Enqueue(packedInputs);
        }
    }

    [Command]
    private void SendPlayerCommands(ushort[] _playerCommands)
    {
        foreach (ushort packedInput in _playerCommands)
        {
            if ((packedInput & (ushort)PlayerCommand.leftMouseClicked) != 0)
            {
                //characterController.GetClickedPosition(Input.mousePosition);
                Debug.Log("Left mouse button was clicked.");
            }

            if ((packedInput & (ushort)PlayerCommand.rightMouseClicked) != 0)
            {
                Debug.Log("Right mouse button was clicked.");
            }
        }
    }
}