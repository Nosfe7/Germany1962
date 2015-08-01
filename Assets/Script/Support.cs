using UnityEngine;
using System.Collections;

public class Support : MonoBehaviour {

	public factions faction;
	
	int airSupportPoints;
	int countToReloadAir;
	
	int nukeSupportPoints;
	int countToReloadNuke;
	
	
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
	
	public int NukeSupportPoints{
		set{
			if (value<=0)
				nukeSupportPoints = 0;
			else 
				nukeSupportPoints = value;
		}
		
		get{
			return nukeSupportPoints;
		}
	}
	

	//evento al click
	public void clickSupport (string type) {

		int targetGroup = GameLogic.provinces[GameLogic.targetProvince].groupID;

		if (type == "Air")
			AirSupport (targetGroup);
		else if (type == "Nuke")
			NukeSupport (targetGroup);
	}

	//Supporto aereo
	public void AirSupport(int targetGroup){
		Debug.Log ("air support request from"); Debug.Log (GameLogic.selectedProvince);
		Debug.Log ("air bombing run to"); Debug.Log ( GameLogic.targetProvince);


		if (airSupportPoints > 0 && GameLogic.ActionPoints >0) {
			foreach (Unit u in GameLogic.groups[targetGroup].units)
				u.defencePoints -= 4 - Random.Range (0, 3);

			AirSupportPoints -= 1;

		} else {
			if (airSupportPoints ==0)
				UnityEditor.EditorUtility.DisplayDialog("SUPPORT POINTS","Not enough air support points","Ok");
			else
				UnityEditor.EditorUtility.DisplayDialog("ACTION POINTS","Not enough action points!","Ok");
		}
		
	}
	
	//Supporto nucleare tattico
	public void NukeSupport(int targetGroup){
		Debug.Log ("tactical nuclear support request from"); Debug.Log (GameLogic.selectedProvince);;
		Debug.Log ("tactical nuclear bombing run to"); Debug.Log ( GameLogic.targetProvince);


		if (nukeSupportPoints > 0 && GameLogic.ActionPoints>0) {
			foreach (Unit u in GameLogic.groups[targetGroup].units)
				u.defencePoints -= 7 - Random.Range (0, 3);
			
			
			NukeSupportPoints -= 1;

		} else {
			if (nukeSupportPoints ==0)
				UnityEditor.EditorUtility.DisplayDialog("SUPPORT POINTS","Not enough nuke support points","Ok");
			else
				UnityEditor.EditorUtility.DisplayDialog("ACTION POINTS","Not enough action points!","Ok");
		}
		
	}
	
	public void ManagePoints(){

		//Quando finiscono i punti supporto aereo, li ricarico dopo 5 turni
		if (airSupportPoints == 0){

			countToReloadAir ++;

			if (countToReloadAir ==5){

				countToReloadAir = 0;
				airSupportPoints = 5;
			}
		}
		
		//Quando finiscono i punti supporto nucleare, li ricarico dopo 15 turni
		if (nukeSupportPoints == 0){

			countToReloadNuke ++;

			if (countToReloadNuke ==15){

				countToReloadAir = 0;
				nukeSupportPoints = 1;
			}
			
			
		}
	}

	// Use this for initialization
	void Start () {
		airSupportPoints = 5;
		nukeSupportPoints = 1;
		countToReloadAir = 0;
		countToReloadNuke = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
