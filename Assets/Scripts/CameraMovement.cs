using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    Vector2 prevMousePos = Vector2.zero;

    void Update()
    {
        //Panning with right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            prevMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector2 mouseDelta = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - prevMousePos;
            transform.position -= (Vector3)mouseDelta;
        }
        //Zooming with mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel") * 15000 * Time.deltaTime;
        if (scroll != 0)
        {
            float orthoSize = Camera.main.orthographicSize - scroll;
            if(orthoSize < 9f) orthoSize = 9f;
            else if(orthoSize > 500f) orthoSize = 500f;
            else
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 direction = (mouseWorldPos - (Vector2)transform.position).normalized;
                Vector2 change = direction * scroll;
                transform.position += new Vector3(change.x, change.y, 0f);
            }
            Camera.main.orthographicSize = orthoSize;
        }
    }
    public void Center()
    {
        transform.position = new Vector3(0, 0, -10);
    }
}