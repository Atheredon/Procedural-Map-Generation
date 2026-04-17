using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrainGenerator : MonoBehaviour
{
    public const float maxViewDistance = 450.0f;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInView;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisableLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInView = Mathf.RoundToInt(maxViewDistance /  chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    terrainChunkDictionary.Add(viewedChunkCord, new TerrainChunk(viewedChunkCord, chunkSize, transform));
                }

            }
        }

    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        public TerrainChunk(Vector2 cord, int size, Transform parent)
        {
            position = cord * size;
            bounds = new Bounds(position,Vector2.one * size);
            Vector3 position3D = new Vector3(position.x,0,position.y);

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = position3D;
            meshObject.transform.localScale = Vector3.one * (float)size / 10.0f;
            meshObject.transform.parent = parent;
            SetVisibility(false);
        }

        public void UpdateChunk()
        {
            float viewerDistanceFromNearstEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visibility = viewerDistanceFromNearstEdge <= maxViewDistance;
            SetVisibility(visibility);
        }

        public void SetVisibility(bool visibility)
        {
            meshObject.SetActive(visibility);
        }

        public bool isVisiable()
        {
            return meshObject.activeSelf;
        }

    }

}
