using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale,
                                            int octaves, float persistance, float lacunarity, 
                                            Vector2 offset) {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random seededRandomness = new System.Random(seed);

        // want each octave to be sampled from a different location
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = seededRandomness.Next(-100000, 100000) + offset.x; // offset for adding our
            float offsetY = seededRandomness.Next(-100000, 100000) + offset.y; // own additional offset
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;    // so that scaling is centered
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for ( int i = 0; i < octaves; i++) {
                    // the higher the frequency, the further the sample points, more radical height changes
                    float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;  
                    float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // to get negative values
                    //noiseMap[x, y] = perlinVale;
                    noiseHeight += perlinValue * amplitude; // drama in height

                    amplitude *= persistance; // range 0-1, decreases each octive
                    frequency *= lacunarity; // increases each octive, since it should be greater than 1
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                // returns a value between 0 - 1 for the min and max
                // need to normalise the data
            }
        }

        return noiseMap;
    }
}
