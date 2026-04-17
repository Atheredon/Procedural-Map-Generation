using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap , Mesh}
    public DrawMode drawMode;

    public const int mapChunkSize = 241; //Max mesh size in unity 255^2 to make an square and make sure vertices count is divisible by even numbers i choose 241 (number of connections is w -1 so 240)
    [Range(0,6)]
    public int levelOfDetail;
    public float mapScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, mapScale, octaves, persistance, lacunarity, offset);

        Color[] colourmap = new Color[noiseMap.Length];

        for (int y = 0; y < mapChunkSize; y++) 
        {
            for(int x = 0; x < mapChunkSize; x++) 
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) 
                {
                    if (currentHeight <= regions[i].MaxHeight)
                    {
                        colourmap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindAnyObjectByType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourmap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourmap, mapChunkSize, mapChunkSize));
        }
    }

    //SafeSwitches
    private void OnValidate()
    {
        if(lacunarity< 1)
            lacunarity = 1;
        if(octaves < 0)
            octaves = 0;
    }

}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float MaxHeight;
    public Color colour;
}
