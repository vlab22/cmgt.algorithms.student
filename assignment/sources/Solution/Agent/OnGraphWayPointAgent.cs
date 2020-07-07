using System;
using System.Collections.Generic;
using System.Linq;
using GXPEngine;

internal class OnGraphWayPointAgent : OffGraphWayPointAgent
{
    public OnGraphWayPointAgent(NodeGraph pNodeGraph) : base(pNodeGraph)
    {
    }

    protected override void OnNodeClickHandler(Node pNode)
    {
        if (_target == null && pNode != _parkedNode)
        {
            if (pNode.connections.Contains(_parkedNode))
            {
                _target = pNode;
            }
            else
            {
                FindRandomNodeAndBuildQueue(pNode);
                _target = _nodesQueue.Dequeue();
            }
        }
    }

    private void FindRandomNodeAndBuildQueue(Node pNode)
    {
        var origin = _parkedNode;
        Node last = null;

        do
        {
            Node next;

            do
            {
                next = origin.connections[Utils.Random(0, origin.connections.Count)];
            } while (origin.connections.Count > 1 && next == last);

            last = origin;

            if (next == pNode)
            {
                _nodesQueue.Enqueue(pNode);
                break;
            }
            else
            {
                _nodesQueue.Enqueue(next);
                origin = next;
            }
        } while (true);

        Console.WriteLine("\r\nQueue:");
        foreach (var node in _nodesQueue)
        {
            Console.WriteLine(node);
        }
        
        Console.WriteLine($"\r\nQueue Count: {_nodesQueue.Count}");
    }
}