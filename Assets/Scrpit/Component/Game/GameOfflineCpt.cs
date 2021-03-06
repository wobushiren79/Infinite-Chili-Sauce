﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GameOfflineCpt : BaseMonoBehaviour
{
    public GameDataCpt gameDataCpt;
    public GameToastCpt gameToastCpt;

    private RebirthTalentItemBean offlineAddGrowData;
    private RebirthTalentItemBean offlineAddTimeData;
    private RebirthTalentItemBean offlineAddMagnificationData;
    private RebirthTalentItemBean offlineAddDoubleData;
    private RebirthTalentItemBean offlineAddMoreDoubleData;
    public void Start()
    {
        if (gameDataCpt == null
            || gameToastCpt == null
            || gameDataCpt.userData.offlineTime == null
            || gameDataCpt.userData.rebirthData == null
            || gameDataCpt.userData.rebirthData.listRebirthTalentData == null)
        {
            return;
        }
        //获取离线时间
        double offlineTotalHours= TimeUtil.SubtractTimeForTotalHours(TimeUtil.GetNowTime(),gameDataCpt.userData.offlineTime);
        offlineTotalHours -= 1;
        if (offlineTotalHours < 0)
        {
            return;
        }
        //获取天赋数据
        List<RebirthTalentItemBean> listTalentData = gameDataCpt.userData.rebirthData.listRebirthTalentData;
        for (int i = 0; i < listTalentData.Count; i++)
        {
            RebirthTalentItemBean itemData = listTalentData[i];
            if (itemData.add_type == 301)
            {
                offlineAddGrowData = itemData;
            }
            else if (itemData.add_type == 302){
                offlineAddTimeData = itemData;
            }
            else if (itemData.add_type == 303)
            {
                offlineAddMagnificationData = itemData;
            }
            else if (itemData.add_type == 304)
            {
                offlineAddDoubleData = itemData;
            }
            else if (itemData.add_type == 305)
            {
                offlineAddMoreDoubleData = itemData;
            }
        };

        if (offlineAddGrowData == null||offlineAddGrowData.talent_level==0)
        {
            return;
        }
        if (offlineAddTimeData != null)
        {
            //添加增加的时间
            offlineTotalHours += offlineAddTimeData.total_add;
        }
        if (offlineAddMagnificationData != null)
        {
            offlineTotalHours = offlineTotalHours * (1 + offlineAddMagnificationData.total_add);
        }

        //添加分数
        double addNumber = gameDataCpt.userData.GetUserGrowByHours() * offlineTotalHours * offlineAddGrowData.total_add;
        //天赋加成
        if (offlineAddDoubleData != null)
        {
            float tempLucky= Random.Range(0f, 1f);
            if(tempLucky <= offlineAddDoubleData.total_add)
            {
                double addTimes = 2;
                if (offlineAddMoreDoubleData!=null)
                {
                    addTimes += offlineAddMoreDoubleData.total_add;
                }
                addNumber = addNumber * addTimes;
                gameToastCpt.ToastHint(GameCommonInfo.GetTextById(115) + addTimes, 10);
            }
        }
        gameDataCpt.userData.userScore += addNumber;
        gameToastCpt.ToastHint(GameCommonInfo.GetTextById(91)+GameCommonInfo.GetPriceStr(addNumber),10);
    }

}