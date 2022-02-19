using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] Vector2 m_lookOffset = new Vector2(0, 0);
    [SerializeField] Vector2 m_posOffset = new Vector2(0, 0);
    [SerializeField] float m_rotateSpeed = 1;
    [SerializeField] float m_distanceSpeed = 5;
    [SerializeField] float m_angleSpeed = 1;

    [SerializeField] float m_rotation = 0;
    [SerializeField] float m_distance = 15;
    [SerializeField] float m_angle = 0;

    void Update()
    {
        if (Keyboard.current.numpad2Key.isPressed)
            m_angle -= m_angleSpeed * Time.deltaTime;
        if (Keyboard.current.numpad8Key.isPressed)
            m_angle += m_angleSpeed * Time.deltaTime;
        if (Keyboard.current.numpad4Key.isPressed)
            m_rotation += m_rotateSpeed * Time.deltaTime;
        if (Keyboard.current.numpad6Key.isPressed)
            m_rotation -= m_rotateSpeed * Time.deltaTime;
        if (Keyboard.current.numpadMinusKey.isPressed)
            m_distance += m_distanceSpeed * Time.deltaTime;
        if (Keyboard.current.numpadPlusKey.isPressed)
            m_distance -= m_distanceSpeed * Time.deltaTime;

        if (m_distance < 0.1f)
            m_distance = 0.1f;

        float height = Mathf.Cos(m_angle) * m_distance + m_posOffset.y;
        float offset = Mathf.Sin(m_angle) * m_distance + m_posOffset.x;

        Vector3 pos = new Vector3(Mathf.Cos(m_rotation) * offset, height, Mathf.Sin(m_rotation) * offset);

        transform.localPosition = pos;

        if(transform.parent != null)
        {
            Vector3 target = transform.parent.position;
            target.y += m_lookOffset.y;
            target.x += m_lookOffset.x * Mathf.Cos(m_rotation);
            target.z += m_lookOffset.x * Mathf.Sin(m_rotation);

            transform.LookAt(target);
        }
    }
}
