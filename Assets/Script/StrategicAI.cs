using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections;
using System.Collections.Generic;



//Posizione sulla mappa
public class MapPosition {
	public int ID;
	public float influence;

	public MapPosition (int i, float inf){
		ID = i;
		influence = inf;
	}

	public List<MapPosition> getNeighbours(){
		List<MapPosition> neighbours = new List<MapPosition> ();


		InfluenceMap influenceMap = InfluenceMap.getInstance ();

		foreach (Province p in GameLogic.GetProvinceNeighbours(ID)) {
			neighbours.Add (influenceMap.GetPosition(p.ID));
		}

		return neighbours;

	}
}


//Mappa di influenza
public class InfluenceMap {

	public List<MapPosition> positions;
	public Graph influenceMapGraph;
	List<Vertex> removedVertices;

	private static InfluenceMap instance;

	private InfluenceMap() {
		positions = new List<MapPosition> ();
		
		//Posizioni sulla mappa di influenza
		for (int i=0; i<GameLogic.provinces.Length; i++) 
			positions.Add (new MapPosition(i,0));

		removedVertices = new List<Vertex> ();
	}


	public static InfluenceMap getInstance() {

		if (instance == null)
			instance = new InfluenceMap ();

		return instance;
	}

	public void BuildGraph() {

		influenceMapGraph = new Graph ();
		
		foreach (MapPosition pos in positions) {
			
			influenceMapGraph.AddNode (new Node (pos.ID));
		}
		
		foreach (MapPosition pos in positions){
			
			foreach (MapPosition n in pos.getNeighbours()) {
				influenceMapGraph.AddVertex(pos.ID,n.ID,1);
			}
			
		}
	}

	public MapPosition GetPosition(int ID) {

		return positions [ID];
	}

	public float GetInfluenceIn(int ID) {

		return positions [ID].influence;
	}

	public void Update () {

		foreach (MapPosition pos in positions) {
			
			pos.influence = 0;
			
			foreach (Unit unit in GameLogic.units) {

				if (unit != null) {
					int distance = GameLogic.mapGraph.Distance (pos.ID, unit.Province.ID);

					float strength = unit.Strength;

					//Se è una unità IA, l'influenza per la provincia è calcolata come influenza corrente + (punti attacco unità / distanza da unità)
					if (unit.Faction == GameLogic.enemy)
						
						pos.influence += strength / (distance + 1);
						
					//Se è una unità giocatore, l'influenza per la provincia è calcolata come influenza corrente - (punti difesa unità / distanza da unità)
					else 
						pos.influence -= strength / (distance + 1);


				}

			}
		}


		/*Aggiorno grafo in base alle informazioni della mappa di influenza*/

		
		foreach (Vertex vertex in removedVertices)
			influenceMapGraph.AddVertex (vertex.START.ID, vertex.END.ID, 0.0f);

		removedVertices.Clear ();
		
		foreach (Vertex vertex in influenceMapGraph.vertices){

				float cost = 0;

				if (GameLogic.GetProvinceFaction(vertex.END.ID) == GameLogic.GetProvinceFaction(vertex.START.ID) &&
			    	GameLogic.GetUnitInProvince(vertex.END.ID)!=null)

					removedVertices.Add (vertex);
				

				else 
					
					cost = 1 - (positions[vertex.END.ID].influence)/100;



				vertex.Cost=cost;

		}

		foreach (Vertex vertex in removedVertices) 
			influenceMapGraph.removeVertex (vertex);


		
	}

}


//Pianifica le azioni per il turno
public class StrategicPlanner {

	int defendTarget;
	int offensiveTarget;

	public List<Agent> agents;

	private static StrategicPlanner instance;

	private StrategicPlanner() {
		agents = new List<Agent> ();

		offensiveTarget = SetOffensiveTarget ();
		defendTarget = SetDefendTarget ();

		foreach (Unit u in GameLogic.units) {
			if (u.Faction == GameLogic.enemy)
				agents.Add (new Agent (u));
		}

	}

	public static StrategicPlanner getInstance() {

		if (instance == null) 
			instance = new StrategicPlanner ();

		return instance;

	}

	//Seleziona obiettivo strategico da attaccare
	int SetOffensiveTarget(){

		int target = 0;

		//Se l'IA è BLU, l'obiettivo è la capitale rossa
		if (GameLogic.enemy == factions.BLUE) 
			target = GameLogic.redCapital;
		
		//Se l'IA è ROSSA, l'obiettivo è la capitale blu
		if (GameLogic.enemy == factions.RED)
			target = GameLogic.blueCapital;

		return target;
	}

	//Seleziona obiettivo strategico da difendere
	int SetDefendTarget(){
		
		int target = 0;
		
		//Se l'IA è ROSSA, la ritirata è verso la capitale rossa
		if (GameLogic.enemy == factions.RED) 
			target = GameLogic.redCapital;
		
		//Se l'IA è BLU, la ritirata è verso la capitale blu
		if (GameLogic.enemy == factions.BLUE)
			target = GameLogic.blueCapital;
		
		return target;
	}


	//Seleziona obiettivo da contrattaccare
	int SetCounterattackTarget() {

		int minDistance = 10 ;
		Unit nearestUnit = null;

		InfluenceMap influenceMap = InfluenceMap.getInstance ();

		/*Sceglie l'unità del giocatore più vicina entro 2 caselle dalla capitale alleata.
		  Se vi è più di una unità alla stessa distanza, sceglie quella più pericolosa, ovvero quella con influenza più bassa nella provincia
		*/
		foreach (Unit u in GameLogic.units) {

			if (u.Faction == GameLogic.player) {

				int distance = GameLogic.mapGraph.Distance (u.Province.ID, defendTarget);

				float influence = influenceMap.GetInfluenceIn (u.Province.ID);

				if (distance <= 2) {
					if (distance < minDistance){
						nearestUnit = u;
						minDistance = distance;
					}

					else if (distance == minDistance && influence < influenceMap.GetInfluenceIn(nearestUnit.Province.ID)) {

						nearestUnit = u;
						minDistance = distance;
					}
				}
			}

		}

		if (nearestUnit != null)
			return nearestUnit.Province.ID;
		return -1;

	}


