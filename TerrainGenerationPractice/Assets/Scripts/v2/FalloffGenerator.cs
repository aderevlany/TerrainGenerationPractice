using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GenerateFallofMap(int size)
    {
        float[,] map = new float[size,size];

        for (int i = 0; i < size; i++)  {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float)size * 2 - 1; // *2-1 to get a number in the range of 0-1
                float y = j / (float)size * 2 - 1;

                float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    // decreases the effect of the falloff map, making the falloff only affect the edges
    static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        // x^a / ( x^a + (b - bx)^a)    formula for a nice sin like curve _/-

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
