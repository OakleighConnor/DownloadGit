using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using JetBrains.Annotations;
using Player;
using UnityEngine.SceneManagement;
using System;
public class NicknameManager : NetworkBehaviour, IPlayerLeft
{
    public static NicknameManager local { get; set; }

    PlayerScoreManager psm;

    [Networked, OnChangedRender(nameof(OnNickNameChanged))] 
    public NetworkString<_16> nickname { get; set; }

    [Header("UI")]
    public TextMeshProUGUI playerNicknameTM;
    public Canvas canvas;
    public Transform textPos;

    void Awake()
    {
        psm = GetComponent<PlayerScoreManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.name = nickname.ToString();
        playerNicknameTM.text = nickname.ToString();
        canvas.gameObject.transform.parent = null;

        psm.nickname = nickname.ToString();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            local = this;

            string nickname = PlayerPrefs.GetString("PlayerNickname");
            if(GetComponent<PlayerScript>().localPlayer)
            {
                nickname += " • 2";
            }

            RPC_SetNickName(nickname);

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

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnSceneChanged(Scene current, Scene next)
    {
        if(SceneManager.GetActiveScene().name == "Lobby" || SceneManager.GetActiveScene().name == "Level")
        {

        }
        else
        {
            Destroy(canvas);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player == Object.InputAuthority)
        {
            Destroy(canvas);
        }
    }

    public void OnNickNameChanged()
    {
        Debug.Log($"Nickname changed for player to {nickname} for player {gameObject.name}");

        gameObject.name = nickname.ToString();
        playerNicknameTM.text = nickname.ToString();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickName(string nickname, RpcInfo info = default)
    {
        Debug.Log($"[RPC] SetNickName {nickname}");
        this.nickname = nickname;
    }
}
