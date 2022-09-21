using UnityEditor.UIElements;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Bounds))]
    public class BoundsConstantNodeView : ConstantNodeView
    {
        public BoundsConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var boundsValue = (Bounds) node.Value;
            AddDefaultField<Bounds, BoundsField>(boundsValue);
        }
    }
}