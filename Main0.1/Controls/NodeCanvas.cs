using System.Drawing.Drawing2D;
using System.Text.Json;
using RayMmdNodeEditor.Graph;

using RayMmdNodeEditor.Services;

namespace RayMmdNodeEditor.Controls;

public sealed partial class NodeCanvas : Control
{
    private const string BackgroundWatermarkText = "RayMmdNodeEditor 0.1.0-preview.1 - 克里斯提亚娜";
    private const float NodeWidth = 260f;
    private const float HeaderHeight = 36f;
    private const float PinRowHeight = 24f;
    private const float PinRadius = 5.5f;
    private const float NodeCornerRadius = 14f;
    private const float GroupNodeWidth = 300f;
    private const float RerouteWidth = 26f;
    private const float RerouteHeight = 16f;
    private const float FrameHeaderHeight = 30f;
    private const float FrameMinWidth = 180f;
    private const float FrameMinHeight = 100f;
    private const float MinZoom = 0.18f;
    private const float MaxZoom = 2.25f;
    private const float SnapDistance = 18f;

    private readonly ContextMenuStrip _canvasMenu = new();
    private readonly ContextMenuStrip _nodeMenu = new();
    private readonly ContextMenuStrip _pinMenu = new();
    private readonly ContextMenuStrip _connectionMenu = new();
    private const string ClipboardPrefix = "MME_NODE_EDITOR_CLIPBOARD:";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private NodeGraph _graph = new();
    private GraphNode? _selectedNode;
    private readonly HashSet<Guid> _selectedNodeIds = [];
    private PointF _dragStartCanvasPoint;
    private Dictionary<Guid, PointF>? _dragNodeOrigins;
    private string? _dragSnapshotBefore;
    private PointF _contextAddPoint;
    private PendingConnection? _pendingConnection;
    private PinHit? _snapPin;
    private PinHit? _hoverPin;
    private GraphNode? _hoverNode;
    private PointF _mouseCanvasPoint;
    private float _zoom = 1f;
    private PointF _panOffset = new(60f, 60f);
    private bool _isPanning;
    private Point _panStartScreen;
    private PointF _panStartOffset;
    private bool _isMarqueeSelecting;
    private bool _marqueeAdditive;
    private PointF _marqueeStartCanvasPoint;
    private RectangleF _marqueeBounds;
    private bool _isCuttingConnections;
    private readonly List<PointF> _cutPath = [];
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();
    private Func<NodeKind, bool>? _nodeKindFilter;
    private Func<GraphNode, string>? _nodeBadgeProvider;
    private Func<NodeDefinition, string>? _searchGroupLabelProvider;
    private string _filterDescription = string.Empty;

    public event EventHandler<GraphChangedEventArgs>? GraphChanged;

    public event EventHandler? SelectedNodeChanged;

    public NodeCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(40, 42, 49);
        ForeColor = Color.FromArgb(230, 230, 230);
        Dock = DockStyle.Fill;
        TabStop = true;
        Font = SystemFonts.MessageBoxFont;
        AllowDrop = true;

