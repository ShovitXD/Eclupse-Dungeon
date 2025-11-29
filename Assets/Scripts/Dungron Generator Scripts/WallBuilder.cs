using System.Collections.Generic;
using UnityEngine;

public static class WallBuilder
{
    // Per-tile orientation lookups (true/false = which way the quad normal faces)
    private static readonly Dictionary<Vector3Int, bool> horizontalFlipLookup =
        new Dictionary<Vector3Int, bool>();

    private static readonly Dictionary<Vector3Int, bool> verticalFlipLookup =
        new Dictionary<Vector3Int, bool>();

    // Optional: clear cached flip data if you regenerate the dungeon
    public static void ResetFlipLookup()
    {
        horizontalFlipLookup.Clear();
        verticalFlipLookup.Clear();
    }

    // Creates a single wall quad mesh + BoxCollider
    // horizontal = true  -> wall along X, "length" in X (normal ±Z)
    // horizontal = false -> wall along Z, "length" in Z (normal ±X)
    private static void CreateWallSegmentMesh(
        Transform wallParent,
        Vector3Int wallPosition,
        bool horizontal,
        bool flipToCentre,
        Material wallMaterial,
        float wallHeight,
        float wallThickness,
        float length)
    {
        float h = Mathf.Max(0.01f, wallHeight);
        float thickness = Mathf.Max(0.01f, wallThickness);
        float len = Mathf.Max(0.01f, length);

        Mesh mesh = new Mesh();
        Vector3[] vertices;
        Vector2[] uvs = new Vector2[4];

        if (horizontal)
        {
            float x = wallPosition.x;
            float z = wallPosition.z;

            vertices = new Vector3[]
            {
                new Vector3(x,       0f, z),
                new Vector3(x + len, 0f, z),
                new Vector3(x,       h,  z),
                new Vector3(x + len, h,  z)
            };
        }
        else
        {
            float x = wallPosition.x;
            float z = wallPosition.z;

            vertices = new Vector3[]
            {
                new Vector3(x, 0f,       z),
                new Vector3(x, 0f,       z + len),
                new Vector3(x, h,        z),
                new Vector3(x, h,        z + len)
            };
        }

        int[] triangles;

        // flipToCentre controls winding → which side is visible with backface culling
        if (flipToCentre)
        {
            // 0,1,2 → +Z for horizontal, -X for vertical
            triangles = new int[]
            {
                0, 1, 2,
                2, 1, 3
            };
        }
        else
        {
            // 0,2,1 → -Z for horizontal, +X for vertical
            triangles = new int[]
            {
                0, 2, 1,
                1, 2, 3
            };
        }

        // UVs scaled by length
        uvs[0] = new Vector2(0f, 0f);
        uvs[1] = new Vector2(len, 0f);
        uvs[2] = new Vector2(0f, 1f);
        uvs[3] = new Vector2(len, 1f);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

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
            col.size = new Vector3(len, h, thickness);
            col.center = new Vector3(wallPosition.x + len / 2f, h / 2f, wallPosition.z);
        }
        else
        {
            col.size = new Vector3(thickness, h, len);
            col.center = new Vector3(wallPosition.x, h / 2f, wallPosition.z + len / 2f);
        }
    }

    // Collect wall positions around one room/corridor
    // and record per-tile orientation towards THAT room/corridor centre
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

        // Local centre of THIS room / corridor
        float centreX = (bottomLeftCorner.x + topRightCorner.x) * 0.5f;
        float centreZ = (bottomLeftCorner.y + topRightCorner.y) * 0.5f;

        // Horizontal edges (bottom / top) – walls run along X
        for (int row = (int)bottomLeftV.x; row < (int)bottomRightV.x; row++)
        {
            Vector3Int p = Vector3Int.CeilToInt(new Vector3(row, 0, bottomLeftV.z));
            AddHorizontalWall(
                p,
                centreZ,
                possibleWallHorizontalPosition,
                possibleDoorHorizontalPosition);
        }

        for (int row = (int)topLeftV.x; row < (int)topRightV.x; row++)
        {
            Vector3Int p = Vector3Int.CeilToInt(new Vector3(row, 0, topRightV.z));
            AddHorizontalWall(
                p,
                centreZ,
                possibleWallHorizontalPosition,
                possibleDoorHorizontalPosition);
        }

        // Vertical edges (left / right) – walls run along Z
        for (int col = (int)bottomLeftV.z; col < (int)topLeftV.z; col++)
        {
            Vector3Int p = Vector3Int.CeilToInt(new Vector3(bottomLeftV.x, 0, col));
            AddVerticalWall(
                p,
                centreX,
                possibleWallVerticalPosition,
                possibleDoorVerticalPosition);
        }

        for (int col = (int)bottomRightV.z; col < (int)topRightV.z; col++)
        {
            Vector3Int p = Vector3Int.CeilToInt(new Vector3(bottomRightV.x, 0, col));
            AddVerticalWall(
                p,
                centreX,
                possibleWallVerticalPosition,
                possibleDoorVerticalPosition);
        }
    }

    private static void AddHorizontalWall(
        Vector3Int point,
        float centreZ,
        List<Vector3Int> wallList,
        List<Vector3Int> doorList)
    {
        if (wallList.Contains(point))
        {
            // Second time we see this position → becomes door
            wallList.Remove(point);
            doorList.Add(point);
            horizontalFlipLookup.Remove(point);
            return;
        }

        if (doorList.Contains(point))
            return;

        wallList.Add(point);

        // For horizontal: flipToCentre == true → normal +Z, false → -Z
        bool flipToCentre = centreZ > point.z;
        horizontalFlipLookup[point] = flipToCentre;
    }

    private static void AddVerticalWall(
        Vector3Int point,
        float centreX,
        List<Vector3Int> wallList,
        List<Vector3Int> doorList)
    {
        if (wallList.Contains(point))
        {
            wallList.Remove(point);
            doorList.Add(point);
            verticalFlipLookup.Remove(point);
            return;
        }

        if (doorList.Contains(point))
            return;

        wallList.Add(point);

        // For vertical: flipToCentre == true → normal -X, false → +X
        bool flipToCentre = centreX < point.x;
        verticalFlipLookup[point] = flipToCentre;
    }

    private static bool GetHorizontalFlip(Vector3Int p)
    {
        if (horizontalFlipLookup.TryGetValue(p, out bool value))
            return value;

        // Fallback: default +Z if somehow missing
        return true;
    }

    private static bool GetVerticalFlip(Vector3Int p)
    {
        if (verticalFlipLookup.TryGetValue(p, out bool value))
            return value;

        // Fallback: default +X if somehow missing
        return false;
    }

    // Build all wall segments from the collected positions
    public static void CreateWallsMesh(
        Transform wallParent,
        IEnumerable<Vector3Int> horizontalPositions,
        IEnumerable<Vector3Int> verticalPositions,
        Material wallMaterial,
        float wallHeight,
        float wallThickness)
    {
        var horizontalList = new List<Vector3Int>(horizontalPositions);
        var verticalList = new List<Vector3Int>(verticalPositions);

        if (horizontalList.Count == 0 && verticalList.Count == 0)
            return;

        // --- HORIZONTAL WALLS (along X), grouped by Z ---
        var wallsByZ = new Dictionary<int, List<int>>(); // z -> list of x

        foreach (var pos in horizontalList)
        {
            if (!wallsByZ.TryGetValue(pos.z, out var xs))
            {
                xs = new List<int>();
                wallsByZ[pos.z] = xs;
            }

            if (!xs.Contains(pos.x))
                xs.Add(pos.x);
        }

        foreach (var kvp in wallsByZ)
        {
            int z = kvp.Key;
            List<int> xs = kvp.Value;
            xs.Sort();

            if (xs.Count == 0)
                continue;

            int runStart = xs[0];
            int prev = xs[0];
            bool runFlip = GetHorizontalFlip(new Vector3Int(prev, 0, z));

            for (int i = 1; i < xs.Count; i++)
            {
                int current = xs[i];
                bool flip = GetHorizontalFlip(new Vector3Int(current, 0, z));

                bool continuousAndSameFlip = (current == prev + 1) && (flip == runFlip);

                if (continuousAndSameFlip)
                {
                    prev = current;
                    continue;
                }

                float length = (prev - runStart) + 1;
                var startPos = new Vector3Int(runStart, 0, z);
                CreateWallSegmentMesh(
                    wallParent,
                    startPos,
                    true,
                    runFlip,
                    wallMaterial,
                    wallHeight,
                    wallThickness,
                    length);

                runStart = current;
                prev = current;
                runFlip = flip;
            }

            float lastLength = (prev - runStart) + 1;
            var lastPos = new Vector3Int(runStart, 0, z);
            CreateWallSegmentMesh(
                wallParent,
                lastPos,
                true,
                runFlip,
                wallMaterial,
                wallHeight,
                wallThickness,
                lastLength);
        }

        // --- VERTICAL WALLS (along Z), grouped by X ---
        var wallsByX = new Dictionary<int, List<int>>(); // x -> list of z

        foreach (var pos in verticalList)
        {
            if (!wallsByX.TryGetValue(pos.x, out var zs))
            {
                zs = new List<int>();
                wallsByX[pos.x] = zs;
            }

            if (!zs.Contains(pos.z))
                zs.Add(pos.z);
        }

        foreach (var kvp in wallsByX)
        {
            int x = kvp.Key;
            List<int> zs = kvp.Value;
            zs.Sort();

            if (zs.Count == 0)
                continue;

            int runStart = zs[0];
            int prev = zs[0];
            bool runFlip = GetVerticalFlip(new Vector3Int(x, 0, prev));

            for (int i = 1; i < zs.Count; i++)
            {
                int current = zs[i];
                bool flip = GetVerticalFlip(new Vector3Int(x, 0, current));

                bool continuousAndSameFlip = (current == prev + 1) && (flip == runFlip);

                if (continuousAndSameFlip)
                {
                    prev = current;
                    continue;
                }

                float length = (prev - runStart) + 1;
                var startPos = new Vector3Int(x, 0, runStart);
                CreateWallSegmentMesh(
                    wallParent,
                    startPos,
                    false,
                    runFlip,
                    wallMaterial,
                    wallHeight,
                    wallThickness,
                    length);

                runStart = current;
                prev = current;
                runFlip = flip;
            }

            float lastLength = (prev - runStart) + 1;
            var lastPos = new Vector3Int(x, 0, runStart);
            CreateWallSegmentMesh(
                wallParent,
                lastPos,
                false,
                runFlip,
                wallMaterial,
                wallHeight,
                wallThickness,
                lastLength);
        }
    }
}
