using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform playerCameraPos;

    void LateUpdate()
    {
        if(playerCameraPos != null)
        {
            transform.position = new Vector3(playerCameraPos.position.x, playerCameraPos.position.y, playerCameraPos.position.z - 15);
        }
    }
}

