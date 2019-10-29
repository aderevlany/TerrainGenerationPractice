using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global } // local min and max, generate map all at once; global min max, generate chunk by chunk (terrain height)

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter) {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random seededRandomness = new System.Random(settings.seed);

        float maxPossibleHeight = 0;   // for global normalization mode
        float amplitude = 1;
        float frequency = 1;

        // want each octave to be sampled from a different location
        Vector2[] octaveOffsets = new Vector2[settings.octaves];
        for (int i = 0; i < settings.octaves; i++) {
            float offsetX = seededRandomness.Next(-100000, 100000) + settings.offset.x + sampleCenter.x; // offset for adding our
            float offsetY = seededRandomness.Next(-100000, 100000) - settings.offset.y - sampleCenter.y; // own additional offset, y-axis is switched
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;    // so that scaling is centered
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for ( int i = 0; i < settings.octaves; i++) {
                    // the higher the frequency, the further the sample points, more radical height changes
                    //float sampleX = (x-halfWidth) / scale * frequency + octaveOffsets[i].x;  
                    //float sampleY = (y-halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;   // so that landmass does not change with offset
                    float sampleY = (y- halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // to get negative values
                    //noiseMap[x, y] = perlinVale;
                    noiseHeight += perlinValue * amplitude; // drama in height

                    amplitude *= settings.persistance; // range 0-1, decreases each octive
                    frequency *= settings.lacunarity; // increases each octive, since it should be greater than 1
                }

                if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
                if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;

                // GLOBAL
                if (settings.normalizeMode == NormalizeMode.Global) { 
                    // If generating map chunk by chunk, you do not and will have differing max and mins per chunk
                    // need to figure max/min height that can have
                    // some points are going to exceed the maxPossibleHeight innevitably, need to account for it (num  dividing maxPosHeight)
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f);   // undo perlin value math + maxPossibleHeight value
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        // LOCAL
        if (settings.normalizeMode == NormalizeMode.Local) {
            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    // returns a value between 0 - 1 for the min and max
                    // need to normalise the data
                    // If generating the entire map at once, you know what the max and min are
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]); // PREFERED WAY IF NOT DOING ENDLESS TERRAIN!!! <- <- <-
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

    public float scale = 50;    // default values

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
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
