using UnityEditor.UIElements;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Vector3))]
    public class Vector3ConstantNodeView : ConstantNodeView
    {
        public Vector3ConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var vector3Value = (Vector3) node.Value;
            AddDefaultField<Vector3, Vector3Field>(vector3Value);
        }
    }
}