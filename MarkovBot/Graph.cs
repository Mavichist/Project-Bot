using System.Collections.Generic;

namespace MarkovBot
{
    public class Graph<TNode, TEdge> where TNode : IEqualityComparer<TNode>
    {
        public Dictionary<TNode, Dictionary<TNode, TEdge>> Edges
        {
            get;
            set;
        }
        
        public Graph(IEqualityComparer<TNode> nodeComparer)
        {
            Edges = new Dictionary<TNode, Dictionary<TNode, TEdge>>(nodeComparer);
        }

        public void SetEdge(TNode start, TNode end, TEdge value)
        {
            if (!Edges.TryGetValue(start, out Dictionary<TNode, TEdge> edges))
            {
                Edges[start] = edges = new Dictionary<TNode, TEdge>(Edges.Comparer);
            }

            edges[end] = value;
        }
        public TEdge GetEdge(TNode start, TNode end)
        {
            if (Edges.TryGetValue(start, out Dictionary<TNode, TEdge> edges))
            {
                if (edges.TryGetValue(end, out TEdge edge))
                {
                    return edge;
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }
        public void RemoveEdge(TNode start, TNode end)
        {
            if (Edges.TryGetValue(start, out Dictionary<TNode, TEdge> edges))
            {
                edges.Remove(end);
            }
        }
        public void RemoveNode(TNode node)
        {
            if (Edges.Remove(node))
            {
                foreach (var kvp in Edges)
                {
                    kvp.Value.Remove(node);
                }
            }
        }
    }
}