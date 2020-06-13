using UnityEngine;
using System.Collections;

public class PlaceholderBlockPlacer : MonoBehaviour
{
    [SerializeField] LayerMask m_collisionLayer;
    [SerializeField] float m_maxPlayerDistance;

    Camera m_camera = null;

    Vector3 m_centerPos = Vector3.zero;

    private void Awake()
    {
        
    }

    private void OnDestroy()
    {
        
    }

    void Start()
    {
        m_camera = Camera.main;
    }

    void Update()
    {
        var ray = m_camera.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        var collision = Physics.Raycast(ray, out hit, 1000, m_collisionLayer.value);

        if (!collision)
            return;

        DrawBlock(hit.point);
    }

    void DrawBlock(Vector3 hitPos)
    {
        
    }

    void OnCenterUpdated(CenterUpdatedEvent e)
    {
        m_centerPos = e.pos;
    }
}
