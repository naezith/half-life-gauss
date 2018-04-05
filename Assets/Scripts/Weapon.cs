using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
    public int gunDamage = 1;
    public float fireRate = 0.25f;
    public float weaponRange = 1337f;
    public float hitForce = 60.0f;
    public Transform gunEnd;

    private Camera fpsCam;
    private WaitForSeconds shotDuration = new WaitForSeconds(0.1f);
    private LineRenderer laserLine;
    private WeaponBobbing weaponBobbing;

    private float charge = 0.0f;
    private float maxCharge = 1.688f;
    private float prevCharge;

    private bool fireButtonPressed = false;
    private bool fireButtonDown = false;
    private bool fireButtonReleased = false;
    private Rigidbody player;

    public AudioClip[] audioShoots;
    private AudioSource[] audioSources;
    private int lastSoundId;

    void Start() {
        laserLine = GetComponent<LineRenderer>();
        laserLine.enabled = false;

        audioSources = GetComponents<AudioSource>();

        fpsCam = GetComponentInParent<Camera>();

        weaponBobbing = GetComponent<WeaponBobbing>();

        player = transform.parent.parent.GetComponent<Rigidbody>();
    }

    private void Update() {
        if(Input.GetButtonDown("Fire1")) { 
            fireButtonPressed = true;
        }
        if(Input.GetButtonUp("Fire1")) {
            fireButtonReleased = true;
        }

        fireButtonDown = Input.GetButton("Fire1");

        weaponBobbing.shakeSpeed = 2.0f * NormalizedCharge();

        laserLine.SetPosition(0, gunEnd.position);
    }

    float NormalizedCharge() {
        return Mathf.Min(charge / maxCharge, 1.0f);
    }

    void FixedUpdate() {
        if(fireButtonPressed) {
            charge = 0.0f;
            fireButtonPressed = false;

            // Spin up sound
            audioSources[1].time = 0.0f;
            audioSources[1].Play();
        }

        if(fireButtonDown) {
            charge += Time.fixedDeltaTime;

            if(charge >= maxCharge && prevCharge < maxCharge) {
                audioSources[2].time = 0.0f;
                audioSources[2].loop = true;
                audioSources[2].Play();
            }
        }

        if(fireButtonReleased || charge > 10.0f) {
            StartCoroutine(ShotEffect());

            Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));

            RaycastHit hit;

            if(Physics.Raycast(rayOrigin, fpsCam.transform.forward, out hit, weaponRange, LayerMask.GetMask("Surface"))) {
                laserLine.SetPosition(1, hit.point);
                player.AddForce((player.position - hit.point).normalized * hitForce * NormalizedCharge());
            }
            else {
                laserLine.SetPosition(1, rayOrigin + (fpsCam.transform.forward * weaponRange));
            }

            charge = 0.0f;
            fireButtonReleased = false;

            // Stop Spin up and Spin Loop
            for(int i = 1; i <= 2; ++i) {
                audioSources[i].Pause();
            }
        }

        prevCharge = charge;
    }

    private IEnumerator ShotEffect() {
        // Main sound
        audioSources[0].PlayOneShot(audioShoots[0]);

        // Second sound
        int soundId;

        do { soundId = Random.Range(1, 4); } while(lastSoundId == soundId);

        audioSources[0].PlayOneShot(audioShoots[soundId]);

        lastSoundId = soundId;

        // Bobbing
        weaponBobbing.targetShootOffset.z = -1.0f;
        weaponBobbing.targetShootRotationOffset.x = -10.0f;

        // Laser
        laserLine.enabled = true;

        yield return shotDuration;

        laserLine.enabled = false;

        weaponBobbing.targetShootOffset.z = 0.0f;
        weaponBobbing.targetShootRotationOffset.x = 0.0f;
    }
}