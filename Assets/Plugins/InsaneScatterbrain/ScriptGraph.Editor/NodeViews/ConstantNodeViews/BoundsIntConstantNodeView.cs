using UnityEditor.UIElements;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(BoundsInt))]
    public class BoundsIntConstantNodeView : ConstantNodeView
    {
        public BoundsIntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var boundsIntValue = (BoundsInt) node.Value;
            AddDefaultField<BoundsInt, BoundsIntField>(boundsIntValue);
        }
    }
}