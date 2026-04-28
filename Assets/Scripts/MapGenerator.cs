using System;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class MapGenerator : MonoBehaviour
{
<<<<<<< Updated upstream
    public enum DrawMode { NoiseMap, ColourMap , Mesh, FallofMap}
=======
    public enum DrawMode { NoiseMap, Mesh, FallofMap }
>>>>>>> Stashed changes
    public DrawMode drawMode;

    [Range(0, MeshGenerator.numberOfSuportedChunkSizes - 1)]
    public int chunkSizeIndex;

    [Range(0,MeshGenerator.numberOfSuportedLODs - 1)]
    public int PreviewLOD;

    public bool autoUpdate;

    public TerrainData terrainData;
    public NoiseData noiseData;

    public TerrainType[] regions;

    float[,] fallofMap;

    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    //number of connections is size - 1 so need + 1 amount of vertices + -2 for calculating normals so total values must be -1 of divisible chunk sizes
    public int mapChunkSize { get => MeshGenerator.suportedChunkSizes[chunkSizeIndex] - 1; }

    void OnValuesUpdate()
    {
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    MapData GenerateMapData(Vector2 center)
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, noiseData.mapScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);

<<<<<<< Updated upstream
        Color[] colourmap = new Color[noiseMap.Length];

        for (int y = 0; y < mapChunkSize; y++) 
=======
        if (terrainData.useFallofMap)
>>>>>>> Stashed changes
        {

            if(fallofMap == null)
            {
                fallofMap = FallofGenerator.GenerateFallofMap(mapChunkSize + 2);
            }

            for (int y = 0; y < mapChunkSize + 2; y++)
            {
                for (int x = 0; x < mapChunkSize + 2; x++)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - fallofMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) 
                {
                    if (currentHeight >= regions[i].MaxHeight)
                        colourmap[y * mapChunkSize + x] = regions[i].colour;
                    else 
                        break;
                }
            }

        }

        return new MapData(noiseMap, colourmap);
    }

    public void DrawMapInEditor()
    {
        if (noiseData == null || terrainData == null)
            return;

        MapData mapData = GenerateMapData(Vector2.zero);

        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
<<<<<<< Updated upstream
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHightMultiplier, terrainData.meshHeightCurve, PreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
=======
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHightMultiplier, terrainData.meshHeightCurve, PreviewLOD));
>>>>>>> Stashed changes
        }
        else if (drawMode == DrawMode.FallofMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FallofGenerator.GenerateFallofMap(mapChunkSize)));
        }
    }

    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(center);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHightMultiplier, terrainData.meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count > 0) 
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) 
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    //SafeSwitches
    private void OnValidate()
    {
        if(terrainData != null)
        {
            terrainData.OnUpdate -= OnValuesUpdate; //to prevent suscribeing over and over
            terrainData.OnUpdate += OnValuesUpdate;
        }
        if (noiseData != null)
        {
            noiseData.OnUpdate -= OnValuesUpdate; //to prevent suscribeing over and over
            noiseData.OnUpdate += OnValuesUpdate;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float MaxHeight;
    public Color colour;
}

public struct MapData
{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;

    public MapData(float[,] heightMap, Color[] colourMap)
    {
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}
