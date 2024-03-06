using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("�̱���")]
    public static GameManager instance;

    [Header("������Ʈ")]
    public GameObject player;
    public GameObject dungeon;

    [Header("class")]
    public PlayerManager playerManager;
    public DungeonManager dungeonManager;
    public Inventory inventory;
    public ItemManager itemManager;

    [Header("����")]
    public float generationTime;

    private void Awake()
    {
        if (instance == null)
        {
            // instance �� ������ �־��ֱ�
            instance = this;
        }
        else
        {
            Destroy(instance);
        }
    }

    void Start()
    {
        playerManager = player.GetComponent<PlayerManager>();
        dungeonManager = dungeon.GetComponent<DungeonManager>();

        generationTime = 20f;

    }

    #region �ӽ� ������ ȹ�� ���
    
    public void DungeonEnter() 
    {
        dungeonManager.startDungeon();

    }

    public void PlayerGetPortion() 
    {
        itemManager.PlayerGetPortion(PortionType.potion);
    }

    public void PlayerGetBomb() 
    {
        itemManager.PlayerGetBomb(BombType.destroyBomb);
    }

    public void PlayerGetClothes() 
    {
        itemManager.PlayerGetEquip(0);
    }

    internal void PlayerGetAccessory()
    {
        itemManager.PlayerGetAccessory(0);
    }
    #endregion
}
