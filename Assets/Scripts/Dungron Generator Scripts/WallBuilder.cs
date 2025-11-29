using System.Collections.Generic;
using UnityEngine;

public static class WallBuilder
{
    // Collect wall positions around one room/corridor
    public static void CollectWallPositions(
        Vector2 bottomLeftCorner,
        Vector2 topRightCorner,
        List<Vector3Int> possibleWallHorizontalPosition,
        List<Vector3Int> possibleWallVerticalPosition,
        List<Vector3Int> possibleDoorHorizontalPosition,
        List<Vector3Int> possibleDoorVerticalPosition)
    {
        Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
        Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
        Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
        Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

        // Horizontal edges (bottom / top) – walls run along X
        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, bottomLeftV.z);
            AddWallPositionToList(
                wallPosition,
                possibleWallHorizontalPosition,
                possibleDoorHorizontalPosition);
        }
        for (int row = (int)topLeftV.x; row < (int)topRightV.x; row++)
        {
            var wallPosition = new Vector3(row, 0, topRightV.z);
            AddWallPositionToList(
                wallPosition,
                possibleWallHorizontalPosition,
                possibleDoorHorizontalPosition);
        }

        // Vertical edges (left / right) – walls run along Z
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            var wallPosition = new Vector3(bottomLeftV.x, 0, col);
            AddWallPositionToList(
                wallPosition,
                possibleWallVerticalPosition,
                possibleDoorVerticalPosition);
        }
        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            var wallPosition = new Vector3(bottomRightV.x, 0, col);
            AddWallPositionToList(
                wallPosition,
                possibleWallVerticalPosition,
                possibleDoorVerticalPosition);
        }
    }

    // Build wall meshes based on collected positions
    public static void CreateWallsMesh(
        Transform wallParent,
        IEnumerable<Vector3Int> horizontalPositions,
        IEnumerable<Vector3Int> verticalPositions,
        Material wallMaterial,
        float wallHeight,
        float wallThickness)
    {
        foreach (var pos in horizontalPositions)
        {
            CreateWallSegmentMesh(wallParent, pos, true, wallMaterial, wallHeight, wallThickness);
        }

        foreach (var pos in verticalPositions)
        {
            CreateWallSegmentMesh(wallParent, pos, false, wallMaterial, wallHeight, wallThickness);
        }
    }

    // horizontal = true  -> wall along X
    // horizontal = false -> wall along Z
    private static void CreateWallSegmentMesh(
        Transform wallParent,
        Vector3Int wallPosition,
        bool horizontal,
        Material wallMaterial,
        float wallHeight,
        float wallThickness)
    {
        float h = Mathf.Max(0.01f, wallHeight);
        float thickness = Mathf.Max(0.01f, wallThickness);

        Mesh mesh = new Mesh();
        Vector3[] vertices;
        int[] triangles = new int[]
        {
            0, 2, 1,
            1, 2, 3
        };
        Vector2[] uvs = new Vector2[4];

        if (horizontal)
        {
            float x = wallPosition.x;
            float z = wallPosition.z;

            vertices = new Vector3[]
            {
                new Vector3(x,     0f, z),
                new Vector3(x + 1, 0f, z),
                new Vector3(x,     h,  z),
                new Vector3(x + 1, h,  z)
            };
        }
        else
        {
            float x = wallPosition.x;
            float z = wallPosition.z;

            vertices = new Vector3[]
            {
                new Vector3(x, 0f,     z),
                new Vector3(x, 0f,     z + 1),
                new Vector3(x, h,      z),
                new Vector3(x, h,      z + 1)
            };
        }

        uvs[0] = new Vector2(0f, 0f);
        uvs[1] = new Vector2(1f, 0f);
        uvs[2] = new Vector2(0f, 1f);
        uvs[3] = new Vector2(1f, 1f);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        // *** Flip normals so lighting faces the opposite way ***
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = -normals[i];
        }
        mesh.normals = normals;

        GameObject wallSegment = new GameObject(
            "Wall",
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(BoxCollider));

        wallSegment.transform.parent = wallParent;
        wallSegment.transform.localPosition = Vector3.zero;
        wallSegment.transform.localRotation = Quaternion.identity;
        wallSegment.transform.localScale = Vector3.one;

        wallSegment.GetComponent<MeshFilter>().mesh = mesh;
        wallSegment.GetComponent<MeshRenderer>().material = wallMaterial;

        BoxCollider col = wallSegment.GetComponent<BoxCollider>();

        if (horizontal)
        {
            col.size = new Vector3(1f, h, thickness);
            col.center = new Vector3(wallPosition.x + 0.5f, h / 2f, wallPosition.z);
        }
        else
        {
            col.size = new Vector3(thickness, h, 1f);
            col.center = new Vector3(wallPosition.x, h / 2f, wallPosition.z + 0.5f);
        }
    }

    // Overlap → turn into door (no wall)
    private static void AddWallPositionToList(
        Vector3 wallPosition,
        List<Vector3Int> wallList,
        List<Vector3Int> doorList)
    {
        Vector3Int point = Vector3Int.CeilToInt(wallPosition);
        if (wallList.Contains(point))
        {
            doorList.Add(point);
            wallList.Remove(point);
        }
        else
        {
            wallList.Add(point);
        }
    }
}
