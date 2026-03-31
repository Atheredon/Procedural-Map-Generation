using System;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for( int i = 0; i<octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + offset.x;
            float offsetY = rng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if(scale <= 0) scale = 0.0001f;
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //For normalization
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 1;
                float noiseHight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    float sampleX = (float)(x - mapWidth/2)/ scale *frequency + octaveOffsets[i].x; // x-w/2 to zoom in the center instead of top right corner, *f to multiply the dots, +offset to move around
                    float sampleY = (float)(y - mapHeight/2) / scale *frequency + octaveOffsets[i].y;

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
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
