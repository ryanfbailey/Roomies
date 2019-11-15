using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    //public float force = 10f;
    //public float forceOffset = 0.1f;
    public float springForce = 20f;
    public float damping = 5f;
    //public float rootedForce = 200f;

    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices;
    Vector3[] vertexVelocities;

    float uniformScale = 1f;

    void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
        vertexVelocities = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        //Debug.DrawLine(transform.position, transform.forward * 20, Color.red);
        //Debug.DrawLine(transform.position, transform.right * 20, Color.blue);
        //Debug.DrawLine(transform.position, transform.up * 20, Color.green);
        //if (Input.GetKey(KeyCode.Alpha1))
        //{
        //    RaycastHit hit;
        //    Debug.DrawLine(transform.localPosition * 2, -transform.forward, Color.magenta);
        //    if (Physics.Raycast(transform.localPosition * 2, -transform.forward, out hit))
        //    {
        //        Vector3 point = hit.point;
        //        point += hit.normal * forceOffset;
        //        AddDeformingForce(point, force);
        //    }
        //}
        //if (Input.GetKey(KeyCode.Alpha2))
        //{

        //    AddDeformingForce(transform.right, rootedForce);
        //}
        //if (Input.GetKey(KeyCode.Alpha3))
        //{

        //    AddDeformingForce(transform.up, rootedForce);
        //}


        uniformScale = transform.localScale.x;
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }
        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        displacement *= uniformScale;
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;
        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        point = transform.InverseTransformPoint(point);
        if (displacedVertices != null)
        {
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                AddForceToVertex(i, point, force);
            }
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = displacedVertices[i] - point;
        pointToVertex *= uniformScale;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
}