using System;
using System.Collections;
using System.Collections.Generic;


						/*GRAFO*/

//nodo
public class Node {

	int Id;

	public Node(int i){
		Id = i;
	}

	public int ID {
		get {return Id;}
	}
}



//Vertice 
public class Vertex {

	Node A;
	Node B;

	public Vertex (Node a, Node b ){
		A = a;
		B = b;
	}

	public Node getA (){
		return A;
	}

	public Node getB() {
		return B;
	}
}

//grafo indiretto
public class Graph {

	protected List<Node> nodes;
	protected List<Vertex> vertices;
	

	public Graph () {

		nodes = new List<Node> ();
		vertices = new List<Vertex> ();
	}

	public void AddNode(Node node) {

		nodes.Add (node);
	}

	public void AddVertex(Vertex vertex) {

		vertices.Add (vertex);
	}

	public bool VertexExists(Node a, Node b) {

		foreach (Vertex vertex in vertices) {
			if (vertex.getA () == a && vertex.getB () == b)
				return true;
			else if (vertex.getB () == a && vertex.getA() == b) 
				return true;

		}

		return false;
	}

	public Node getNode(int ID) {

		foreach (Node node in nodes)
			if (node.ID == ID)
				return node;

		return null;
	}

	public List<Node> getNeighbours(Node node){

		List<Node> neighbours = new List<Node> ();

		foreach (Vertex vertex in vertices) {
			if (vertex.getA() == node) 
				neighbours.Add(vertex.getB());
			else if (vertex.getB() == node)
				neighbours.Add(vertex.getA());

		}

		return neighbours;

	}

	bool process(Node node,Func<Node,bool> method) {

		return method.Invoke(node);
	}

	//ricerca per ampiezza
	public Node BFSSearch(Node start,Func<Node,bool> method) {


		List<Node> visited = new List<Node> ();

		Queue queue = new Queue ();
		queue.Enqueue (start);
		visited.Add (start);

		while (queue.Count > 0) {

			Node current = (Node)queue.Dequeue();

			if (process(current,method))
				return current;
			else
				foreach (Node neighbour in getNeighbours(current))
					if (!visited.Contains(neighbour)){
						queue.Enqueue(neighbour);
						visited.Add(neighbour);
				}
		}

		return null;
	}

	//attraversamento in ampiezza
	public void BFSTraversal(Node start,int limit, Func<Node,bool> method) {

		List<Node> visited = new List<Node> ();
		
		Queue queue = new Queue ();
		queue.Enqueue (start);
		visited.Add (start);
		
		while (queue.Count > 0) {
			
			Node current = (Node)queue.Dequeue();
			
			process(current,method);

			foreach (Node neighbour in getNeighbours(current))
			if (!visited.Contains(neighbour)){
				queue.Enqueue(neighbour);
				visited.Add(neighbour);
			}
		}



	}
}

