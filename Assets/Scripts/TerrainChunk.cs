using UnityEngine;

public class TerrainChunk
{
    const float coliderGenerationDistance = 10f;

    public Vector2 cord;

    public event System.Action<TerrainChunk, bool> onVisibilityChange;

    Transform viewer;

    float maxViewDistance;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;

    GameObject meshObject;
    Vector2 sampleCenter;
    Bounds bounds;

    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;

    int colliderLODIndex;
    bool hasSetColider;

    HeightMap heightMap;
    bool heightMapRecived;

    int previousLODIndex = -1;

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public TerrainChunk(Vector2 cord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material, GameObject waterPrefab, float waterLevel)
    {
        this.cord = cord;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.viewer = viewer;

        Vector2 position = cord * meshSettings.meshWorldSize;
        sampleCenter = cord * meshSettings.meshWorldSize / meshSettings.meshScale;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        meshObject.transform.localScale = Vector3.one;
        SetVisibility(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);

            lodMeshes[i].updateCallback += UpdateChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].updateCallback += UpdateColisionMesh;
            }
        }

        GameObject chunksWater = GameObject.Instantiate(waterPrefab);
        chunksWater.transform.parent = meshObject.transform;
        chunksWater.transform.position = meshObject.transform.position + Vector3.up * waterLevel * meshSettings.meshScale;
        chunksWater.transform.localScale = Vector3.one * ((meshSettings.meshWorldSize) / 50f);

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;


    }

    public void UpdateChunk()
    {
        if (heightMapRecived)
        {
            float viewerDistanceFromNearstEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

            bool wasVisible = isVisiable();
            bool visibility = viewerDistanceFromNearstEdge <= maxViewDistance;


            if (visibility)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++) //no need to check the last item, visibility would be false anyway
                {
                    if (viewerDistanceFromNearstEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }

            }

            if (wasVisible != visibility)
            {
                SetVisibility(visibility);

                if (onVisibilityChange  != null)
                {
                    onVisibilityChange(this, visibility);
                }
            }

        }
    }

    public void UpdateColisionMesh()
    {
        if (!hasSetColider)
        {
            float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshHold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (sqrDistanceFromViewerToEdge < coliderGenerationDistance * coliderGenerationDistance)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetColider = true;
                }
            }
        }
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numberOfVerticesPerLine, meshSettings.numberOfVerticesPerLine, heightMapSettings, sampleCenter), OnHeightMapRecived);
    }

    public void SetVisibility(bool visibility)
    {
        meshObject.SetActive(visibility);
    }

    public bool isVisiable()
    {
        return meshObject.activeSelf;
    }

    void OnHeightMapRecived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapRecived = true;

        UpdateChunk();
    }


}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;

    public event System.Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataRecive);
    }

    void OnMeshDataRecive(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }
}