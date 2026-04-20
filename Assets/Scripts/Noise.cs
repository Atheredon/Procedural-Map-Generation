using System;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global}

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossiableHeight = 0;

        float amplitude = 1;
        float frequency = 1;
        float noiseHight = 0;

        for ( int i = 0; i<octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000);
            float offsetY = rng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossiableHeight += amplitude;
            amplitude *= persistance;
        }

        if(scale <= 0) scale = 0.0001f;
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //For normalization
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                noiseHight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + offset.x) / scale * frequency + octaveOffsets[i].x * frequency; // x-w/2 to zoom in the center instead of top right corner, *f to multiply the dots, +offset to move around
                    float sampleY = (y - halfHeight - offset.y) / scale * frequency + octaveOffsets[i].y * frequency; ;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) *2 -1; //Made the Range -1 , 1 to cause heigh to lower time to time
                    noiseHight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if(noiseHight >  maxNoiseHeight) 
                    maxNoiseHeight = noiseHight;
                else if(noiseHight < minNoiseHeight) 
                    minNoiseHeight = noiseHight;

                noiseMap[x, y] = noiseHight;

            }
        }

        //Normalize the values
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if(normalizeMode == NormalizeMode.Local)
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1.0f) / (2.0f * maxPossiableHeight / 2f);
                    noiseMap[x,y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }
}
