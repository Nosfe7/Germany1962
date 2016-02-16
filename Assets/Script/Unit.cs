using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class Unit : MonoBehaviour {

	public int ID;
	Province province;
	public	factions faction;
	int actionPoints;
	int supportPoints;
	float strength;
	StrengthIndicator strengthIndicator;
	
	//Punti azione
	public int ActionPoints {
		set {
			actionPoints = value;
			if (actionPoints<=0)
				actionPoints = 0;
		}
		
		get {
			return actionPoints;
		}
		
	}

	//Punti supporto (quanti supporti fuori mappa può usare l'unità per turno)

	public int SupportPoints {

		set {
			supportPoints = value;
			if (supportPoints<=0)
				supportPoints = 0;
		}
		
		get {
			return supportPoints;
		}



	}

	//Forza 
	public float Strength {
		set {
			strength = value;

			if (strength<=0)
				strength = 0;

			int maxValue = 0;

			maxValue = 20;

			if (strength>=maxValue)
				strength = maxValue;

			UpdateIndicator ();
		}
		
		get {
			return strength;
		}
		
	}

	public factions Faction {

		get {return faction;}
	}

	public Province Province {

		set {
			transform.parent = value.transform;
			transform.position = value.transform.position;
			transform.Translate(Vector3.back/10 );
			province = value;

			UpdateIndicator();

			if (this == GameLogic.selectedUnit)
				GameLogic.selectBox.transform.position = transform.position;

			if (this == GameLogic.targetUnit)
				GameLogic.targetBox.transform.position = transform.position;


		}

		get {
			return province;
		}

	}


	//Set-up indicatore forza
	public void SetupIndicator() {

		strengthIndicator = GameLogic.strengthIndicators[ID];
		UpdateIndicator ();

	}

	//Aggiorna indicatore forza
	public void UpdateIndicator(){




		Text strengthText = strengthIndicator.gameObject.GetComponent<Text> ();

		strengthText.text = strength.ToString ();

		if (strengthText.text.Length > 2)
			strengthText.text = strengthText.text.Substring(0,2);

		/*if (strength >= 15)
			strengthText.color = new Color(0.2F,0.4F,0.2F);
		else if (strength >= 7 && strength <= 14)
			strengthText.color = new Color(0.6F,0.7F,0.1F);
		else if (strength<7)
			strengthText.color = new Color(0.9F,0F,0F);*/


		strengthIndicator.transform.position = Camera.main.WorldToScreenPoint(transform.position);
	

	}

	// Use this for initialization
	void Awake () {

		//Ogni gruppo di unità ha un punto azione a e un supporto a disposizione ogni turno
		actionPoints = 1;
		supportPoints = 1;

		//Provincia dell'unità 
		province = transform.GetComponentInParent<Province>();

		//Forza dell'unità
		strength = 20;
	
	}

	public bool Attack(Unit targetUnit, string type, List<Unit> attackSupporters){

		ActionPoints-=1;


		//Provincia obiettivo
		Province targetProvince = targetUnit.province; 


		UnityEditor.EditorUtility.DisplayDialog("ATTACK","Attack from " + province + " to " + targetProvince,"Ok");
		
									/*ATTACCO*/
		float totalAttackingStrength = strength;

		if (type == "Total assault") {

			foreach (Unit u in attackSupporters)

				if (u.ActionPoints>0){
					totalAttackingStrength+=u.Strength;
					u.ActionPoints-=1;
			}

		}

		
		
		//Per l'unità difensiva, calcola un numero di perdite pari a forza difensiva - (forza totale d'attacco/10) + random

		float defenceStrenght = targetUnit.Strength;
			
		targetUnit.Strength -= (totalAttackingStrength / 10) - Random.Range (-totalAttackingStrength / 10, totalAttackingStrength / 10);

		if (targetUnit.strength <= 0)
			targetUnit.DestroyUnit ();


									/*DIFESA*/
			
		//Per l'unità attacante calcolo un numero di perdite pari a forza attacco - (forza totale difesa/10) + random

		Strength -= (defenceStrenght / 10) - Random.Range (-defenceStrenght / 10, defenceStrenght / 10);

		if (type == "Total assault") {
			
			foreach (Unit u in attackSupporters){
				u.Strength -=  (defenceStrenght / 10) - Random.Range (-defenceStrenght / 10, defenceStrenght / 10);

				if (u.Strength<=0)
					u.DestroyUnit();
			}
		}

		if (strength <= 0)
			DestroyUnit ();
			

		
									/*RISULTATO BATTAGLIA*/
		
		
		//calcolo nuova forza difesa (dopo attacco)
		float newDefenceStrength = targetUnit.Strength;
		
		
		//Se forza delle unità attaccanti maggiore o uguale al doppio rispetto a quelle in difesa, gli attaccanti occupano la provincia
		if (newDefenceStrength <= strength / 2) {

			//Il gruppo di unità in difesa si ritira in una provincia alleata, raggiungibile senza passare per province nemiche
				
			Province allied = FindAlliedProvinceFrom (targetProvince);
			
			
			if (allied != null) {
				targetUnit.Move (allied);
			}
			
			//Se non trova la provincia (unità accerchiata), distrugge il gruppo di unità
			else {
				targetUnit.DestroyUnit();

			}
			
							/*MOVIMENTO VERSO LA NUOVA PROVINCIA*/
		


			Move(targetProvince);

			return true;
		} 


		return false;
	}

	public void Move(Province target) {

			ActionPoints -= 1;
				
			//Movimento verso una provincia nemica
			if (target.Owner != faction) {

					UnityEditor.EditorUtility.DisplayDialog ("MOVE", "Move from " + province + " to " + target, "Ok");

					//aggiorna numero province giocatore e nemico
					if (faction == GameLogic.player) {
						GameLogic.numPlayerProvinces ++; 
						GameLogic.numEnemyProvinces --;
					} else {
						GameLogic.numEnemyProvinces ++;
						GameLogic.numPlayerProvinces --;
					
					}

			}

			//La vecchia provincia non ha più una unità
			province.Unit = null;
			
			//Cambio Provincia
			target.Unit = this;
			Province = target;

			//Cambio fazione della nuova provincia
			target.Owner = faction;

		
	}

	


	/*Cerca una provincia alleata nelle vicinanze con spazio libero per il gruppo 
	 * e più vicina alla capitale alleata
	*/

	Province FindAlliedProvinceFrom(Province p){

		int alliedCapital = 0;

		if (p.Owner == factions.BLUE)
			alliedCapital = GameLogic.blueCapital;
		else if (p.Owner == factions.RED)
			alliedCapital = GameLogic.redCapital;

		int minDistance = 10;
		Province nearestNeighbour = null;

		foreach (Province neighbour in p.getNeighbours())
			if (neighbour.Owner ==  p.Owner && 
				neighbour.Unit == null){

				int distance = GameLogic.mapGraph.Distance (neighbour.ID, alliedCapital);
				if ( distance< minDistance) {
					minDistance = distance;
					nearestNeighbour = neighbour;
				}
		}

		if (nearestNeighbour != null)
			return nearestNeighbour;
		else 
			return null;
	}


	void DestroyUnit(){

		Destroy (this.gameObject);
		Destroy(this.strengthIndicator.gameObject);
		GameLogic.units[ID] = null;
		
		
		StrategicPlanner sp = StrategicPlanner.getInstance();
		
		foreach (Agent agent in sp.agents){
			
			if (agent.Unit == this){
				sp.agents.Remove(agent);
				break;
			}
		}

	}


	// Update is called once per frame
	void Update () {
		
	}
}
