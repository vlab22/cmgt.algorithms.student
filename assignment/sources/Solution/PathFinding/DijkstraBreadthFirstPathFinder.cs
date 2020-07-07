using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GXPEngine;

internal class DijkstraBreadthFirstPathFinder : BreadthFirstPathFinder
{
    public DijkstraBreadthFirstPathFinder(NodeGraph pGraph) : base(pGraph)
    {
        _todoList = new List<Node>();
        _doneList = new HashSet<Node>();

        _todoListDebug = new List<Node>();
        _doneListDebug = new List<Node>();
    }

    protected override List<Node> generate(Node pFrom, Node pTo)
    {
        ClearNodesParents();

        _todoList = new List<Node>();
        _doneList = new HashSet<Node>();

        var path = DijkstraBFS(pFrom, pTo);

        return path;
    }

    private List<Node> DijkstraBFS(Node pFrom, Node pTo)
    {
        _todoList.Add(pFrom);

        bool done = false;

        while (_todoList.Count > 0 && done == false)
        {
            var currentNode = _todoList.First();
            _todoList.Remove(currentNode);
            _doneList.Add(currentNode);
            
            _todoListDebug.Add(currentNode);
            _doneListDebug.Add(currentNode);

            if (currentNode == pTo)
            {
                done = true;
            }
            else
            {
                for (int i = 0; i < currentNode.connections.Count; i++)
                {
                    var connectedNode = currentNode.connections[i];

                    if (connectedNode.enabled &&
                        !(_todoList.Contains(connectedNode) || _doneList.Contains(connectedNode)))
                    {
                        float distanceFromStart = PointTools.PointDistance(currentNode.location, _startNode.location);
                        float distanceToNew = PointTools.PointDistance(connectedNode.location, currentNode.location);

                        connectedNode.distanceCost = distanceFromStart + distanceToNew;
                    }
                    else
                    {
                        connectedNode.distanceCost = -1;
                    }
                }

                var sortedConnections =
                    currentNode.connections.Where(n => n.distanceCost > 0).OrderBy(n => n.distanceCost);

                foreach (var sortedConnection in sortedConnections)
                {
                    var connectedNode = sortedConnection;
                    connectedNode.nodeParent = currentNode;
                    _todoList.Add(connectedNode);
                }
            }
        }

        if (done == false)
            return new List<Node>();

        var path = BuildPath(_doneList);

        Console.WriteLine("\r\nPath:");
        foreach (var n in path)
        {
            Console.WriteLine($"{n.id}");
        }

        Console.WriteLine("");

        return path;
    }

    List<Node> BuildPath(ICollection<Node> doneList)
    {
        var path = new List<Node>();
        var pathNode = doneList.Last();
        while (pathNode.nodeParent != null)
        {
            path.Add(pathNode);
            pathNode = pathNode.nodeParent;
        }

        path.Add(pathNode);

        return path;
    }

    protected override void handleInput()
    {
        base.handleInput();

        if (Input.GetKeyDown(Key.F))
        {
            StepBytepDijkstraBFS();
        }
    }

    void StepBytepDijkstraBFS()
    {
        if (_startNode != null && _endNode != null)
        {
            if (_isStep == false)
            {
                _isStep = true;
                _todoList.Clear();
                _doneList.Clear();
                _todoListDebug.Clear();
                _doneListDebug.Clear();

                ClearNodesParents();

                _done = false;

                _todoList.Add(_startNode);
                _todoListDebug.Add(_startNode);
            }

            if (_todoList.Count > 0 && _done == false)
            {
                var currentNode = _todoList.First();
                _todoList.Remove(currentNode);
                _doneList.Add(currentNode);

                _todoListDebug.Add(currentNode);
                _doneListDebug.Add(currentNode);

                if (currentNode == _endNode)
                {
                    _done = true;
                }
                else
                {
                    for (int i = 0; i < currentNode.connections.Count; i++)
                    {
                        var connectedNode = currentNode.connections[i];

                        if (connectedNode.enabled &&
                            !(_todoList.Contains(connectedNode) || _doneList.Contains(connectedNode)))
                        {
                            float distanceFromStart =
                                PointTools.PointDistance(currentNode.location, _startNode.location);
                            float distanceToNew =
                                PointTools.PointDistance(connectedNode.location, currentNode.location);

                            connectedNode.distanceCost = distanceFromStart + distanceToNew;
                        }
                        else
                        {
                            connectedNode.distanceCost = -1;
                        }
                    }

                    var sortedConnections =
                        currentNode.connections.Where(n => n.distanceCost > 0).OrderBy(n => n.distanceCost);

                    foreach (var sortedConnection in sortedConnections)
                    {
                        var connectedNode = sortedConnection;
                        connectedNode.nodeParent = currentNode;
                        _todoList.Add(connectedNode);
                    }
                }
            }

            if (_done)
            {
                _isStep = false;
                Generate(_startNode, _endNode);
            }
            else
            {
                DrawFullPath();
            }
        }
        else
        {
            _isStep = false;
        }
    }

    protected override void ClearPaths()
    {
        base.ClearPaths();

        _isStep = false;
    }

    void ClearNodesParents()
    {
        for (int i = 0; i < _nodeGraph.nodes.Count; i++)
        {
            _nodeGraph.nodes[i].nodeParent = null;
        }
    }
}