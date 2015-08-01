using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

	public factions faction;
	public int attackPoints;
	public int defencePoints;
	public string type;


	public int AttackPoints{
		set{

			if (type == "Tank"){
				if (value>20)
					attackPoints = 20;
				else if (value<0)
					attackPoints = 0;
				else 
					attackPoints = value;

			}
		
			else if (type == "Infantry"){
				if (value>10)
					attackPoints = 10;
				else if (value<0)
					attackPoints = 0;
				else 
					attackPoints = value;
				
			}
		}

		get {

			return attackPoints;
		}


	}


	public int DefencePoints{
		set{
			if (value>10)
				defencePoints = 10;
			else if (value<0)
				defencePoints = 0;
			else
				defencePoints = value;
		}
		
		get{

			return defencePoints;
		}
	}



 	// Use this for initialization
	void Start () {

		if (type == "Tank")
			attackPoints = 20;
		else if (type == "Infantry")
			attackPoints = 10;

	}
	
	// Update is called once per frame
	void Update () {


	}


}

