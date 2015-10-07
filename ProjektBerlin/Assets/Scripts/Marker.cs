using UnityEngine;
using System.Collections;

public class Marker : MonoBehaviour
{

    private const float FLOOR_DISPACEMENT = 1f;
    [HideInInspector]
    public Vector3 markerStart = new Vector3();
    [HideInInspector]
    public Vector3 markerPrevious = new Vector3();
    [HideInInspector]
    public float maxDistance = 0;
    private Rigidbody rb;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        if (Combat.markerMoving)
        {
            float v = Input.GetAxis("JoystickLV");
            float h = Input.GetAxis("JoystickLH");
            rb.velocity = (Quaternion.Euler(0, Camera.main.GetComponent<CameraController>().angle, 0) * new Vector3(h, 0, v).normalized) * 20;
            Camera.main.GetComponent<CameraController>().setCameraTarget(rb.transform.position, true);

            if (rb.velocity.magnitude > 0)
            {
                float t = Terrain.activeTerrain.SampleHeight(transform.position) + FLOOR_DISPACEMENT;
                transform.position = new Vector3(transform.position.x, t, transform.position.z);
                if ((markerStart - transform.position).magnitude >= maxDistance)
                {
                    transform.position = markerPrevious;
                }
                if (transform.position.x > 62 || transform.position.x < -63 || transform.position.z > 100 || transform.position.z < -97)
                    transform.position = markerPrevious;
            }
            else
            {
                rb.Sleep();
            }
            markerPrevious = transform.position;
        }
    }
}
