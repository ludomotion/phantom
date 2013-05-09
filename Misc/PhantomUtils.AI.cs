using System;
using System.Collections.Generic;
using System.Text;

namespace Phantom.Misc
{
    public static partial class PhantomUtils
    {
        /// <summary>
        /// A* search algorithm, calculates the shortest path in a weighted graph of nodes from start to end node using a monotonic (consistent) heuristic
        /// </summary>
        /// <typeparam name="T">The node objects</typeparam>
        /// <param name="start">The starting node</param>
        /// <param name="goal">The goal node</param>
        /// <param name="travelableNeighbors">A function that returns all neighbors that are available (e.g. not blocked and traversable) from a given node</param>
        /// <param name="heuristic">A function thet returns the estimated distance from any node to another - this should always be a lower value than the actual distance</param>
        /// <param name="neighborDistance">A function that returns the distance (travel cost) between two given nodes</param>
        /// <returns>The shortest (lowest cost) path from start to goal, or null if no such path exists</returns>
        public static List<T> AStar<T>(T start, T goal, Func<T, T[]> travelableNeighbors, Func<T, T, float> heuristic, Func<T, T, float> neighborDistance)
        {
            List<T> closed = new List<T>();
            LinkedList<T> open = new LinkedList<T>();
            Dictionary<T, T> cameFrom = new Dictionary<T, T>();
            Dictionary<T, float> fScore = new Dictionary<T, float>();
            Dictionary<T, float> gScore = new Dictionary<T, float>();

            open.AddFirst(start);
            gScore[start] = 0;
            fScore[start] = heuristic(start, goal);

            T current;
            float tentativeG;
            float fNeighbor;

            while (open.Count > 0)
            {
                current = open.First.Value;

                if (current.Equals(goal))
                {
                    List<T> result = new List<T>();
                    T node = goal;
                    while (!node.Equals(start))
                    {
                        result.Add(node);
                        node = cameFrom[node];
                    }
                    result.Reverse();

                    return result;
                }

                open.RemoveFirst();
                closed.Add(current);

                foreach (T neighbor in travelableNeighbors(current))
                {
                    if (!gScore.ContainsKey(neighbor)) gScore[neighbor] = float.PositiveInfinity;

                    tentativeG = gScore[current] + neighborDistance(current, neighbor);

                    if (closed.Contains(neighbor) && tentativeG >= gScore[neighbor]) continue;

                    if (!open.Contains(neighbor) || tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fNeighbor = tentativeG + heuristic(neighbor, goal);
                        fScore[neighbor] = fNeighbor;
                        if (!open.Contains(neighbor))
                        {
                            foreach(T openNode in open)
                            {
                                if (fScore[openNode] > fNeighbor)
                                {
                                    open.AddBefore(open.Find(openNode), neighbor);
                                    break;
                                }
                            }
                            open.AddLast(neighbor);
                        }
                    }
                }
            }

            return null;
        }

    }
}
