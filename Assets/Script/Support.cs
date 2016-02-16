using UnityEngine;
using System.Collections;

public class Support : MonoBehaviour {

	public factions faction;
	
	int airSupportPoints;
	int countToReloadAir;
	
	int artSupportPoints;
	int countToReloadArt;
	
	
	public int AirSupportPoints{
		set {
			if (value<=0)
				airSupportPoints = 0;
			else 
				airSupportPoints = value;
		}
		
		get {
			return airSupportPoints;
		}
	}
	
	public int ArtSupportPoints{
		set{
			if (value<=0)
				artSupportPoints = 0;
			else 
				artSupportPoints = value;
		}
		
		get{
			return artSupportPoints;
		}
	}
	

	//evento al click
	public void clickSupport (string type) {

		Unit selectedUnit = GameLogic.selectedUnit;
		Unit targetUnit = GameLogic.targetUnit;

		if (selectedUnit.SupportPoints > 0) {

			selectedUnit.SupportPoints -= 1;

			if (type == "Air")
				AirSupport (targetUnit);
			else if (type == "Artillery")
				ArtSupport (targetUnit);
		}

		else 
			UnityEditor.EditorUtility.DisplayDialog("OFF-MAP SUPPORT","Can't call off-map support","Ok");
	}

	//Supporto aereo
	public void AirSupport(Unit targetUnit){


		if (airSupportPoints > 0) {
			targetUnit.Strength -= 4 - Random.Range (-3, 3);

			AirSupportPoints -= 1;

		} else {
				UnityEditor.EditorUtility.DisplayDialog("SUPPORT POINTS","Not enough air support points","Ok");
		}
		
	}
	
	//Supporto di artiglieria
	public void ArtSupport(Unit targetUnit){


		if (artSupportPoints > 0) {
			targetUnit.Strength -= 2 - Random.Range (-1, 1);

			ArtSupportPoints -= 1;

		} else {
				UnityEditor.EditorUtility.DisplayDialog("SUPPORT POINTS","Not enough artillery support points","Ok");
		}
		
	}
	
	public void ManagePoints(){

		//Quando finiscono i punti supporto aereo, li ricarico dopo 5 turni
		if (airSupportPoints == 0){

			countToReloadAir ++;

			if (countToReloadAir ==5){

				countToReloadAir = 0;
				airSupportPoints = 1;
			}
		}
		
		//Quando finiscono i punti supporto artiglieria, li ricarico dopo 2 turni
		if (artSupportPoints == 0){

			countToReloadArt ++;

			if (countToReloadArt ==2){

				countToReloadAir = 0;
				artSupportPoints = 5;
			}
			
			
		}
	}

	// Use this for initialization
	void Awake () {
		airSupportPoints = 1;
		artSupportPoints = 5;
		countToReloadAir = 0;
		countToReloadArt = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
