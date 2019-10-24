using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for terrain and noise data
public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

    protected virtual void OnValidate()
    {
        if (autoUpdate) NotifyOfUpdatedValues();
    }

    public void NotifyOfUpdatedValues()
    {
        if (OnValuesUpdated != null) OnValuesUpdated();
    }
}
