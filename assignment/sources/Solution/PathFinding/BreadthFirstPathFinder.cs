﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GXPEngine;

internal class BreadthFirstPathFinder : PathFinder
{
    protected List<Node> _todoList;
    protected HashSet<Node> _doneList;
    protected bool _done;
    protected bool _isStep;

    protected List<Node> _todoListDebug;
    protected List<Node> _doneListDebug;

    public BreadthFirstPathFinder(NodeGraph pGraph) : base(pGraph)
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

        var path = BFS(pFrom, pTo);

        return path;
    }

    private List<Node> BFS(Node pFrom, Node pTo)
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
                foreach (var connectedNode in currentNode.connections)
                {
                    if (connectedNode.enabled &&
                        !(_todoList.Contains(connectedNode) || _doneList.Contains(connectedNode)))
                    {
                        connectedNode.nodeParent = currentNode;
                        _todoList.Add(connectedNode);
                    }
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

        if (Input.GetKeyDown(Key.D))
        {
            if (_startNode != null)
            {
                _nodeGraph.ActivateNode(_startNode, !_startNode.enabled);
                _nodeGraph.draw();
            }
        }

        if (Input.GetKeyDown(Key.F))
        {
            StepBytepBFS();
        }

        if (Input.GetKeyDown(Key.H))
        {
            if (_startNode != null && _endNode != null)
            {
                Generate(_startNode, _endNode);
                DrawFullPath();
            }
        }
    }

    void StepBytepBFS()
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
                    foreach (var connectedNode in currentNode.connections)
                    {
                        if (connectedNode.enabled &&
                            !(_todoList.Contains(connectedNode) || _doneList.Contains(connectedNode)))
                        {
                            connectedNode.nodeParent = currentNode;
                            _todoList.Add(connectedNode);
                        }
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

    protected void DrawFullPath()
    {
        _outlinePen = new Pen(Color.Black, 4);
        _lastCalculatedPath = _todoListDebug;
        Draw();

        _lastCalculatedPath = BuildPath(_doneList);
        _outlinePen = new Pen(Color.Cyan, 4);
        Draw(false);
    }

    protected void Draw(bool clear = true)
    {
        //to keep things simple we redraw all debug info every frame
        if (clear)
            graphics.Clear(Color.Transparent);

        //draw path if we have one
        if (_lastCalculatedPath != null) drawPath();

        //draw start and end if we have one
        if (_startNode != null) drawNode(_startNode, _startNodeColor);
        if (_endNode != null) drawNode(_endNode, _endNodeColor);
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