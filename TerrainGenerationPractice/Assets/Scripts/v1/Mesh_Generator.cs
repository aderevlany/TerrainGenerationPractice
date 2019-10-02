using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Unity draw mesh triangles between vertice points in a clock-wise fashion
 * Backsides are culled (or not shown) -> counter-clockwise drawn tringles are not shown
 */

// forces the object to which this script is attached to to always have a
// MeshFilter, so that you are never assigning a mesh to a nonexisting MeshFilter in Start()
[RequireComponent(typeof(MeshFilter))]
public class Mesh_Generator : MonoBehaviour
{
    // vertex count = (xSize + 1) * (ySize + 1)
    public int xSize = 20;
    public int zSize = 20;

    public float scale = .3f;
    public float offset = 2f;

    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        offset = Random.Range(0f, 999999f); // this would be the world seed

        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int i = 0, z = 0; z <= zSize; z++) {
            for (int x = 0; x <= xSize; x++) {
                // add randomness to the map
                // requires float coord 0-1
                float y = Mathf.PerlinNoise(x * scale + offset, z * scale + offset) * 2f; 
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[xSize * zSize * 6]; // need 6 points for each quad (two traingles)
                                                // rendered in clock-wise fashion

        int vert = 0;
        int tris = 0;
        
        for (int z = 0; z < zSize; z++) {
            for (int x = 0; x < xSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xSize + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xSize + 1;
                triangles[tris + 5] = vert + xSize + 2;

                vert++;
                tris += 6;
            }

            vert++; // to stop last triangle in row from connecting to first triangle in next row
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals(); // tell unity to recalculate the normals so that the mesh
                                   // reacts to light and shadow appropriately
    }

    private void OnDrawGizmos()
    {
        if (vertices == null) return;

        for (int i = 0; i < vertices.Length; i++) {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
}
