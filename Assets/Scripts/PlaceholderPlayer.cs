using UnityEngine;
using System.Collections;

public class PlaceholderPlayer : MonoBehaviour
{
    string horizontalAxis = "horizontal";
    string verticalAxis = "vertical";

    [SerializeField] float m_speed = 1;

    private void Start()
    {
        Event<CenterUpdatedEvent>.Broadcast(new CenterUpdatedEvent(transform.position));
    }

    void Update()
    {
        Vector3 dir = new Vector3(Input.GetAxis(horizontalAxis), Input.GetAxis(verticalAxis), 0) * Time.deltaTime * m_speed;
        var pos = transform.position;
        pos += dir;
        transform.position = pos;

        Event<CenterUpdatedEvent>.Broadcast(new CenterUpdatedEvent(pos));
    }
}
