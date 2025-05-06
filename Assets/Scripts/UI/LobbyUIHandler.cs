using UnityEngine;
using Fusion;

public class LobbyUIHandler : NetworkBehaviour
{
    GameObject settingsButtons;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Spawned()
    {
        if(!Object.HasStateAuthority)
        {
            ToggleSettings(false);
        }
    }

    public void ToggleSettings(bool state) // Toggles all UI elements that change settings
    {
        settingsButtons.SetActive(state);
    }

    public void SetWinRequirement() // Sets the number of disks the players must collect to win
    {

    }
}
