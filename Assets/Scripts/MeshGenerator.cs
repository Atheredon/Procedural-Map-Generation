using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightmap, MeshSettings meshSettings, int levelOfDetail)
    {
        int meshSimplificationIncrement; //level of detail is 0-4, the numbers divisible by our chunksizes are 1,2,4,6,8
        if (levelOfDetail == 0)
            meshSimplificationIncrement = 1;
        else
            meshSimplificationIncrement = levelOfDetail * 2;

        int borderedSize = heightmap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimlified = borderedSize - 2;

        //To Center Mesh into the screen
        float topLeftX = ((float)meshSizeUnsimlified - 1.0f) / -2.0f;
        float topLeftZ = ((float)meshSizeUnsimlified - 1.0f) / 2.0f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderedVertexIndex = -1;

        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            { 
                bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                if (isBorderVertex)
                {
                    vertexIndicesMap[x,y ] = borderedVertexIndex;
                    borderedVertexIndex--;
                }
                else
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }

        for (int y = 0; y < borderedSize; y+= meshSimplificationIncrement)
        {
            for (int x = 0; x < borderedSize; x+= meshSimplificationIncrement)
            {
                int vertexIndex = vertexIndicesMap[x, y];

                Vector2 percent = new Vector2(((float)x - meshSimplificationIncrement) / (float)meshSize, ((float)y - meshSimplificationIncrement) / (float)meshSize);
                float height = heightmap[x, y];
                Vector3 vertexPosition = new Vector3((topLeftX + percent.x * meshSizeUnsimlified) * meshSettings.meshScale, height, (topLeftZ - percent.y * meshSizeUnsimlified) * meshSettings.meshScale);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);

                if(x < borderedSize - 1 && y < borderedSize - 1)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                    int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                    int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                    meshData.AddTriangle(a,d,c);                 //      a --- b
                    meshData.AddTriangle(d,a,b);                //       |  \  |     tri1: a -> d -> c , tri2: d -> a -> b
                                                               //        c --- d
                }
                vertexIndex++;
            }
        }

        meshData.BakeNormals();

        return meshData;
    }

}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;

    Vector2[] uvs;

    Vector3[] bakedNormals;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int lastTriangleIndex;
    int borderTriangleIndex;

    public MeshData(int verticesPerLine)
    {
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1)* 6];
        uvs = new Vector2[verticesPerLine * verticesPerLine];

        borderVertices = new Vector3[verticesPerLine* 4 + 4];
        borderTriangles = new int[24 * verticesPerLine]; // 6 * 4 * verticesPerLine
    }

    public void AddVertex(Vector3 position, Vector2 uv, int index)
    {
        if(index < 0)
        {
            borderVertices[-index - 1] = position;
        }
        else
        {
            vertices[index] = position;
            uvs[index] = uv;
        }
    }

    public void AddTriangle(int x, int y, int z) 
    {
        if (x < 0 || y < 0 || z < 0)
        {
            borderTriangles[borderTriangleIndex] = x;
            borderTriangles[borderTriangleIndex + 1] = y;
            borderTriangles[borderTriangleIndex + 2] = z;
            borderTriangleIndex += 3;
        }
        else
        {
            triangles[lastTriangleIndex] = x;
            triangles[lastTriangleIndex + 1] = y;
            triangles[lastTriangleIndex + 2] = z;
            lastTriangleIndex += 3;
        }
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = bakedNormals;
        return mesh;
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

            Vector3 normal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += normal;
            vertexNormals[vertexIndexB] += normal;
            vertexNormals[vertexIndexC] += normal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 normal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if(vertexIndexA >= 0)
                vertexNormals[vertexIndexA] += normal;
            if (vertexIndexB >= 0)
                vertexNormals[vertexIndexB] += normal;
            if (vertexIndexC >= 0)
                vertexNormals[vertexIndexC] += normal;
        }

        for (int i = 0; i < vertexNormals.Length; ++i) 
        { 
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }
    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA;
        Vector3 pointB;
        Vector3 pointC;

        if (indexA < 0)
            pointA = borderVertices[-indexA - 1];
        else
            pointA = vertices[indexA];
        if (indexB < 0)
            pointB = borderVertices[-indexB - 1];
        else
            pointB = vertices[indexB];
        if (indexC < 0)
            pointC = borderVertices[-indexC - 1];
        else
            pointC = vertices[indexC];


        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void BakeNormals()
    {
        bakedNormals = CalculateNormals();
    }

}
