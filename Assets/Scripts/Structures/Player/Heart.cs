using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heart : MonoBehaviour
{
    [SerializeField] private VeinEntrance _output;
    [SerializeField] private VeinEntrance _input;

    private List<Junction> _connectedOutputs = new List<Junction>();
    private List<Junction> _connectedJunctions = new List<Junction>();

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
        _connectedJunctions = BFS(oj);
        if(!IsCircutClosed(_connectedJunctions)){
            Debug.Log("circut open");
            return;
        }
        
        if(HasCycle(_connectedJunctions)){
            Debug.Log("circut has cycles");
            return;
        }

        Debug.Log("all good!");
        ClearResources();
        foreach(Junction o in _connectedOutputs){
            if(o!=oj){
                UpdateResources(o);
            }
        }

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
        List<Junction> outputJunctions = new List<Junction>();
        List<Junction> inputJunctions = new List<Junction>(); 

        
        _connectedOutputs.Clear();

        Queue<Junction> queue = new Queue<Junction>();

        List<VeinEntrance> _allEntrances = RoadManager.Instance.GetEntrances();

        foreach(VeinEntrance v in _allEntrances){
            if(v.GetType() == EntranceType.Output){
                outputJunctions.Add(v.GetJunction());
            }else{
                inputJunctions.Add(v.GetJunction());
            }
        }

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count != 0)
        {
            Junction current = queue.Dequeue();
            if(outputJunctions.Contains(current) && !_connectedOutputs.Contains(current)){
                _connectedOutputs.Add(current);
            }

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
        Dictionary<Junction, int> inDegree = new Dictionary<Junction, int>();

        Junction oj = _output.GetJunction();
        Junction ij = _input.GetJunction();

        foreach (var junction in graph)
        {
            inDegree[junction] = 0;
        }

        foreach (var junction in graph)
        {
            foreach (var neighbor in junction.GetNext())
            {
                if (junction == ij && neighbor == oj)
                {
                    continue;
                }
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

        Queue<Junction> queue = new Queue<Junction>();

        foreach (var junction in graph)
        {
            if (inDegree[junction] == 0)
            {
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
                if (current == ij && neighbor == oj)
                {
                    continue;
                }
                inDegree[neighbor]--;

                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        if (processedCount == graph.Count)
        {
            return false;
        }
        return true;
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

    private void UpdateResources(Junction start)
    {
        Dictionary<Junction, Dictionary<ResourceType, float>> jRes = new Dictionary<Junction, Dictionary<ResourceType, float>>();

        List<Junction> visited = new List<Junction>();
        Queue<Junction> queue = new Queue<Junction>();

        visited.Add(start);
        queue.Enqueue(start);

        foreach (Junction j in _connectedJunctions)
        {
            jRes[j] = new Dictionary<ResourceType, float>();
        }

        jRes[start] = start.GetEntrance().GetNetResources();

        while (queue.Count != 0)
        {
            Junction current = queue.Dequeue();
            var nextJunctions = current.GetNext();

            foreach (Junction next in nextJunctions)
            {
                if (!jRes.ContainsKey(next))
                {
                    jRes[next] = new Dictionary<ResourceType, float>();
                }

                Dictionary<ResourceType, float> currentRes = jRes[current];

                foreach (KeyValuePair<ResourceType, float> entry in currentRes)
                {
                    ResourceType resType = entry.Key;
                    float resAmount = entry.Value / nextJunctions.Count; // Evenly distribute resources

                    if (jRes[next].ContainsKey(resType))
                    {
                        jRes[next][resType] += resAmount;
                    }
                    else
                    {
                        jRes[next][resType] = resAmount;
                    }
                }

                var prevVisited = true;
                foreach(var prev in next.GetPrev()){
                    if(!visited.Contains(prev)){
                        prevVisited = false;
                        break;
                    }
                }
                if (!visited.Contains(next) && prevVisited && next != _output.GetJunction())
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        foreach (Junction j in _connectedJunctions)
        {
            if (jRes.ContainsKey(j))
            {
                foreach (KeyValuePair<ResourceType, float> entry in jRes[j]){
                    j.AddResource(entry.Key, entry.Value);
                }
            }
        }
    }
    
    private void ClearResources(){
        foreach(Junction j in _connectedJunctions){
            j.ClearResources();
        }
    }
}

