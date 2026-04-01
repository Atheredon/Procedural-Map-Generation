using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightmap, float HeightMultiplier, AnimationCurve heightCurve)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        //To Center Mesh into the screen
        float topLeftX = ((float)width - 1.0f) / -2.0f;
        float topLeftZ = ((float)height - 1.0f) / 2.0f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.vertices[y * width + x] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightmap[x, y]) * HeightMultiplier, topLeftZ -y);

                meshData.uvs[vertexIndex] = new Vector2( (float)x / (float)width, (float)y / (float)height);

                if(x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);   //      a -- b
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);      //       |    |     tri1: a -> d -> c , tri2: d -> a -> b
                                                                                                     //        c -- d
                }
                vertexIndex++;
            }
        }
        return meshData;
    }

}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;

    public Vector2[] uvs;

    int lastTriangleIndex;

    public MeshData(int meshWidth, int MeshHeight)
    {
        vertices = new Vector3[meshWidth * MeshHeight];
        triangles = new int[(meshWidth - 1) * (MeshHeight - 1)* 6];
        uvs = new Vector2[meshWidth * MeshHeight];
    }

    public void AddTriangle(int x, int y, int z) 
    {
        triangles[lastTriangleIndex] = x;
        triangles[lastTriangleIndex + 1] = y;
        triangles[lastTriangleIndex + 2] = z;

        lastTriangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}
