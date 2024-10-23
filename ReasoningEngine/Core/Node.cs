// File: /home/user/code/reasoning-engine/ReasoningEngine/Core/Node.cs

namespace ReasoningEngine
{
    public class NodeV1 : NodeBase
    {
        public override int Version => 1;
        public new string Content { get; set; }

        public NodeV1(long id, string content) : base(id, NodeType.Standard)
        {
            Content = content;
        }

        public override NodeBase UpgradeToLatest()
        {
            return new NodeV2(this.Id, this.Content, this.Type);
        }
    }

    public class NodeV2 : NodeBase
    {
        public override int Version => 2;
        public new string Content { get; set; }

        public NodeV2(long id, string content, NodeType type = NodeType.Standard) : base(id, type)
        {
            Content = content;
        }

        public override NodeBase UpgradeToLatest()
        {
            return this;
        }
    }

    // Alias the latest version as Node for easier use
    public class Node : NodeV2
    {
        public Node(long id, string content, NodeType type = NodeType.Standard) 
            : base(id, content, type) { }
    }
}