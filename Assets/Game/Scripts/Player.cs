using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    public override void OnStartServer()
    {
        gameObject.name = $"Player_{netIdentity.netId}";
    }
}