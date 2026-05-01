using System.Drawing.Drawing2D;
using RayMmdNodeEditor.Graph;

namespace RayMmdNodeEditor.Controls;

public sealed partial class NodeCanvas
{
    private static RectangleF NormalizeRectCore(PointF a, PointF b)
    {
        var x = Math.Min(a.X, b.X);
        var y = Math.Min(a.Y, b.Y);
        var width = Math.Abs(a.X - b.X);
        var height = Math.Abs(a.Y - b.Y);
        return new RectangleF(x, y, width, height);
    }

    private static PointF GetPinCenterCore(GraphNode node, NodePinDefinition pin, bool input)
    {
        if (node.Kind == NodeKind.Group && ((input && node.CustomInputs.Count > 0) || (!input && node.CustomOutputs.Count > 0)))
        {
            var customPins = input ? node.CustomInputs : node.CustomOutputs;
            var pinIndex = 0;
            for (var i = 0; i < customPins.Count; i++)
            {
                if (customPins[i].Name == pin.Name)
                {
                    pinIndex = i;
                    break;
                }
            }

            var pinX = input ? node.X : node.X + GroupNodeWidth;
            var pinY = node.Y + HeaderHeight + 30f + (pinIndex * PinRowHeight);
            return new PointF(pinX, pinY);
        }

        if (node.Kind == NodeKind.Reroute)
        {
            var rerouteX = input ? node.X : node.X + RerouteWidth;
            var rerouteY = node.Y + (RerouteHeight * 0.5f);
            return new PointF(rerouteX, rerouteY);
        }

        var pins = GetVisiblePins(node, input);
        var index = 0;
        for (var i = 0; i < pins.Count; i++)
        {
            if (pins[i].Name == pin.Name)
            {
                index = i;
                break;
            }
        }

        var x = input ? node.X : node.X + NodeWidth;
        var y = node.Y + HeaderHeight + 22f + (index * PinRowHeight);
        return new PointF(x, y);
    }

    private GraphNode? HitTestNodeCore(PointF canvasPoint)
    {
        for (var i = _graph.Nodes.Count - 1; i >= 0; i--)
        {
            var node = _graph.Nodes[i];
            if (IsFrameNode(node))
            {
                continue;
            }
            if (GetNodeBounds(node).Contains(canvasPoint))
            {
                return node;
            }
        }

        for (var i = _graph.Nodes.Count - 1; i >= 0; i--)
        {
            var node = _graph.Nodes[i];
            if (!IsFrameNode(node))
            {
                continue;
            }
            if (GetNodeBounds(node).Contains(canvasPoint))
            {
                return node;
            }
        }

        return null;
    }

    private PinHit? HitTestPinCore(PointF canvasPoint)
    {
        foreach (var node in _graph.Nodes)
        {
            foreach (var pin in GetVisiblePins(node, true))
            {
                var center = GetPinCenter(node, pin, true);
                if (Distance(canvasPoint, center) <= PinRadius + 5f)
                {
                    return new PinHit(node, pin, true, center);
                }
            }

            foreach (var pin in GetVisiblePins(node, false))
            {
                var center = GetPinCenter(node, pin, false);
                if (Distance(canvasPoint, center) <= PinRadius + 5f)
                {
                    return new PinHit(node, pin, false, center);
                }
            }
        }

        return null;
    }

