namespace ReasoningEngine
{
    public class MISONode : Node
    {
        public long? SingleOutputEdgeId { get; private set; }

        public MISONode(long id, string content) 
            : base(id, content, NodeType.MISO)
        {
        }

        public void SetSingleOutputEdge(long edgeId)
        {
            if (SingleOutputEdgeId.HasValue)
                throw new InvalidOperationException("MISO node already has an output edge");
            SingleOutputEdgeId = edgeId;
        }

        public void ClearSingleOutputEdge()
        {
            SingleOutputEdgeId = null;
        }

        public void ChangeSingleOutputEdge(long newEdgeId)
        {
            SingleOutputEdgeId = newEdgeId;
        }
    }
}