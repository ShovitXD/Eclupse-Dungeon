using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public abstract class Node
{
    private List<Node> childrenNodeList;
    public List<Node> ChildrenNodeLst { get => childrenNodeList;}
    public bool Visited { get; set; }
    public Vector2Int BottomLeftAreaCorner { get; set; }
    public Vector2Int BottomRightAreaCorner { get; set; }
    public Vector2Int TopRightAreaCorner { get; set; }
    public Vector2Int TopLeftAreaCorner { get; set; }
    public int TreeLayerIndex { get; set; }
    public Node Parent { get; set; }
    protected Node(Node parentNode)
    {
        childrenNodeList = new List<Node>();
        this.Parent = parentNode;
        if(parentNode != null)
        {
            parentNode.Addchild(this);
        }
    }
    public void Addchild(Node node)
    {
        childrenNodeList.Add(node);

    }
    public void RemoveChild(Node node)
    {
        childrenNodeList.Remove(node);
    }
}