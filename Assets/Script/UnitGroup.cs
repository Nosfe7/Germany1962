using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitGroup : MonoBehaviour {

	public int ID;
	public List<Unit> units;
	public int province;
	public factions faction;

	// Use this for initialization
	void Start () {

		//riempio la lista di unità
		for (int i=0; i<this.transform.childCount; i++)
			units.Add (this.transform.GetChild (i).GetComponent<Unit> ());

		//Setto la provincia di appartenenza
		province = this.transform.parent.GetComponent<Province> ().ID;
	
	}

	public bool Attack(int target){

		GameLogic.ActionPoints-=1;


		//Forza totale attaccanti (somma punti attacco unità attaccanti)
		int attackStrenght = 0;
		//Forza totale difensori (somma punti difesa difensori)
		int defenceStrenght = 0;
		//Provincia obiettivo
		int targetProvince = GameLogic.groups[target].province; 
		//Gruppo
		UnitGroup targetGroup = GameLogic.groups [target];
		
		///*FASE DI ATTACCO*///
		
		//sommo la forza di tutti gli attaccanti 
		foreach (Unit u in units) {
			attackStrenght += u.AttackPoints;
		}
		
		
		//Per ogni unità difensiva calcolo un numero di perdite pari a forza difensiva - (forza totale d'attacco/10) + random
		List<Unit> destroyList = new List<Unit> ();

		foreach (Unit u in targetGroup.units) {
			defenceStrenght += u.DefencePoints;
			
			u.DefencePoints -= (int)(attackStrenght / 10) - Random.Range (0, 5);
			
			//Se vado a 0 o sotto zero l'unità viene distrutta
			if (u.DefencePoints <= 0) {
				destroyList.Add (u);
			}
			
		}
		
		foreach (Unit u in destroyList ) {
			GameLogic.groups[target].destroyUnit(u);
		}
		
		
		///*FASE DI DIFESA*///
		
		//Per ogni unità attacante calcolo un numero di perdite pari a forza attacco - (forza totale difesa/10) + random
		destroyList = new List<Unit> ();

		foreach (Unit u in units) {
			u.AttackPoints -= (int)(defenceStrenght / 10) - Random.Range (0, 5);
			
			//Se vado a 0 o sotto zero l'unità viene distrutta
			if (u.AttackPoints <= 0) {
				destroyList.Add (u);
			}
		}
		
		foreach (Unit u in destroyList) {
			destroyUnit(u);
		}

		
		///*RISULTATO BATTAGLIA///
		
		
		//calcolo nuova forza totale difesa (dopo attacco)
		int newDefenceStrength = 0;
		foreach (Unit u in targetGroup.units)
			newDefenceStrength+=u.defencePoints;
		
		
		//Se forza delle unità attaccanti maggiore o uguale al doppio rispetto a quelle in difesa, gli attaccanti occupano la provincia
		if (newDefenceStrength <= attackStrenght / 2) {

			//Il gruppo di unità in difesa si ritira in una provincia alleata, raggiungibile senza passare per province nemiche
				
			int allied = findAlliedProvince (targetProvince);
			
			
			if (allied != -1) {
				targetGroup.Move (allied);
			}
			
			//Se non trovo la provincia (unità accerchiate), distruggo il gruppo di unità
			else {
				Destroy (targetGroup.gameObject);
			}
			
			//Sposto il gruppo di unità in attacco nella nuova provincia 
		
			Move (targetProvince);

			return true;
		} 

		return false;
	}

	public void Move(int target) {

		GameLogic.ActionPoints-=1;

		//Fazione attaccante
		factions attackerFaction = GameLogic.provinces[province].Owner;

		//Cambio Provincia
		GameLogic.provinces[target].setGroup(ID);
		province = target;
		
		//Cambio fazione della nuova provincia
		GameLogic.provinces[target].Owner = attackerFaction;
		
		//Aggiorno numero province giocatore e nemico
		if (GameLogic.provinces[province].Owner == GameLogic.player){
			GameLogic.numPlayerProvinces ++; 
			GameLogic.numEnemyProvinces --;
		}
		
		else {
			GameLogic.numEnemyProvinces ++;
			GameLogic.numPlayerProvinces --;
		}
		
	}


	public void destroyUnit(Unit u){
		Destroy (u.gameObject);
		units.Remove (u); 
	}


	/*DA CAMBIARE CON UNA RICERCA DELLA ZONA PIU SICURA, DETERMINATA DALLA MAPPA DI INFLUENZA !
	 * INOLTRE LE UNITA' DEL GRUPPO DI UNITA' CHE NON TROVA UNA PROVINCIA LIBERA DEVONO ESSERE TRASFERITE IN ALTRI GRUPPI DI UNITA' 
	 CON SPAZIO LIBERO, PRIMA CHE IL GRUPPO DI UNITA' VENGA DISTRUTTO ! */

	//Cerca ricorsivamente una provincia alleata, raggiungibile senza passare da province nemiche, con spazio libero per una unità

	int recursiveFindAllied(int start, List<int> visited){ 


		foreach (int n in GameLogic.provinces[start].neighbours) {

			if (GameLogic.provinces[n].Owner != GameLogic.turn && !visited.Contains (n)) {
				if (GameLogic.provinces[n].groupID == -1) 
					return n;
				else {
					visited.Add (n);
					int end = recursiveFindAllied (n,  visited);
					if (end != -1)
						return end;
				}
			}
		}
		
		return -1;
	}
	
	
	public int findAlliedProvince(int p){
		
		List<int> visited = new List<int> ();
		visited.Add (p);
		
		return recursiveFindAllied(p, visited);
	}


	// Update is called once per frame
	void Update () {
		
	}
}