        BuildContextMenus();
        InitializeInlineEditorHost();
    }

    public NodeGraph Graph
    {
        get => _graph;
        set
        {
            ClearInlineEditorState(commitTextEditor: false);
            _graph = value;
            _selectedNode = null;
            _selectedNodeIds.Clear();
            _pendingConnection = null;
            _dragNodeOrigins = null;
            _isMarqueeSelecting = false;
            ResetHistory();
            Invalidate();
            SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public GraphNode? SelectedNode => _selectedNode;

    public Func<NodeKind, bool>? NodeKindFilter
    {
        get => _nodeKindFilter;
        set
        {
            _nodeKindFilter = value;
            RefreshContextMenus();
        }
    }

    public string FilterDescription
    {
        get => _filterDescription;
        set
        {
            _filterDescription = value ?? string.Empty;
            RefreshContextMenus();
            Invalidate();
        }
    }

    public Func<GraphNode, string>? NodeBadgeProvider
    {
        get => _nodeBadgeProvider;
        set
        {
            _nodeBadgeProvider = value;
            Invalidate();
        }
    }

    public Func<NodeDefinition, string>? SearchGroupLabelProvider
    {
        get => _searchGroupLabelProvider;
        set => _searchGroupLabelProvider = value;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        DrawGrid(e.Graphics);

        var state = e.Graphics.Save();
        e.Graphics.TranslateTransform(_panOffset.X, _panOffset.Y);
        e.Graphics.ScaleTransform(_zoom, _zoom);
        DrawConnections(e.Graphics);
        DrawPendingConnection(e.Graphics);
        DrawNodes(e.Graphics);
        DrawSelectionRectangle(e.Graphics);
        DrawCutPath(e.Graphics);
        e.Graphics.Restore(state);

        DrawViewportInfo(e.Graphics);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();

        if (e.Button == MouseButtons.Middle)
        {
            BeginPan(e.Location);
            return;
        }

        _mouseCanvasPoint = ScreenToCanvas(e.Location);
        var pinHit = HitTestPin(_mouseCanvasPoint);
        var connectionHit = HitTestConnection(_mouseCanvasPoint);
        var nodeHit = HitTestNode(_mouseCanvasPoint);
        _hoverPin = pinHit;
        _hoverNode = nodeHit;

        if (e.Button == MouseButtons.Left)
        {
            if (TryHandleQuickSpawnClick(pinHit, connectionHit, nodeHit))
            {
                return;
            }

            if ((ModifierKeys & Keys.Alt) == Keys.Alt && pinHit is not null)
            {
                DisconnectByPin(pinHit.Value);
                return;
            }

            if (_pendingConnection is not null && pinHit is not null)
            {
                if (pinHit.Value.IsInput != _pendingConnection.Value.IsInput)
                {
                    CompleteConnection(pinHit.Value);
                }
                else
                {
                    BeginPendingConnection(pinHit.Value);
                }
                return;
            }

            if (pinHit is not null)
            {
                BeginPendingConnection(pinHit.Value);
                Invalidate();
                return;
            }

            if (HandleInlineEditorMouseDown(e, nodeHit))
            {
                return;
            }

            if (nodeHit is not null)
            {
                if ((ModifierKeys & Keys.Control) == Keys.Control)
                {
                    ToggleNodeSelection(nodeHit);
                    return;
                }
                else if (!_selectedNodeIds.Contains(nodeHit.Id))
                {
                    SelectSingleNode(nodeHit);
                }

                _dragStartCanvasPoint = _mouseCanvasPoint;
                _dragNodeOrigins = GetSelectedNodes()
                    .ToDictionary(item => item.Id, item => new PointF(item.X, item.Y));
                foreach (var framedNode in GetFramedChildNodes(GetSelectedNodes()))
                {
                    _dragNodeOrigins.TryAdd(framedNode.Id, new PointF(framedNode.X, framedNode.Y));
                }
                _dragSnapshotBefore = SerializeGraph(_graph);
                BringSelectionToFront();
                return;
            }

            _isMarqueeSelecting = true;
            _marqueeAdditive = (ModifierKeys & Keys.Control) == Keys.Control;
            _marqueeStartCanvasPoint = _mouseCanvasPoint;
            _marqueeBounds = RectangleF.Empty;
            _pendingConnection = null;
            Invalidate();
        }

        if (e.Button == MouseButtons.Right)
        {
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                BeginConnectionCut(_mouseCanvasPoint);
                return;
            }

            if (pinHit is not null && PreparePinMenu(pinHit.Value))
            {
                _pinMenu.Show(this, e.Location);
                return;
            }

            if (connectionHit is not null && PrepareConnectionMenu(connectionHit))
            {
                _connectionMenu.Show(this, e.Location);
                return;
            }

            if (nodeHit is not null)
            {
                SelectNode(nodeHit);
                _nodeMenu.Show(this, e.Location);
                return;
            }

            _contextAddPoint = _mouseCanvasPoint;
            _canvasMenu.Show(this, e.Location);
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);

        if (e.Button != MouseButtons.Left)
        {
            return;
        }

        _mouseCanvasPoint = ScreenToCanvas(e.Location);
        var connectionHit = HitTestConnection(_mouseCanvasPoint);
        if (connectionHit is null)
        {
            var nodeHit = HitTestNode(_mouseCanvasPoint);
            if (HandleInlineEditorDoubleClick(e, nodeHit))
            {
                return;
            }

            if (nodeHit?.Kind == NodeKind.Group)
            {
                SelectSingleNode(nodeHit);
                OpenGroupNodeAction?.Invoke(nodeHit.Id);
                return;
            }

            if (nodeHit?.Kind is NodeKind.GroupInput or NodeKind.GroupOutput)
            {
                SelectSingleNode(nodeHit);
                ExitGroupEditorAction?.Invoke();
            }
            return;
        }

        InsertRerouteOnConnection(connectionHit, _mouseCanvasPoint);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isPanning)
        {
            _panOffset = new PointF(
                _panStartOffset.X + (e.X - _panStartScreen.X),
                _panStartOffset.Y + (e.Y - _panStartScreen.Y));
            Invalidate();
            return;
        }

        _mouseCanvasPoint = ScreenToCanvas(e.Location);

        if (_isCuttingConnections)
        {
            AppendCutPoint(_mouseCanvasPoint);
            _hoverPin = null;
            _hoverNode = null;
            _snapPin = null;
            Invalidate();
            return;
        }

        _hoverPin = HitTestPin(_mouseCanvasPoint);
        _hoverNode = HitTestNode(_mouseCanvasPoint);
        _snapPin = _pendingConnection is not null
            ? FindSnapPin(_mouseCanvasPoint, _pendingConnection.Value)
            : null;

        if (_dragNodeOrigins is not null)
        {
            var deltaX = _mouseCanvasPoint.X - _dragStartCanvasPoint.X;
            var deltaY = _mouseCanvasPoint.Y - _dragStartCanvasPoint.Y;
            foreach (var entry in _dragNodeOrigins)
            {
                var node = _graph.FindNode(entry.Key);
                if (node is null)
                {
                    continue;
                }

                node.X = entry.Value.X + deltaX;
                node.Y = entry.Value.Y + deltaY;
            }
            Invalidate();
        }
        else if (_isMarqueeSelecting)
        {
            _marqueeBounds = NormalizeRect(_marqueeStartCanvasPoint, _mouseCanvasPoint);
            Invalidate();
        }
        else if (_numericDragState is not null)
        {
            HandleInlineEditorMouseMove();
            Invalidate();
        }
        else if (_pendingConnection is not null)
        {
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_isCuttingConnections)
        {
            CompleteConnectionCut();
            _dragNodeOrigins = null;
            _isPanning = false;
            _snapPin = null;
            Cursor = Cursors.Default;
            return;
        }

        if (_pendingConnection is not null &&
            _snapPin is not null &&
            _snapPin.Value.IsInput != _pendingConnection.Value.IsInput)
        {
            CompleteConnection(_snapPin.Value);
        }
        else if (_pendingConnection is not null &&
                 _snapPin is null &&
                 e.Button == MouseButtons.Left)
        {
            _pendingConnection = null;
            Invalidate();
        }

        if (_dragNodeOrigins is not null)
        {
            CommitDragHistory();
        }

        EndInlineEditorDrag();

        if (_isMarqueeSelecting)
        {
            CompleteMarqueeSelection();
        }

        _dragNodeOrigins = null;
        _isPanning = false;
        _snapPin = null;
        Cursor = Cursors.Default;
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        var oldZoom = _zoom;
        var zoomStep = e.Delta > 0 ? 1.1f : 0.9f;
        var nextZoom = Math.Clamp(oldZoom * zoomStep, MinZoom, MaxZoom);
        if (Math.Abs(nextZoom - oldZoom) < 0.0001f)
        {
            return;
        }

        var canvasPoint = ScreenToCanvas(e.Location);
        _zoom = nextZoom;
        _panOffset = new PointF(
            e.X - (canvasPoint.X * _zoom),
            e.Y - (canvasPoint.Y * _zoom));
        Invalidate();
    }

    protected override void OnDragEnter(DragEventArgs drgevent)
    {
        base.OnDragEnter(drgevent);
        drgevent.Effect = TryGetDroppedImageFiles(drgevent.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    protected override void OnDragDrop(DragEventArgs drgevent)
    {
        base.OnDragDrop(drgevent);

        if (!TryGetDroppedImageFiles(drgevent.Data, out var imagePaths))
        {
            return;
        }

        var clientPoint = PointToClient(new Point(drgevent.X, drgevent.Y));
        var canvasPoint = ScreenToCanvas(clientPoint);
        var targetNode = HitTestNode(canvasPoint);
        var firstImagePath = imagePaths[0];

        if (targetNode is not null && SupportsDroppedImageResource(targetNode.Kind))
        {
            ApplyGraphMutation(() =>
            {
                AssignImageToTextureNode(targetNode, firstImagePath);
                SelectSingleNode(targetNode);
            }, notifySelection: true);
            return;
        }
        else
        {
            ApplyGraphMutation(() =>
            {
                GraphNode? firstNode = null;
                for (var index = 0; index < imagePaths.Count; index++)
                {
                    var node = _graph.AddNode(NodeKind.RayTextureSlot, canvasPoint.X + index * 260f, canvasPoint.Y);
                    AssignImageToTextureNode(node, imagePaths[index]);
                    BringNodeToFront(node);
                    firstNode ??= node;
                }

                if (firstNode is not null)
                {
                    SelectSingleNode(firstNode);
                }
            }, notifySelection: true);
            return;
        }
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData == Keys.Delete
            || keyData == (Keys.Control | Keys.D0)
            || keyData == (Keys.Shift | Keys.A)
            || keyData == (Keys.Control | Keys.C)
            || keyData == (Keys.Control | Keys.V)
            || keyData == (Keys.Control | Keys.Z)
            || keyData == (Keys.Control | Keys.Y)
            || keyData == (Keys.Control | Keys.Shift | Keys.Z)
            || keyData == (Keys.Control | Keys.J)
            || keyData == (Keys.Control | Keys.G)
            || QuickSpawnNodeMap.ContainsKey(keyData & Keys.KeyCode)
            || base.IsInputKey(keyData);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Shift | Keys.A))
        {
            OpenNodeSearch();
            return true;
        }

        if (keyData == (Keys.Control | Keys.C))
        {
            CopySelectionToClipboard();
            return true;
        }

        if (keyData == (Keys.Control | Keys.V))
        {
            PasteSelectionFromClipboard();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Z))
        {
            Undo();
            return true;
        }

        if (keyData == (Keys.Control | Keys.Y) || keyData == (Keys.Control | Keys.Shift | Keys.Z))
        {
            Redo();
            return true;
        }

        if (keyData == (Keys.Control | Keys.J))
        {
            CreateFrameFromSelection();
            return true;
        }

        if (keyData == (Keys.Control | Keys.G))
        {
            CreateGroupFromSelectionAction?.Invoke();
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if ((ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) == Keys.None &&
            QuickSpawnNodeMap.ContainsKey(e.KeyCode))
        {
            _activeQuickSpawnKey = e.KeyCode;
        }

        if (e.KeyCode == Keys.Delete)
        {
            DeleteSelectedNode();
        }
        else if (e.KeyCode == Keys.Escape)
        {
            _pendingConnection = null;
            Invalidate();
        }
        else if (e.Control && e.KeyCode == Keys.D0)
        {
            ResetViewport();
        }
        else if (e.Control && (e.KeyCode == Keys.D9 || e.KeyCode == Keys.NumPad9))
        {
            FitGraphToView();
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);

        if (_activeQuickSpawnKey == e.KeyCode)
        {
            _activeQuickSpawnKey = null;
        }
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        _activeQuickSpawnKey = null;
    }

    private void BuildContextMenus()
    {
        BuildContextMenusCore();
    }

    public void RefreshContextMenus()
    {
        RefreshContextMenusCore();
    }

    private void AddScenePostProcessMenus(ContextMenuStrip menu)
    {
        AddScenePostProcessMenusCore(menu);
    }

    private void AddBufferPassMenus(ContextMenuStrip menu)
    {
        AddBufferPassMenusCore(menu);
    }

    private void AddObjectLayerMenu(ContextMenuStrip menu)
    {
        AddObjectLayerMenuCore(menu);
    }

    private void AddGeometryMenu(ContextMenuStrip menu)
    {
        AddGeometryMenuCore(menu);
    }

    private void AddTextureMenu(ContextMenuStrip menu)
    {
        AddTextureMenuCore(menu);
    }

    private void AddShadingMenu(ContextMenuStrip menu)
    {
        AddShadingMenuCore(menu);
    }

    private void AddExperimentalMenu(ContextMenuStrip menu)
    {
        AddExperimentalMenuCore(menu);
    }

    private void AddLayoutMenu(ContextMenuStrip menu)
    {
        AddLayoutMenuCore(menu);
    }

    private void AddCategoryMenu(ContextMenuStrip menu, string title, NodeCategory category)
    {
        AddCategoryMenuCore(menu, title, category);
    }

    private void AddMathMenu(ContextMenuStrip menu)
    {
        AddMathMenuCore(menu);
    }

    private void AddNodeGroup(ToolStripMenuItem parent, string title, params NodeKind[] nodeKinds)
    {
        AddNodeGroupCore(parent, title, nodeKinds);
    }

    private void AddMathGroup(ToolStripMenuItem parent, string title, params NodeKind[] nodeKinds)
    {
        AddMathGroupCore(parent, title, nodeKinds);
    }

    private void AddNodeMenuItem(ToolStripItemCollection items, NodeDefinition definition)
    {
        AddNodeMenuItemCore(items, definition);
    }

    private bool PreparePinMenu(PinHit pinHit)
    {
        return PreparePinMenuCore(pinHit);
    }

    private static bool TryGetDroppedImageFile(IDataObject? dataObject, out string imagePath)
    {
        imagePath = string.Empty;
        if (!TryGetDroppedImageFiles(dataObject, out var imagePaths))
        {
            return false;
        }

        imagePath = imagePaths[0];
        return true;
    }

    private static bool TryGetDroppedImageFiles(IDataObject? dataObject, out List<string> imagePaths)
    {
        imagePaths = [];
        if (dataObject?.GetData(DataFormats.FileDrop) is not string[] files || files.Length == 0)
        {
            return false;
        }

        imagePaths = files.Where(IsSupportedImageFile).ToList();
        return imagePaths.Count > 0;
    }

    private static bool IsSupportedImageFile(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".png" or ".bmp" or ".dds" or ".dib" or ".jpg" or ".jpeg" or ".tga" or ".gif" or ".hdr" or ".exr";
    }

    private static void AssignImageToExternalTextureNode(GraphNode node, string imagePath)
    {
        AssignImageToTextureNode(node, imagePath);
    }

    private static void AssignImageToTextureNode(GraphNode node, string imagePath)
    {
        if (node.Kind != NodeKind.RayTextureSlot)
        {
            return;
        }

        node.Properties["File"] = imagePath.Replace('\\', '/');
        node.Properties["Source"] = string.Equals(Path.GetExtension(imagePath), ".gif", StringComparison.OrdinalIgnoreCase)
            ? "Animated"
            : "File";
    }

    private static bool SupportsDroppedImageResource(NodeKind kind)
    {
        return kind is NodeKind.RayTextureSlot;
    }

    private bool PrepareConnectionMenu(GraphConnection connection)
    {
        return PrepareConnectionMenuCore(connection);
    }

    private void AddNode(NodeKind kind, bool connectPending = false)
    {
        if (!IsKindAllowed(kind))
        {
            return;
        }

        ApplyGraphMutation(() =>
        {
            var node = _graph.AddNode(kind, _contextAddPoint.X, _contextAddPoint.Y);
            if (connectPending)
            {
                TryConnectPendingToNode(node);
            }
            SelectSingleNode(node);
            BringNodeToFront(node);
        }, notifySelection: true);
    }

    private void DeleteSelectedNode()
    {
        if (_selectedNodeIds.Count == 0)
        {
            return;
        }

        ApplyGraphMutation(() =>
        {
            foreach (var nodeId in _selectedNodeIds.ToList())
            {
                _graph.RemoveNode(nodeId);
            }

            ClearSelectionInternal();
            _pendingConnection = null;
        }, notifySelection: true);
    }

    private void CreateFrameFromSelection()
    {
        var contentNodes = GetSelectedNodes()
            .Where(node => !IsFrameNode(node))
            .ToList();
        if (contentNodes.Count == 0)
        {
            return;
        }

        ApplyGraphMutation(() =>
        {
            var minX = contentNodes.Min(node => GetNodeBounds(node).Left);
            var minY = contentNodes.Min(node => GetNodeBounds(node).Top);
            var maxX = contentNodes.Max(node => GetNodeBounds(node).Right);
            var maxY = contentNodes.Max(node => GetNodeBounds(node).Bottom);

            var frame = _graph.AddNode(
                NodeKind.Frame,
                minX - 34f,
                minY - (FrameHeaderHeight + 18f));
            frame.Properties["Width"] = FloatParser.Format(Math.Max(FrameMinWidth, (maxX - minX) + 68f));
            frame.Properties["Height"] = FloatParser.Format(Math.Max(FrameMinHeight, (maxY - minY) + FrameHeaderHeight + 32f));
            frame.Properties["Title"] = "Frame";

            SelectSingleNode(frame);
            BringNodeToFront(frame);
        }, notifySelection: true);
    }

    private IEnumerable<GraphNode> GetFramedChildNodes(IEnumerable<GraphNode> selectedNodes)
    {
        var frames = selectedNodes.Where(node => IsFrameNode(node)).ToList();
        if (frames.Count == 0)
        {
            return Enumerable.Empty<GraphNode>();
        }

        var selectedIds = selectedNodes.Select(node => node.Id).ToHashSet();
        return _graph.Nodes
            .Where(node => !selectedIds.Contains(node.Id))
            .Where(node => !IsFrameNode(node))
            .Where(node => frames.Any(frame => FrameContainsNode(frame, node)))
            .ToList();
    }

    private void InsertRerouteOnConnection(GraphConnection connection, PointF canvasPoint)
    {
        ApplyGraphMutation(() =>
        {
            _graph.RemoveConnection(connection);
            var reroute = _graph.AddNode(
                NodeKind.Reroute,
                canvasPoint.X - (RerouteWidth * 0.5f),
                canvasPoint.Y - (RerouteHeight * 0.5f));
            _graph.AddOrReplaceConnection(new GraphConnection
            {
                SourceNodeId = connection.SourceNodeId,
                SourcePin = connection.SourcePin,
                TargetNodeId = reroute.Id,
                TargetPin = "Value",
            });
            _graph.AddOrReplaceConnection(new GraphConnection
            {
                SourceNodeId = reroute.Id,
                SourcePin = "Value",
                TargetNodeId = connection.TargetNodeId,
                TargetPin = connection.TargetPin,
            });
            SelectSingleNode(reroute);
            BringNodeToFront(reroute);
        }, notifySelection: true, changeKind: GraphChangeKind.ConnectionChanged);
    }

    private void DisconnectByPin(PinHit pinHit)
    {
        ApplyGraphMutation(() =>
        {
            if (pinHit.IsInput)
            {
                _graph.RemoveInputConnection(pinHit.Node.Id, pinHit.Pin.Name);
            }
            else
            {
                _graph.RemoveOutgoingConnections(pinHit.Node.Id, pinHit.Pin.Name);
            }
        }, changeKind: GraphChangeKind.ConnectionChanged);
    }

    private void BeginPendingConnection(PinHit pinHit)
    {
        _pendingConnection = new PendingConnection(
            pinHit.Node.Id,
            pinHit.Pin.Name,
            pinHit.Pin.Type,
            pinHit.IsInput,
            pinHit.Center);
    }

    private void CompleteConnection(PinHit otherHit)
    {
        if (_pendingConnection is null)
        {
            return;
        }

        var pending = _pendingConnection.Value;
        if (pending.IsInput == otherHit.IsInput)
        {
            _pendingConnection = null;
            Invalidate();
            return;
        }

        var sourceNodeId = pending.IsInput ? otherHit.Node.Id : pending.NodeId;
        var sourcePin = pending.IsInput ? otherHit.Pin.Name : pending.PinName;
        var sourceType = pending.IsInput ? otherHit.Pin.Type : pending.Type;
        var targetNodeId = pending.IsInput ? pending.NodeId : otherHit.Node.Id;
        var targetPin = pending.IsInput ? pending.PinName : otherHit.Pin.Name;
        var targetType = pending.IsInput ? pending.Type : otherHit.Pin.Type;

        if (sourceNodeId == targetNodeId)
        {
            _pendingConnection = null;
            Invalidate();
            return;
        }

        if (!AreTypesCompatible(sourceType, targetType))
        {
            _pendingConnection = null;
            Invalidate();
            return;
        }

        if (WouldCreateCycle(sourceNodeId, targetNodeId))
        {
            _pendingConnection = null;
            Invalidate();
            return;
        }

        ApplyGraphMutation(() =>
        {
            _graph.AddOrReplaceConnection(new GraphConnection
            {
                SourceNodeId = sourceNodeId,
                SourcePin = sourcePin,
                TargetNodeId = targetNodeId,
                TargetPin = targetPin,
            });

            _pendingConnection = null;
        }, changeKind: GraphChangeKind.ConnectionChanged);
    }

    private bool WouldCreateCycle(Guid sourceNodeId, Guid targetNodeId)
    {
        var pending = new Queue<Guid>();
        var seen = new HashSet<Guid>();
        pending.Enqueue(targetNodeId);

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            if (!seen.Add(current))
            {
                continue;
            }

            if (current == sourceNodeId)
            {
                return true;
            }

            foreach (var next in _graph.GetOutgoing(current))
            {
                pending.Enqueue(next.TargetNodeId);
            }
        }

        return false;
    }

    private bool CanConnectPendingToNode(NodeDefinition definition)
    {
        if (_pendingConnection is null)
        {
            return true;
        }

        var pending = _pendingConnection.Value;
        var pins = pending.IsInput ? definition.Outputs : definition.Inputs;
        return pins.Any(pin => pending.IsInput
            ? AreTypesCompatible(pin.Type, pending.Type)
            : AreTypesCompatible(pending.Type, pin.Type));
    }

    private int GetConnectionRecommendationScore(NodeDefinition definition)
    {
        if (_pendingConnection is null)
        {
            return 0;
        }

        var pending = _pendingConnection.Value;
        var name = pending.PinName.ToLowerInvariant();
        var kind = definition.Kind;
        var score = 0;
        if (pending.IsInput)
        {
            score += kind is NodeKind.Scalar or NodeKind.Color or NodeKind.RayTextureSlot ? 120 : 0;
            if (name.Contains("smoothness") || name.Contains("metalness") || name.Contains("alpha") || name.Contains("occlusion"))
            {
                score += kind is NodeKind.Scalar or NodeKind.RayTextureSlot or NodeKind.Multiply or NodeKind.OneMinus or NodeKind.ColorRamp ? 90 : 0;
            }
            else if (name.Contains("albedo") || name.Contains("emissive") || name.Contains("specular") || name.Contains("custom"))
            {
                score += kind is NodeKind.Color or NodeKind.RayTextureSlot or NodeKind.ColorRamp or NodeKind.LayerBlend or NodeKind.ColorAdjust ? 90 : 0;
            }
        }
        else
        {
            score += kind is NodeKind.RayMaterialOutput or NodeKind.Multiply or NodeKind.Lerp or NodeKind.LayerBlend ? 80 : 0;
            score += pending.Type is GraphValueType.Float3 or GraphValueType.Float4 && kind is NodeKind.ColorAdjust or NodeKind.RgbCurve or NodeKind.LayerBlend ? 70 : 0;
            score += pending.Type is GraphValueType.Float1 && kind is NodeKind.ColorRamp or NodeKind.Remap or NodeKind.Clamp ? 70 : 0;
        }

        return score;
    }

    private void TryConnectPendingToNode(GraphNode node)
    {
        if (_pendingConnection is null)
        {
            return;
        }

        var pending = _pendingConnection.Value;
        var definition = NodeRegistry.Get(node.Kind);
        if (pending.IsInput)
        {
            var output = definition.Outputs.FirstOrDefault(pin => AreTypesCompatible(pin.Type, pending.Type));
            if (output is null)
            {
                return;
            }

            _graph.AddOrReplaceConnection(new GraphConnection
            {
                SourceNodeId = node.Id,
                SourcePin = output.Name,
                TargetNodeId = pending.NodeId,
                TargetPin = pending.PinName,
            });
        }
        else
        {
            var input = definition.Inputs.FirstOrDefault(pin => AreTypesCompatible(pending.Type, pin.Type));
            if (input is null)
            {
                return;
            }

            _graph.AddOrReplaceConnection(new GraphConnection
            {
                SourceNodeId = pending.NodeId,
                SourcePin = pending.PinName,
                TargetNodeId = node.Id,
                TargetPin = input.Name,
            });
        }

        _pendingConnection = null;
    }

    private void SelectNode(GraphNode? node)
    {
        ClearInlineEditorState(commitTextEditor: true);
        if (_selectedNode?.Id == node?.Id)
        {
            return;
        }

        if (node is null)
        {
            ClearSelectionInternal();
            SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
            return;
        }

        _selectedNodeIds.Clear();
        _selectedNodeIds.Add(node.Id);
        _selectedNode = node;
        SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private void SelectSingleNode(GraphNode node)
    {
        ClearInlineEditorState(commitTextEditor: true);
        _selectedNodeIds.Clear();
        _selectedNodeIds.Add(node.Id);
        _selectedNode = node;
        SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private void ToggleNodeSelection(GraphNode node)
    {
        ClearInlineEditorState(commitTextEditor: true);
        if (_selectedNodeIds.Contains(node.Id))
        {
            _selectedNodeIds.Remove(node.Id);
            if (_selectedNode?.Id == node.Id)
            {
                _selectedNode = GetSelectedNodes().LastOrDefault();
            }
        }
        else
        {
            _selectedNodeIds.Add(node.Id);
            _selectedNode = node;
        }

        SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private IEnumerable<GraphNode> GetSelectedNodes()
    {
        return _graph.Nodes.Where(node => _selectedNodeIds.Contains(node.Id));
    }

    private void ClearSelectionInternal()
    {
        _selectedNodeIds.Clear();
        _selectedNode = null;
    }

    private void BringSelectionToFront()
    {
        var selected = GetSelectedNodes().ToList();
        if (selected.Count == 0)
        {
            return;
        }

        foreach (var node in selected)
        {
            _graph.Nodes.Remove(node);
            _graph.Nodes.Add(node);
        }

        Invalidate();
    }

    private void BringNodeToFront(GraphNode node)
    {
        _graph.Nodes.Remove(node);
        _graph.Nodes.Add(node);
        Invalidate();
    }

    private void BeginPan(Point screenPoint)
    {
        _isPanning = true;
        _panStartScreen = screenPoint;
        _panStartOffset = _panOffset;
        Cursor = Cursors.Hand;
    }

    private void BeginConnectionCut(PointF canvasPoint)
    {
        _isCuttingConnections = true;
        _cutPath.Clear();
        _cutPath.Add(canvasPoint);
        _pendingConnection = null;
        _dragNodeOrigins = null;
        _isMarqueeSelecting = false;
        _marqueeBounds = RectangleF.Empty;
        Cursor = Cursors.Cross;
        Invalidate();
    }

    private void AppendCutPoint(PointF canvasPoint)
    {
        if (_cutPath.Count == 0)
        {
            _cutPath.Add(canvasPoint);
            return;
        }

        if (Distance(_cutPath[^1], canvasPoint) < 2f / _zoom)
        {
            return;
        }

        _cutPath.Add(canvasPoint);
    }

    private void CompleteConnectionCut()
    {
        _isCuttingConnections = false;
        if (_cutPath.Count < 2)
        {
            _cutPath.Clear();
            Invalidate();
            return;
        }

        var hitConnections = _graph.Connections
            .Where(ConnectionIntersectsCutPath)
            .ToList();

        if (hitConnections.Count > 0)
        {
            ApplyGraphMutation(() =>
            {
                foreach (var connection in hitConnections)
                {
                    _graph.RemoveConnection(connection);
                }
            }, changeKind: GraphChangeKind.ConnectionChanged);
        }

        _cutPath.Clear();
        Invalidate();
    }

    private void ResetViewport()
    {
        _zoom = 1f;
        _panOffset = new PointF(60f, 60f);
        Invalidate();
    }

    private void FitGraphToView()
    {
        if (_graph.Nodes.Count == 0)
        {
            ResetViewport();
            return;
        }

        const float horizontalPadding = 72f;
        const float verticalPadding = 88f;

        var bounds = GetGraphBounds();
        var availableWidth = Math.Max(Width - horizontalPadding, 1f);
        var availableHeight = Math.Max(Height - verticalPadding, 1f);
        var targetZoom = Math.Min(
            availableWidth / Math.Max(bounds.Width, 1f),
            availableHeight / Math.Max(bounds.Height, 1f));

        _zoom = Math.Clamp(targetZoom, MinZoom, MaxZoom);
        _panOffset = new PointF(
            ((Width - (bounds.Width * _zoom)) * 0.5f) - (bounds.X * _zoom),
            ((Height - (bounds.Height * _zoom)) * 0.5f) - (bounds.Y * _zoom));
        Invalidate();
    }

    private RectangleF GetGraphBounds()
    {
        var firstBounds = GetNodeBounds(_graph.Nodes[0]);
        var left = firstBounds.Left;
        var top = firstBounds.Top;
        var right = firstBounds.Right;
        var bottom = firstBounds.Bottom;

        foreach (var node in _graph.Nodes.Skip(1))
        {
            var bounds = GetNodeBounds(node);
            left = Math.Min(left, bounds.Left);
            top = Math.Min(top, bounds.Top);
            right = Math.Max(right, bounds.Right);
            bottom = Math.Max(bottom, bounds.Bottom);
        }

        return RectangleF.FromLTRB(left, top, right, bottom);
    }

    private void DrawGrid(Graphics graphics)
    {
        DrawGridCore(graphics);
    }

    private void DrawBackgroundWatermark(Graphics graphics)
    {
        DrawBackgroundWatermarkCore(graphics);
    }

    private void DrawConnections(Graphics graphics)
    {
        DrawConnectionsCore(graphics);
    }

    private void DrawPendingConnection(Graphics graphics)
    {
        DrawPendingConnectionCore(graphics);
    }

    private void DrawSelectionRectangle(Graphics graphics)
    {
        DrawSelectionRectangleCore(graphics);
    }

    private void DrawCutPath(Graphics graphics)
    {
        DrawCutPathCore(graphics);
    }

    private void DrawNodes(Graphics graphics)
    {
        DrawNodesCore(graphics);
    }

    private static IReadOnlyList<NodePinDefinition> GetVisiblePins(GraphNode node, bool input)
    {
        return GetVisiblePinsCore(node, input);
    }

    private void DrawNode(Graphics graphics, GraphNode node)
    {
        DrawNodeCore(graphics, node);
    }

    private static void DrawBezier(Graphics graphics, PointF from, PointF to, Color color, float width)
    {
        DrawBezierCore(graphics, from, to, color, width);
    }

    private void DrawViewportInfo(Graphics graphics)
    {
        DrawViewportInfoCore(graphics);
    }

    private RectangleF GetNodeBounds(GraphNode node)
    {
        return GetNodeBoundsCore(node);
    }

    private static string GetNodeHeaderTitle(GraphNode node, NodeDefinition definition)
    {
        return GetNodeHeaderTitleCore(node, definition);
    }

    private void CompleteMarqueeSelection()
    {
        var rect = _marqueeBounds;
        _isMarqueeSelecting = false;
        _marqueeBounds = RectangleF.Empty;

        if (rect.Width < 4f / _zoom || rect.Height < 4f / _zoom)
        {
            if (!_marqueeAdditive)
            {
                ClearSelectionInternal();
                SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
            }

            Invalidate();
            return;
        }

        var nodes = _graph.Nodes.Where(node => rect.IntersectsWith(GetNodeBounds(node))).ToList();
        if (!_marqueeAdditive)
        {
            _selectedNodeIds.Clear();
        }

        foreach (var node in nodes)
        {
            _selectedNodeIds.Add(node.Id);
        }

        _selectedNode = GetSelectedNodes().LastOrDefault();
        SelectedNodeChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    private void CommitDragHistory()
    {
        CommitDragHistoryCore();
    }

    private void ApplyGraphMutation(Action mutation, bool notifySelection = false, GraphChangeKind changeKind = GraphChangeKind.General)
    {
        ApplyGraphMutationCore(mutation, notifySelection, changeKind);
    }

    public void CaptureUndoState()
    {
        CaptureUndoStateCore();
    }

    public void CommitExternalChange()
    {
        CommitExternalChangeCore();
    }

    public bool Undo()
    {
        return UndoCore();
    }

    public bool Redo()
    {
        return RedoCore();
    }

    private void RestoreSnapshot(string snapshot)
    {
        RestoreSnapshotCore(snapshot);
    }

    private void ResetHistory()
    {
        ResetHistoryCore();
    }

    private void OpenNodeSearch(bool forPendingConnection = false)
    {
        var availableDefinitions = NodeRegistry.All
            .Where(definition =>
                IsKindAllowed(definition.Kind) &&
                IsNodeVisibleInWorkspaceUi(_graph.WorkspaceMode, definition.Kind) &&
                (!forPendingConnection || CanConnectPendingToNode(definition)))
            .ToList();
        var hint = forPendingConnection && _pendingConnection is { } pending
            ? $"连接 {pending.PinName}"
            : _filterDescription;
        using var dialog = new NodeSearchDialog(
            availableDefinitions,
            hint,
            _searchGroupLabelProvider,
            definition => forPendingConnection ? GetConnectionRecommendationScore(definition) : 0);
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.SelectedDefinition is null)
        {
            if (forPendingConnection)
            {
                _pendingConnection = null;
                Invalidate();
            }

            return;
        }

        if (!forPendingConnection)
        {
            _contextAddPoint = ScreenToCanvas(PointToClient(Cursor.Position));
        }

        AddNode(dialog.SelectedDefinition.Kind, connectPending: forPendingConnection);
    }

    public void ShowNodeSearch()
    {
        OpenNodeSearch();
    }

    public IReadOnlyList<Guid> GetSelectedNodeIdsSnapshot()
    {
        return _selectedNodeIds.ToList();
    }

    public void SelectNodeById(Guid nodeId)
    {
        var node = _graph.FindNode(nodeId);
        if (node is null)
        {
            return;
        }

        SelectSingleNode(node);
    }

    private static bool IsNodeVisibleInWorkspaceUi(GraphWorkspaceMode workspaceMode, NodeKind kind)
    {
        return kind switch
        {
            NodeKind.LayerBlend => workspaceMode == GraphWorkspaceMode.ScenePostProcess,
            NodeKind.LayerSource => workspaceMode == GraphWorkspaceMode.ScenePostProcess,
            NodeKind.LayerSourceOutput => workspaceMode is GraphWorkspaceMode.BufferPass or GraphWorkspaceMode.ObjectMaterial,
            _ => true,
        };
    }

    private bool IsKindAllowed(NodeKind kind)
    {
        return _nodeKindFilter?.Invoke(kind) ?? GraphWorkspaceRules.IsAllowed(_graph.WorkspaceMode, kind);
    }

    private void CopySelectionToClipboard()
    {
        CopySelectionToClipboardCore();
    }

    private void PasteSelectionFromClipboard()
    {
        PasteSelectionFromClipboardCore();
    }

    private static string SerializeGraph(NodeGraph graph)
    {
        return SerializeGraphCore(graph);
    }

    private static NodeGraph DeserializeGraph(string json)
    {
        return DeserializeGraphCore(json);
    }

    private static bool TryDeserializeGraph(string json, out NodeGraph graph)
    {
        return TryDeserializeGraphCore(json, out graph);
    }

    private static void NormalizeNodeForWorkspace(GraphNode node, GraphWorkspaceMode workspaceMode)
    {
        NormalizeNodeForWorkspaceCore(node, workspaceMode);
    }

    private static RectangleF NormalizeRect(PointF a, PointF b)
    {
        return NormalizeRectCore(a, b);
    }

    private static PointF GetPinCenter(GraphNode node, NodePinDefinition pin, bool input)
    {
        return GetPinCenterCore(node, pin, input);
    }

    private GraphNode? HitTestNode(PointF canvasPoint)
    {
        return HitTestNodeCore(canvasPoint);
    }

    private PinHit? HitTestPin(PointF canvasPoint)
    {
        return HitTestPinCore(canvasPoint);
    }

    private PinHit? FindSnapPin(PointF canvasPoint, PendingConnection pendingConnection)
    {
        return FindSnapPinCore(canvasPoint, pendingConnection);
    }

    private GraphConnection? HitTestConnection(PointF canvasPoint)
    {
        return HitTestConnectionCore(canvasPoint);
    }

    private bool ConnectionIntersectsCutPath(GraphConnection connection)
    {
        return ConnectionIntersectsCutPathCore(connection);
    }

    private bool TryGetConnectionPoints(GraphConnection connection, out PointF from, out PointF to, out GraphValueType valueType)
    {
        return TryGetConnectionPointsCore(connection, out from, out to, out valueType);
    }

    private PointF ScreenToCanvas(Point screenPoint)
    {
        return ScreenToCanvasCore(screenPoint);
    }

    private static float Distance(PointF point, PointF center)
    {
        return DistanceCore(point, center);
    }

    private static float DistanceToBezier(PointF point, PointF from, PointF to)
    {
        return DistanceToBezierCore(point, from, to);
    }

    private static PointF EvaluateBezier(PointF p0, PointF p1, PointF p2, PointF p3, float t)
    {
        return EvaluateBezierCore(p0, p1, p2, p3, t);
    }

    private static float DistanceToSegment(PointF point, PointF a, PointF b)
    {
        return DistanceToSegmentCore(point, a, b);
    }

    private static bool SegmentsIntersect(PointF a1, PointF a2, PointF b1, PointF b2)
    {
        return SegmentsIntersectCore(a1, a2, b1, b2);
    }

    private static float Cross(PointF origin, PointF target, PointF point)
    {
        return CrossCore(origin, target, point);
    }

    private static bool PointOnSegment(PointF point, PointF a, PointF b)
    {
        return PointOnSegmentCore(point, a, b);
    }

    private static float Mod(float value, float modulo)
    {
        return ModCore(value, modulo);
    }

    private static GraphicsPath CreateRoundedRectangle(RectangleF rect, float radius)
    {
        return CreateRoundedRectangleCore(rect, radius);
    }

    private static GraphicsPath CreateTopRoundedRectangle(RectangleF rect, float radius)
    {
        return CreateTopRoundedRectangleCore(rect, radius);
    }

    private static Color GetPinColor(GraphValueType type)
    {
        return GetPinColorCore(type);
    }

    private static Color BlendColor(Color a, Color b, float t)
    {
        return BlendColorCore(a, b, t);
    }

    private static bool AreTypesCompatible(GraphValueType sourceType, GraphValueType targetType)
    {
        return AreTypesCompatibleCore(sourceType, targetType);
    }

    public Action? CreateGroupFromSelectionAction { get; set; }

    public Action<Guid>? OpenGroupNodeAction { get; set; }

    public Action? ExitGroupEditorAction { get; set; }

    public Action? RenameGroupAction { get; set; }

    public Action? SyncGroupInterfaceAction { get; set; }

    public Action? UngroupAction { get; set; }

    private bool TryHandleQuickSpawnClick(PinHit? pinHit, GraphConnection? connectionHit, GraphNode? nodeHit)
    {
        if (pinHit is not null ||
            connectionHit is not null ||
            nodeHit is not null ||
            _pendingConnection is not null ||
            _activeQuickSpawnKey is null ||
            (ModifierKeys & (Keys.Control | Keys.Alt | Keys.Shift)) != Keys.None ||
            !QuickSpawnNodeMap.TryGetValue(_activeQuickSpawnKey.Value, out var nodeKind))
        {
            return false;
        }

        _contextAddPoint = _mouseCanvasPoint;
        AddNode(nodeKind);
        return true;
    }

    private static bool IsFrameNode(GraphNode node)
    {
        return node.Kind == NodeKind.Frame;
    }

    private static float ReadNodeFloat(GraphNode node, string propertyName, float defaultValue)
    {
        if (!node.Properties.TryGetValue(propertyName, out var rawValue) ||
            !FloatParser.TryParse(rawValue, out var parsedValue))
        {
            return defaultValue;
        }

        return parsedValue;
    }

    private static RectangleF GetFrameBounds(GraphNode node)
    {
        var width = Math.Max(FrameMinWidth, ReadNodeFloat(node, "Width", 420f));
        var height = Math.Max(FrameMinHeight, ReadNodeFloat(node, "Height", 240f));
        return new RectangleF(node.X, node.Y, width, height);
    }

    private bool FrameContainsNode(GraphNode frame, GraphNode node)
    {
        var frameBounds = GetFrameBounds(frame);
        var nodeBounds = GetNodeBounds(node);
        return frameBounds.Contains(nodeBounds.Left, nodeBounds.Top) &&
               frameBounds.Contains(nodeBounds.Right, nodeBounds.Bottom);
    }

    private readonly record struct PinHit(GraphNode Node, NodePinDefinition Pin, bool IsInput, PointF Center);

    private readonly record struct PendingConnection(Guid NodeId, string PinName, GraphValueType Type, bool IsInput, PointF StartPoint);
}
