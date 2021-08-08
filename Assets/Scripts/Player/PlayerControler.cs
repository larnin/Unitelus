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
    [SerializeField] float m_jumpBufferTimeBeforeLand = 0.2f;
    [SerializeField] float m_jumpBufferTimerAfterLand = 0.2f;
    [SerializeField] float m_jumpApexSpeed = 1;

    Rigidbody m_rigidbody;
    CapsuleCollider m_collider;

    Vector2 m_direction;
    Vector2 m_oldDirection;
    bool m_jumping;

    bool m_grounded = false;
    bool m_oldGrounded = false;
    Vector3 m_groundNormal = Vector3.up;
    float m_outGroundTime = -1;

    float m_jumpPressTime = 0;
    float m_jumpTime = 1;

    Vector2 m_velocity = Vector2.zero;
    
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
        UpdateVelocity();
        UpdateSpeed();

        UpdateFallSpeed();

        UpdateJump();
        
        m_oldPosition = transform.position;
        m_oldGrounded = m_grounded;

        m_jumpPressTime += Time.deltaTime;

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

    void UpdateVelocity()
    {
        GetCameraEvent camera = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(camera);

        //up
        Vector3 cameraDir = Vector3.ProjectOnPlane(camera.camera.transform.forward, Vector3.up);
        cameraDir += Vector3.ProjectOnPlane(camera.camera.transform.up, Vector3.up);
        cameraDir.Normalize();
        //left
        Vector3 cameraDirOrtho = new Vector3(cameraDir.z, cameraDir.y, -cameraDir.x);

        Vector2 inputDirection = new Vector2(cameraDirOrtho.x, cameraDirOrtho.z) * m_direction.x + new Vector2(cameraDir.x, cameraDir.z) * m_direction.y;

        float directionMagnitude = inputDirection.magnitude;
        var directionNormalized = directionMagnitude > 0.001f ? inputDirection / directionMagnitude : inputDirection;

        var vecSee = new Vector2(transform.forward.x, transform.forward.z);

        float angle = Mathf.Atan2(vecSee.y, vecSee.x);
        float targetAngle = angle;
        if (directionMagnitude > 0.001f)
            targetAngle = Mathf.Atan2(inputDirection.y, inputDirection.x);
        else if (m_oldDirection.magnitude > 0.001f)
            targetAngle = Mathf.Atan2(m_oldDirection.y, m_oldDirection.x);

        float deltaAngle = DeltaRadAngle(angle, targetAngle);
        if (Mathf.Abs(deltaAngle) > 0.001f && (directionMagnitude > 0.001f || m_velocity.magnitude > 0.001f))
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

        float normSpeed = Vector2.Dot(direction, m_velocity);
        float orthoSpeed = Vector2.Dot(orthoDirection, m_velocity);

        orthoSpeed -= m_acceleration * Time.deltaTime;
        if (orthoSpeed < 0)
            orthoSpeed = 0;
        if (normSpeed < targetSpeed)
        {
            normSpeed += m_acceleration * Time.deltaTime;
            if (normSpeed > targetSpeed)
                normSpeed = targetSpeed;
        }
        if (normSpeed > targetSpeed)
        {
            normSpeed -= m_acceleration * Time.deltaTime;
            if (normSpeed < targetSpeed)
                normSpeed = targetSpeed;
        }

        m_velocity = orthoSpeed * orthoDirection + normSpeed * direction;
        
        m_rigidbody.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y), Vector3.up);

        if (directionMagnitude > 0.001f)
            m_oldDirection = inputDirection;
    }
    
    void UpdateSpeed()
    {
        if (m_grounded || m_oldGrounded)
        {
            if (!UpdateGroundedSpeed())
                UpdateAirSpeed();
        }
        else UpdateAirSpeed();
    }

    bool UpdateGroundedSpeed()
    {
        // don't ground player during jump
        if (m_jumpTime < 0.3f)
            return false;

        float radius;
        Vector3 point1, point2;
        GetCapsuleParameters(out point1, out point2, out radius);

        Vector3 point = point1.y < point2.y ? point1 : point2;
        float distance = radius / Mathf.Cos(Mathf.Deg2Rad * m_maxGroundAngle) + m_groundCheckDistance * 2;

        float deltaPos = m_velocity.magnitude * Time.deltaTime;
        distance += deltaPos;

        var hits = Physics.RaycastAll(point, -Vector3.up, distance, m_groundMask);
        if (hits.Length == 0)
            return false;

        bool haveHit = false;
        foreach(var h in hits)
        {
            float angle = Vector3.Angle(h.normal, Vector3.up);
            if (angle > m_maxGroundAngle)
                continue;
            haveHit = true;
            break;
        }

        if (!haveHit)
            return false;

        distance -= radius;

        hits = Physics.CapsuleCastAll(point1, point2, radius, -Vector3.up, distance, m_groundMask);
        if (hits.Length == 0)
            return false;

        haveHit = false;
        Vector3 normal = Vector3.up;
        foreach (var h in hits)
        {
            float angle = Vector3.Angle(h.normal, Vector3.up);
            if (angle > m_maxGroundAngle)
                continue;
            if (distance < h.distance)
                continue;
            haveHit = true;
            normal = h.normal;
            distance = h.distance;
        }

        if (!haveHit)
            return false;
        
        var pos = transform.position - Vector3.up * distance;
        m_rigidbody.MovePosition(pos);

        var velocity = Vector3.ProjectOnPlane(new Vector3(m_velocity.x, 0, m_velocity.y), normal);
        if(velocity.y < 0)
        {
            velocity.Normalize();
            velocity *= m_velocity.magnitude;
        }
        m_rigidbody.velocity = velocity;

        m_grounded = true;
        m_outGroundTime = 0;
        m_groundNormal = normal;

        return true;
    }

    void UpdateAirSpeed()
    {
        var velocity = m_rigidbody.velocity;
        velocity.x = m_velocity.x;
        velocity.z = m_velocity.y;
        m_rigidbody.velocity = velocity;
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
        bool canJump = m_jumping && ((m_grounded && m_jumpPressTime < m_jumpBufferTimeBeforeLand) || (!m_grounded && m_outGroundTime < m_jumpBufferTimerAfterLand && m_jumpPressTime == 0));

        if(canJump)
        {
            var velocity = m_rigidbody.velocity;
            velocity.y = m_jumpSpeed;
            m_rigidbody.velocity = velocity;

            m_jumpTime = 0;

            m_jumpPressTime = 1000;
        }

        m_jumpTime += Time.deltaTime;

        if (m_jumping)
            m_jumpPressTime += Time.deltaTime;
        else m_jumpPressTime = 0;
    }

    void GetCapsuleParameters(out Vector3 point1, out Vector3 point2, out float radius)
    {
        const float moveUp = 0.02f;

        radius = m_collider.radius;

        float distanceToPoint = m_collider.height / 2 - m_collider.radius;

        point1 = transform.position + m_collider.center + Vector3.up * (distanceToPoint + moveUp);
        point2 = transform.position + m_collider.center - Vector3.up * (distanceToPoint - moveUp);
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
        GUI.Label(new Rect(10, 50, 300, 20), "Grounded " + m_grounded);
    }
}
