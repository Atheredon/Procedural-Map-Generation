using System;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global}

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter)
    {
        System.Random rng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossiableHeight = 0;

        float amplitude = 1;
        float frequency = 1;
        float noiseHight = 0;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + +settings.offset.x + sampleCenter.x;
            float offsetY = rng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossiableHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float[,] noiseMap = new float[mapWidth, mapHeight];

        //For normalization
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                noiseHight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {

                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;// x-w/2 to zoom in the center instead of top right corner, *f to multiply the dots, +offset to move around
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; //Made the Range -1 , 1 to cause heigh to lower time to time
                    noiseHight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }

                if (noiseHight > maxNoiseHeight)
                    maxNoiseHeight = noiseHight;

                if (noiseHight < minNoiseHeight)
                    minNoiseHeight = noiseHight;

                noiseMap[x, y] = noiseHight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1.0f) / (2.0f * maxPossiableHeight / 1.75f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }

            }
        }

        //Normalize the values
        if (settings.normalizeMode == NormalizeMode.Local) {
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;

    public float scale = 50f;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.6f;
    public float lacunarity = 2;
    public int seed;
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}