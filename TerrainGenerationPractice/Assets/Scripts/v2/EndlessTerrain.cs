using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float colliderGenerationDistanceThreshold = 5f;

    public int colliderLODIndex;
    public LevelOfDetailInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDstThreshold;

        chunkSize = mapGenerator.mapChunkSize - 1;  // 1 - the map chunk size
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChuncks(); // might not eval to true in update upon start
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

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

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

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
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, colliderLODIndex, this.transform, mapMaterial));
                    }
                }
            }
        }
    }

    public class TerrainChunk
    {
        public Vector2 coord;

        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;

        LevelOfDetailInfo[] detailLevels;
        int prevLevelOfDetailIndex = -1;    // dont update lod if it is the same as last time
        LevelOfDetailMesh[] levelOfDetailMeshes;
        //LevelOfDetailMesh collisionLODMesh;
        int colliderLODIndex;

        MapData mapData;
        bool mapDataRecieved = false;
        bool hasSetCollider = false;

        public TerrainChunk(Vector2 coord, int size, LevelOfDetailInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            //meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = meshObject.AddComponent<MeshFilter>(); // when adding, returns object that was added
            meshCollider = meshObject.AddComponent<MeshCollider>();

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            //meshObject.transform.localScale = Vector3.one * size / 10f; // default scale is 10 units (for planes)
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            SetVisible(false);

            // make a mesh for each level of detail
            levelOfDetailMeshes = new LevelOfDetailMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                levelOfDetailMeshes[i] = new LevelOfDetailMesh(detailLevels[i].levelOfDetail);
                //if (detailLevels[i].useForCollider) collisionLODMesh = levelOfDetailMeshes[i];

                // since no longer doing it in constructor
                levelOfDetailMeshes[i].updateCallback += UpdateTerrainChunk;
                if (i == colliderLODIndex) levelOfDetailMeshes[i].updateCallback += UpdateCollisionMesh;
            }

            mapGenerator.RequestMapData(position, OnMapDataRecieved);
        }


        void OnMapDataRecieved(MapData mapData)
        {
            // when recieve the mapData, we want to store it
            this.mapData = mapData;
            mapDataRecieved = true;

            UpdateTerrainChunk();
        }
        /*
        void OnMeshDataRecieved(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }*/

        // tell terrian chunk to update itself
        // find point on parameter that is closest to viewers position, find that distance
        // if the distance is less then the max view distance, make sure mesh is enabled, else dissable mesh object
        // dependeing on the distance, display mesh with appropriate level of detail
        public void UpdateTerrainChunk()
        {
            if (mapDataRecieved)
            {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));

                bool wasVisible = this.isVisible();
                bool visible = viewerDstFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int levelOfDetailIndex = 0;
                    // if greater then the last level of detail, it should be deleted
                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                        {
                            levelOfDetailIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (levelOfDetailIndex != prevLevelOfDetailIndex)
                    {
                        LevelOfDetailMesh levelOfDetailMesh = levelOfDetailMeshes[levelOfDetailIndex];
                        if (levelOfDetailMesh.hasMesh)
                        {
                            prevLevelOfDetailIndex = levelOfDetailIndex;
                            meshFilter.mesh = levelOfDetailMesh.mesh;
                            //meshCollider.sharedMesh = levelOfDetailMesh.mesh; // creates too high of complexity mesh
                        }
                        else if (!levelOfDetailMesh.hasRequestedMesh)
                        {
                            levelOfDetailMesh.RequestMesh(mapData);
                        }
                    }

                    // only if the player is close enough, generate the collider (slighty lower then need be as well)
                    /*if (levelOfDetailIndex == 0) {
                        if (collisionLODMesh.hasMesh) meshCollider.sharedMesh = collisionLODMesh.mesh;
                        else if (!collisionLODMesh.hasRequestedMesh) collisionLODMesh.RequestMesh(mapData);
                    }*/

                    //visibleTerrainChunks.Add(this);   // to fix terrain chunks getting displayed but not added to list
                }

                // only update visibility if it has changed
                if (wasVisible != visible) {
                    if (visible) visibleTerrainChunks.Add(this);
                    else visibleTerrainChunks.Remove(this);

                    SetVisible(visible);
                    }
            }
        }

        public void UpdateCollisionMesh()
        {
            if (!hasSetCollider)
            {
                float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if (sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold) {
                    if (!levelOfDetailMeshes[colliderLODIndex].hasRequestedMesh) {
                        levelOfDetailMeshes[colliderLODIndex].RequestMesh(mapData);
                    }
                }

                if (sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold) {
                    if (levelOfDetailMeshes[colliderLODIndex].hasMesh) {
                        meshCollider.sharedMesh = levelOfDetailMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool isVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LevelOfDetailMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh = false;
        public bool hasMesh = false;
        int levelOfDetail;
        public event System.Action updateCallback;

        public LevelOfDetailMesh(int lod)
        {
            levelOfDetail = lod;
            //this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();   // have to manually update the mesh
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, levelOfDetail, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LevelOfDetailInfo
    {
        [Range(0, MeshGenerator.numSupportedLODs-1)]
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
}
