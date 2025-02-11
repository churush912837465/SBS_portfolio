using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class PlayerManager : MonoBehaviour
{
    /// <summary>
    ///  1. player 상위 스크립트
    ///  
    /// </summary>

    [Header("공통 필드")]
    [Header("Init")]
    [SerializeField]
    protected PlayerData _playerData;
    [SerializeField]
    protected Skill[] _playerSkill;
    [SerializeField]
    protected Skill _currSkill;
    [SerializeField]
    protected GameObject _playerModel;

    [Space]
    [SerializeField]
    protected Animator _playerAnimator;
    [SerializeField]
    protected bool _canMove     = true;
    private string _moveAni     = "run";
    private string _dieAni      = "die";
    private string _hitAni      = "isHit";

    [Space]
    [SerializeField]
    Vector3 _moveVec;
    [SerializeField]
    float _playerRot = 90f;
    [SerializeField]
    float _moveDir = -1;

    // 프로퍼티
    public bool CanMove { get => _canMove; set { _canMove = value; } }
    public string MoveAni   { get => _moveAni; }
    public Skill CurrSkill  { get => _currSkill;  }
    public PlayerData playerData { get => _playerData; }    
    public float PlayerRot { get => _playerRot; set { _playerRot = value; } }
    public float MoveDir { get => _moveDir; set { _moveDir = value; } }

    // 함수
    public virtual void InitPlayerData()  
    {
        // 본인 player data를 init (공통)
        _playerData = new PlayerData();
        _playerData.HP              = 100f;
        _playerData.MoveSpeed       = 10f;
        _playerData.ATK             = 10f;
        _playerData.EXP             = 0;
        _playerData.AdditionalHp    = 0;  
        _playerData.PhyDefencity    = 0;
        _playerData.MasicDefencity  = 0;
        _playerData.Counter         = 0;

        // GetComponent
        _playerAnimator = GetComponent<Animator>();
    }

    public virtual void PlayerMove()        // 플레이어 움직임
    {
        float _hAxis = Input.GetAxis("Horizontal");
        float _vAxis = Input.GetAxis("Vertical");

        // 회전을 90으로 고정
        this.transform.rotation = Quaternion.Euler(0, _playerRot, 0);
        // 움직임
        _moveVec = new Vector3( _vAxis, 0, _moveDir * _hAxis).normalized;
        //(+)던전 입장 시 rotation과 _moveRot을 변경해야함

        transform.position += _moveVec * _playerData.MoveSpeed * Time.deltaTime;
        _playerModel.transform.LookAt(transform.position + _moveVec);            // 플레이어를 회전

        _playerAnimator.SetBool(_moveAni, _moveVec != Vector3.zero);

    }

    public virtual void PlayerGetDamage(float v_damage) // player 피격
    {
        _playerData.HP -= v_damage;
        _playerAnimator.SetTrigger(_hitAni);

        bool _flag = true;
        if (PlayerHPIsUnderZero() && _flag) 
        {
            PlayerIsDie();
            _flag = false;
        }
    }

    public virtual void PlayerIsDie()   // player가 죽었을 때 행동
    {
        Debug.Log("Player 죽음");
        _canMove = false;
        UIManager.instance.OnOffPlayerDiePanel(true);   // die panel open
        _playerAnimator.SetTrigger(_dieAni);
    }

    public virtual bool PlayerHPIsUnderZero()   // hp 검사
    {
        if (_playerData.HP <= 0)
            return true;
        else
            return false;
    }

    public virtual void PlayerPlaySkill(int v_idx , Transform posi)   // 해당 idx에 대한 스킬 실행 
    {
        _canMove = false;

        _currSkill  = _playerSkill[v_idx];              // 현재 스킬 저장
        _playerSkill[v_idx].SkillUse(_playerAnimator , posi);

        Invoke("AgainCanMove", 3f);     // 스킬 사용 후 Nf 후에 움직일 수 있도록
    }

    public virtual void AgainCanMove() 
    {
        _canMove = true;
    }

    public virtual float PlayerReturnSKillDamage()      // 현재 스킬에 대한 damage를 return
    {
        /*
        if (_currSkill == null)
            return 0;
        */

        float _ranDamage = Random.Range(_currSkill.MinDamage , _currSkill.MaxDamage);
        return _ranDamage;
    }

    // public abstract void InitSkill();           // 본인 player 스킬을 init 
    public abstract void PlayerUseSkill();      // 본인 skill을 사용

    #region Use Item 아이템 사용시

    // Portion 사용 -> Hp 증가
    internal void UserPortion(float healingAmont)
    {
        float _nowHp = _playerData.HP;
        _nowHp += healingAmont;
        _playerData.HP = _nowHp;

        Debug.Log(_playerData.HP);
    }

    // playerInfoUi 에서 equip 착용 시 추가체력
    public virtual void AddPlayerHP(float v_addHP)
    {
        _playerData.HP += v_addHP;                  // 내 hp += 추가 hp
        _playerData.AdditionalHp += v_addHP;

        //Debug.Log("현재 체력은 " + _playerData.HP);
    }
    // playerInfoUi 에서 equip 착용 시 추가 물리방어력
    public virtual void AddPhyDefen(float v_addPD)
    {
        _playerData.PhyDefencity += v_addPD;
        //Debug.Log("현재 물리 공격력은 " + _playerData.PhyDefencity);
    }
    // playerInfoUi 에서 equip 착용 시 추가 마법 방어력
    public virtual void AddMasicDefen(float v_addMD)
    {
        _playerData.MasicDefencity += v_addMD;
        //Debug.Log("현재 마법 공격력은 " + _playerData.MasicDefencity);
    }

    public virtual void AddCounter(float v_cnter) 
    {
        _playerData.Counter += v_cnter;
        //Debug.Log("현재 치명타율은 " + _playerData.Counter);
    }
    #endregion

}
