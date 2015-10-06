//using UnityEngine;
//using System.Collections;

//public class Marker : MonoBehaviour {

//    private const float FLOOR_DISPACEMENT = 1f;
//    private Vector3 positionAtActionStart;

//    // Use this for initialization
//    void Start () {
	
//	}
	
//	// Update is called once per frame
//	void Update () {
	
//	}

//    void FixedUpate()
//    {
//        if (_midMovement && rb.velocity.magnitude > 0)
//        {
//            float h = Terrain.activeTerrain.SampleHeight(transform.position) + FLOOR_DISPACEMENT;
//            transform.position = new Vector3(transform.position.x, h, transform.position.z);
//            if ((positionAtActionStart - transform.position).magnitude >= movementDistance)
//            {
//                transform.position = prevPosition;
//            }
//            if (transform.position.x > 62 || transform.position.x < -63 || transform.position.z > 100 || transform.position.z < -97)
//                transform.position = prevPosition;
//        }
//        else
//        {
//            rb.Sleep();
//        }
//        prevPosition = transform.position;
//    }
//}
