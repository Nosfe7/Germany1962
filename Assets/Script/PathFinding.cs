using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*PATHFINDING*/


//Lista di nodi
class PathFindingList {

	List<PathFindingNode> list;

	public PathFindingList(){
		list = new List<PathFindingNode> ();
	}

	public void AddNode(PathFindingNode node){
		list.Add (node);
	}

	public void RemoveNode(PathFindingNode node){
		list.Remove(node);
	}
	
	public PathFindingNode SmallestElement() { 

		float minCost = 1000;
		PathFindingNode minElement = null;

		foreach (PathFindingNode node in list)
			if (node.estimatedTotalCost < minCost) {
				minElement = node;
				minCost = node.estimatedTotalCost;
			}

		return minElement;
	}

	public bool Contains(Node node){

		foreach (PathFindingNode pnode in list)
			if (pnode.ID == node.ID)
				return true;

		return false;
 	}

	public PathFindingNode Find(Node node) {

		foreach (PathFindingNode pnode in list)
			if (pnode.ID == node.ID)
				return pnode;

		return null;
	}

	public int Length(){
		return list.Count;
	}
}

//Nodo pathfinding
class PathFindingNode{
	
	public int ID;
	public PathFindingVertex connection; 
	public float costSoFar;
	public float estimatedTotalCost;
	
	public PathFindingNode (int id) {
		ID = id;
		connection = null;
		costSoFar = 0;
		estimatedTotalCost = 0;
	}

	public PathFindingNode(int id,float cost,float ecost){
		ID = id;
		costSoFar = cost;
		estimatedTotalCost = ecost;
		connection = null;
	}
	
}

class PathFindingVertex {
	
	public PathFindingNode START;
	public PathFindingNode END;
	
	public PathFindingVertex(PathFindingNode s, PathFindingNode e) {
		START = s;
		END = e;
	}
	
	
}



class PathFinding {
	
	private static PathFinding instance;
	
	private PathFinding () {}
	
	public static PathFinding getInstance() {
		
		if (instance == null)
			instance = new PathFinding ();
		
		return instance;
	}
	
	//Euristica
	float EstimateFrom(int nodeID,int goal) {

		Province nodeProvince = GameLogic.GetProvince(nodeID);
		Province goalProvince = GameLogic.GetProvince(goal);

		//Uso distanza euclidea
		float startX = nodeProvince.transform.position.x;
		float startY = nodeProvince.transform.position.y;
		
		float endX = goalProvince.transform.position.x;
		float endY = goalProvince.transform.position.y;
		
		return Mathf.Sqrt(Mathf.Pow((startX-endX),2)+Mathf.Pow((startY-endY),2));

	}
	
	//Pathfinding tattico: costruisce il percorso a costo minimo con A* con costi delle connessioni che tengono conto della mappa di influenza
	public List<PathFindingVertex> GetPath(Graph graph, int start, int goal){
		
		PathFindingNode startNode = new PathFindingNode (start, 0, EstimateFrom(start,goal));
		
		PathFindingNode current = new PathFindingNode(-1,0,0);
		
		//Inizializza lista nodi chiusi (già visitati) e aperti (da visitare)
		PathFindingList open = new PathFindingList();
		PathFindingList closed = new PathFindingList();
		
		
		open.AddNode(startNode);


		//Itera tutti i nodi da visitare
		while (open.Length() > 0) {


			current = open.SmallestElement();
		

			//Se il nodo corrente da visitare è l'obiettivo, termina iterazione
			if (current.ID == goal){
				break;
			}
			
			List<Vertex> connections = graph.GetVertices(current.ID);
			
			//Itera tutte le connessioni che partono dal nodo corrente
			foreach (Vertex connection in connections){
				
				
				Node end = connection.END;
				float endCost = current.costSoFar + connection.Cost;
				float endHeuristic;
				PathFindingNode endRecord;
				
				//Se il nodo al termine della connessione è tra i chiusi....
				if (closed.Contains(end)){
					
					//Record corrispondente al nodo terminale nella lista dei visitati
					endRecord = closed.Find(end);
					
					/*Se è stato trovato un percorso migliore verso il nodo terminale 
					rispetto a quello già registrato tra gli aperti...*/
					if (endRecord!=null && endRecord.costSoFar > endCost) {
						
						//Rimuove il nodo dai chiusi
						closed.RemoveNode(endRecord);
						
						//Usa i vecchi valori del nodo per ricalcolare l'euristica
						endHeuristic = endRecord.estimatedTotalCost - endRecord.costSoFar;
					}
					
					else continue;
					
					
				}
				
				//Se invece il nodo terminale è tra gli aperti....
				else if (open.Contains(end)) {
					
					//Record corrispondente al nodo terminale nella lista degli aperti
					endRecord = open.Find(end);
					
					/*Se è stato trovato un percorso migliore verso il nodo terminale 
					rispetto a quello già registrato tra gli aperti....*/
					if (endRecord.costSoFar > endCost) {
						
						//Uso i vecchi valori del nodo per ricalcolare l'euristica
						endHeuristic = endRecord.estimatedTotalCost - endRecord.costSoFar;
					}
					
					else continue;
					
					
				}
				
				/*Se invece il nodo terminale non è nè tra i chiusi nè tra gli aperti, crea un nuovo record 
				 * ed effettua la stima della distanza dall'obiettivo*/
				else {


					endRecord = new PathFindingNode(end.ID);

					endHeuristic = EstimateFrom(end.ID,goal);
					
				
				}
				
				//Aggiorno record con nuovi valori
				endRecord.costSoFar = endCost;
				endRecord.connection = new PathFindingVertex(current,endRecord);
				endRecord.estimatedTotalCost = endCost + endHeuristic;

				if (!open.Contains(end))
					open.AddNode(endRecord);
				
				
				
			}
			
			open.RemoveNode(current);
			closed.AddNode(current);
			
		}
		




		//Se il nodo corrente non è l'obiettivo, il nodo non è stato trovato e restituisce null
		if (current.ID!=goal)
			return null;


		
		//Altrimenti ricostruisce il percorso all'indietro partendo dal nodo corrente.
		else{
			
			List<PathFindingVertex> path = new List<PathFindingVertex>();


			while (current.ID!=start){
				
				path.Add(current.connection);
				current = current.connection.START;

			}



			
			path.Reverse();
			
			return path;
		}
		
	}
	
	
}



