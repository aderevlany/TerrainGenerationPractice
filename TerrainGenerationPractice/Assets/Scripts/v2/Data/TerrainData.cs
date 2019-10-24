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

    protected override void OnValidate()
    {
        if (meshHeightMultiplier < 1) meshHeightMultiplier = 1;

        base.OnValidate();  // still need base class OnValidate funtionality to autoUpdate
    }
}
