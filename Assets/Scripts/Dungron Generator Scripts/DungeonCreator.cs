using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCreator : MonoBehaviour
{
    public int dungeonWidth, dungeonLength;
    public int roomWidthMin, roomLengthMin;
    public int maxIterations;
    public int corridorWidth;
    public Material material;   // floor material
    public Material wallMaterial; // wall material (if null, uses floor material)

    [Range(0.0f, 0.3f)]
    public float roomBottomCornerModifier;
    [Range(0.7f, 1.0f)]
    public float roomTopCornerMidifier;
    [Range(0, 2)]
    public int roomOffset;

    [Header("Wall settings")]
    public float wallHeight = 3f;
    public float wallThickness = 0.2f;

    // wall positions / doors
    List<Vector3Int> possibleDoorVerticalPosition;
    List<Vector3Int> possibleDoorHorizontalPosition;
    List<Vector3Int> possibleWallHorizontalPosition;
    List<Vector3Int> possibleWallVerticalPosition;

    // Centers to use for spawning
    public Vector3 largestRoomCenter;
    public Vector3 smallestRoomCenter;

    void Start()
    {
        CreateDungeon();
    }

    public void CreateDungeon()
    {
        DestroyAllChildren();

        DugeonGenerator generator = new DugeonGenerator(dungeonWidth, dungeonLength);
        var listOfRooms = generator.CalculateDungeon(
            maxIterations,
            roomWidthMin,
            roomLengthMin,
            roomBottomCornerModifier,
            roomTopCornerMidifier,
            roomOffset,
            corridorWidth);

        GameObject wallParent = new GameObject("WallParent");
        wallParent.transform.parent = transform;

        possibleDoorVerticalPosition = new List<Vector3Int>();
        possibleDoorHorizontalPosition = new List<Vector3Int>();
        possibleWallHorizontalPosition = new List<Vector3Int>();
        possibleWallVerticalPosition = new List<Vector3Int>();

        // Clear per-tile orientation cache before rebuilding
        WallBuilder.ResetFlipLookup();

        float largestArea = 0f;
        float smallestArea = float.MaxValue;
        Vector2 largestBottomLeft = Vector2.zero;
        Vector2 largestTopRight = Vector2.zero;
        Vector2 smallestBottomLeft = Vector2.zero;
        Vector2 smallestTopRight = Vector2.zero;

        for (int i = 0; i < listOfRooms.Count; i++)
        {
            Vector2 bottomLeft = listOfRooms[i].BottomLeftAreaCorner;
            Vector2 topRight = listOfRooms[i].TopRightAreaCorner;

            // Build floor mesh + collider
            CreateMesh(bottomLeft, topRight);

            // Track largest/smallest areas for centers
            float width = topRight.x - bottomLeft.x;
            float length = topRight.y - bottomLeft.y;
            float area = width * length;

            if (area > largestArea)
            {
                largestArea = area;
                largestBottomLeft = bottomLeft;
                largestTopRight = topRight;
            }

            if (area < smallestArea)
            {
                smallestArea = area;
                smallestBottomLeft = bottomLeft;
                smallestTopRight = topRight;
            }

            // Register wall positions for this room/corridor
            WallBuilder.CollectWallPositions(
                bottomLeft,
                topRight,
                possibleWallHorizontalPosition,
                possibleWallVerticalPosition,
                possibleDoorHorizontalPosition,
                possibleDoorVerticalPosition);
        }

        // Build wall meshes (no prefabs)
        Material wallMatToUse = wallMaterial != null ? wallMaterial : material;
        WallBuilder.CreateWallsMesh(
            wallParent.transform,
            possibleWallHorizontalPosition,
            possibleWallVerticalPosition,
            wallMatToUse,
            wallHeight,
            wallThickness);

        // Compute centers
        if (largestArea > 0f)
        {
            largestRoomCenter = new Vector3(
                (largestBottomLeft.x + largestTopRight.x) / 2f,
                0f,
                (largestBottomLeft.y + largestTopRight.y) / 2f);
        }
        else
        {
            largestRoomCenter = Vector3.zero;
        }

        if (smallestArea < float.MaxValue)
        {
            smallestRoomCenter = new Vector3(
                (smallestBottomLeft.x + smallestTopRight.x) / 2f,
                0f,
                (smallestBottomLeft.y + smallestTopRight.y) / 2f);
        }
        else
        {
            smallestRoomCenter = Vector3.zero;
        }
    }

    // Builds a single floor mesh + BoxCollider for a room / corridor
    private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        Vector3[] vertices = new Vector3[]
        {
            topLeftV,
            topRightV,
            bottomLeftV,
            bottomRightV
        };

        Vector2[] uvs = new Vector2[vertices.Length];

        uvs[0] = new Vector2(0, 1);
        uvs[1] = new Vector2(1, 1);
        uvs[2] = new Vector2(0, 0);
        uvs[3] = new Vector2(1, 0);

        int[] triangles = new int[]
        {
            0, 1, 2,
            2, 1, 3
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GameObject floor = new GameObject(
            "Mesh " + bottomLeftCorner,
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(BoxCollider));

        // IMPORTANT: put generated floors on the Floor layer
        floor.layer = LayerMask.NameToLayer("Floor");

        floor.transform.position = Vector3.zero;
        floor.transform.localScale = Vector3.one;
        floor.transform.parent = transform;
        floor.GetComponent<MeshFilter>().mesh = mesh;
        floor.GetComponent<MeshRenderer>().material = material;

        BoxCollider collider = floor.GetComponent<BoxCollider>();
        float width = topRightCorner.x - bottomLeftCorner.x;
        float length = topRightCorner.y - bottomLeftCorner.y;
        collider.size = new Vector3(width, 1, length);
        collider.center = new Vector3(
            bottomLeftCorner.x + width / 2f,
            -0.5f,
            bottomLeftCorner.y + length / 2f);
    }

    private void DestroyAllChildren()
    {
        while (transform.childCount != 0)
        {
            foreach (Transform item in transform)
            {
                DestroyImmediate(item.gameObject);
            }
        }
    }
}
