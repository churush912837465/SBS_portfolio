using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum Enemy_State
{
    Idle,
    Tracking,
    Attack,
    Die
}

public class EnemyParent : MonoBehaviour
{
    // FSM
    [SerializeField]
    public FSM[] enemyFSM = new FSM[System.Enum.GetValues(typeof(Enemy_State)).Length];
    [SerializeField]
    public HeadMachine enemyMachine;        // HeadMachin 
    public Enemy_State currState;           // 현재 상태
    public Enemy_State preSate;             // 과거 상태

    // 컴포넌트
    [SerializeField]
    protected EnemyDB _myEnemyDB;           // EnemyPooling에서 생성할 때 DB를 할당해준다.
    [SerializeField]
     Animator _animator;
    [SerializeField]
    protected TextMeshProUGUI _damageText;  // 데미지가 적힐 text

    //변수
    [SerializeField]
    bool _endAttack;
    [SerializeField]
    protected float _myEnemyHp;
    // 각 enemy 는 공통된 DB를 가지고 있는데
    // 계속 변해야 하는 hp는 따로 변수를 가지고 있는게 편할듯

    // 프로퍼티
    public EnemyDB myEnemyDB { get => _myEnemyDB; }
    public Animator animator { get => _animator; }
    public bool EndAttack { get => _endAttack; }
    public float myEnemyHP { get => _myEnemyHp; }

    //FSM init
    public void FSM_Init()
    {
        enemyMachine = new HeadMachine();

        enemyFSM[(int)Enemy_State.Idle]     = new Enemy_Idle(this);         // Enemy_Idle 생성자
        enemyFSM[(int)Enemy_State.Tracking] = new Enemy_Tracking(this);     // Enemy_Walk 생성자
        enemyFSM[(int)Enemy_State.Attack]   = new Enemy_Attack(this);       // Enemey_Attack 생성자
        enemyFSM[(int)Enemy_State.Die]      = new Enemy_Die(this);          // Enemy_Die 생성자   

        // 일단 기본상태를 Idle로?
        enemyMachine.SetState(enemyFSM[(int)Enemy_State.Idle]);

        // 컴포넌트 가져오기
        _animator = gameObject.GetComponent<Animator>();

    }

    public void changeEnemyState(Enemy_State state)
    {
        for (int i = 0; i < System.Enum.GetValues(typeof(Enemy_State)).Length; i++)
        {
            if ((int)state == i)                        // for문 돌면서 같은 상태를 찾으면
                enemyMachine.ChangeState(enemyFSM[i]);  // 그 상태로 바꿈
        }
    }

    // enemy가 죽었을 때 실행중인 코루틴 종료
    public void stopCorutineEnemy() 
    {
        StopAllCoroutines();
    }

    //FSM 실헹
    protected IEnumerator FSM_Run()
    {
        while (true)
        {
            enemyMachine.H_Run();               // update문 대신 현재 상태의 run을 매프레임 실행
            yield return new WaitForSeconds(0.01f);
        }
    }

    // hp 체크
    public bool checkHp()
    {
        if (_myEnemyHp <= 0)
        {
            return true;
        }

        return false;
    }

    // 시야 내 Player를 탐색하는
    // tracking -> attack
    public bool searchRangePlayer()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;
        Vector3 center = new Vector3(x, y, z);     // 범위의 중심 (즉, enemy)

        Collider[] colliders = Physics.OverlapSphere(center, myEnemyDB.Sight); //시작 위치 , 범위

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    // Attack 상태일 때, 공격 시작
    public void startAttackPlayer()
    {
        _endAttack = false;
        _animator.SetTrigger(myEnemyDB.AttackAni);
    }

    // Attak이 끝날 때, 애니메이션 이벤트로 실행
    public void endAttackplayer()
    {
        _endAttack = true;
    }

    // Enemy가 피격 당했을 때
    public void HiEnemy()
    {
        _animator.SetTrigger(myEnemyDB.GetDamageAni);
        _myEnemyHp -= GameManager.instance.playerManager.PlayerReturnSKillDamage();         // Player의 현재 스킬의 데미지 return

        StartCoroutine(damageText());               // 데미지 표시 코루틴
    }

    // Enemy 옆에 Text 뜨게 하는
    protected IEnumerator damageText()
    {
        if (_damageText.gameObject == null)
            yield return null;

        _damageText.gameObject.SetActive(true);     // 텍스트 켜기

        _damageText.text = GameManager.instance.playerManager.PlayerReturnSKillDamage().ToString();        // 데미지 표시

        yield return new WaitForSeconds(0.5f);
        _damageText.gameObject.SetActive(false);
    }

    // enemy 죽었을 때 아이템 획득 랜덤
    public void EnemyDieAndPlayerGetItem() 
    {
        // 1. 장비 & 장신구 랜덤 획득
        // 2. 장비 4종, 장신구 4종 중 랜덤 획득
        int _randA = Random.Range( 0, 2 );
        int _randB = Random.Range( -10 , GameManager.instance.itemManager.ReturnEquipListCount() );

        switch (_randA) 
        {
            case 0:
                GameManager.instance.itemManager.PlayerGetEquip(_randB);
                break;
            case 1:
                GameManager.instance.itemManager.PlayerGetAccessory(_randB);
                break;
        }
    }

    // enemy 죽었을 때 재화 획득
    public void EnemyDieAndPlayerGetGoods() 
    {
        // 1. 실링 , 명예의파편 2종 랜덤 획득
        // 2. 랜덤 수량 1개 ~ 50개
        int _randA = Random.Range( 0, 2 );
        int _randB = Random.Range( 1 , 51 );

        switch (_randA) 
        {
            case 0:
                GameManager.instance.goodsManager.PlayerGetGoods( GoodsType.Silling , _randB);
                break;
            case 1:
                GameManager.instance.goodsManager.PlayerGetGoods( GoodsType.Honor, _randB);
                break;
        }
    }
}
