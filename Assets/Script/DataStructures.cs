using System;
using System.Collections;
using System.Collections.Generic;






						/*GRAFO DIRETTO*/

//nodo
public class Node {

	int id;

	public Node(int i){
		id = i;
	}

	public int ID {
		get {return id;}
	}
}



//Vertice 
public class Vertex {

	Node start;
	Node end;
	float cost;

	public Vertex (Node s, Node e , float c){
		start = s;
		end = e;
		cost = c;
	}
	

	public Node START{
		get{return start;}
	}

	public Node END{
		get{return end;}
	}

	public float Cost {
		get{return cost;}
		set{cost = value;}
	}
}

//grafo 
public class Graph {

	public List<Node> nodes;
	public List<Vertex> vertices;
	

	public Graph () {

		nodes = new List<Node> ();
		vertices = new List<Vertex> ();
	}

	public void AddNode(Node node) {

		nodes.Add (node);
	}

	public void AddVertex(int start, int end, float cost) {

		vertices.Add (new Vertex(GetNode(start),GetNode(end),cost));
	}
	

	public Node GetNode(int ID) {

		foreach (Node node in nodes)
			if (node.ID == ID)
				return node;

		return null;
	}

	public List<Node> GetNeighbours(int ID){

		List<Node> neighbours = new List<Node> ();

		foreach (Vertex vertex in vertices) {
			if (vertex.START.ID == ID) 
				neighbours.Add(vertex.END);
		}

		return neighbours;

	}

	//Vertici a partire dal nodo ID
	public List<Vertex> GetVertices(int ID){

		List<Vertex> connections = new List<Vertex> ();

		foreach (Vertex vertex in vertices) {
			if (vertex.START.ID == ID)
				connections.Add(vertex);
		}

		return connections;
	}

	//Vertice dal nodo A al nodo B 
	public Vertex GetVertex(int A, int B ) {

		foreach (Vertex vertex in vertices) 
			if (vertex.START.ID == A && vertex.END.ID == B)
				return vertex;
		return null;
	}

	public void removeVertex(Vertex vertex) {

		vertices.Remove (vertex);
	}
	


	//Calcolo distanza tra due nodi con una ricerca breadth-first
	public int Distance(int A, int B) {

		int distance = 0;

		//con un dizionario mantengo l'informazione sia del nodo visitato che del suo genitore
		Dictionary<int,int> visited = new Dictionary<int,int> ();

		Queue queue = new Queue ();
		queue.Enqueue (GetNode(A));
		visited.Add (A, -1);

		Node current = null;
		
		while (queue.Count > 0) {
			
			current = (Node)queue.Dequeue();


			if (current.ID == B)
				break;
			else {
				foreach (Node neighbour in GetNeighbours(current.ID))
					if (!visited.ContainsKey(neighbour.ID)){
						queue.Enqueue(neighbour);
						visited.Add(neighbour.ID,current.ID);
					}
			}


		}


		//Ripercorre il percorso dal nodo attuale al nodo A

		while (true) {
			current = GetNode(visited[current.ID]);

			if (current==null)
				break;

			distance ++;
		}

		return distance;
	}
	

}

