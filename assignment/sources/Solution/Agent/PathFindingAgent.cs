using System.Collections.Generic;
using System.Linq;

internal class PathFindingAgent : OnGraphWayPointAgent
{
    protected PathFinder _pathFinder;

    public PathFindingAgent(NodeGraph pNodeGraph, PathFinder pPathFinder) : base(pNodeGraph)
    {
        _pathFinder = pPathFinder;
    }

    protected override void OnNodeClickHandler(Node pNode)
    {
        var path = _pathFinder.Generate(_target ?? _parkedNode, pNode);
        path.Reverse();
        _nodesQueue = new Queue<Node>(path);

        if (_nodesQueue.Count > 0)
            _target = _nodesQueue.Dequeue();
    }
}