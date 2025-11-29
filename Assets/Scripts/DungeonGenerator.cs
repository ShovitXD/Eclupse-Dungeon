using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class DungeonGenerator
{
    List<RoomNode> allNodeCollection = new List<RoomNode>();
    private int dungeonWidth;
    private int dungeonLength;

    public DungeonGenerator(int dungeonWidth, int dungeonLength)
    {
        this.dungeonWidth = dungeonWidth;
        this.dungeonLength = dungeonLength;
    }

    public List <Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin,
        float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset, int corridorWidth)
    {
        BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth,dungeonLength);
        allNodeCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
        List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);

        RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
        List<RoomNode> roomList = roomGenerator.GenerateRoomInAGivenSpaces(roomSpaces, 
            roomBottomCornerModifier, roomTopCornerModifier,roomOffset);

        CorridorsGenerator corridorsGenerator = new CorridorsGenerator();
        var corridorList = corridorsGenerator.CreateCorridor(allNodeCollection, corridorWidth);

        return new List<Node>(roomList).Concat(corridorList).ToList();
    }
}