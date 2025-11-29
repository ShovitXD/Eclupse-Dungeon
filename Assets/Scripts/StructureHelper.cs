using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class StructureHelper
{
    public static List<Node> TraverseGraphToExtractLowestLeaves(Node parentNode)
    {
        Queue<Node> nodesToCheck = new Queue<Node>();
        List<Node> listToRetirn = new List<Node>();
        if(parentNode.ChildrenNodeLst.Count == 0)
        {
            return new List<Node>() { parentNode };
        }
        foreach(var child in parentNode.ChildrenNodeLst)
        {
            nodesToCheck.Enqueue(child);
        }
        while (nodesToCheck.Count > 0)
        {
            var currentNode = nodesToCheck.Dequeue();
            if(currentNode.ChildrenNodeLst.Count == 0)
            {
                listToRetirn.Add(currentNode);
            }
            else
            {
                foreach(var child in currentNode.ChildrenNodeLst)
                {
                    nodesToCheck.Enqueue(child);
                }
            }
        }
        return listToRetirn;
    }

    public static Vector2Int GenerateBottomLeftCornerBetween(
        Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointmodifier, int offset)
    {
        int minX = boundaryLeftPoint.x + offset;
        int maxX = boundaryRightPoint.x + offset;
        int minY = boundaryLeftPoint.y + offset;
        int maxY = boundaryRightPoint.y + offset;
        return new Vector2Int(
            Random.Range(minX, (int)(minX + (maxX - minX) * pointmodifier)),
            Random.Range(minY,(int)(minY + (maxY - minY)* pointmodifier)));
    }

    public static Vector2Int GenerateTopRightCornerBetween(
        Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointmodifier, int offset)
    {
        int minX = boundaryLeftPoint.x + offset;
        int maxX = boundaryRightPoint.x + offset;
        int minY = boundaryLeftPoint.y + offset;
        int maxY = boundaryRightPoint.y + offset;
        return new Vector2Int
            (Random.Range((int)(minX + (maxX - minX) * pointmodifier), maxX),
             Random.Range((int)(minY + (maxY - minY) * pointmodifier),maxY)   );

    }

    public static Vector2Int CalculateMiddlePoint(Vector2Int v1, Vector2Int v2)
    {
        Vector2 sum = v1 + v2;
        Vector2 tempVector = sum / 2;
        return new Vector2Int((int)tempVector.x, (int)tempVector.y);
    }
}

public enum RelativePosition
{
    Up,
    Down,
    Right,
    Left
}