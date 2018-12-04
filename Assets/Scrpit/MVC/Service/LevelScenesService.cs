﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScenesService 
{
    private readonly string mTableName;

    public LevelScenesService()
    {
        mTableName = "level_scenes";
    }

    /// <summary>
    /// 查询所有场景数据
    /// </summary>
    /// <returns></returns>
    public  List<LevelScenesBean> QueryAllData()
    {
      return SQliteHandle.LoadTableData<LevelScenesBean>(ProjectConfigInfo.DATA_BASE_INFO_NAME, mTableName);
    }

    /// <summary>
    /// 根据场景等级查询场景数据
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public List<LevelScenesBean> QueryDataByLevel(int level)
    {
        string[] colName = new string[] { "level" };
        string[] operations = new string[] { "="};
        string[] colValue = new string[] { level+"" };
        return SQliteHandle.LoadTableData<LevelScenesBean>(ProjectConfigInfo.DATA_BASE_INFO_NAME, mTableName, colName, operations, colValue);
    }
}