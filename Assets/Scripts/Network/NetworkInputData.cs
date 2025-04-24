using Fusion;
using UnityEngine;

public struct NetworkInputData : INetworkInput
{
    public const byte jumpP1 = 1, jumpP2 = 1;
    public const byte actionP1 = 1, actionP2 = 1;
    public NetworkButtons jumpButtonsP1, jumpButtonsP2, actionButtonsP1, actionButtonsP2;
    public Vector2 directionP1, directionP2;
}

