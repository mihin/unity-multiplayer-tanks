using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class HostGame : MonoBehaviour
{
	public string roomName;
	public uint roomSize = 8;

	void Start () 
	{
		System.Random random = new System.Random();
		roomName = "TOG" + random.Next(1000,9999);
	}

	void OnGUI()
	{
		int posY = Screen.height/2;
		
		GUI.Label(new Rect(Screen.width/2-100, posY, 100, 30), "Room Name:");
		roomName = GUI.TextField(new Rect(Screen.width/2, posY, 200, 30), roomName);
		posY += 40;
		
		GUI.Label(new Rect(Screen.width/2-100, posY, 100, 30), "Room Size:");
		roomSize = System.Convert.ToUInt32(GUI.TextField(new Rect(Screen.width/2, posY, 200, 30), roomSize.ToString()));
		posY += 40;
		
		if (roomName != "")
		{
			if(GUI.Button(new Rect(Screen.width/2-60,posY,120,30),"Create Room"))
			{
				Debug.Log ("Creating match [" + roomName + ":" + roomSize + "]");
				NetworkManager.singleton.matchMaker.CreateMatch(roomName, roomSize, true, "", "", "", 0, 0, NetworkManager.singleton.OnMatchCreate);
			}
		}
		
		posY += 140;
		if (GUI.Button (new Rect(Screen.width/2-100 , Screen.height - 50, 200, 30), "[ Back ]") || Input.GetKeyDown(KeyCode.Escape))
		{
			NetworkManager.singleton.ServerChangeScene("title");
		}
	}	
}
