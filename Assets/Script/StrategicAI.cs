using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

//Unità sulla mappa  
public class MapAgent {

	public int ID;
	public int positionID;	
	public factions faction;

	public MapAgent(int p, int i, factions f){
		positionID = p;
		ID = i;
		faction = f;
	}

	public float calculateInfluence(){
		float influence = 0;

		foreach (Unit u in GameLogic.groups[ID].units)
			influence += u.AttackPoints + u.DefencePoints;

		return influence;
	}
}

//Posizione sulla mappa
public class MapPosition {
	public int ID;
	public float influence;

	public MapPosition (int i, float inf){
		ID = i;
		influence = inf;
	}

	public List<int> getNeighbours(){
		List<int> neighboursIDs = new List<int> ();


		foreach (int p in GameLogic.provinces[ID].neighbours) {
			neighboursIDs.Add (p);
		}

		return neighboursIDs;

	}
}



//Mappa di influenza
public static class InfluenceMap {

	public static List<MapAgent> agents; 
	public static List<MapPosition> positions;
	public static Graph mapGraph;



	public static void InitMap(){

		agents = new List<MapAgent> ();
		positions = new List<MapPosition> ();

		foreach (UnitGroup g in GameLogic.groups) 
			agents.Add (new MapAgent (g.province,g.ID,g.faction));


		for (int i=0; i<GameLogic.provinces.Length; i++) 
			positions.Add (new MapPosition(i,0));


		//Creo grafo della mappa di influenza
		mapGraph = new Graph ();

		foreach (MapPosition pos in positions) {

			mapGraph.AddNode(new Node(pos.ID));

			foreach (int n in pos.getNeighbours()) {
				if (!mapGraph.VertexExists(new Node(pos.ID),new Node(n)))
					mapGraph.AddVertex(new Vertex(new Node(pos.ID),new Node(n)));
			}
		}
	}


	public static  void Update() {

		//azzero l'influenza nelle posizioni
		foreach (MapPosition pos in positions) {

			pos.influence = 0;
			
		}

		//ogni unità diffonde l'influenza nella posizione
		foreach (MapAgent agent in agents){

			//aggiorno la posizione dell'unità
			agent.positionID = GameLogic.groups[agent.ID].province;

			float agentInfluence = agent.calculateInfluence();
			int mul = 1;

			//Sommo influenza per le unità delle IA, sottraggo influenza per le unità del giocatore
			if (GameLogic.provinces[agent.positionID].Owner == GameLogic.player ) 
				mul = -1;

			SpreadInfluence(0,agent.positionID,2,agentInfluence,mul);
		}



	}

	//calcola l'influenza nella posizione corrispondente a un nodo del grafo
	static void calcInfluence(Node n, int mul) {


	}


	//Diffonde l'influenza delle unità sulla mappa con un raggio ray e drop-off lineare (1/(d+1))
	static void SpreadInfluence(int d, int positionID, int ray, float agentInfluence,int mul) {


	}
	

}

//Ordine generico
abstract class Order {

	public abstract void Execute ();

}

//Offensiva
class Offensive : Order {

	public int attackerID;
	public int targetID;

	public Offensive (int aID, int tID)  {

		attackerID = aID;
		targetID = tID;
	}

	//Conduce l'offensiva richiamando il decision tree del gruppo attaccante
	public override void Execute(){

		Decision root = new isTargetWeaker (attackerID, targetID);

		Action result = (Action) root.makeDecision ();

	}

}



//Piano strategico : gli ordini vengono dati alle singole unità
static class Plan {
	

	//piano
	static List<Order> plan;

	public static void Init(){

		plan = new List<Order> ();
	}


	//trovo le unità del giocatore più deboli
	public static List<MapAgent> getWeakestUnits() {


		List<MapPosition> playerPositions = new List<MapPosition> ();

		foreach (MapPosition position in InfluenceMap.positions)
			if (GameLogic.provinces [position.ID].Owner == GameLogic.player)
				playerPositions.Add (position);

		playerPositions.ToArray ().OrderBy (p => Mathf.Abs (p.influence));

		List<MapAgent> weakest = new List<MapAgent> ();

		for (int i=0; i<GameLogic.ActionPoints; i++) {

			foreach (MapAgent agent in InfluenceMap.agents)
				if (agent.positionID == playerPositions[i].ID){
					weakest.Add (agent);
					break;
			}

		}

		return weakest;

	}

	//controlla se c'è un'unità dell'IA nella posizione
	static bool isAIUnit(Node n) {

		int position = n.ID;

		foreach (MapAgent agent in InfluenceMap.agents)
			if (agent.positionID == position && agent.faction == GameLogic.enemy)
				return true;

		return false;
	}

	//pianifica offensive
	static List<Offensive> planOffensives(){

		List<Offensive> offensives = new List<Offensive> ();

		//trova una unità dell'IA più vicina alle unità più deboli del giocatore
		foreach (MapAgent playerUnit in getWeakestUnits()) {

			Node AIUnitNode = InfluenceMap.mapGraph.BFSSearch(InfluenceMap.mapGraph.getNode(playerUnit.positionID),isAIUnit);
		
			foreach (MapAgent AIUnit in InfluenceMap.agents)
				if (AIUnit.positionID == AIUnitNode.ID){
					offensives.Add(new Offensive(AIUnit.ID,playerUnit.ID));
					break;
				}
		}

		return offensives;
	}
	

	//Esegue il piano
	public static void ExecutePlan(){

		foreach (Offensive off in planOffensives())
			plan.Add (off);

		foreach (Order ord in plan)
			ord.Execute ();
	}


}


