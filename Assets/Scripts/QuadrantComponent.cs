using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadrantComponent : MonoBehaviour
{
    public Quadrant quadrant;
    public Color color;

    public QuadrantComponent(){
        color = new Color();
    }

    void OnDrawGizmosSelected(){
        Gizmos.DrawMesh(GetComponent<MeshFilter>().mesh, -1, transform.position, Quaternion.identity);
    }
}
