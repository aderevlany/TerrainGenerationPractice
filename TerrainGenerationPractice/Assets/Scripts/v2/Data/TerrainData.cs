using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale = 1f; // scale x,y,z

    public bool useFlatShading;
    public bool useFalloff;

    public float meshHeightMultiplier;  // scales on y axis
    public AnimationCurve meshHeightCurve;

    public float minHeight
    {
        get
        {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }

    protected override void OnValidate()
    {
        if (meshHeightMultiplier < 1) meshHeightMultiplier = 1;

        base.OnValidate();  // still need base class OnValidate funtionality to autoUpdate
    }
}
