using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;

/*
public class BatchModeStartup : MonoBehaviour
{
	public string roomName = "";
	public uint roomSize = 64;
	public bool batchMode = false;
	
	void Awake()
	{
		string[] args = System.Environment.GetCommandLineArgs();
		for (int i=0; i < args.Length; i++)
		{
			string arg = args[i];
			if (arg == "-batchmode")
			{
				playmode = Manager.PlayMode.SinglePlayer;
				batchMode = true;
			}
			if (arg == "-roomName")
			{
				roomName = args[i+1];
			}
			if (arg == "-roomSize")
			{
				roomSize = System.Convert.ToUInt32(args[i+1]);
			}
			
			if (arg == "-multiHost" && batchMode)
			{
				playmode = Manager.PlayMode.MultiplayerHost;
			}
			
			if (arg == "-multiJoin" && batchMode)
			{
				playmode = Manager.PlayMode.MultiplayerJoin;
			}
			
			if (arg == "-singleJoin" && batchMode)
			{
				playmode = Manager.PlayMode.SinglePlayerJoin;
			}			
		}
		
		if (batchMode)
		{
			Debug.Log("Batchmode room:[" + roomName + "] mode:" + playmode);
			
			if (playmode == Manager.PlayMode.MultiplayerHost)
			{
				manager.CreateMatch(roomName, roomSize, true, "", manager.OnMatchCreate);
				return;
			}
			
			if (playmode == Manager.PlayMode.MultiplayerJoin)
			{
				manager.roomName = roomName;
				manager.ListMatches(0, 20, "", manager.OnMatchList);
			}
			
			if (playmode == Manager.PlayMode.SinglePlayerJoin || playmode == Manager.PlayMode.SinglePlayer)
			{
				manager.StartNetwork(playmode,"");
			}
			
		}
	}
}
*/
