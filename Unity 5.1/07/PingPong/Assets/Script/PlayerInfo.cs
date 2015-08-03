using System.Collections;
using System.Collections.Generic;

public class PlayerInfo {

	public const int 			playerNum = 2;

	private static PlayerInfo	instance = new PlayerInfo();

	private int					playerId = 0;

	// Use this for initialization
	private PlayerInfo(){
	}
	
	// Get singleton instance
	public static PlayerInfo GetInstance(){
		return instance;
	}

	//
	public void SetPlayerId(int id){
		playerId = id;
	}
 
	//
	public int GetPlayerId(){
		return playerId;
	}
}
