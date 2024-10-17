using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    [SerializeField] private VeinEntrance _output;
    [SerializeField] private VeinEntrance _input;
    

    private void ValidateCircut(){
        Junction oj = _output.GetJunction();
        Junction ij = _input.GetJunction();

        if(oj == null || ij == null){
            Debug.Log("Heart junctions not connected");
            return;
        }
        
        if (!DFS(oj, ij))
        {
            Debug.Log("No path between haeart junctions");
            return;
        }
        List<Junction> connectedJunctions = BFS(oj);
        if(!IsCircutClosed(connectedJunctions)){
            Debug.Log("circut open");
            return;
        }
        
        if(HasCycle(connectedJunctions)){
            Debug.Log("circut has cycles");
            return;
        }

        Debug.Log("all good!");

    }

    [ContextMenu("Check circut")]
    public void CheckCircut(){

        ValidateCircut();
    }

    private bool IsCircutClosed(List<Junction> junctions){
        foreach( Junction j in junctions){
            if(j.GetPrev().Count == 0 && j != _output.GetJunction()){
                return false;
            }
            if(j.GetNext().Count == 0 && j != _input.GetJunction()){
                return false;
            }
        }
        return true;
    }

    public List<Junction> BFS(Junction start){
        List<Junction> visited = new List<Junction>(); 
        Queue<Junction> queue = new Queue<Junction>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count != 0)
        {
            Junction current = queue.Dequeue();
            
            var nextJunctions = current.GetNext();
            foreach (Junction neighbor in nextJunctions)
            {
                if (!visited.Contains(neighbor))
                {   
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }
        return visited;
    }


    public bool HasCycle(List<Junction> graph)
    {
        // Step 1: Calculate in-degree of each junction
        Dictionary<Junction, int> inDegree = new Dictionary<Junction, int>();

        foreach (var junction in graph)
        {
            inDegree[junction] = 0;
        }

        // Calculate the in-degree of each node
        foreach (var junction in graph)
        {
            foreach (var neighbor in junction.GetNext())
            {
                if (inDegree.ContainsKey(neighbor))
                {
                    inDegree[neighbor]++;
                }
                else
                {
                    inDegree[neighbor] = 1;
                }
            }
        }

        // Step 2: Enqueue all nodes with in-degree 0
        Queue<Junction> queue = new Queue<Junction>();

        foreach (var junction in graph)
        {
            if (inDegree[junction] == 0)
            {
                Debug.Log("a");
                queue.Enqueue(junction);
            }
        }

        int processedCount = 0;
        while (queue.Count > 0)
        {
            Junction current = queue.Dequeue();
            processedCount++;

            foreach (var neighbor in current.GetNext())
            {
                inDegree[neighbor]--;

                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (processedCount == graph.Count)
        {
            return false;  // No cycle
        }

        return true;  // Cycle exists
    }

    public bool DFS(Junction start, Junction target)
    {
        Stack<Junction> stack = new Stack<Junction>();
        List<Junction> visited = new List<Junction>();

        visited.Add(start);
        foreach (Junction neighbor in start.GetNext())  // Assuming GetNext gets the neighbors
        {
            stack.Push(neighbor);
        }

        while (stack.Count > 0)
        {
            Junction current = stack.Pop();

            if (current == target)
            {
                return true;  // Path found
            }

            if (!visited.Contains(current))
            {
                visited.Add(current);

                foreach (Junction neighbor in current.GetNext())
                {
                    if (!visited.Contains(neighbor) && neighbor != start)
                    {
                        stack.Push(neighbor);
                    }
                }
            }
        }

        return false;  // No path found
    }
}

