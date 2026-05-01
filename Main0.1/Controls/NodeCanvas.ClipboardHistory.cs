using System.Text.Json;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Controls;

public sealed partial class NodeCanvas
{
    private void CommitDragHistoryCore()
    {
        if (string.IsNullOrWhiteSpace(_dragSnapshotBefore))
        {
            return;
        }

        var after = SerializeGraph(_graph);
        if (after != _dragSnapshotBefore)
        {
            _undoStack.Push(_dragSnapshotBefore);
            _redoStack.Clear();
            GraphChanged?.Invoke(this, new GraphChangedEventArgs(GraphChangeKind.LayoutChanged));
        }
    }

    private void ApplyGraphMutationCore(Action mutation, bool notifySelection, GraphChangeKind changeKind)
    {
        var before = SerializeGraph(_graph);
        mutation();
        var after = SerializeGraph(_graph);
        if (after == before)
        {
            return;
        }

        _undoStack.Push(before);
        _redoStack.Clear();
        Invalidate();
        if (notifySelection)
        {
            SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        }

        GraphChanged?.Invoke(this, new GraphChangedEventArgs(changeKind));
    }

    public void CaptureUndoStateCore()
    {
        _undoStack.Push(SerializeGraph(_graph));
        _redoStack.Clear();
    }

    public void CommitExternalChangeCore()
    {
        Invalidate();
        GraphChanged?.Invoke(this, new GraphChangedEventArgs(GraphChangeKind.General));
    }

    public bool UndoCore()
    {
        if (_undoStack.Count == 0)
        {
            return false;
        }

        _redoStack.Push(SerializeGraph(_graph));
        RestoreSnapshot(_undoStack.Pop());
        return true;
    }

    public bool RedoCore()
    {
        if (_redoStack.Count == 0)
        {
            return false;
        }

        _undoStack.Push(SerializeGraph(_graph));
        RestoreSnapshot(_redoStack.Pop());
        return true;
    }

    private void RestoreSnapshotCore(string snapshot)
    {
        ClearInlineEditorState(commitTextEditor: false);
        _graph = DeserializeGraph(snapshot);
        ClearSelectionInternal();
        _pendingConnection = null;
        _dragNodeOrigins = null;
        _dragSnapshotBefore = null;
        Invalidate();
        SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        GraphChanged?.Invoke(this, new GraphChangedEventArgs(GraphChangeKind.General));
    }

    private void ResetHistoryCore()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    private void CopySelectionToClipboardCore()
    {
        var selectedNodes = GetSelectedNodes().ToList();
        if (selectedNodes.Count == 0)
        {
            return;
        }

        var selectedIds = selectedNodes.Select(node => node.Id).ToHashSet();
        var payload = new NodeGraph
        {
            Nodes = selectedNodes
                .Select(node => new GraphNode
                {
                    Id = node.Id,
                    Kind = node.Kind,
                    X = node.X,
                    Y = node.Y,
                    Properties = new Dictionary<string, string>(node.Properties, StringComparer.OrdinalIgnoreCase),
                    CustomInputs = node.CustomInputs.Select(pin => new NodePinDefinition(pin.Name, pin.DisplayName, pin.Type)).ToList(),
                    CustomOutputs = node.CustomOutputs.Select(pin => new NodePinDefinition(pin.Name, pin.DisplayName, pin.Type)).ToList(),
                })
                .ToList(),
            Connections = _graph.Connections
                .Where(connection => selectedIds.Contains(connection.SourceNodeId) && selectedIds.Contains(connection.TargetNodeId))
                .Select(connection => new GraphConnection
                {
                    SourceNodeId = connection.SourceNodeId,
                    SourcePin = connection.SourcePin,
                    TargetNodeId = connection.TargetNodeId,
                    TargetPin = connection.TargetPin,
                })
                .ToList(),
        };

        Clipboard.SetText(ClipboardPrefix + SerializeGraph(payload));
    }

    private void PasteSelectionFromClipboardCore()
    {
        if (!Clipboard.ContainsText())
        {
            return;
        }

        var text = Clipboard.GetText();
        if (!text.StartsWith(ClipboardPrefix, StringComparison.Ordinal))
        {
            return;
        }

        if (!TryDeserializeGraph(text.Substring(ClipboardPrefix.Length), out var payload))
        {
            return;
        }

        if (payload.Nodes.Count == 0)
        {
            return;
        }

        ApplyGraphMutation(() =>
        {
            var allowedNodes = payload.Nodes
                .Where(node => IsKindAllowed(node.Kind))
                .ToList();
            if (allowedNodes.Count == 0)
            {
                return;
            }

            var minX = allowedNodes.Min(node => node.X);
            var minY = allowedNodes.Min(node => node.Y);
            var pastePoint = ScreenToCanvas(PointToClient(Cursor.Position));
            var idMap = new Dictionary<Guid, Guid>();
            _selectedNodeIds.Clear();

            foreach (var node in allowedNodes)
            {
                var clone = _graph.AddNode(node.Kind, pastePoint.X + (node.X - minX) + 24f, pastePoint.Y + (node.Y - minY) + 24f);
                clone.Properties.Clear();
                foreach (var pair in node.Properties)
                {
                    clone.Properties[pair.Key] = pair.Value;
                }
                clone.CustomInputs.Clear();
                clone.CustomInputs.AddRange(node.CustomInputs.Select(pin => new NodePinDefinition(pin.Name, pin.DisplayName, pin.Type)));
                clone.CustomOutputs.Clear();
                clone.CustomOutputs.AddRange(node.CustomOutputs.Select(pin => new NodePinDefinition(pin.Name, pin.DisplayName, pin.Type)));

                NormalizeNodeForWorkspace(clone, _graph.WorkspaceMode);

                idMap[node.Id] = clone.Id;
                _selectedNodeIds.Add(clone.Id);
                _selectedNode = clone;
            }

            foreach (var connection in payload.Connections)
            {
                if (!idMap.TryGetValue(connection.SourceNodeId, out var newSourceId) ||
                    !idMap.TryGetValue(connection.TargetNodeId, out var newTargetId))
                {
                    continue;
                }

                _graph.AddOrReplaceConnection(new GraphConnection
                {
                    SourceNodeId = newSourceId,
                    SourcePin = connection.SourcePin,
                    TargetNodeId = newTargetId,
                    TargetPin = connection.TargetPin,
                });
            }

            BringSelectionToFront();
        }, notifySelection: true);
    }

    private static string SerializeGraphCore(NodeGraph graph)
    {
        return JsonSerializer.Serialize(graph, JsonOptions);
    }

    private static NodeGraph DeserializeGraphCore(string json)
    {
        return JsonSerializer.Deserialize<NodeGraph>(json, JsonOptions) ?? new NodeGraph();
    }

    private static bool TryDeserializeGraphCore(string json, out NodeGraph graph)
    {
        try
        {
            graph = DeserializeGraph(json);
            return true;
        }
        catch (JsonException)
        {
            graph = new NodeGraph();
            return false;
        }
        catch (NotSupportedException)
        {
            graph = new NodeGraph();
            return false;
        }
    }

    private static void NormalizeNodeForWorkspaceCore(GraphNode node, GraphWorkspaceMode workspaceMode)
    {
        if (node.Kind != NodeKind.Output)
        {
            return;
        }

        node.Properties["Pipeline"] = workspaceMode == GraphWorkspaceMode.ScenePostProcess
            ? "ScenePostProcess"
            : "Object";
    }
}
