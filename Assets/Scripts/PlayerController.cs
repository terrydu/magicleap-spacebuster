using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.MagicLeap;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax, zMin, zMax;
}

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float speed;
    public float tilt;
    public Boundary boundary;

    // Shots
    public GameObject shot;
    public Transform shotSpawn;
    public float fireDelta = 0.5F;
    private float nextFire = 0.5F;
    private float myTime = 0.0F;
    private AudioSource audioSource;

    // Magic Leap
    private MLInputController mlController;
    private bool isBumperTapped = false;

    /*
     * Setup the Magic Leap controller input.
     */
    void Awake() {
        MLInput.Start();
        MLInput.OnControllerButtonDown += OnButtonDown;
        MLInput.OnControllerButtonUp += OnButtonUp;
        mlController = MLInput.GetController(MLInput.Hand.Left);
    }

    /*
     * Stop listening for Magic Leap input.
     */
    void OnDestroy () {
        MLInput.OnControllerButtonDown -= OnButtonDown;
        MLInput.OnControllerButtonUp -= OnButtonUp;
        MLInput.Stop();
    }

    /*
     * Listen for the Magic Leap bumper being pressed ("tapped").
     */
    void OnButtonDown(byte controller_id, MLInputControllerButton button) {
        if (button == MLInputControllerButton.Bumper) {
            isBumperTapped = true;
        }
    }

    /*
     * Listen for letting go of the Magic Leap controller bumper button.
     */
    void OnButtonUp(byte controller_id, MLInputControllerButton button) {
        if (button == MLInputControllerButton.Bumper) {
            isBumperTapped = false;
        }
    }

    /*
     * Get the main component of this attached Unity component, the RigidBody.
     */
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    /*
     * Fire the main laser. This is checked every Update, and we don't need to worry
     * about how frequent updates are since we already limit the fire rate based on time.
     * Handles both PC mouse input as well as Magic Leap controller input.
     */
    void Update()
    {
        myTime = myTime + Time.deltaTime;

        if (myTime > nextFire &&
            (Input.GetButton("Fire1") || mlController.TriggerValue > 0.2f))
        {
            nextFire = myTime + fireDelta;
            Instantiate(shot, shotSpawn.position, shotSpawn.rotation);

            nextFire = nextFire - myTime;
            myTime = 0.0F;

            audioSource = GetComponent<AudioSource>();
            audioSource.Play();
        }        
    }

    /*
     * Move the player.
     * Handles PC keyboard input first, then handles Magic Leap input. These are additive.
     */
    void FixedUpdate()
    {
        // Try keyboard input first.
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.velocity = movement * speed;

        rb.position = new Vector3(
            Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax), 
            0.0f, 
            Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax));

        // Then try Magic Leap input.
        if (mlController.Touch1PosAndForce.z > 0.0f) {
            float X = mlController.Touch1PosAndForce.x;
            float Y = mlController.Touch1PosAndForce.y;
            Vector3 forward = Vector3.Normalize(Vector3.ProjectOnPlane(transform.forward, Vector3.up));
            Vector3 right = Vector3.Normalize(Vector3.ProjectOnPlane(transform.right, Vector3.up));
            Vector3 force = Vector3.Normalize((X * right) + (Y * forward));

            // TODO: Change this to rb.velocity and rb.position=
            rb.transform.position += force * Time.deltaTime * speed / 2;

            rb.position = new Vector3(
                Mathf.Clamp(rb.position.x, boundary.xMin, boundary.xMax), 
                0.0f, 
                Mathf.Clamp(rb.position.z, boundary.zMin, boundary.zMax)); 
        }

        // Regardless of keyboard or controller input (or both), tilt based on X-axis velocity.
        Debug.Log("rb.velocity.x: " + rb.velocity.x + "; total: " + (rb.velocity.x * -tilt));
        rb.rotation = Quaternion.Euler(0.0f, 0.0f, rb.velocity.x * -tilt);
    }

}
