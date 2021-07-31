using UnityEngine;
using System.Collections;

public class PlaceholderPlayer : MonoBehaviour
{
    string horizontalAxis = "Horizontal";
    string verticalAxis = "Vertical";

    [SerializeField] float m_speed = 1;

    private void Start()
    {
        Event<CenterUpdatedEventInstant>.Broadcast(new CenterUpdatedEventInstant(transform.position));
    }

    void Update()
    {
        Vector3 dir = new Vector3(Input.GetAxis(horizontalAxis), 0, Input.GetAxis(verticalAxis)) * Time.deltaTime * m_speed;
        var pos = transform.position;
        pos += dir;
        transform.position = pos;

        Event<CenterUpdatedEvent>.Broadcast(new CenterUpdatedEvent(pos));
    }
}
