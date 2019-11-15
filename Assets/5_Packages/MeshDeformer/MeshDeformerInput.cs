using UnityEngine;

public class MeshDeformerInput : MonoBehaviour {
	
	public float force = 10f;
	public float forceOffset = 0.1f;
	
	void Update () {
		if (Input.GetMouseButton(0)) {
            //Debug.Log("DAFAQ?");
			HandleInput();
		}
	}

	void HandleInput () {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		
		if (Physics.Raycast(inputRay, out hit)) {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            // Debug.Log(hit);
			if (deformer) {
                //Debug.Log("YOLO");
				Vector3 point = hit.point;
				point += hit.normal * forceOffset;
				deformer.AddDeformingForce(point, force);
			}
		}
	}
}