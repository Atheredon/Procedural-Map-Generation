using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float moveThresholdBeforeUpdate = 25.0f;
    const float sqrMoveThresholdBeforeUpdate = moveThresholdBeforeUpdate * moveThresholdBeforeUpdate;

    public int colliderLODIndex;

    public float waterLevel = 15f;
    public GameObject waterPrefab;

    public Transform viewer;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;

    public Material mapMaterial;

    public LODInfo[] detailLevels;

    Vector2 viewerPosition;
    Vector2 oldViewerPosition;

    float meshWorldSize;
    int chunksVisibleInView;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInView = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

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
            TerrainChunk chunk = visibleTerrainChunks[i];

            chunk.UpdateChunk();
            allreadyUpdatedChunkCords.Add(chunk.cord);
        }

        int currentChunkCordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

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
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, mapMaterial, waterPrefab, waterLevel);
                        terrainChunkDictionary.Add(viewedChunkCord, newChunk);
                        newChunk.onVisibilityChange += OnChunkVisibilityChange;
                        newChunk.Load();
                    }
                }

            }
        }

    }

    void OnChunkVisibilityChange(TerrainChunk chunk, bool visibility)
    {
        if (visibility)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }

}


[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numberOfSuportedLODs - 1)]
    public int lod;
    public float visibleDistanceThreshold;

    public float sqrVisibleDistanceThreshHold
    {
        get { return visibleDistanceThreshold * visibleDistanceThreshold; }
    }
}
