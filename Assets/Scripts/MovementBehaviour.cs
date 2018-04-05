using UnityEngine;

public class MovementBehaviour : MonoBehaviour {
    public float accel = 200f;
    public float airAccel = 200f;
    public float maxSpeed = 7.25f;
    public float maxAirSpeed = 0.6f;
    public float friction = 8f;
    public float jumpForce = 5f;
    public float longJumpMultiplier = 1.5f;
    public float maxStepHeight = 0.201f;
    public LayerMask groundLayers;
    
    public float lastJumpPress = -1f;
    public float jumpPressDuration = 0.1f;

    public AudioClip[] audioStep;
    private AudioSource audioSource;

    private GameObject camObj;
    private bool onGround = false;

    private Vector3 lastFrameVelocity = Vector3.zero;
    private bool prevOnGround;
    private float lastStepSoundTime;
    private float stepSoundDuration = 0.3f;
    private int lastStepId = 0;
    private bool jumpKeyPressed;

    private void Awake() {
        camObj = transform.Find("Camera").gameObject;
    }

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        // QualitySettings.vSyncCount = 0;
        // Application.targetFrameRate = 300;
    }

    private void Update() {
        if(Input.GetKey("escape"))  Application.Quit();

        // Set key states
        jumpKeyPressed = Input.GetButton("Jump");
        if(jumpKeyPressed) lastJumpPress = Time.time;
    }

    public void ResetVelocity() {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void FixedUpdate() {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Friction
        Vector3 tempVelocity = CalculateFriction(GetComponent<Rigidbody>().velocity);

        // Add movement
        tempVelocity += CalculateMovement(input, tempVelocity);

        // Apply
        if(!GetComponent<Rigidbody>().isKinematic) {
            GetComponent<Rigidbody>().velocity = tempVelocity;
        }

        lastFrameVelocity = GetComponent<Rigidbody>().velocity;



        // Step sound
        if(Time.time > lastStepSoundTime + stepSoundDuration && lastFrameVelocity.magnitude > 1.0f && CheckGround()) {
            int stepId;

            do { stepId = Random.Range(0, 4); } while(lastStepId == stepId);

            audioSource.PlayOneShot(audioStep[stepId]);

            lastStepId = stepId;

            lastStepSoundTime = Time.time;
        }
    }

    public Vector3 CalculateFriction(Vector3 currentVelocity) {
        onGround = CheckGround();
        float speed = currentVelocity.magnitude;

        // Code from https://flafla2.github.io/2015/02/14/bunnyhop.html
        if(!onGround || Input.GetButton("Jump") || speed == 0f)
            return currentVelocity;

        float drop = speed * friction * Time.deltaTime;
        return currentVelocity * (Mathf.Max(speed - drop, 0f) / speed);
    }

    // Do movement input here
    public Vector3 CalculateMovement(Vector2 input, Vector3 velocity) {
        onGround = CheckGround();

        // Different acceleration values for ground and air
        float curAccel = accel;
        if(!onGround)
            curAccel = airAccel;

        // Ground speed
        float curMaxSpeed = maxSpeed;

        // Air speed
        if(!onGround)
            curMaxSpeed = maxAirSpeed;

        // Get rotation input and make it a vector
        Vector3 camRotation = new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f);
        Vector3 inputVelocity = Quaternion.Euler(camRotation) *
                                new Vector3(input.x * curAccel, 0f, input.y * curAccel);

        // Ignore vertical component of rotated input
        Vector3 alignedInputVelocity = new Vector3(inputVelocity.x, 0f, inputVelocity.z) * Time.deltaTime;

        // Get current velocity
        Vector3 currentVelocity = new Vector3(velocity.x, 0f, velocity.z);

        // How close the current speed to max velocity is (1 = not moving, 0 = at/over max speed)
        float max = Mathf.Max(0f, 1 - (currentVelocity.magnitude / curMaxSpeed));

        // How perpendicular the input to the current velocity is (0 = 90°)
        float velocityDot = Vector3.Dot(currentVelocity, alignedInputVelocity);

        // Scale the input to the max speed
        Vector3 modifiedVelocity = alignedInputVelocity * max;

        // The more perpendicular the input is, the more the input velocity will be applied
        Vector3 correctVelocity = Vector3.Lerp(alignedInputVelocity, modifiedVelocity, velocityDot);

        // Apply jump
        Vector3 jumpVelocity = GetJumpVelocity(velocity.y);
        correctVelocity.y += jumpVelocity.y;

        // Long jump set
        if(NUtility.GetXZ(jumpVelocity).magnitude > NUtility.GetXZ(correctVelocity).magnitude) { 
            correctVelocity.x = jumpVelocity.x;
            correctVelocity.z = jumpVelocity.z;
        }

        // Return
        return correctVelocity;
    }

    private Vector3 GetJumpVelocity(float yVelocity) {
        Vector3 jumpVelocity = Vector3.zero;
            
        // Calculate jump
        if(Time.time < lastJumpPress + jumpPressDuration && yVelocity < jumpForce && CheckGround()) {
            lastJumpPress = -1f;

            jumpVelocity = Quaternion.Euler(new Vector3(0f, camObj.transform.rotation.eulerAngles.y, 0f)) * 
                        new Vector3(0, jumpForce - yVelocity, 
                        prevOnGround && Input.GetButton("Crouch") && NUtility.GetXZ(GetComponent<Rigidbody>().velocity).magnitude > 0.01f ? maxSpeed * longJumpMultiplier : 0f); // Long jump
        }
        prevOnGround = CheckGround();

        return jumpVelocity;
    }

    private void OnCollisionEnter(Collision col) {
        bool doStepUp = true;
        float footHeight = transform.position.y - GetComponent<Collider>().bounds.extents.y;

        foreach(ContactPoint p in col.contacts) {
            Debug.DrawLine(p.point, p.point + p.normal, Color.red, 1f);

            if(p.otherCollider is BoxCollider) {
                if(footHeight + maxStepHeight < p.otherCollider.transform.position.y +
                    p.otherCollider.bounds.extents.y)
                    doStepUp = false;
            }
            else if(p.otherCollider is MeshCollider) {
                doStepUp = false;
            }
        }

        if(doStepUp) {
            transform.position = new Vector3(transform.position.x,
                col.collider.transform.position.y + col.collider.bounds.extents.y +
                GetComponent<Collider>().bounds.extents.y + 0.001f, transform.position.z);
            GetComponent<Rigidbody>().velocity = lastFrameVelocity;
        }
    }

    public bool CheckGround() {
        Vector3 pos = new Vector3(transform.position.x,
            transform.position.y - GetComponent<Collider>().bounds.extents.y + 0.05f, transform.position.z);
        Vector3 radiusVector = new Vector3(GetComponent<Collider>().bounds.extents.x, 0f, 0f);
        return CheckCylinder(pos, radiusVector, -0.1f, 8);
    }

    private bool CheckCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount,
        out float dist, bool slopeCheck = true) {
        bool tempHit = false;
        float tempDist = -1f;

        for(int i = -1; i < rayCount; i++) {
            RaycastHit hit;
            bool hasHit = false;
            float verticalDirection = Mathf.Sign(verticalLength);

            // Check directly from origin
            if(i == -1) { 
                hasHit = Physics.Raycast(origin, Vector3.up * verticalDirection, out hit, Mathf.Abs(verticalLength),
                    groundLayers);
            }
            // Check in a circle around the origin
            else { 
                Vector3 radius = Quaternion.Euler(new Vector3(0f, i * (360f / rayCount), 0f)) * radiusVector;
                Vector3 circlePoint = origin + radius;

                hasHit = Physics.Raycast(circlePoint, Vector3.up * verticalDirection, out hit,
                    Mathf.Abs(verticalLength), groundLayers);
            }

            // Collided with something
            if(hasHit) {
                // Assign tempDist to the shortest distance
                if(tempDist == -1f)
                    tempDist = hit.distance;
                else if(tempDist > hit.distance)
                    tempDist = hit.distance;

                // Only return true if the angle is 40° or lower (if slopeCheck is active)
                if(!slopeCheck || hit.normal.y > 0.75f) {
                    tempHit = true;
                }
            }
        }

        dist = tempDist;

        return tempHit;
    }

    private bool CheckCylinder(Vector3 origin, Vector3 radiusVector, float verticalLength, int rayCount, bool slopeCheck = true) {
        float dist;
        return CheckCylinder(origin, radiusVector, verticalLength, rayCount, out dist, slopeCheck);
    }
}