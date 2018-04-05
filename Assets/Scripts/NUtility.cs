using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NUtility : MonoBehaviour {
    public static Vector3 GetXZ(Vector3 vector) {
        return new Vector3(vector.x, 0f, vector.z);
    }
}
