using UnityEditor.UIElements;

namespace InsaneScatterbrain.ScriptGraph.Editor
{
    [ConstantNodeView(typeof(int))]
    public class IntConstantNodeView : ConstantNodeView
    {
        public IntConstantNodeView(ConstantNode node, ScriptGraphView graphView) : base(node, graphView)
        {
            var intValue = (int) node.Value;
            AddDefaultField<int, IntegerField>(intValue);
        }
    }
}