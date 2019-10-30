using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LevelOfDetailInfo[] detailLevels;
    //public static float maxViewDistance;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TexturedTextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;

    public Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    //static MapGenerator mapGenerator;
    float meshWorldSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        //mapGenerator = FindObjectOfType<MapGenerator>();

        float maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / meshWorldSize);

        UpdateVisibleChuncks(); // might not eval to true in update upon start
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld)
        {
            foreach( TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        // sqrMagnitude = the square distance between the two
        if ( (viewerPositionOld - viewerPosition).sqrMagnitude > sqrMoveThresholdForChunkUpdate )
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChuncks();
        }
    }

    void UpdateVisibleChuncks()
    {
        /*
         *     *----*----*
         *     |    |    |
         *(-1,0)--(0,0)--*      (-240,0)    (0,0)
         *     |    |    |
         *     *----*----(1,-1)                     (240, -240)
         */
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
         
        // go through chuncks and update visibilty status, go backwards to deal with changing array
        for (int i = visibleTerrainChunks.Count-1; i >= 0; i--) {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }
        //visibleTerrainChunks.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // only if we have not already updated, will we update it
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    // need to keep dictionary of coordinates to make sure no terrain is double instantiated!
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        //if (terrainChunkDictionary[viewedChunkCoord].isVisible()) {
                        //    terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                        //}
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, this.transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        // subscribe to on viviblility changed event
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible) visibleTerrainChunks.Add(chunk);
        else visibleTerrainChunks.Remove(chunk);
    }
}

[System.Serializable]
public struct LevelOfDetailInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int levelOfDetail;
    public float visibleDstThreshold;
    //public bool useForCollider;

    public float sqrVisibleDstThreshold
    {
        get
        {
            return visibleDstThreshold * visibleDstThreshold;
        }
    }
}