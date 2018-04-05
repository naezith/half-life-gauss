using UnityEngine;

public class MouseLook : MonoBehaviour {
	public float sensitivity = 1.25f; // Enter source engine sensitivity, balancer values will convert it for Unity
	public float sensitivityYBalancer = 0.85F;
    public float sensitivityBalancer = 0.43F; // Source 

    float GetSourceSensitivity() {
        return sensitivity * sensitivityBalancer;
    }

    void Update () {
        float sens = GetSourceSensitivity();

        float rotationX = transform.localEulerAngles.y + Input.GetAxisRaw("Mouse X") * sens;
        float rotationY = -transform.localEulerAngles.x + Input.GetAxisRaw("Mouse Y") * sens * sensitivityYBalancer;

        // Limit vertical angle
        if(rotationY >= -270.0f && rotationY <= -180.0f) rotationY = -270.0f;
        else if(rotationY <= -90.0f && rotationY >= -180.0f) rotationY = -90.0f;
			
		transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }
}