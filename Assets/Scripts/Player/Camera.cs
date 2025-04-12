using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : NetworkBehaviour
{
    public Transform playerCameraPos;

    public int lowerCameraBound= 5;
    public int upperCameraBound = 15;

    public override void Spawned()
    {
        if(HasInputAuthority) return;

        Debug.Log("Disabling Camera");
        Camera camera = GetComponent<Camera>();
        camera.enabled = false;
    }

    void LateUpdate()
    {
        LevelCamera();
    }

    public void AssignTargetPlayer(Transform targetCameraPos)
    {
        if(playerCameraPos)
        {
            Debug.LogWarning("PlayerCameraPos already assigned. Have you attempted to assign a player to a camera with an already existing assignment?");
        }
        else
        {
            playerCameraPos = targetCameraPos;
            Debug.Log($"PlayerCameraPos successfully assigned to: {targetCameraPos}");
        }
    }

    void LevelCamera()
    {
        if(playerCameraPos == null) return;

        // Define the positions of the camera as the current positions
        float playerCameraPosX = transform.position.x;
        float playerCameraPosY = transform.position.y;
        float playerCameraPosZ = transform.position.z;

        // Change the values of the positions depending on possible limitations such as reaching the bottom of the screen
        /*if(playerCameraPos.position.y >= lowerCameraBound && playerCameraPos.position.y <= upperCameraBound)
        {
            playerCameraPosY = playerCameraPos.position.y;
        }*/
        playerCameraPosX = playerCameraPos.position.x;
        playerCameraPosY = playerCameraPos.position.y;
        playerCameraPosZ = playerCameraPos.position.z;


        // Apply the position of the camera 
        transform.position = new Vector3(playerCameraPosX, playerCameraPosY, playerCameraPosZ - 15);
    }
}
