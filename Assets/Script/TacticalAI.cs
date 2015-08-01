
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


//si chiede se il gruppo attaccato è debole
class isTargetWeaker : Decision {

	public static int targetID;
	public static int attackerID;

	public isTargetWeaker (int aID, int tID) {

		attackerID = aID;
		targetID = tID;

		trueNode = new NoSupport ();
		falseNode = new isAirSupportAvailable ();

	}

	protected override DecisionTreeNode getBranch () {

		float attackStrength = 0;

	 	foreach (Unit u in GameLogic.groups[attackerID].units)
			attackStrength += u.AttackPoints;

		float defenceStrength = 0;

		foreach (Unit u in GameLogic.groups[targetID].units)
			defenceStrength += u.DefencePoints;

	
		if (defenceStrength <= attackStrength / 2)
			return trueNode;
		else
			return falseNode;

	}
}

//Attacco senza supporto
class NoSupport : Action {
	

	public NoSupport () {}

	public override bool execute ()
	{
		return GameLogic.groups [isTargetWeaker.attackerID].Attack (isTargetWeaker.targetID);
	}

}

//Controlla se è disponibile supporto aereo
class isAirSupportAvailable : Decision {


	public isAirSupportAvailable() {

		trueNode = new AirAttack ();
		falseNode = new isNukeSupportAvailable ();

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


	public AirAttack() {}

	public override bool execute () {

		GameLogic.enemySupport.AirSupport (isTargetWeaker.targetID);

		return GameLogic.groups [isTargetWeaker.attackerID].Attack (isTargetWeaker.targetID);
	}
}

//Controlla se è disponibile il supporto nucleare 
class isNukeSupportAvailable : Decision {

	public isNukeSupportAvailable() {
		
		trueNode = new NukeAttack ();
		falseNode = new NoAction ();
		
	}
	
	protected override DecisionTreeNode getBranch () {
		
		if (GameLogic.enemySupport.NukeSupportPoints > 0)
			
			return trueNode;
		else
			return falseNode;
	}
	
}

//Attacco con supporto nucleare
class NukeAttack : Action {

	public NukeAttack() {}
	
	public override bool execute () {
		
		GameLogic.enemySupport.NukeSupport (isTargetWeaker.targetID);
		
		return GameLogic.groups [isTargetWeaker.attackerID].Attack (isTargetWeaker.targetID);
	}

}


//Nessuna azione
class NoAction : Action {

	public NoAction() {}
	
	public override bool execute () {
		
		return false;
	}

}
