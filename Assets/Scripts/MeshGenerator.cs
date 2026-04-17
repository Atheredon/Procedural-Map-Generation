using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightmap, float HeightMultiplier, AnimationCurve heightCurve, int levelOfDetail)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        //To Center Mesh into the screen
        float topLeftX = ((float)width - 1.0f) / -2.0f;
        float topLeftZ = ((float)height - 1.0f) / 2.0f;

        int meshSimplificationIncrement; //level of detail is 0-6, the numbers divisible by our chunksize(240) are 1,2,4,6,8,10,12
        if (levelOfDetail == 0)
            meshSimplificationIncrement = 1;
        else
            meshSimplificationIncrement = levelOfDetail * 2;

        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int y = 0; y < height; y+= meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x+= meshSimplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightmap[x, y]) * HeightMultiplier, topLeftZ -y);

                meshData.uvs[vertexIndex] = new Vector2( (float)x / (float)width, (float)y / (float)height);

                if(x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);   //      a --- b
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);                //       |  \  |     tri1: a -> d -> c , tri2: d -> a -> b
                                                                                                                         //        c --- d
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
