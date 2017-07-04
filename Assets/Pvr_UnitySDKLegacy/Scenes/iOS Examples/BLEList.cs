using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using LitJson;
public class BLEList : MonoBehaviour {
	public GameObject root;
	public BLEListener listener;

	// Use this for initialization
	void Start () {

	}
		
	public void Show()
	{
		listener.ToggleBLEState ();
	}
		
	public void Scan( )
	{
		if (PicoVRManager.SDK.currentDevice.IsBluetoothOpened()) {
			bool result = PicoVRManager.SDK.currentDevice.ScanBLEDevice ();
			if (result) {
				root.transform.Find ("Canvas").gameObject.SetActive (false);
				root.transform.Find ("HeadWearListCanvas").gameObject.SetActive (false);
				root.transform.Find ("BLEListCanvas").gameObject.SetActive (false);
				root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "scanning devices...";
				root.transform.Find ("TTT").gameObject.SetActive (true);
			} else {
				root.transform.Find ("Canvas").gameObject.SetActive (true);
				root.transform.Find ("HeadWearListCanvas").gameObject.SetActive (false);
				root.transform.Find ("BLEListCanvas").gameObject.SetActive (false);
				root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "please turn on the BLE...";
				root.transform.Find ("TTT").gameObject.SetActive (true);
			}
		} else {
			root.transform.Find ("Canvas").gameObject.SetActive (true);
			root.transform.Find ("HeadWearListCanvas").gameObject.SetActive (false);
			root.transform.Find ("BLEListCanvas").gameObject.SetActive (false);
			root.transform.Find ("TTT/Panel/Text").GetComponent<Text> ().text = "please turn on the BLE...";
			root.transform.Find ("TTT").gameObject.SetActive (true);
		}
	}


	public void deviceClicked (int index){
		if (index < listener.devicelist.Count) {
			BLEListener.BLEDevice dmodel = listener.devicelist [index];
			PicoVRManager.SDK.currentDevice.ConnectBLEDevice (dmodel.mac);
			root.transform.Find ("BLEListCanvas").gameObject.SetActive (false);
		}
	}
}
