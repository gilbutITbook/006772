using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    public AudioClip m_landingSE;   //착지음.
    public AudioClip m_missSE;      //헛스윙음.
    public AudioClip m_slipSE;      //미끄러지는음.

    //날아가는 처리는 AddComponent에서 추가-->효과음을 가질 장소가 없으므로 여기서 가집니다.
    public AudioClip m_hitSE;       //최초의 날아가는 소리.
    public AudioClip m_collideSE;   //무대 장치에 부딪혔을 때의 소리.
    public AudioClip m_collideGroundSE; //지면에 부딪혔을 때의 소리.

    //애니메이션 정의.
    public enum Motion {
        In,             //입장.
        Idle,           //대기 동작.
        RPSInputWait,   //가위바위보 선택 대기.
        Rock,           //바위.
        Paper,          //보.
        Scissor,        //가위.
        AttackRock,     //공격 바위.
        AttackPaper,    //공격 보.
        AttackScissor,  //공격 가위.
        MissRock,       //실패 바위.
        MissPaper,      //실패 보.
        MissScissor,    //실패 가위.
        Defence,        //방어.
        Damage,         //날아가기.
    };
    Motion m_currentMotion;
    Animation m_anim;

    RPSKind m_rps;
    RPSKind m_opponentRps;  //대전 상태의 가위바위보. 애니메이션시키는 사정으로 여기서 가집니다.
    int m_damage;           //날아가는 액션용 대미지 값.
    bool m_actionEffectEnable; //액션의 효과가 유효하면 true. (대미지 시에는 서로 OFF로 되어 있을 것).
    bool m_actionSoundEnable;  //액션의 효과음이 유효하면 true. (대미지 시는 서로 OFF로 되어 있을 것).

    void Awake() {
        GetComponent<AudioSource>().clip = m_landingSE;
        GetComponent<AudioSource>().PlayDelayed(0.2f); //착지음을 늦게 재생시킨다.

        m_currentMotion = Motion.In;
        m_anim = GetComponentInChildren<Animation>();

        m_rps = RPSKind.None;
        m_opponentRps = RPSKind.None;
        m_damage = 0;
        m_actionEffectEnable = false;
        m_actionSoundEnable = false;
    }
	
	// Update is called once per frame
	void Update () {
        switch (m_currentMotion) {
        case Motion.In:             //입장.
            if (m_anim.isPlaying == false) {
                ChangeAnimation(Motion.Idle);
                //대기 모션으로 전환할 때 플레이어 표기를 낸다.
                GameObject board = GameObject.Find("BoardYou");
                board.GetComponent<BoardYou>().Run();
            }
            break;
        case Motion.Idle:           //대기 모션.
        case Motion.RPSInputWait:   //가위바위보 선택 대기.
        case Motion.Rock:           //바위.
        case Motion.Paper:          //보.
        case Motion.Scissor:        //가위.
            break;

        case Motion.AttackRock:     //공격 바위.
        case Motion.AttackPaper:    //공격 보.
        case Motion.AttackScissor:  //공격 가위.
            //SE.
            if (m_actionSoundEnable) {
                if (GetRemainAnimationTime() < 1.7f) {
                    m_actionSoundEnable = false;
                    GetComponent<AudioSource>().clip = m_missSE;
                    GetComponent<AudioSource>().Play();
                }
            }
            break;

        case Motion.MissRock:       //실패 바위.
        case Motion.MissPaper:      //실패 보.
        case Motion.MissScissor:    //실패 가위.
            //SE.
            if (m_actionSoundEnable) {
                if (GetRemainAnimationTime() < 1.1f) {
                    m_actionSoundEnable = false;
                    GetComponent<AudioSource>().clip = m_slipSE;
                    GetComponent<AudioSource>().Play();
                }
            }
            //Effect.
            if (m_actionEffectEnable) {
                if (GetRemainAnimationTime() < 0.5f) {
                    m_actionEffectEnable = false;
                    transform.FindChild("kurukuru").gameObject.SetActive(true);
                }
            }
            break;

        case Motion.Defence:        //방어.
            //Effect.
            if (m_actionEffectEnable) {
                if (GetRemainAnimationTime() < 1.7f) {
                    m_actionEffectEnable = false;
                    transform.FindChild("SweatEffect").gameObject.SetActive(true);
                }
            }
            if (IsCurrentAnimationEnd()) {
                transform.FindChild("SweatEffect").gameObject.SetActive(false);
            }
            break;

        case Motion.Damage:         //날리기.
            break;
        }
	}


    public void ChangeAnimation(Motion motion) {
        m_currentMotion = motion;
        m_anim.Play(m_currentMotion.ToString());
    }
    public void ChangeAnimationJanken() {
        switch (m_rps) {
        case RPSKind.Rock:
            ChangeAnimation(Motion.Rock);
            break;
        case RPSKind.Paper:
            ChangeAnimation(Motion.Paper);
            break;
        case RPSKind.Scissor:
            ChangeAnimation(Motion.Scissor);
            break;
        }
        Invoke("StarEffectOn", 0.5f); //효과 재생.
    }

    //별 효과를 유효하게 한다.
    void StarEffectOn() {
        GameObject star = transform.FindChild("StarEffect").gameObject;
        star.GetComponent<ParticleSystem>().Play();
    }


    public void ChangeAnimationAction(ActionKind action) {
        //서버 클라이언트에서의 판정만 할 수 있으므로 Winner.serverPlayer면 자신의 승리로 다룹니다.
        Winner rpsWinner = ResultChecker.GetRPSWinner(m_rps, m_opponentRps);
        switch (rpsWinner) {
        case Winner.ServerPlayer:   //가위바위보는 자신의 승리.
            if (action == ActionKind.Attack) {
                ChangeAnimationAttack();
            }
            else if (action == ActionKind.Block) {
                ChangeAnimation(Motion.Defence);
            }
            break;
        case Winner.ClientPlayer:   //가위바위보는 자신의 패배.
            if (action == ActionKind.Attack) {
                ChangeAnimationMiss();
            }
            else if (action == ActionKind.Block) {
                ChangeAnimation(Motion.Defence);
            }
            break;
        case Winner.Draw:           //가위바위보는 무승부.
            if (action == ActionKind.Attack) {
                ChangeAnimationMiss();
            }
            else if (action == ActionKind.Block) {
                ChangeAnimation(Motion.Defence);
            }
            break;
        }
        //Debug.Log(m_currentMotion.ToString() + m_anim[m_currentMotion.ToString()].length);
        //Debug.Log(m_anim[m_currentMotion.ToString()].speed);
        //Debug.Log(m_anim[m_currentMotion.ToString()].normalizedTime);

        m_anim[m_currentMotion.ToString()].speed = 0.1f; //느리게 재생.
    }

    //일반 재생 속도로.
    public void SetDefaultAnimationSpeed() {
        m_anim[m_currentMotion.ToString()].speed = 1.0f;
    }

    //애니메이션의 남은 시간.
    public float GetRemainAnimationTime() {
        AnimationState anim = m_anim[m_currentMotion.ToString()];
        float time = anim.time;
        while (time > anim.length) {
            time -= anim.length;
        }
        //Debug.Log(anim.length - time);
        return anim.length - time;
    }

    
    void ChangeAnimationAttack() {
        switch (m_rps) {
        case RPSKind.Rock:
            ChangeAnimation(Motion.AttackRock);
            break;
        case RPSKind.Paper:
            ChangeAnimation(Motion.AttackPaper);
            break;
        case RPSKind.Scissor:
            ChangeAnimation(Motion.AttackScissor);
            break;
        }
    }
    void ChangeAnimationMiss() {
        switch (m_rps) {
        case RPSKind.Rock:
            ChangeAnimation(Motion.MissRock);
            break;
        case RPSKind.Paper:
            ChangeAnimation(Motion.MissPaper);
            break;
        case RPSKind.Scissor:
            ChangeAnimation(Motion.MissScissor);
            break;
        }
    }
    
    //자신과 대전 상대의 가위바위보를 세팅한다.
    public void SetRPS(RPSKind rps, RPSKind opponentRps) {
        m_rps = rps;
        m_opponentRps = opponentRps;
    }


    //startTime초 후에 날아가는 처리 시작.
    public void StartDamage(int damage /*[0:2]*/, float startTime) {
        m_damage = damage;
        Invoke("SetDamage", startTime);
    }

    void SetDamage() {
        SetDefaultAnimationSpeed(); // 애니메이션 스피드를 원래대로.
        if (m_damage == 0) {
            //gameObject.AddComponent<LightDamage>();
            gameObject.AddComponent<Damage>();
        }
        else if (m_damage == 1) {
            gameObject.AddComponent<Damage>();
        }
        else {
            gameObject.AddComponent<HeavyDamage>();
        }
    }
    

    // 애니메이션이 끝나면 true.
    public bool IsCurrentAnimationEnd() {
        return (m_anim.isPlaying == false);
        //AnimationState current = m_anim[m_currentMotion.ToString()];
        //if (current.time >= current.length) {
        //    return true;
        //}
        //return false;
    }

    // 대기 애니메이션이면 true.
    public bool IsIdleAnimation() {
        return (m_currentMotion == Motion.Idle);
    }

    //액션 중인 효과를 유효하게.
    public void ActionEffectOn() {
        m_actionEffectEnable = true;
        m_actionSoundEnable = true;
    }
}
