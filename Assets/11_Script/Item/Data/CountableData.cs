using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountableData 
{
    // 필드
    [SerializeField]
    int _amount;
    [SerializeField]
    int _maxAmount;
    [SerializeField]
    int _price;


    // 프로퍼티
    public int Amount { get => _amount; set { _amount = value; } }
    public int MaxAmount { get => _maxAmount; }
    public int Price { get => _price; }

    // 필드 세팅 (초기 amout는 항상 1)
    public CountableData(int amount , int maxAmount, int _price) 
    {
        this._amount = amount;
        this._maxAmount = maxAmount;
        this._price = _price;
    }

    
}
