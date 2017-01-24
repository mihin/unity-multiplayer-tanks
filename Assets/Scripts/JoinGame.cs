using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

public class JoinGame : MonoBehaviour
{
	List<MatchInfoSnapshot> roomList = null;
	
	public int pageNum = 0;

	NetworkManager manager;
	
	void Start () 
	{
		manager = NetworkManager.singleton;
	    manager.matchMaker.ListMatches(0, 10, "", true, 0, 0, OnMatchList);
	}

    private void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    //public void OnMatchList(ListMatchResponse matchList)
	{
        if (!success)
        {
            Debug.LogWarning("JoinGame :: Match List unsuccessful");
            return;
        }
        if (matchList == null)
        {
            Debug.LogWarning("JoinGame :: null Match List returned from server");
            return;
        }

        Debug.Log("JoinGame :: Match List successful, extendedInfo = " + extendedInfo + ", data: " + matchList);
        roomList = matchList;
	}
	
	void OnGUI()
	{
		if (roomList == null)
		{
			GUI.Label(new Rect(Screen.width/2 - 60, Screen.height/4, 100, 20), "Loading..");
		}
		else
		{	
			int posY = Screen.height/4;
			foreach (var info in roomList)
			{
                if (GUI.Button(new Rect(Screen.width / 2 - 100, posY, 200, 20), "Join Game: " + info.name))
				{
					manager.matchMaker.JoinMatch(info.networkId, "" , "", "", 0, 0, manager.OnMatchJoined);
				}
				posY += 25;
			}
			if (pageNum > 1) {
				if (GUI.Button (new Rect(Screen.width/2 - 100, posY, 90, 30), " << Prev (" + (pageNum-1) + ")"))
				{
					pageNum -= 1;
					if (pageNum < 1)
					{
						pageNum = 1;
					}
					else
					{
						roomList = null;
						manager.matchMaker.ListMatches(pageNum, 10, "", OnMatchList);
					}
				}
			}
			
			if (GUI.Button (new Rect(Screen.width/2 , posY, 90, 30), "Next (" + (pageNum+1) + ") >>"))
			{
				pageNum += 1;
				roomList = null;
				manager.matchMaker.ListMatches(pageNum, 10, "", OnMatchList);
			}
			posY += 40;
		}
		
		if (GUI.Button (new Rect(Screen.width/2-100 , Screen.height - 50, 200, 30), "[ Back ]") || Input.GetKeyDown(KeyCode.Escape))
		{
			manager.ServerChangeScene("title");
		}
	}
}
