using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    //speed
    [SerializeField] float m_maxSpeed = 5;
    [SerializeField] float m_acceleration = 15;
    [SerializeField] float m_maxFallSpeed = 20;
    [SerializeField] float m_rotationSpeed = 10;
    //ground
    [SerializeField] float m_groundCheckDistance = 0.2f;
    [SerializeField] LayerMask m_groundMask;
    [SerializeField] float m_maxGroundAngle = 50;
    //jump
    [SerializeField] float m_jumpSpeed = 10;
    [SerializeField] float m_maxJumpDuration = 0.2f;
    [SerializeField] float m_jumpBufferTimeBeforeLand = 0.2f;
    [SerializeField] float m_jumpBufferTimerAfterLand = 0.2f;
    [SerializeField] float m_jumpApexSpeed = 1;

    Rigidbody m_rigidbody;
    CapsuleCollider m_collider;

    Vector2 m_direction;
    Vector2 m_oldDirection;
    bool m_jumping;

    bool m_grounded = false;
    Vector3 m_groundNormal = Vector3.up;
    float m_outGroundTime = -1;

    Vector3 m_oldVelocity;
    Vector3 m_oldPosition;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void Start()
    {
        Event<CenterUpdatedEventInstant>.Broadcast(new CenterUpdatedEventInstant(transform.position));
    }
    
    void FixedUpdate()
    {
        GetDirectionEvent direction = new GetDirectionEvent();
        Event<GetDirectionEvent>.Broadcast(direction, gameObject);
        m_direction = direction.direction;
        float dirLenght = m_direction.magnitude;
        if (dirLenght > 1)
            m_direction /= dirLenght;
        if (dirLenght < 0.1f)
            dirLenght = 0;

        GetJumpEvent jump = new GetJumpEvent();
        Event<GetJumpEvent>.Broadcast(jump, gameObject);
        m_jumping = jump.jump;

        UpdateGrounded();
        UpdateSpeed();

        UpdateFallSpeed();

        UpdateJump();

        m_oldVelocity = m_rigidbody.velocity;
        m_oldPosition = transform.position;

        if (m_direction.magnitude > 0.001f)
            m_oldDirection = m_direction;

        Event<CenterUpdatedEvent>.Broadcast(new CenterUpdatedEvent(transform.position));
    }

    void UpdateGrounded()
    {
        float radius;
        Vector3 point1, point2;
        GetCapsuleParameters(out point1, out point2, out radius);

        var collisions = Physics.CapsuleCastAll(point1, point2, radius, -Vector3.up, m_groundCheckDistance, m_groundMask);

        if(collisions.Length == 0)
            m_grounded = false;
        else
        {
            m_grounded = false;
            foreach(var c in collisions)
            {
                float angle = Vector3.Angle(c.normal, Vector3.up);
                if (angle > m_maxGroundAngle)
                    continue;
                m_grounded = true;
                m_groundNormal = c.normal;
            }
        }

        m_outGroundTime += Time.deltaTime;
        if (m_grounded)
            m_outGroundTime = 0;
    }

    void UpdateSpeed()
    {
        //if (m_grounded)
        //    UpdateGrounded();
        //else UpdateAirSpeed();

        UpdateAirSpeed();
    }

    void UpdateGroundedSpeed()
    {
        var velocity = m_rigidbody.velocity;



        var axisX = Vector3.Cross(m_groundNormal, new Vector3(0, 0, 1));
        var axisZ = Vector3.Cross(m_groundNormal, axisX);

        float angleX = Vector3.SignedAngle(axisX, new Vector3(1, 0, 0), Vector3.up);
        float angleZ = Vector3.SignedAngle(axisZ, new Vector3(0, 0, 1), Vector3.up);

        var velocityX = Vector3.Project(velocity, axisX);
        var velocityY = Vector3.Project(velocity, m_groundNormal);
        var velocityZ = Vector3.Project(velocity, axisZ);



    }

    void UpdateAirSpeed()
    {
        var velocity = m_rigidbody.velocity;
        
        float directionMagnitude = m_direction.magnitude;
        var directionNormalized = directionMagnitude > 0.001f ? m_direction / directionMagnitude : m_direction;
        
        var vecSee = new Vector2(transform.forward.x, transform.forward.z);

        float angle = Mathf.Atan2(vecSee.y, vecSee.x);
        float targetAngle = angle;
        if (directionMagnitude > 0.001f)
            targetAngle = Mathf.Atan2(m_direction.y, m_direction.x);
        else if(m_oldDirection.magnitude > 0.001f)
            targetAngle = Mathf.Atan2(m_oldDirection.y, m_oldDirection.x);

        float deltaAngle = DeltaRadAngle(angle, targetAngle);
        if (Mathf.Abs(deltaAngle) > 0.001f)
        {
            float rotation = m_rotationSpeed * Mathf.Sign(deltaAngle) * Time.deltaTime;
            if (Mathf.Abs(rotation) > Mathf.Abs(deltaAngle))
                rotation = deltaAngle;
            angle += rotation;
        }

        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        Vector2 orthoDirection = new Vector2(direction.y, -direction.x);
        var dirDot = Vector2.Dot(directionNormalized, direction);

        Debug.DrawRay(transform.position, new Vector3(direction.x, 0, direction.y) * 2, new Color(1, 0, 0));

        float targetSpeed = dirDot > 0 ? directionMagnitude * m_maxSpeed : 0;

        Vector2 planeVelocity = new Vector2(velocity.x, velocity.z);
        float normSpeed = Vector2.Dot(direction, planeVelocity);
        float orthoSpeed = Vector2.Dot(orthoDirection, planeVelocity);
        
        orthoSpeed -= m_acceleration * Time.deltaTime;
        if (orthoSpeed < 0)
            orthoSpeed = 0;
        if(normSpeed < targetSpeed)
        {
            normSpeed += m_acceleration * Time.deltaTime;
            if (normSpeed > targetSpeed)
                normSpeed = targetSpeed;
        }
        if(normSpeed > targetSpeed)
        {
            normSpeed -= m_acceleration * Time.deltaTime;
            if (normSpeed < targetSpeed)
                normSpeed = targetSpeed;
        }

        planeVelocity = orthoSpeed * orthoDirection + normSpeed * direction;
        velocity.x = planeVelocity.x;
        velocity.z = planeVelocity.y;

        m_rigidbody.velocity = velocity;
        m_rigidbody.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);
    }

    void UpdateFallSpeed()
    {
        Vector3 velocity = m_rigidbody.velocity;
        if (velocity.y < -m_maxFallSpeed)
            velocity.y = -m_maxFallSpeed;

        m_rigidbody.velocity = velocity;
    }

    void UpdateJump()
    {

    }

    void GetCapsuleParameters(out Vector3 point1, out Vector3 point2, out float radius)
    {
        radius = m_collider.radius;

        float distanceToPoint = m_collider.height / 2 - m_collider.radius;

        point1 = transform.position + m_collider.center + Vector3.up * distanceToPoint;
        point2 = transform.position + m_collider.center - Vector3.up * distanceToPoint;
    }

    float DeltaRadAngle(float from, float to)
    {
        float delta = to - from;

        while (delta < -Mathf.PI)
            delta += 2 * Mathf.PI;
        while (delta > Mathf.PI)
            delta -= 2 * Mathf.PI;

        return delta;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "Direction " + m_direction.x + " " + m_direction.y);
        GUI.Label(new Rect(10, 30, 300, 20), "Jump " + m_jumping);
    }
}
