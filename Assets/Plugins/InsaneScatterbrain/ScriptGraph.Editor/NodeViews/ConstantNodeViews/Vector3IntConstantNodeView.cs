using UnityEditor.UIElements;
using UnityEngine;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(Vector3Int))]
    public class Vector3IntConstantNodeView : ConstantNodeView
    {
        public Vector3IntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var vector3IntValue = (Vector3Int) node.Value;
            AddDefaultField<Vector3Int, Vector3IntField>(vector3IntValue);
        }
    }
}