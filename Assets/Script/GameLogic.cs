using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;




public enum factions {NATO,WARSAW_PACT};
public enum support {AIR, NUKE};

public class GameLogic : MonoBehaviour {
		
	public static factions player;
	public static factions enemy;
	public static factions turn;

	public static int selectedProvince;
	public static int targetProvince;
	public static Province[] provinces;
	public static UnitGroup[] groups;

	private static GameLogic instance;

	public static Support playerSupport;
	public static Support enemySupport;

	public static int numPlayerProvinces;
	public static int numEnemyProvinces;

	static int actionPoints;

	static int turnCounter;


	//Punti azione
	public static int ActionPoints {
		set {
			if (value <=0)
				actionPoints = 0;
			else
				actionPoints = value;
		}
		
		get {
			return actionPoints;
		}
		
	}


	//Risolvo l'offensiva terrestre
	public  void SolveOffensive(){

		if (ActionPoints > 0) {




		
			int selectedGroup = provinces[selectedProvince].groupID;
			int targetGroup = provinces[targetProvince].groupID;

			//Se la provincia è occupata dall'avversario e c'è un gruppo di unità avversarie, il gruppo attacca il gruppo avversario
			if (provinces[selectedProvince].Owner != provinces[targetProvince].Owner && targetGroup!=-1) {
					

				groups[selectedGroup].Attack(targetGroup);
			}

			//Altrimenti sposto semplicemente le unità nella nuova provincia
			else {
				if (targetGroup == -1)
					groups[selectedGroup].Move(targetProvince);

			}
		

		} else {
			UnityEditor.EditorUtility.DisplayDialog("ACTION POINTS","Not enough action points!","Ok");
		}
	
	}




	//Gestione turno del nemico
	public void EnemyTurn(){

		turnCounter ++;

		Debug.Log (turnCounter);

		if (turn == player) {

			//Gestisco i punti attacco di supporto
			playerSupport.ManagePoints();

			enemySupport.ManagePoints();

								//TURNO NEMICO
			turn = enemy;
			Debug.Log ("Solving enemy moves");


			//Ripristino punti azione
			ActionPoints = 2;

			/*IA NEMICA*/

			InfluenceMap.InitMap();

			while (ActionPoints > 0){

				InfluenceMap.Update();
				Plan.Init();
				Plan.ExecutePlan();
			}

			Debug.Log ("Enemy turn ended");

			turnCounter ++;
			
			Debug.Log (turnCounter);

								//TURNO GIOCATORE//
			turn = player;

			//Rinforzi a tutti gruppi di unità
			foreach (UnitGroup g in groups) {
				if (provinces[g.province].Owner == player)

					foreach (Unit u in g.units){
						u.AttackPoints+= (numPlayerProvinces)/2;
						u.DefencePoints+= (numPlayerProvinces)/2;
					}
			}


			//Ripristino punti azione
			ActionPoints = 4;

		}

	}


	// Use this for initialization
	void Start () {
		player = factions.WARSAW_PACT;
		enemy = factions.NATO;

		//Numero punti azione iniziale
		ActionPoints = 2;

		//Assegno il numero di province iniziale al giocatore e all'IA
		if (player == factions.WARSAW_PACT)
			numPlayerProvinces = 5;
		else if (player == factions.NATO)
			numPlayerProvinces = 7;

		if (enemy == factions.WARSAW_PACT)
			numEnemyProvinces = 5;
		else if (enemy == factions.NATO)
			numEnemyProvinces = 7;

		/*Random r = new Random();
		turn = (factions)r.Next();*/
		turn = player;
		
		turnCounter = 0;

		playerSupport = GameObject.Find ("playerSupport").GetComponent<Support> ();
		enemySupport = GameObject.Find ("enemySupport").GetComponent<Support> ();

		provinces = FindObjectsOfType<Province> ();
		provinces = provinces.OrderBy (p => p.ID).ToArray ();
		groups = FindObjectsOfType<UnitGroup> ();
		groups = groups.OrderBy (g => g.ID).ToArray ();

	}

	// Update is called once per frame
	void Update () {
	
	}

	/*void OnGUI() {

		if ( InfluenceMap.Instance.showValues)
			for (int i=0; i<InfluenceMap.Instance.positions.Count; i++) {
				GUI.Label(new Rect(provinces[i].transform.position.x + 5,provinces[i].transform.position.y + 5,200,200),InfluenceMap.Instance.positions[i].influence.ToString());
			}
	}*/
}


