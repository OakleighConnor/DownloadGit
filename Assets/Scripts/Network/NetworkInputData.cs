using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte jumpP1 = 1, jumpP2 = 1;
    public NetworkButtons buttonsP1, buttonsP2;
    public Vector2 directionP1, directionP2;
}

