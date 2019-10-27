using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UpdatableData), true)] // true => do want it to work for dirived classes
public class UpdatableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UpdatableData data = (UpdatableData)target;

        if (GUILayout.Button("Update"))
        {
            data.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target); // notifies that something has changed
        }
    }
}
