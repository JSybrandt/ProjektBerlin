using UnityEngine;
using System.Collections;

public class Controller : MonoBehaviour {

	private GameObject[] squads;
	private int selectedSquadIndex;
	private GameObject selectedLight;
	private Camera mainCamera;
	private Rigidbody selectedRB; //used to move selected squad
	public float theta;
	public float zoomSpeed;
	private float distance;

	//TODO: this needs to be changeable
	private Vector3 defaultCameraOffset = new Vector3(0,20,-5);
	private Vector3 cameraTarget = new Vector3(0,0,0);
	private Vector3 lightOffset = new Vector3(0,2,0);

    //TODO: Cleanup and organize the FoV
    //FoV necessary items
    int quality = 15;
    Mesh mesh;
    public Material materialFov;
    float angle_fov = 20;
    float dist_min = 5.0f;
    float dist_max = 15.0f;
    Quaternion FovRotation = Quaternion.identity; //Default direction

    void Start(){
		selectedLight = GameObject.Find ("SelectedLight");
		if(selectedLight==null) throw new MissingReferenceException("Need SelectedLight");
		mainCamera = Camera.main;
		if(mainCamera==null) throw new MissingReferenceException("Need Camera.main");
		distance = defaultCameraOffset.y;
	}

	public void updateSquadList(string tag){
		squads = GameObject.FindGameObjectsWithTag (tag);
		selectedSquadIndex = 0;
		if(squads.Length==0)throw new UnityException("Failed to find squad.");
		setCamera ();
		setLight ();
	}

	private void setCamera(){
		cameraTarget = squads [selectedSquadIndex].transform.position;
		mainCamera.transform.position = cameraTarget + defaultCameraOffset;
		mainCamera.transform.LookAt (cameraTarget);
		
		Vector3 vec =  mainCamera.transform.position - squads [selectedSquadIndex].transform.position;
		distance += Input.GetAxisRaw ("Fire2") * zoomSpeed;
		theta += Input.GetAxisRaw ("Fire3");
		Vector3 newCameraOffset = Quaternion.Euler (0, theta, 0) * defaultCameraOffset;
		newCameraOffset *= distance;

		mainCamera.transform.position = cameraTarget + newCameraOffset;
		mainCamera.transform.LookAt (cameraTarget);

	}

	private void setLight(){
		selectedLight.transform.position = squads[selectedSquadIndex].transform.position+lightOffset;
	}

    float GetSquadAngle()
    {
        return 90 - Mathf.Rad2Deg * Mathf.Atan2(transform.forward.z, transform.forward.x); // Left handed CW. z = angle 0, x = angle 90
    }

    private void drawFoV()
    {
        float angle_lookat = GetSquadAngle();

        float angle_start = angle_lookat - angle_fov;
        float angle_end = angle_lookat + angle_fov;
        float angle_delta = (angle_end - angle_start) / quality;

        float angle_curr = angle_start;
        float angle_next = angle_start + angle_delta;

        Vector3 pos_curr_min = Vector3.zero;
        Vector3 pos_curr_max = Vector3.zero;

        Vector3 pos_next_min = Vector3.zero;
        Vector3 pos_next_max = Vector3.zero;

        Vector3[] vertices = new Vector3[4 * quality];   // Could be of size [2 * quality + 2] if circle segment is continuous
        int[] triangles = new int[3 * 2 * quality];

        for (int i = 0; i < quality; i++)
        {
            Vector3 sphere_curr = new Vector3(
            Mathf.Sin(Mathf.Deg2Rad * (angle_curr)), 0,   // Left handed CW
            Mathf.Cos(Mathf.Deg2Rad * (angle_curr)));

            Vector3 sphere_next = new Vector3(
            Mathf.Sin(Mathf.Deg2Rad * (angle_next)), 0,
            Mathf.Cos(Mathf.Deg2Rad * (angle_next)));

            pos_curr_min = transform.position + sphere_curr * dist_min;
            pos_curr_max = transform.position + sphere_curr * dist_max;

            pos_next_min = transform.position + sphere_next * dist_min;
            pos_next_max = transform.position + sphere_next * dist_max;

            int a = 4 * i;
            int b = 4 * i + 1;
            int c = 4 * i + 2;
            int d = 4 * i + 3;

            vertices[a] = pos_curr_min;
            vertices[b] = pos_curr_max;
            vertices[c] = pos_next_max;
            vertices[d] = pos_next_min;

            triangles[6 * i] = a;       // Triangle1: abc
            triangles[6 * i + 1] = b;
            triangles[6 * i + 2] = c;
            triangles[6 * i + 3] = c;   // Triangle2: cda
            triangles[6 * i + 4] = d;
            triangles[6 * i + 5] = a;

            angle_curr += angle_delta;
            angle_next += angle_delta;

        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        Graphics.DrawMesh(mesh, selectedRB.position, FovRotation, materialFov, 0);
    }

	// Update is called once per frame
	void Update () {

		if (squads.Length > 0) {

			if (Input.GetButtonUp ("NextSquad")) {
				selectedSquadIndex++;
				selectedSquadIndex %= squads.Length;
				if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
			}
			if (Input.GetButtonUp ("PrevSquad")) {
				selectedSquadIndex--;
				if(selectedSquadIndex<0)selectedSquadIndex=squads.Length-1;

				if(selectedRB!=null)selectedRB.velocity=Vector3.zero;
			}

            float vert = Input.GetAxis("JoystickRV");
            float horz = Input.GetAxis("JoystickRH");

            bool isAiming = false;
            if (vert != 0f || horz != 0f)
                isAiming = true;

            if(isAiming)
            {
                Debug.Log("Right Stick Moving");
                var angle = Mathf.Atan2(horz, vert) * Mathf.Rad2Deg;
                FovRotation = Quaternion.Euler(0, angle, 0);

                drawFoV();
            }

			selectedRB = squads[selectedSquadIndex].GetComponent<Rigidbody>();
			float v = Input.GetAxisRaw("Vertical");
			float h = Input.GetAxisRaw("Horizontal");
			selectedRB.velocity = new Vector3(h,0,v);

			setCamera();
			setLight();
		}


	}
}
