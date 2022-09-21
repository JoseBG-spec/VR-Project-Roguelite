using UnityEditor.UIElements;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Rect))]
    public class RectConstantNodeView : ConstantNodeView
    {
        public RectConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var rectValue = (Rect) node.Value;
            AddDefaultField<Rect, RectField>(rectValue);
        }
    }
}