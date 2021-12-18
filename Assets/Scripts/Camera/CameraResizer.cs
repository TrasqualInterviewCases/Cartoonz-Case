using UnityEngine;

public class CameraResizer : MonoBehaviour
{
    [SerializeField] Board board;
    [SerializeField] int offset;
    Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        transform.position = new Vector3((float)(board.Width - 1) / 2, (float)(board.Height - 1) / 2, transform.position.z);

        var ratio = (float)Screen.width / (float)Screen.height;
        var sizeX = (float)(board.Width / 2) + (float)offset;
        var sizeY = ((float)(board.Height / 2) + (float)offset) / ratio;
        cam.orthographicSize = (sizeX < sizeY) ? sizeY : sizeX;
    }

}
