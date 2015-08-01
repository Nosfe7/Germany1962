using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Province : MonoBehaviour {
	public factions owner;
	public int ID;


	//Provincie vicine
	public List<int> neighbours;
	//Gruppo di unità nella provincia
	public int groupID;
	

	public factions Owner{

		get{
			return owner;
		}

		set{
				//Se cambio occupante, cambio bandiera
				SpriteRenderer sr = GetComponent<SpriteRenderer> ();

				if (value == factions.NATO)
					sr.sprite = Resources.Load<Sprite> ("Graphics/Images/natoFlag");
				else if (value == factions.WARSAW_PACT)
					sr.sprite = Resources.Load<Sprite> ("Graphics/Images/redstar");
				owner = value;
		}

	}


	public void setGroup(int g){
		GameLogic.groups[g].transform.parent = transform;
		GameLogic.groups[g].transform.position = transform.position;
		groupID = g;
	}
	

	// Use this for initialization
	void Start () {
			
		//Gruppo di unità
		if (this.transform.childCount > 0)
			groupID = this.transform.GetChild (0).GetComponent<UnitGroup> ().ID;
	}	
	
	// Update is called once per frame
	void Update () {

			
	}

	/*Il giocatore, durante il suo turno, può selezionare la provincia alleata col tasto sinistro, e la provincia obiettivo con quello destro.
	 La provincia obiettivo deve essere vicina a quella selezionata*/
	void OnMouseOver(){
		if (GameLogic.player == GameLogic.turn){



			if (owner == GameLogic.player && Input.GetMouseButton(0)) {


				GameLogic.selectedProvince = ID;
				Debug.Log (this);
			}

			if (Input.GetMouseButton(1) && neighbours.Contains(GameLogic.selectedProvince)){
				GameLogic.targetProvince = ID;
				Debug.Log (this);

			}


		}
	}
}
