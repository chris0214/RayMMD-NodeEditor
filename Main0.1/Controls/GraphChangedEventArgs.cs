namespace RayMmdNodeEditor.Controls;

public enum GraphChangeKind
{
    LayoutChanged,
    General,
    ConnectionChanged,
}

public sealed class GraphChangedEventArgs : EventArgs
{
    public GraphChangedEventArgs(GraphChangeKind changeKind)
    {
        ChangeKind = changeKind;
    }

    public GraphChangeKind ChangeKind { get; }
}
