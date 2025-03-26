using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public Node cameFrom;
    public List<Node> connections;

    public float gScore;
    public float hScore;
    public float fScore
    {
        get
        {
            return gScore + hScore;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
   
        if(connections.Count > 0)
        {
            for(int i = 0; i < connections.Count; i++)
            {
                Gizmos.DrawLine(transform.position, connections[i].transform.position);
            }
        }
    }
}
