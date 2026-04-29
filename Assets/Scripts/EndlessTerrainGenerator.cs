using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrainGenerator : MonoBehaviour
{
    public static float maxViewDistance;

    public int colliderLODIndex;

    public float waterLevel = 15f;
    public GameObject waterPrefab;

    public Transform viewer;

    public static Vector2 viewerPosition;

    public Material mapMaterial;

    public LODInfo[] detailLevels;

    static MapGenerator mapGenerator;

    const float coliderGenerationDistance = 5f;

    const float moveThresholdBeforeUpdate = 25.0f;
    const float sqrMoveThresholdBeforeUpdate = moveThresholdBeforeUpdate * moveThresholdBeforeUpdate;
    Vector2 oldViewerPosition;

    int chunkSize;
    int chunksVisibleInView;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        chunkSize = mapGenerator.mapChunkSize - 1;
        chunksVisibleInView = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        if(viewerPosition != oldViewerPosition)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateColisionMesh();
            }
        }

        if((oldViewerPosition - viewerPosition).sqrMagnitude > sqrMoveThresholdBeforeUpdate)
        {
            oldViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }

    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> allreadyUpdatedChunkCords = new HashSet<Vector2>();
        for(int i = visibleTerrainChunks.Count -1; i >= 0; i--)
        {
            visibleTerrainChunks[i].UpdateChunk();
            allreadyUpdatedChunkCords.Add(visibleTerrainChunks[i].cord);
        }

        int currentChunkCordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int offsetY = -chunksVisibleInView;  offsetY <= chunksVisibleInView; offsetY++)
        {
            for (int offsetX = -chunksVisibleInView; offsetX <= chunksVisibleInView; offsetX++)
            {
                Vector2 viewedChunkCord = new Vector2(currentChunkCordX + offsetX, currentChunkCordY + offsetY);

                if (!allreadyUpdatedChunkCords.Contains(viewedChunkCord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCord))
                    {
                        terrainChunkDictionary[viewedChunkCord].UpdateChunk();
                    }
                    else
                    {
                        terrainChunkDictionary.Add(viewedChunkCord, new TerrainChunk(viewedChunkCord, chunkSize, detailLevels, colliderLODIndex, transform, mapMaterial, waterPrefab, waterLevel));
                    }
                }

            }
        }

    }

    public class TerrainChunk
    {
        public Vector2 cord;

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        int colliderLODIndex;
        bool hasSetColider;

        MapData mapData;
        bool mapDataRecived;

        int previousLODIndex = -1;

        public TerrainChunk(Vector2 cord, int size, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material, GameObject waterPrefab, float waterLevel)
        {
            this.cord = cord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;

            position = cord * size;
            bounds = new Bounds(position,Vector2.one * size);
            Vector3 position3D = new Vector3(position.x,0,position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;
            meshObject.transform.position = position3D * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            SetVisibility(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) 
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod);

                lodMeshes[i].updateCallback += UpdateChunk;
                if(i == colliderLODIndex)
                {
                    lodMeshes[i].updateCallback += UpdateColisionMesh;
                }
            }

            GameObject chunksWater = GameObject.Instantiate(waterPrefab);
            chunksWater.transform.parent = meshObject.transform;
            chunksWater.transform.position = meshObject.transform.position + Vector3.up * waterLevel * mapGenerator.terrainData.uniformScale;
            chunksWater.transform.localScale = Vector3.one * ((mapGenerator.mapChunkSize - 1.0f) / 50.0f);

            mapGenerator.RequestMapData(position, OnMapDataRecived);
        }

        public void UpdateChunk()
        {
            if (mapDataRecived)
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
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    visibleTerrainChunks.Add(this);

                }

                if (wasVisible != visibility) 
                {
                    if (visibility)
                    {
                        visibleTerrainChunks.Add(this);
                    }
                    else
                    {
                        visibleTerrainChunks.Remove(this);
                    }
                }

                SetVisibility(visibility);
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
                        lodMeshes[colliderLODIndex].RequestMesh(mapData);
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

        public void SetVisibility(bool visibility)
        {
            meshObject.SetActive(visibility);
        }

        public bool isVisiable()
        {
            return meshObject.activeSelf;
        }

        void OnMapDataRecived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataRecived = true;

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

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataRecive);
        }

        void OnMeshDataRecive(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        [Range(0, MeshGenerator.numberOfSuportedLODs - 1)]
        public int lod;
        public float visibleDistanceThreshold;

        public float sqrVisibleDistanceThreshHold
        {
            get { return visibleDistanceThreshold * visibleDistanceThreshold; }
        }
    }

}
