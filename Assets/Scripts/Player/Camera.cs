using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform playerCameraPos;

    void LateUpdate()
    {
        LevelCamera();
    }

    void LevelCamera()
    {
        if(playerCameraPos == null) return;

        // Define the positions of the camera as the current positions
        float playerCameraPosX = transform.position.x;
        float playerCameraPosY = transform.position.y;
        float playerCameraPosZ = transform.position.z;
        
        playerCameraPosX = playerCameraPos.position.x;
        playerCameraPosY = playerCameraPos.position.y;
        playerCameraPosZ = playerCameraPos.position.z;

        // Apply the position of the camera 
        transform.position = new Vector3(playerCameraPosX, playerCameraPosY, playerCameraPosZ - 15);
    }
}

