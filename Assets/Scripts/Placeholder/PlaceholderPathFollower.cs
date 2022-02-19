using System.Collections;
using UnityEngine;


public class PlaceholderPathFollower : MonoBehaviour
{
    [SerializeField] float m_speed = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    Vector3 m_target = Vector3.zero;
    bool m_targetSet = false;

    Path m_path;

    private void Awake()
    {
        m_subscriberList.Add(new Event<CenterUpdatedEvent>.Subscriber(OnCenterChange));
        m_subscriberList.Subscribe();
    }

    private void Start()
    {
        PathSettings settings = new PathSettings();
        m_path = new Path(settings);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Update()
    {
        m_path.Draw();

        m_path.Process(transform.position);

        if (m_path.GetStatus() == Path.Status.Valid)
        {
            var target = m_path.GetPos();
            var dir = target - transform.position;
            dir.Normalize();
            dir *= Time.deltaTime * m_speed;
            transform.position += dir;
        }
    }

    void OnCenterChange(CenterUpdatedEvent e)
    {
        float distance = (m_target - e.pos).sqrMagnitude;

        if(distance > 1 || !m_targetSet)
        {
            m_target = e.pos;

            m_path.Generate(transform.position, m_target);
        }

        m_targetSet = true;
    }
}