	//sceglie le unità che andranno in attacco 
	List<Agent> GetAttackers(){


		List<Agent> attackers = new List<Agent> ();


		InfluenceMap influenceMap = InfluenceMap.getInstance ();

		//Aggiornamento mappa di influenza
		influenceMap.Update ();


		/*Seleziona per l'attacco le unità più vicine alla capitale nemica 
		Se due unità hanno la stessa distanza, sceglie quella con influenza più alta
		*/

		int minDistance = 10;
		Agent nearestAgent = null;

		foreach (Agent a in agents) {

			Unit u = a.Unit;

			int distance = GameLogic.mapGraph.Distance (u.Province.ID, offensiveTarget);

			if (distance < minDistance) {
				
				nearestAgent = a;
				minDistance = distance;

			} else if (distance == minDistance) {

				Unit nearestUnit = nearestAgent.Unit;

				if (influenceMap.GetInfluenceIn (u.Province.ID) > 
				    influenceMap.GetInfluenceIn (nearestUnit.Province.ID)) {

					nearestAgent = a;
				}
			}

		}
			
		attackers.Add (nearestAgent);
			



		//Seleziona per l'attacco l'unità con influenza più alta
	
		float maxInfluence = -10;
		Agent strongestAgent = null;
		
		foreach (Agent a in agents) {

			Unit u = a.Unit;


			float influence = influenceMap.GetInfluenceIn (u.Province.ID);
		
			if (influence > maxInfluence) {
			
				strongestAgent = a;
				maxInfluence = influence;
			
			}

		}

		if (!attackers.Contains(strongestAgent))
			attackers.Add (strongestAgent);



		return attackers;

	}

	//Sceglie le unità che si ritireranno


	List<Agent> GetRetreaters(){

		List<Agent> retreaters = new List<Agent>();
		

		List<Unit> threats = new List<Unit>();

		foreach (Unit u in GameLogic.units) {

			if (u!=null){

				bool alliedInDefence = false;


				if (u.Faction == GameLogic.player){

					PathFinding pathfinding = PathFinding.getInstance();

					List<PathFindingVertex> path = 
						pathfinding.GetPath(GameLogic.mapGraph,
						                            u.Province.ID,defendTarget);

					foreach (PathFindingVertex connection in path){

						PathFindingNode end = connection.END;

						if (GameLogic.GetUnitInProvince(end.ID)!=null){
							alliedInDefence = true;
							break;
						}
					}

					if (alliedInDefence == false)
						threats.Add(u);
				}

			}
		}




		//Sceglie per la ritirata le unità  più vicine a una minaccia
		foreach (Unit threat in threats)
			foreach (Agent a in agents) {
				
				Unit u = a.Unit;

				if (!retreaters.Contains(a)){

					int threatLine = 
						GameLogic.mapGraph.Distance(threat.Province.ID,
					                            defendTarget);
					int unitLine = 
						GameLogic.mapGraph.Distance(u.Province.ID, 
					                            defendTarget);

					int distanceFromThreat = 
						GameLogic.mapGraph.Distance (u.Province.ID, 
					                             threat.Province.ID);
				                                                      
					
					if (distanceFromThreat == 1 && threatLine<=unitLine)
							retreaters.Add (a);

				}
			}

		return retreaters;

	}

	//Sceglie le unità che  contrattaccheranno

	List<Agent> GetCounterattackers(int target){

		List<Agent> counterAttackers = new List<Agent>();


		//Sceglie per il contrattacco le unità più vicine
	
		
		foreach (Agent a in agents) {

			Unit u = a.Unit; 

			if (target != -1) {
				
				int distance = GameLogic.mapGraph.Distance (u.Province.ID, target);
				
				if (distance <= 1)
					counterAttackers.Add (a);
			}

		}



		return counterAttackers;

	}

	public void ExecutePlan() {

		int counterAttackTarget = SetCounterattackTarget ();

		//Pianifica le offensive
		List<Agent> attackers = GetAttackers();

		//Pianifica ritirate 
		List<Agent> retreaters= GetRetreaters();

		//Pianifica contrattacchi 
		List<Agent> counterAttackers= GetCounterattackers(counterAttackTarget);

		/*Possono esserci delle unità che sono sia tra gli attaccanti, che tra coloro
		  che si ritirano o contrattaccano. Tuttavia ogni unità può ricevere un solo ordine: 
		  la priorità è la ritirata, seguita dal contrattacco
		*/

		foreach (Agent u in retreaters){
			attackers.Remove(u);
			counterAttackers.Remove(u);
		}

		foreach (Agent u in counterAttackers){
			attackers.Remove(u);
		}

	

		//Esecuzione del piano
		foreach (Agent attacker in attackers) 
			if (attacker.IsOrderFeasible(offensiveTarget))
				attacker.ExecuteOrder(offensiveTarget);

		foreach (Agent retreater in retreaters) 
			if (retreater.IsOrderFeasible(defendTarget))
				retreater.ExecuteOrder(defendTarget);

		foreach (Agent counterAttacker in counterAttackers)
			if (counterAttacker.IsOrderFeasible(counterAttackTarget))
				counterAttacker.ExecuteOrder(counterAttackTarget);



	}
	

}












