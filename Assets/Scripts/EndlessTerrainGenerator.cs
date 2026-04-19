using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrainGenerator : MonoBehaviour
{
    public static float maxViewDistance;
    public Transform viewer;

    public static Vector2 viewerPosition;

    public Material mapMaterial;

    public LODInfo[] detailLevels;

    static MapGenerator mapGenerator;

    const float moveThresholdBeforeUpdate = 25.0f;
    const float sqrMoveThresholdBeforeUpdate = moveThresholdBeforeUpdate * moveThresholdBeforeUpdate;
    Vector2 oldViewerPosition;

    int chunkSize;
    int chunksVisibleInView;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisableLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInView = Mathf.RoundToInt(maxViewDistance / chunkSize);
        mapGenerator = FindAnyObjectByType<MapGenerator>();

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if((oldViewerPosition - viewerPosition).sqrMagnitude > sqrMoveThresholdBeforeUpdate)
        {
            oldViewerPosition = viewerPosition;
            UpdateVisibleChunks();
        }

    }

    void UpdateVisibleChunks()
    {
        foreach(var oldChuk in terrainChunksVisableLastUpdate)
        {
            oldChuk.SetVisibility(false);
        }
        terrainChunksVisableLastUpdate.Clear();

        int currentChunkCordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int offsetY = -chunksVisibleInView;  offsetY <= chunksVisibleInView; offsetY++)
        {
            for (int offsetX = -chunksVisibleInView; offsetX <= chunksVisibleInView; offsetX++)
            {
                Vector2 viewedChunkCord = new Vector2(currentChunkCordX + offsetX, currentChunkCordY + offsetY);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCord))
                {
                    terrainChunkDictionary[viewedChunkCord].UpdateChunk();

                    if (terrainChunkDictionary[viewedChunkCord].isVisiable())
                        terrainChunksVisableLastUpdate.Add(terrainChunkDictionary[viewedChunkCord]);
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCord, new TerrainChunk(viewedChunkCord, chunkSize, detailLevels, transform, mapMaterial));
                }

            }
        }

    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;

        MapData mapData;
        bool mapDataRecived;

        int previousLODIndex = -1;

        public TerrainChunk(Vector2 cord, int size, LODInfo[] detailLevels,Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = cord * size;
            bounds = new Bounds(position,Vector2.one * size);
            Vector3 position3D = new Vector3(position.x,0,position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;
            meshObject.transform.position = position3D;
            meshObject.transform.parent = parent;
            SetVisibility(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) 
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataRecived);
        }

        public void UpdateChunk()
        {
            if (mapDataRecived)
            {
                float viewerDistanceFromNearstEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
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

                }

                SetVisibility(visibility);
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

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }


    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;

        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
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
        public int lod;
        public float visibleDistanceThreshold;
    }

}