    private PinHit? FindSnapPinCore(PointF canvasPoint, PendingConnection pendingConnection)
    {
        PinHit? best = null;
        var bestDistance = SnapDistance / _zoom;

        foreach (var node in _graph.Nodes)
        {
            if (node.Id == pendingConnection.NodeId)
            {
                continue;
            }

            var pins = pendingConnection.IsInput ? GetVisiblePins(node, false) : GetVisiblePins(node, true);
            foreach (var pin in pins)
            {
                var sourceType = pendingConnection.IsInput ? pin.Type : pendingConnection.Type;
                var targetType = pendingConnection.IsInput ? pendingConnection.Type : pin.Type;
                if (!AreTypesCompatible(sourceType, targetType))
                {
                    continue;
                }

                var sourceNodeId = pendingConnection.IsInput ? node.Id : pendingConnection.NodeId;
                var targetNodeId = pendingConnection.IsInput ? pendingConnection.NodeId : node.Id;
                if (WouldCreateCycle(sourceNodeId, targetNodeId))
                {
                    continue;
                }

                var center = GetPinCenter(node, pin, !pendingConnection.IsInput);
                var distance = Distance(canvasPoint, center);
                if (distance > bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                best = new PinHit(node, pin, !pendingConnection.IsInput, center);
            }
        }

        return best;
    }

    private GraphConnection? HitTestConnectionCore(PointF canvasPoint)
    {
        for (var i = _graph.Connections.Count - 1; i >= 0; i--)
        {
            var connection = _graph.Connections[i];
            if (!TryGetConnectionPoints(connection, out var from, out var to, out _))
            {
                continue;
            }

            var distance = DistanceToBezier(canvasPoint, from, to);
            if (distance <= 8f / _zoom)
            {
                return connection;
            }
        }

        return null;
    }

    private bool ConnectionIntersectsCutPathCore(GraphConnection connection)
    {
        if (_cutPath.Count < 2 || !TryGetConnectionPoints(connection, out var from, out var to, out _))
        {
            return false;
        }

        var distance = Math.Max(70f, Math.Abs(to.X - from.X) * 0.48f);
        var cp1 = new PointF(from.X + distance, from.Y);
        var cp2 = new PointF(to.X - distance, to.Y);

        var previousCurvePoint = from;
        for (var curveIndex = 1; curveIndex <= 24; curveIndex++)
        {
            var t = curveIndex / 24f;
            var currentCurvePoint = EvaluateBezier(from, cp1, cp2, to, t);

            for (var cutIndex = 1; cutIndex < _cutPath.Count; cutIndex++)
            {
                var cutStart = _cutPath[cutIndex - 1];
                var cutEnd = _cutPath[cutIndex];
                if (SegmentsIntersect(previousCurvePoint, currentCurvePoint, cutStart, cutEnd))
                {
                    return true;
                }
            }

            previousCurvePoint = currentCurvePoint;
        }

        return false;
    }

    private bool TryGetConnectionPointsCore(GraphConnection connection, out PointF from, out PointF to, out GraphValueType valueType)
    {
        from = PointF.Empty;
        to = PointF.Empty;
        valueType = GraphValueType.Float4;

        var source = _graph.FindNode(connection.SourceNodeId);
        var target = _graph.FindNode(connection.TargetNodeId);
        if (source is null || target is null)
        {
            return false;
        }

        var sourcePin = GetVisiblePins(source, false).FirstOrDefault(pin => pin.Name == connection.SourcePin);
        var targetPin = GetVisiblePins(target, true).FirstOrDefault(pin => pin.Name == connection.TargetPin);
        if (sourcePin is null || targetPin is null)
        {
            return false;
        }

        from = GetPinCenter(source, sourcePin, false);
        to = GetPinCenter(target, targetPin, true);
        valueType = sourcePin.Type;
        return true;
    }

    private PointF ScreenToCanvasCore(Point screenPoint)
    {
        return new PointF(
            (screenPoint.X - _panOffset.X) / _zoom,
            (screenPoint.Y - _panOffset.Y) / _zoom);
    }

    private static float DistanceCore(PointF point, PointF center)
    {
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;
        return MathF.Sqrt((dx * dx) + (dy * dy));
    }

    private static float DistanceToBezierCore(PointF point, PointF from, PointF to)
    {
        var distance = Math.Max(70f, Math.Abs(to.X - from.X) * 0.48f);
        var cp1 = new PointF(from.X + distance, from.Y);
        var cp2 = new PointF(to.X - distance, to.Y);

        var bestDistance = float.MaxValue;
        var previous = from;
        for (var i = 1; i <= 24; i++)
        {
            var t = i / 24f;
            var current = EvaluateBezier(from, cp1, cp2, to, t);
            bestDistance = Math.Min(bestDistance, DistanceToSegment(point, previous, current));
            previous = current;
        }

        return bestDistance;
    }

    private static PointF EvaluateBezierCore(PointF p0, PointF p1, PointF p2, PointF p3, float t)
    {
        var u = 1f - t;
        var uu = u * u;
        var tt = t * t;
        var uuu = uu * u;
        var ttt = tt * t;

        return new PointF(
            (uuu * p0.X) + (3f * uu * t * p1.X) + (3f * u * tt * p2.X) + (ttt * p3.X),
            (uuu * p0.Y) + (3f * uu * t * p1.Y) + (3f * u * tt * p2.Y) + (ttt * p3.Y));
    }

    private static float DistanceToSegmentCore(PointF point, PointF a, PointF b)
    {
        var abX = b.X - a.X;
        var abY = b.Y - a.Y;
        var abLengthSquared = (abX * abX) + (abY * abY);
        if (abLengthSquared <= 0.0001f)
        {
            return Distance(point, a);
        }

        var apX = point.X - a.X;
        var apY = point.Y - a.Y;
        var t = Math.Clamp(((apX * abX) + (apY * abY)) / abLengthSquared, 0f, 1f);
        var closest = new PointF(a.X + (abX * t), a.Y + (abY * t));
        return Distance(point, closest);
    }

    private static bool SegmentsIntersectCore(PointF a1, PointF a2, PointF b1, PointF b2)
    {
        var d1 = Cross(b1, b2, a1);
        var d2 = Cross(b1, b2, a2);
        var d3 = Cross(a1, a2, b1);
        var d4 = Cross(a1, a2, b2);

        if (((d1 > 0f && d2 < 0f) || (d1 < 0f && d2 > 0f)) &&
            ((d3 > 0f && d4 < 0f) || (d3 < 0f && d4 > 0f)))
        {
            return true;
        }

        return (Math.Abs(d1) < 0.0001f && PointOnSegment(a1, b1, b2)) ||
               (Math.Abs(d2) < 0.0001f && PointOnSegment(a2, b1, b2)) ||
               (Math.Abs(d3) < 0.0001f && PointOnSegment(b1, a1, a2)) ||
               (Math.Abs(d4) < 0.0001f && PointOnSegment(b2, a1, a2));
    }

    private static float CrossCore(PointF origin, PointF target, PointF point)
    {
        return ((target.X - origin.X) * (point.Y - origin.Y)) -
               ((target.Y - origin.Y) * (point.X - origin.X));
    }

    private static bool PointOnSegmentCore(PointF point, PointF a, PointF b)
    {
        return point.X >= Math.Min(a.X, b.X) - 0.0001f &&
               point.X <= Math.Max(a.X, b.X) + 0.0001f &&
               point.Y >= Math.Min(a.Y, b.Y) - 0.0001f &&
               point.Y <= Math.Max(a.Y, b.Y) + 0.0001f;
    }

    private static float ModCore(float value, float modulo)
    {
        var result = value % modulo;
        return result < 0f ? result + modulo : result;
    }

    private static GraphicsPath CreateRoundedRectangleCore(RectangleF rect, float radius)
    {
        var diameter = radius * 2f;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180f, 90f);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270f, 90f);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0f, 90f);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90f, 90f);
        path.CloseFigure();
        return path;
    }

    private static GraphicsPath CreateTopRoundedRectangleCore(RectangleF rect, float radius)
    {
        var diameter = radius * 2f;
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, diameter, diameter, 180f, 90f);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270f, 90f);
        path.AddLine(rect.Right, rect.Bottom, rect.X, rect.Bottom);
        path.CloseFigure();
        return path;
    }

    private static Color GetPinColorCore(GraphValueType type)
    {
        return type switch
        {
            GraphValueType.Float1 => Color.FromArgb(188, 132, 255),
            GraphValueType.Float2 => Color.FromArgb(91, 168, 255),
            GraphValueType.Float3 => Color.FromArgb(88, 208, 174),
            GraphValueType.Float4 => Color.FromArgb(245, 164, 84),
            GraphValueType.Material => Color.FromArgb(255, 206, 96),
            _ => Color.Gainsboro,
        };
    }

    private static Color BlendColorCore(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)Math.Round(a.A + ((b.A - a.A) * t)),
            (int)Math.Round(a.R + ((b.R - a.R) * t)),
            (int)Math.Round(a.G + ((b.G - a.G) * t)),
            (int)Math.Round(a.B + ((b.B - a.B) * t)));
    }

    private static bool AreTypesCompatibleCore(GraphValueType sourceType, GraphValueType targetType)
    {
        if (sourceType == GraphValueType.Material || targetType == GraphValueType.Material)
        {
            return sourceType == targetType;
        }

        return true;
    }
}
