using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class Province : MonoBehaviour {
	public factions owner;

	//Identificativo
	public int ID;
	//Provincie vicine
	public List<Province> neighbours;
	//Unità nella provincia
	public Unit unit;
	

	public factions Owner{

		get{
			return owner;
		}

		set{
				//Se cambio occupante, cambio bandiera
				SpriteRenderer sr = GetComponent<SpriteRenderer> ();

				if (value == factions.RED)
					sr.sprite = Resources.Load<Sprite> ("Graphics/Images/redhex");
				else if (value == factions.BLUE)
					sr.sprite = Resources.Load<Sprite> ("Graphics/Images/bluehex");
				owner = value;
		}

	}


	public Unit Unit{

		set {

			unit = value;
		}

		get{return unit;}

	}

	public List<Province> getNeighbours() {

		return neighbours;
	}

	// Use this for initialization
	void Start () {

		unit = transform.GetComponentInChildren<Unit> ();

	}	
	
	// Update is called once per frame
	void Update () {

			
	}

	/*Il giocatore, durante il suo turno, può selezionare la provincia alleata col tasto sinistro, e la provincia obiettivo con quello destro.
	 La provincia obiettivo deve essere vicina a quella selezionata*/
	void OnMouseOver(){
		if (GameLogic.player == GameLogic.turn){

			if (owner == GameLogic.player && Input.GetMouseButton(0)) {


				GameLogic.selectedProvince = this;

				if (GameLogic.selectedProvince.Unit!=null){
					GameLogic.selectedUnit = GameLogic.selectedProvince.Unit;
					GameLogic.selectBox.transform.position = GameLogic.selectedUnit.transform.position;
				}
			}

			if (Input.GetMouseButton(1) && neighbours.Contains(GameLogic.selectedProvince)){

				GameLogic.targetProvince = this;

				if (GameLogic.targetProvince.Unit!=null){
					GameLogic.targetUnit = GameLogic.targetProvince.Unit;
					GameLogic.targetBox.transform.position = GameLogic.targetUnit.transform.position;
				}

			}


		}
	}
}
