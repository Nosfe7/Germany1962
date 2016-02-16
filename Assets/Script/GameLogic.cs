using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;




public enum factions {BLUE,RED};

public class GameLogic : MonoBehaviour {
		
	public static factions player;
	public static factions enemy;
	public static factions turn;
	

	public static Province selectedProvince;
	public static Province targetProvince;
	public static Unit selectedUnit;
	public static Unit targetUnit;
	public static GameObject selectBox;
	public static GameObject targetBox;

	public static Text turnText; 


	public static Province[] provinces;
	public static Unit[] units;
	public static StrengthIndicator[] strengthIndicators;

	public static int blueCapital;
	public static int redCapital;


	public static Support playerSupport;
	public static Support enemySupport;

	public static int numPlayerProvinces;
	public static int numEnemyProvinces;

	//mappa di influenza per debug
	public static GameObject[] debugInfluenceMap;

	static int turnCounter;

	//mappa di influenza, IA strategica e grafo mappa di gioco
	public static InfluenceMap influenceMap;
	public static StrategicPlanner strategicPlanner;
	public static Graph mapGraph;

	

	// Use this for initialization
	void Start () {

		//Giocatore e nemico
		player = factions.BLUE;
		enemy = factions.RED;

		//Assegno il numero di province iniziale al giocatore e all'IA
		numPlayerProvinces = 16;
		numEnemyProvinces = 16;

		//Supporto
		playerSupport = GameObject.Find ("playerSupport").GetComponent<Support> ();
		enemySupport = GameObject.Find ("enemySupport").GetComponent<Support> ();


		//Array di provincie e gruppi unitòà
		provinces = FindObjectsOfType<Province> ();
		provinces = provinces.OrderBy (p => p.ID).ToArray ();
		strengthIndicators = Canvas.FindObjectsOfType<StrengthIndicator> ();
		strengthIndicators = strengthIndicators.OrderBy (i => i.ID).ToArray ();
		units = FindObjectsOfType<Unit> ();
		units = units.OrderBy (u => u.ID).ToArray ();


		//Settaggio indicatori di forza
		foreach (Unit u in units) {
			u.SetupIndicator ();
		}

		debugInfluenceMap = GameObject.FindGameObjectsWithTag("InfluenceDebug");



		//Box di selezione unità alleata e target attacco
		selectBox = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/SelectBox"));

		targetBox = GameObject.Instantiate((GameObject)Resources.Load("Prefabs/TargetBox"));


		//Indicatore turno corrente
		turnText = Canvas.FindObjectOfType<Text> ();

		//Capitali blu e rossa
		blueCapital = 1;
		redCapital = 30;
		//Creazione grafo della mappa
		mapGraph = new Graph ();
		
		foreach (Province province in provinces) {
			
			mapGraph.AddNode (new Node (province.ID));
		}
		
		foreach (Province province in provinces){
			
			foreach (Province n in province.getNeighbours()) {
				mapGraph.AddVertex(province.ID,n.ID,1);
			}

		}


		//Inizializzazione IA
		influenceMap = InfluenceMap.getInstance ();

		influenceMap.BuildGraph ();

		strategicPlanner = StrategicPlanner.getInstance();

		turn = player;
		
		turnCounter = 0;

		//Inizia gioco
		if (turn == player)
			PlayerTurn ();
		else if (turn == enemy)
			EnemyTurn ();

	}



	//Risolve la mossa
	public  void SolveMove(){

		
		
		if (selectedUnit.ActionPoints > 0) {
			
			//Se la provincia è occupata dall'avversario e c'è una unità avversaria, l'unità selezionata attacca l'unità avversaria
			if (selectedProvince.Owner != targetProvince.Owner && targetProvince.Unit!=null) {
				
				List<Unit> attackSupporters = new List<Unit>();
				
				//Unità che potrebbero supportare l'attacco
				foreach (Province n in targetProvince.getNeighbours()){
					if (n.Unit!=null && n.Owner == selectedUnit.Faction
					    &&n.Unit!=selectedUnit)
						attackSupporters.Add(n.Unit);
				}
				
				/*Se ci sono unità che potrebbero supportare l'attacco, 
				 * e il giocatore decide per l'assalto totale, 
				 * le unità supportano l'attacco
				*/
				if (attackSupporters.Count > 0
				    && UnityEditor.EditorUtility.DisplayDialog(
					"OPTION", "Do you want support from neighbouring units?",
					"YES","NO"))
					selectedUnit.Attack(targetUnit,"Total assault", attackSupporters);
				else 
					selectedUnit.Attack(targetUnit,"Limited attack", attackSupporters);
				
			}
			
			//Altrimenti sposto semplicemente le unità nella nuova provincia
			else {
				if (targetProvince.Unit == null){
					selectedUnit.Move(targetProvince);
				}
				
			}
			
			
		} else {
			UnityEditor.EditorUtility.DisplayDialog("ACTION POINTS","Not enough action points!","Ok");
		}
		
	}

	

	public static Province GetProvince(int ID) {

		return provinces [ID];

	}

	public static List<Province> GetProvinceNeighbours(int ID) {

		return provinces [ID].getNeighbours ();
	}

	public static factions GetProvinceFaction(int ID) {

		return provinces [ID].Owner;
	}

