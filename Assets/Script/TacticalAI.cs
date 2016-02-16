using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Agent {
	
	Unit unit;
	PathFinding pathfinding;
	InfluenceMap influenceMap;
	List<PathFindingVertex> path;

	public Agent (Unit u)  {
		
		unit = u;
		pathfinding = PathFinding.getInstance ();
		influenceMap = InfluenceMap.getInstance ();
	}


	public Unit Unit{

		get{return unit;}
	}

	//Valutà l'eseguibilità dell'ordine sulla base del percorso calcolato
	public bool IsOrderFeasible(int targetID){

		//Se l'unità non può raggiungere l'obiettivo (percorso non esiste), allora restituisce false
		
		path = pathfinding.GetPath (influenceMap.influenceMapGraph,
			                        unit.Province.ID,targetID);

		if (path == null) return false;

		return  true;
	}

	//Esegue l'ordine
	public void ExecuteOrder(int targetID){


		//Aggiornamento mappa di influenza
		influenceMap.Update ();


		//Se non c'è percorso l'unità tiene la posizione. Altrimenti segue il percorso
		foreach (PathFindingVertex connection in path){

			PathFindingNode end = connection.END;

			Unit frontUnit = GameLogic.GetUnitInProvince(end.ID);
			factions frontFaction = GameLogic.GetProvinceFaction(end.ID);

			if (unit.ActionPoints == 0)
				break;

			//Se nella posizione vi è una unità nemica, decide come attaccare
			if ( frontUnit!=null && frontFaction == GameLogic.player){

				Decision root = new NotEnemyStronger(unit, frontUnit);
				
				Action result = (Action) root.makeDecision ();

				if (!result.execute())
					break;

			}

			//Altrimenti, muove verso la posizione
			else if (frontUnit==null) 
				unit.Move(GameLogic.provinces[end.ID]);

		}

	
	}


}



//generico nodo
interface DecisionTreeNode{

	DecisionTreeNode makeDecision();
}

//generica azione
abstract class Action : DecisionTreeNode {
	
	public DecisionTreeNode makeDecision(){
		
		return this;
	}
	
	abstract public bool execute();
}

//generica decisione
abstract class Decision : DecisionTreeNode {
	
	protected DecisionTreeNode trueNode;
	protected DecisionTreeNode falseNode;
	
	abstract protected  DecisionTreeNode getBranch();
	
	public DecisionTreeNode makeDecision(){
		
		//prende uno dei nodi figli
		DecisionTreeNode branch = getBranch ();
		
		//decisione corrispondente al figlio
		return branch.makeDecision ();
	}
}

class NotEnemyStronger : Decision {
	
	Unit attacker;
	Unit target;
	
	public NotEnemyStronger(Unit a, Unit t) {
		
		attacker = a;
		target = t;
		
		
		trueNode = new isAirSupportAvailable (attacker, target);
		falseNode = new NoAction ();
		
	}
	
	protected override DecisionTreeNode getBranch () {
		
		
		
		if (attacker.Strength >= target.Strength)
			return trueNode;
		
		else 
			return falseNode;
	}
	
}


//Controlla se è disponibile il supporto aereo
class isAirSupportAvailable : Decision {

	Unit attacker;
	Unit target;



	public isAirSupportAvailable(Unit a, Unit t) {

		attacker = a;
		target = t;


		trueNode = new AirAttack(attacker, target);
		falseNode = new isArtSupportAvailable(attacker, target);

	}

	protected override DecisionTreeNode getBranch () {

		if (GameLogic.enemySupport.AirSupportPoints > 0)

			return trueNode;
		else
			return falseNode;
	}

}

//Attacco con supporto aereo
class AirAttack : Action {

	Unit attacker;
	Unit target;


	public AirAttack(Unit a, Unit t) {

		attacker = a;
		target = t;
	}

	public override bool execute () {


		GameLogic.enemySupport.AirSupport (target);

		List<Unit> attackSupporters = new List<Unit>();

		//Unità che potrebbero supportare l'attacco
		foreach (Province n in target.Province.getNeighbours()){
			if (n.Unit!=null && n.Owner == attacker.Faction 
			    && n!=attacker && n.Unit.Strength >= 4)
				attackSupporters.Add(n.Unit);
		}
		
		string type = "Limited attack";
		
		if (attackSupporters.Count > 0)
			type = "Total assault"; 
		
		return attacker.Attack (target,type, attackSupporters);
	}
}

//Controlla se è disponibile il supporto d'artiglieria 
class isArtSupportAvailable : Decision {

	Unit attacker;
	Unit target;



	public isArtSupportAvailable(Unit a, Unit t) {

		attacker = a;
		target = t;


		trueNode = new ArtilleryAttack (attacker, target);
		falseNode = new NoSupport (attacker, target);
		
	}
	
	protected override DecisionTreeNode getBranch () {


		
		if (GameLogic.enemySupport.ArtSupportPoints > 0)
			
			return trueNode;

		else 
			return falseNode;
	}
	
}

//Attacco con supporto artiglieria
class ArtilleryAttack : Action {

	Unit attacker; 
	Unit target;


	public ArtilleryAttack(Unit a, Unit t) {

		attacker = a;
		target = t;
	}
	
	public override bool execute () {

		GameLogic.enemySupport.ArtSupport(target);

		
		List<Unit> attackSupporters = new List<Unit>();
		
		//Unità che potrebbero supportare l'attacco
		foreach (Province n in target.Province.getNeighbours()){
			if (n.Unit!=null && n.Owner == attacker.Faction 
			    && n!=attacker && n.Unit.Strength >= 4)
				attackSupporters.Add(n.Unit);
		}
		
		string type = "Limited attack";
		
		if (attackSupporters.Count > 0)
			type = "Total assault"; 
		
		return attacker.Attack (target,type, attackSupporters);
	}

}

//Attacco senza supporto
class NoSupport : Action {
	
	Unit attacker;
	Unit target;
	
	public NoSupport (Unit a, Unit t) {

		attacker = a;
		target = t;

	}
	
	public override bool execute ()
	{
		List<Unit> attackSupporters = new List<Unit>();
		
		//Unità che potrebbero supportare l'attacco
		foreach (Province n in target.Province.getNeighbours()){
			if (n.Unit!=null && n.Owner == attacker.Faction 
			    && n!=attacker && n.Unit.Strength >= 4)
				attackSupporters.Add(n.Unit);
		}
		
		string type = "Limited attack";
		
		if (attackSupporters.Count > 0)
			type = "Total assault"; 
		
		return attacker.Attack (target,type, attackSupporters);
	}
	
}





//Nessuna azione
class NoAction : Action {

	public override bool execute () {
		return false;
	}
}


