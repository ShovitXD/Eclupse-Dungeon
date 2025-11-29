using System;
using System.Collections.Generic;
using System.Linq;

public class CorridorsGenerator
{
    public List<Node> CreateCorridor(List<RoomNode> allNodeCollection, int corridorWidth)
    {
        List<Node> corridorList = new List<Node>();
        Queue<RoomNode> structureToCheck = new Queue<RoomNode>
            (allNodeCollection.OrderByDescending(node => node.TreeLayerIndex).ToList());
        while (structureToCheck.Count>0)
        {
            var node = structureToCheck.Dequeue();
            if (node.ChildrenNodeLst.Count == 0)
            {
                continue;
            }
            CorridorNode corridor = new CorridorNode(node.ChildrenNodeLst[0], node.ChildrenNodeLst[1],corridorWidth);
            corridorList.Add(corridor);
        }
        return corridorList;
    }
}