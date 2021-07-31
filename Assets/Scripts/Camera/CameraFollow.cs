using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float m_minSpeed = 1;
    [SerializeField] float m_maxSpeed = 10;
    [SerializeField] float m_speedFactor = 1;
    [SerializeField] float m_speedPow = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector3 m_targetPos;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CenterUpdatedEvent>.Subscriber(OnMove));
        m_subscriberList.Add(new Event<CenterUpdatedEventInstant>.Subscriber(OnInstantMove));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 dir = m_targetPos - pos;

        float distance = dir.magnitude;
        if (distance <= 0.001f)
            return;

        dir /= distance;

        float speed = Mathf.Pow(distance, m_speedPow) * m_speedFactor;
        if (speed > m_maxSpeed)
            speed = m_maxSpeed;
        if (speed < m_minSpeed)
            speed = m_minSpeed;

        speed *= Time.deltaTime;
        if (speed > distance)
            speed = distance;

        pos += speed * dir;

        transform.position = pos;
    }

    void OnMove(CenterUpdatedEvent e)
    {
        m_targetPos = e.pos;
    }

    void OnInstantMove(CenterUpdatedEventInstant e)
    {
        transform.position = e.pos;
        m_targetPos = e.pos;
    }
}
