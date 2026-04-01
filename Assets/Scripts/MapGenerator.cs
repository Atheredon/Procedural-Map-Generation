using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap , Mesh}
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
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
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, mapScale, octaves, persistance, lacunarity, offset);

        Color[] colourmap = new Color[noiseMap.Length];

        for (int y = 0; y < mapHeight; y++) 
        {
            for(int x = 0; x < mapWidth; x++) 
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) 
                {
                    if (currentHeight <= regions[i].MaxHeight)
                    {
                        colourmap[y * mapWidth + x] = regions[i].colour;
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
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourmap, mapWidth, mapHeight));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHightMultiplier, meshHeightCurve), TextureGenerator.TextureFromColourMap(colourmap, mapWidth, mapHeight));
        }
    }

    //SafeSwitches
    private void OnValidate()
    {
        if(mapWidth < 1)
            mapWidth = 1;
        if(mapHeight < 1)
            mapHeight = 1;
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
