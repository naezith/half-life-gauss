using UnityEngine;

public class WeaponBobbing : MonoBehaviour {
    Vector3 cameraBasePosition;
    Vector3 gunBasePosition;
    Vector3 gunBaseRotation;

    Vector3 shootOffset;
    public Vector3 targetShootOffset;
    Vector3 shootRotationOffset;
    public Vector3 targetShootRotationOffset;

    public float shakeSpeed = 1.0f;
    private float time = 0.0f;

    // Use this for initialization
    void Start () {
        cameraBasePosition = transform.parent.localPosition;
        gunBasePosition = transform.localPosition;
        gunBaseRotation = transform.localRotation.eulerAngles;
    }

	// Update is called once per frame
	void FixedUpdate () {
        float playerSpeed = Mathf.Min(NUtility.GetXZ(transform.parent.parent.GetComponent<Rigidbody>().velocity).magnitude / 15.0f, 0.6f);

        shootOffset = Vector3.Lerp(shootOffset, targetShootOffset, 0.2f);
        shootRotationOffset = Vector3.Lerp(shootRotationOffset, targetShootRotationOffset, 0.2f);

        time += shakeSpeed;

        // Gun
        {
            // Position
            Vector3 pos = gunBasePosition + shootOffset;

            pos.x += Mathf.Sin(Time.time + time * 0.5f) * 0.01f;
            pos.y += Mathf.Sin(Time.time + time * 0.25f) * 0.01f;
            pos.z += Mathf.Sin(Time.time * 8.0f) * 0.1f * playerSpeed;
            
            transform.localPosition = pos;

            // Rotation
            Vector3 rot = gunBaseRotation + shootRotationOffset;
        
            rot.x += Mathf.Sin(Time.time + 0.5f * time) * 0.5f * shakeSpeed;
            rot.y += Mathf.Sin(Time.time + 0.5f * time) * 0.5f * shakeSpeed;
            rot.z += Mathf.Sin(Time.time + 0.5f * time) * 0.5f * shakeSpeed;
            
            transform.localRotation = Quaternion.Euler(rot);
        }

        // Camera
        { 
            Vector3 pos = cameraBasePosition;

            pos.y = cameraBasePosition.y + Mathf.Sin(Time.time * 8.0f) * 0.1f * playerSpeed;

            transform.parent.localPosition = pos;
        }
    }
}
