using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using GXPEngine;

internal class OffGraphWayPointAgent : NodeGraphAgent
{
    //Current target to move towards
    protected Node _target = null;

    protected Queue<Node> _nodesQueue = new Queue<Node>();

    protected Node _lastAdded;
    
    protected Node _parkedNode;

    public OffGraphWayPointAgent(NodeGraph pNodeGraph) : base(pNodeGraph)
    {
        SetOrigin(width / 2, height / 2);

        //position ourselves on a random node
        if (pNodeGraph.nodes.Count > 0)
        {
            var node = pNodeGraph.nodes[Utils.Random(0, pNodeGraph.nodes.Count)];

            _parkedNode = node;

            jumpToNode(node);
        }

        //listen to nodeclicks
        pNodeGraph.OnNodeLeftClicked += OnNodeClickHandler;
    }

    protected virtual void OnNodeClickHandler(Node pNode)
    {
        if (_nodesQueue.Count == 0 && _target == null)
        {
            if (pNode.connections.Contains(_parkedNode))
            {
                _target = pNode;
            }
            return;
        }

        if (_nodesQueue.Count == 0)
        {
            if (pNode.connections.Contains(_target))
            {
                _nodesQueue.Enqueue(pNode);
                _lastAdded = pNode;
            }
            return;
        }

        if (pNode.connections.Contains(_lastAdded))
        {
            _nodesQueue.Enqueue(pNode);
            _lastAdded = pNode;
        }
}

    protected override void Update()
    {
        //no target? Don't walk
        if (_target == null) return;

        //Move towards the target node, if we reached it, clear the target
        if (moveTowardsNode(_target))
        {
            if (_nodesQueue.Count > 0)
            {
                _target = _nodesQueue.Dequeue();
            }
            else
            {
                _parkedNode = _target;
                _target = null;
            }
        }
    }
}