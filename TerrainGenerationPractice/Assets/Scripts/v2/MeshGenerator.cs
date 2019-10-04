using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, 
                                               AnimationCurve _heightCurve, int levelOfDetail) {
        // create a new animation curve for each mesh so that weird spikes dont be spiky (fixes an animation curve 'optamization')
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (width - 1) / 2f;

        // so further meshes don't have to be as detailed
        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int verticesPerLIne = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLIne, verticesPerLIne);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement) {
            for (int x = 0; x < width; x += meshSimplificationIncrement) {

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, 
                                                 heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, 
                                                 topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                // if not bottom or far right, add square
                //      i --- i+1
                //      |  \  |
                //    i+w --- i+w+1
                if (x < width-1 && y < height-1 ) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLIne + 1, vertexIndex + verticesPerLIne);
                    meshData.AddTriangle(vertexIndex + verticesPerLIne + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;    // return meshData, not mesh for treading later
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    int triangleIndex = 0;

    public MeshData(int meshWidth, int meshHeight) {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        // normalise
        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        // cross product, to get the y line from (x,z)
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        mesh.normals = CalculateNormals();
        return mesh;
    }
}
