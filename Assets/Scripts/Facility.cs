using System;
using UnityEngine;

/// <summary>
/// 設備のデータ構造
/// </summary>
[Serializable]
public class Facility
{
    public string facilityId;       // 設備ID
    public string facilityName;     // 設備名
    public string description;      // 説明
    public int level;               // レベル
    public bool isUnlocked;         // 解放済みかどうか
    public FacilityType type;       // 設備タイプ

    public Facility(string id, string name, string desc, int lvl, bool unlocked, FacilityType facilityType)
    {
        facilityId = id;
        facilityName = name;
        description = desc;
        level = lvl;
        isUnlocked = unlocked;
        type = facilityType;
    }
}

/// <summary>
/// 設備の種類
/// </summary>
public enum FacilityType
{
    Production,     // 生産施設
    Storage,        // 保管施設
    Defense,        // 防衛施設
    Research,       // 研究施設
    Other           // その他
}