	public static Unit GetUnitInProvince(int ID){

		return provinces [ID].Unit;
	}


	//Gestore turno giocatore
	void PlayerTurn() {
		
		turnCounter ++;
		
		turnText.text = "Turn " + turnCounter;

		Debug.Log (turnCounter);
		
		ResultManager ();
		
		//Gestisco i punti attacco di supporto
		playerSupport.ManagePoints ();
		
		
		
		//Ripristino punti azione (e ripristino  anche la forza delle unità del giocatore)
		foreach (Unit u in units) {
			if (u != null) {

				u.ActionPoints = 1;
				u.SupportPoints = 1;
				
				float reinforceStrength = 0;
				
				//ripristino = forza iniziale + 12 / distanza da fonte rifornimenti (capitale alleata)) 
				
				if (u.Faction == factions.BLUE && factions.BLUE == player) {
					reinforceStrength = 12 / (mapGraph.Distance(u.Province.ID,blueCapital) + 1);
					
					
					u.Strength += reinforceStrength;
				}
				
				else if (u.Faction == factions.RED && factions.RED == player ) {
					reinforceStrength = 12 / (mapGraph.Distance(u.Province.ID,redCapital) + 1);
					
					u.Strength += reinforceStrength;
				}
			}
			
		}
		
	}



	//Gestione turno del nemico
	void EnemyTurn(){
		
		turnCounter ++;

		//turnText.text = "Turn " + turnCounter;

		Debug.Log (turnCounter);

		ResultManager ();

		enemySupport.ManagePoints ();


		//Ripristino punti azione (e ripristino anche la forza delle unità dell'IA)
		foreach (Unit u in units) {
			if (u != null) {

				u.ActionPoints = 1;
				u.SupportPoints = 1;

				
				float reinforceStrength = 0;

				//ripristino = forza iniziale + 12 / distanza da fonte rifornimenti (capitale alleata)) 
				
				if (u.Faction== factions.BLUE && factions.BLUE == enemy) {
					reinforceStrength = 12 / (mapGraph.Distance(u.Province.ID,blueCapital) + 1);
					
					
					u.Strength += reinforceStrength;
				}
				
				else if (u.Faction == factions.RED && factions.RED == enemy ) {
					reinforceStrength = 12 / (mapGraph.Distance(u.Province.ID,redCapital) + 1);
					
					u.Strength += reinforceStrength;
				}
			}
			
		}



		
		
		/*IA NEMICA*/
		
		//influenceMap.Update ();
		strategicPlanner.ExecutePlan ();


		turn = player;
		PlayerTurn ();
		
		
	}




	//Gestione risultato del gioco
	void ResultManager(){

		Unit unitInBlueCapital = GetUnitInProvince(blueCapital);
		Unit unitInRedCapital = GetUnitInProvince(redCapital);

		if (unitInBlueCapital != null && unitInBlueCapital.Faction == factions.RED) {

			UnityEditor.EditorUtility.DisplayDialog("RED TOTAL VICTORY","","OK");
			Application.Quit();
	
		}

		if (unitInRedCapital != null && unitInRedCapital.Faction == factions.BLUE) {
			
			UnityEditor.EditorUtility.DisplayDialog("BLUE TOTAL VICTORY","","OK");
			Application.Quit();
			
		}

		if (turnCounter == 21) {


			//Prendo le unità blu e vedo qual'è quella più vicina alla capitale rossa
			int minBlueDistance = 10;
			
			foreach (Unit u in GameLogic.units)
			if (u!=null && u.Faction == factions.BLUE) {
				int distance =  GameLogic.mapGraph.Distance (u.Province.ID, redCapital);
				
				if (distance < minBlueDistance) {

					minBlueDistance = distance;
					
				}
				
			}

			//Prendo le unità rosse e vedo qual'è quella più vicina alla capitale blu
			int minRedDistance = 10;
			
			foreach (Unit u in GameLogic.units)
			if (u!=null && u.Faction == factions.RED) {
				int distance =  GameLogic.mapGraph.Distance (u.Province.ID, blueCapital);
				
				if (distance < minRedDistance) {

					minRedDistance = distance;
					
				}
				
			}


			//Se la distanza tra la più vicina unità blue e la più vicina unità rossa sono uguali è pareggio
			if (minBlueDistance == minRedDistance) {

				UnityEditor.EditorUtility.DisplayDialog("DRAW: NO FACTION WON","","OK");
				Application.Quit();


			}

			/*Se la distanza tra la più vicina unità blu è minore di quella della più vicina unità rossa, 
			 * è una vittoria limitata per la fazione blu
			 */
			else if (minBlueDistance < minRedDistance) {
				
				UnityEditor.EditorUtility.DisplayDialog("BLUE LIMITED VICTORY","","OK");
				Application.Quit();
				
				
			}

			/*Se la distanza tra la più vicina unità rossa è minore di quella della più vicina unità blu, 
			 * è una vittoria limitata per la fazione rossa
			 */
			else if (minBlueDistance > minRedDistance) {
				
				UnityEditor.EditorUtility.DisplayDialog("RED LIMITED VICTORY","","OK");
				Application.Quit();
				
				
			}



		}


	}

	// Update is called once per frame
	void Update () {

	}
}


