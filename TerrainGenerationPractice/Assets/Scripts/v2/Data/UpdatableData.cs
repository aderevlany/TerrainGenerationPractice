using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base class for terrain and noise data
public class UpdatableData : ScriptableObject
{
    public event System.Action OnValuesUpdated;
    public bool autoUpdate;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (autoUpdate) UnityEditor.EditorApplication.update += NotifyOfUpdatedValues;  // to deal with not updating after changes
    }

    public void NotifyOfUpdatedValues()
    {
        UnityEditor.EditorApplication.update -= NotifyOfUpdatedValues;  // unsubscribe as to not constantly update
        if (OnValuesUpdated != null) OnValuesUpdated();
    }

#endif
}
