using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using JetBrains.Annotations;
public class NicknameManager : NetworkBehaviour, IPlayerLeft
{
    private ChangeDetector changeDetector;
    public static NicknameManager local { get; set; }

    [Networked, OnChangedRender (nameof(OnNickNameChanged))] 
    public NetworkString<_16> nickname { get; set; }

    [Header("UI")]
    public TextMeshProUGUI playerNicknameTM;
    public Canvas canvas;
    public Transform textPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.name = nickname.ToString();
        playerNicknameTM.text = nickname.ToString();
        canvas.gameObject.transform.parent = null;
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            local = this;

            RPC_SetNickName(PlayerPrefs.GetString("PlayerNickname"));

            Debug.Log("Spawned local player");
        }
        else
        {
            Debug.Log("Spawned remote player");
        }

        canvas.gameObject.transform.parent = null;
    }

    void LateUpdate()
    {
        canvas.transform.position = textPos.position;
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        {
            Runner.Despawn(Object);
        }
    }

    public void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickname} for player {gameObject.name}");

        gameObject.name = nickname.ToString();
        playerNicknameTM.text = nickname.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SetNickName(string nickname, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickname}");
        this.nickname = nickname;
    }
}
