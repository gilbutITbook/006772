using UnityEngine;
using System.Collections;

// 가위바위보에 낼 손 종류.
public enum RPSKind {
    None = -1,		// 미결정.
    Rock = 0,		// 바위.
    Paper,			// 보.
    Scissor,		// 가위.
};

// 공격/방어 설정.
public enum ActionKind {
    None = 0,		// 미결정.
    Attack,			// 공격.
    Block,			// 방어.
};

// 공격/방어 정보 구조체.
public struct AttackInfo {
    public ActionKind actionKind;
    public float actionTime;        // 경과 시간.

    public AttackInfo(ActionKind kind, float time) {
        actionKind = kind;
        actionTime = time;
    }
};



public struct InputData{
    public RPSKind rpsKind;         //가위바위보 선택.
    public AttackInfo attackInfo;   //공방 정보.
} 





// 승자 식별.
public enum Winner {
    None = 0,		// 미결정.
    ServerPlayer,	// 서버 쪽(1P) 승리.
    ClientPlayer,	// 클라이언트 쪽(2P) 승리.
    Draw,			// 무승부.
};


class ResultChecker {
    //가위바위보 승패를 구합니다.
    public static Winner GetRPSWinner(RPSKind server, RPSKind client) {
        // 1P와 2P의 수를 수치화합니다.
        int serverRPS = (int)server;
        int clientRPS = (int)client;

        if (serverRPS == clientRPS) {
            return Winner.Draw; //무승부.
        }

        // 수치의 차이를 이용해 처리 판정을 합니다.
        if (serverRPS == (clientRPS + 1) % 3) {
            return Winner.ServerPlayer;  //1P 승리.
        }
        return Winner.ClientPlayer; //2P 승리.
    }

    
    // 가위바위보 결과와 공격/방어에서 승패를 구합니다.
    public static Winner GetActionWinner(AttackInfo server, AttackInfo client, Winner rpsWinner) {
        string debugStr = "rpsWinner:" + rpsWinner.ToString();
        debugStr += "    server.actionKind:" + server.actionKind.ToString() + " time:" + server.actionTime.ToString();
        debugStr += "    client.actionKind:" + client.actionKind.ToString() + " time:" + client.actionTime.ToString();
        Debug.Log(debugStr);


        ActionKind serverAction = server.actionKind;
        ActionKind clientAction = client.actionKind;

        // 공격/방어가 바르게 이루어졌는지 판정합니다.
        switch (rpsWinner) {
        case Winner.ServerPlayer:
            if (serverAction != ActionKind.Attack) {
                // 1P가 공격을 하지 않았으므로 무승부.
                return Winner.Draw;
            }
            else if (clientAction != ActionKind.Block) {
                // 2P가 틀렸으므로 1P 승리.
                return Winner.ServerPlayer;
            }
            // 決着は시간이 됩니다.
            break;

        case Winner.ClientPlayer:
            if (clientAction != ActionKind.Attack) {
                // 2P가 공격을 하지 않았으므로 무승부..
                return Winner.Draw;
            }
            else if (serverAction != ActionKind.Block) {
                // 1P가 틀렸으므로 2P가 승리합니다.
                return Winner.ClientPlayer;
            }
            // 決着は시간이 됩니다.
            break;

        case Winner.Draw:
            //무승부일 때는 무엇을 해도 무승부입니다.
            return Winner.Draw;
        }

        
        // 시간 대결.
        float serverTime = server.actionTime;
        float clientTime = client.actionTime;

        if (serverAction == ActionKind.Attack) {
            // 1P가 공격인 경우는 2P보다 빠를 때 이기게 됩니다.
            if (serverTime < clientTime) {
                // 1P 쪽이 빠르므로 승리입니다.
                return Winner.ServerPlayer;
            }
        }
        else {
            // 2P가 공격인 경우는 2P보다도 빠르게 방어하지 않으면 패배입니다.
            if (serverTime > clientTime) {
                return Winner.ClientPlayer;
            }
        }

        // 같은 시간이므로 무승부입니다.
        return Winner.Draw;
    }



    // 테스트 코드.
    static void Assert(bool condition) {
        if (!condition) {
            throw new System.Exception();
        }
    }
    public static void WinnerTest(){
        
        Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Paper) == Winner.Draw);
        Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Rock) == Winner.ServerPlayer);
        Assert(GetRPSWinner(RPSKind.Paper, RPSKind.Scissor) == Winner.ClientPlayer);
        Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Paper) == Winner.ClientPlayer);
        Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Rock) == Winner.Draw);
        Assert(GetRPSWinner(RPSKind.Rock, RPSKind.Scissor) == Winner.ServerPlayer);
        Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Paper) == Winner.ServerPlayer);
        Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Rock) == Winner.ClientPlayer);
        Assert(GetRPSWinner(RPSKind.Scissor, RPSKind.Scissor) == Winner.Draw);

        AttackInfo s;
        s.actionKind = ActionKind.Attack;
        s.actionTime = 1.0f;
        // 시간：같다, 빠르다, 느리다를 시험합니다.
        //win & attack
        s.actionKind = ActionKind.Attack;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.ServerPlayer) == Winner.ServerPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.ServerPlayer) == Winner.ServerPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.ServerPlayer) == Winner.ServerPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.ServerPlayer) == Winner.ServerPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.ServerPlayer) == Winner.ServerPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.ServerPlayer) == Winner.ServerPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.ServerPlayer) == Winner.ServerPlayer);
        //win & block
        s.actionKind = ActionKind.Block;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.ServerPlayer) == Winner.Draw);
        //win & none
        s.actionKind = ActionKind.None;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.ServerPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.ServerPlayer) == Winner.Draw);

        //lose & attack
        s.actionKind = ActionKind.Attack;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.ClientPlayer) == Winner.Draw);
        //lose & block
        s.actionKind = ActionKind.Block;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.ClientPlayer) == Winner.Draw);
        //lose & none
        s.actionKind = ActionKind.None;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.ClientPlayer) == Winner.ClientPlayer);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.ClientPlayer) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.ClientPlayer) == Winner.Draw);

        //draw & attack
        s.actionKind = ActionKind.Attack;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.Draw) == Winner.Draw);
        //draw & block
        s.actionKind = ActionKind.Block;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.Draw) == Winner.Draw);
        //draw & none
        s.actionKind = ActionKind.None;
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 1), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 2), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Attack, 0), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.Block, 0), Winner.Draw) == Winner.Draw);
        Assert(GetActionWinner(s, new AttackInfo(ActionKind.None, 0), Winner.Draw) == Winner.Draw);
    }

}
