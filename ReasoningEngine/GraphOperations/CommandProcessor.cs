using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReasoningEngine.GraphFileHandling;
using DebugUtils;

namespace ReasoningEngine.GraphAccess
{
    public class CommandProcessor
    {
        private readonly GraphFileManager graphFileManager;

        public CommandProcessor(GraphFileManager graphFileManager)
        {
            this.graphFileManager = graphFileManager;
        }

        public virtual string ProcessCommand(string command, string payload)
        {
            switch (command.ToLower())
            {
                case "node_query":
                    return QueryNode(payload);
                case "outgoing_edge_query":
                    return QueryEdges(payload, true);
                case "incoming_edge_query":
                    return QueryEdges(payload, false);
                case "add_node":
                    return AddNode(payload);
                case "delete_node":
                    return DeleteNode(payload);
                case "edit_node":
                    return EditNode(payload);
                case "add_edge":
                    return AddEdge(payload);
                case "delete_edge":
                    return DeleteEdge(payload);
                case "edit_edge":
                    return EditEdge(payload);
                default:
                    return "Unknown command";
            }
        }

        public virtual async Task<string> ProcessCommandAsync(string command, string payload)
        {
            return await Task.Run(() => ProcessCommand(command, payload));
        }

        private string QueryNode(string payload)
        {
            if (long.TryParse(payload, out long nodeId))
            {
                NodeBase? node = graphFileManager.LoadNode(nodeId);
                if (node != null)
                {
                    return $"Node {nodeId}: Version {node.Version}, Content: {(node as dynamic).Content}";
                }
                return $"Node {nodeId} not found.";
            }
            return "Invalid node ID.";
        }

        private string QueryEdges(string payload, bool outgoing)
        {
            if (long.TryParse(payload, out long nodeId))
            {
                List<EdgeBase> edges = graphFileManager.LoadEdges(nodeId, outgoing);
                if (edges.Count > 0)
                {
                    string direction = outgoing ? "Outgoing" : "Incoming";
                    string result = $"{direction} edges for node {nodeId}:\n";
                    foreach (var edge in edges)
                    {
                        string connectedNode = outgoing ? edge.ToNode.ToString() : edge.FromNode.ToString();
                        result += $"Connected Node: {connectedNode}, Version: {edge.Version}, Weight: {(edge as dynamic).Weight}, Content: {(edge as dynamic).EdgeContent}\n";
                    }
                    return result;
                }
                return $"No {(outgoing ? "outgoing" : "incoming")} edges found for node {nodeId}.";
            }
            return "Invalid node ID.";
        }

        private string AddNode(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for adding a node.";
            }
            string content = parts[1];
            Node newNode = new Node(nodeId, content);
            if (graphFileManager.SaveNode(newNode))
            {
                return $"Node {nodeId} added successfully.";
            }
            return $"Failed to add node {nodeId}.";
        }

        private string DeleteNode(string payload)
        {
            if (long.TryParse(payload, out long nodeId))
            {
                if (graphFileManager.DeleteNode(nodeId))
                {
                    return $"Node {nodeId} and all its associated edges have been deleted successfully.";
                }
                else
                {
                    return $"Failed to delete node {nodeId}. It may not exist or an error occurred.";
                }
            }
            return "Invalid node ID.";
        }

        private string EditNode(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long nodeId))
            {
                return "Invalid payload for editing a node.";
            }
            string newContent = parts[1];
            Node updatedNode = new Node(nodeId, newContent);
            if (graphFileManager.SaveNode(updatedNode))
            {
                return $"Node {nodeId} updated successfully.";
            }
            return $"Failed to update node {nodeId}.";
        }

        private string AddEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 4 || !long.TryParse(parts[0], out long fromNodeId) || 
                !long.TryParse(parts[1], out long toNodeId) || !double.TryParse(parts[2], out double weight))
            {
                return "Invalid payload for adding an edge.";
            }
            string content = parts[3];
            Edge newEdge = new Edge(fromNodeId, toNodeId, weight, content);
            if (graphFileManager.SaveEdge(newEdge))
            {
                return $"Edge from {fromNodeId} to {toNodeId} added successfully.";
            }
            return $"Failed to add edge from {fromNodeId} to {toNodeId}. One or both nodes may not exist.";
        }

        private string DeleteEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 2 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId))
            {
                return "Invalid payload for deleting an edge.";
            }
            if (graphFileManager.DeleteEdge(sourceNodeId, destNodeId))
            {
                return $"Edge from node {sourceNodeId} to node {destNodeId} has been deleted successfully.";
            }
            else
            {
                return $"Failed to delete edge from node {sourceNodeId} to node {destNodeId}. It may not exist or an error occurred.";
            }
        }

        private string EditEdge(string payload)
        {
            string[] parts = payload.Split('|');
            if (parts.Length != 4 || !long.TryParse(parts[0], out long sourceNodeId) || 
                !long.TryParse(parts[1], out long destNodeId) || !double.TryParse(parts[2], out double newWeight))
            {
                return "Invalid payload for editing an edge.";
            }
            string newContent = parts[3];
            Edge updatedEdge = new Edge(sourceNodeId, destNodeId, newWeight, newContent);
            if (graphFileManager.SaveEdge(updatedEdge))
            {
                return $"Edge from {sourceNodeId} to {destNodeId} updated successfully.";
            }
            return $"Failed to update edge from {sourceNodeId} to {destNodeId}.";
        }
    }
}