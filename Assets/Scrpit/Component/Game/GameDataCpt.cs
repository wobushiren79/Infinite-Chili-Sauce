﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System;

public class GameDataCpt : BaseObservable<IGameDataCallBack>, IUserDataView, IGameScenesView, IGameSkillsView,ITalentInfoView,IOilInfoView
{

    //用户数据
    public UserDataBean userData;
    //场景数据
    public List<LevelScenesBean> listScenesData;
    //所有技能数据
    public List<LevelSkillsBean> listSkillsData;
    //所有天赋数据
    public List<TalentInfoBean> listTalentData;
    //所有辣椒油技能
    public List<OilInfoBean> listOilData;
    //图标列表
    public List<IconKV> listIconData;
    public List<IconKV> listSauceData;
    public List<IconKV> listChiliData;

    //用户数据管理
    private UserDataController mUserDataController;
    private GameScenesController mGameScenesController;
    private GameSkillsController mGameSkillsController;
    private TalentInfoController mTalentInfoController;
    private OilInfoController mOilInfoController;

    public bool isUpdate = true;

    private void Awake()
    {
        mUserDataController = new UserDataController(this, this);
        mGameScenesController = new GameScenesController(this, this);
        mGameSkillsController = new GameSkillsController(this, this);
        mTalentInfoController = new TalentInfoController(this,this);
        mOilInfoController = new OilInfoController(this,this);

        mUserDataController.GetUserData(GameCommonInfo.gameUserId);
        mGameScenesController.GetAllGameScenesData();
        mGameSkillsController.GetAllLevelSkill();
        mTalentInfoController.GetAllTalentInfo();
        mOilInfoController.GetAllOilInfo();
    }

    private void FixedUpdate()
    {
        if (!isUpdate)
            return;
        float updateNumber = 1 / Time.fixedDeltaTime;
        if (updateNumber <= 0)
            return;
        double tempGrow = userData.GetUserGrowBySecond() / updateNumber;
        userData.userScore += tempGrow;
        CheckLevel();
        CheckAchievement();
    }

    /// <summary>
    /// 检测成就
    /// </summary>
    public void CheckAchievement()
    {
        if (userData.userAchievement == null)
            userData.userAchievement = new AchievementBean();
        if(userData.userScore> userData.userAchievement.maxUserScore)
        {
            //最高得分修改
            userData.userAchievement.maxUserScore = userData.userScore;
        }
        if (userData.goodsLevel > userData.userAchievement.maxUserGoodsLevel)
        {
            //最高物品等级修改
            userData.userAchievement.maxUserGoodsLevel = userData.goodsLevel;
        }
        if (userData.scoreLevel > userData.userAchievement.maxUserScoreLevel)
        {
            //最高物品等级修改
            userData.userAchievement.maxUserScoreLevel = userData.scoreLevel;
        }
    }

    /// <summary>
    /// 检测等级
    /// </summary>
    public void CheckLevel()
    {
        if (userData == null)
            return;
        if (listScenesData != null)
            for (int i = 0; i < listScenesData.Count; i++)
            {
                LevelScenesBean itemData = listScenesData[i];
                if (userData.userScore >= itemData.goods_sell_price && userData.scoreLevel < itemData.level)
                {
                    userData.scoreLevel = itemData.level;

                    //通知所有观察者
                    List<IGameDataCallBack> listObserver = GetAllObserver();
                    for (int f = 0; f < listObserver.Count; f++)
                    {
                        IGameDataCallBack itemObserver = listObserver[f];
                        itemObserver.ScoreLevelChange(userData.scoreLevel);
                    }
                }
            }
        if (userData.listUserLevelData != null)
        {
            for(int i=0;i< userData.listUserLevelData.Count; i++)
            {
                UserItemLevelBean itemData = userData.listUserLevelData[i];
                if (itemData.level > userData.goodsLevel&&itemData.goodsNumber!=0)
                {
                    userData.goodsLevel = itemData.level;
                    //通知所有观察者
                    List<IGameDataCallBack> listObserver = GetAllObserver();
                    for (int f = 0; f < listObserver.Count; f++)
                    {
                        IGameDataCallBack itemObserver = listObserver[f];
                        itemObserver.GoodsLevelChange(userData.goodsLevel);
                    }
                }
            }
        }

    }

    /// <summary>
    /// 是否拥有技能  通过Id查询
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool HasSkillsById(long id)
    {
        if (CheckUtil.ListIsNull(userData.listSkillsData))
        {
            return false;
        }
        for (int i = 0; i < userData.listSkillsData.Count; i++)
        {
            long skillsId = userData.listSkillsData[i];
            if (skillsId == id)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 检测当前等级是否还有空间可以添加物品
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public bool HasSpaceToAddGoodsByLevel(int level,int number)
    {
        UserItemLevelBean itemData= GetUserItemLevelDataByLevel(level);
        return HasSpaceToAddGoodsByLevel(itemData, number);
    }

    public bool HasSpaceToAddGoodsByLevel(UserItemLevelBean itemData,int number)
    {
        if (itemData == null || itemData.goodsNumber + number > itemData.spaceNumber * 25)
            return false;
        return true;
    }

    /// <summary>
    /// 保存当前数据
    /// </summary>
    /// <returns></returns>
    public void SaveUserData()
    {
        mUserDataController.SaveUserData(userData);
    }

    /// <summary>
    /// 重生用户数据
    /// </summary>
    public void RebirthUserData()
    {
        mUserDataController.RebirthUserData(userData);
    }

    /// <summary>
    /// 通过ID获取天赋数据
    /// </summary>
    /// <param name="id"></param>
    public RebirthTalentItemBean GetRebirthTalentById(long id)
    {
        List<RebirthTalentItemBean> listData = userData.rebirthData.listRebirthTalentData;
        if (listData == null)
            return null;
        for(int i = 0; i < listData.Count; i++)
        {
            RebirthTalentItemBean itemData = listData[i];
            if (itemData.talent_id == id)
            {
                return itemData;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取用户名称
    /// </summary>
    /// <returns></returns>
    public string GetUserName()
    {
        if (CheckUtil.StringIsNull(userData.userName))
            return "";
        else
            return userData.userName;
    }

    /// <summary>
    /// 根据键值对关系获取图标
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Sprite GetIconByKey(string key)
    {
        List<IconKV> listTempData = new List<IconKV>();
        listTempData.AddRange(listSauceData);
        listTempData.AddRange(listChiliData);
        listTempData.AddRange(listIconData);
        for (int i = 0; i < listTempData.Count; i++)
        {
            IconKV itemData = listTempData[i];
            if (itemData.key.Equals(key))
            {
                return itemData.value;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据等级获取相应技能列表
    /// </summary>
    /// <param name="level">技能等级</param>
    /// <param name="userLevel">技能解锁等级</param>
    /// <returns></returns>
    public List<LevelSkillsBean> GetSkillsListByLevel(int level, int userLevel)
    {
        List<LevelSkillsBean> tempListData = new List<LevelSkillsBean>();
        if (CheckUtil.ListIsNull(listSkillsData))
        {
            return tempListData;
        }
        for (int i = 0; i < listSkillsData.Count; i++)
        {
            LevelSkillsBean itemData = listSkillsData[i];
            if (itemData.level == level && userLevel >= itemData.unlock_level)
            {
                tempListData.Add(itemData);
            }
        }
        return tempListData;
    }

    /// <summary>
    /// 根据ID列表获取相应技能列表
    /// </summary>
    /// <param name="idList"></param>
    /// <returns></returns>
    public List<LevelSkillsBean> GetSkillsListByIds(List<long> idList)
    {
        List<LevelSkillsBean> tempListData = new List<LevelSkillsBean>();
        if (CheckUtil.ListIsNull(listSkillsData)|| CheckUtil.ListIsNull(idList))
        {
            return tempListData;
        }
        for (int i = 0; i < idList.Count; i++)
        {
            long itemId = idList[i];

            for(int f=0;f< listSkillsData.Count; f++)
            {
                LevelSkillsBean itemSkill= listSkillsData[f];
                if(itemSkill.id == itemId)
                {
                    tempListData.Add(itemSkill);
                    break;
                }
            }
        }
        return tempListData;
    }


    /// <summary>
    /// 根据用户等级获取场景数据
    /// </summary>
    /// <param name="userLevel"></param>
    /// <returns></returns>
    public List<LevelScenesBean> GetScenesListByLevel(int userLevel)
    {
        List<LevelScenesBean> tempListData = new List<LevelScenesBean>();
        if (CheckUtil.ListIsNull(listScenesData))
            return tempListData;
        for (int i = 0; i < listScenesData.Count; i++)
        {
            LevelScenesBean itemData = listScenesData[i];
            if (userLevel >= itemData.level)
            {
                tempListData.Add(itemData);
            }
        }
        return tempListData;
    }

    public LevelScenesBean GetScenesByLevel(int level)
    {
        if (CheckUtil.ListIsNull(listScenesData))
            return null;
        for (int i = 0; i < listScenesData.Count; i++)
        {
            LevelScenesBean itemData = listScenesData[i];
            if (level == itemData.level)
            {
                return itemData;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据等级获取等级数据
    /// </summary>
    /// <param name="level"></param>
    public UserItemLevelBean GetUserItemLevelDataByLevel(int level)
    {
        if (level == 0)
            return userData.clickData;
        if (userData == null || CheckUtil.ListIsNull(userData.listUserLevelData))
            return null;
        List<UserItemLevelBean> listLevelData = userData.listUserLevelData;
        for (int i = 0; i < listLevelData.Count; i++)
        {
            UserItemLevelBean itemLevel = listLevelData[i];
            if (itemLevel.level.Equals(level))
            {
                return itemLevel;
            }
        }
        return null;
    }

    /// <summary>
    /// 根据等级获取成就数据
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public AchievementItemLevelBean GetAchItemLevelDataByLevel(int  level)
    {
        if (userData == null ||userData.userAchievement==null||CheckUtil.ListIsNull(userData.userAchievement.listLevelData))
            return null;
        List<AchievementItemLevelBean> listAchData = userData.userAchievement.listLevelData;
        for(int i=0;i< listAchData.Count; i++)
        {
            AchievementItemLevelBean itemAch= listAchData[i];
            if(itemAch.level== level)
            {
                return itemAch;
            }
        }
        return null;
    }

    /// <summary>
    /// 增加分数
    /// </summary>
    /// <param name="score"></param>
    public void AddScore(double score)
    {
        userData.userScore += score;
        //通知所有观察者
        List<IGameDataCallBack> listObserver = GetAllObserver();
        foreach (IGameDataCallBack itemObserver in listObserver)
        {
            itemObserver.ScoreChange(score);
        }
    }

    /// <summary>
    /// 移除分数
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public bool RemoveScore(double score)
    {
        if (userData.userScore < score)
        {
            return false;
        }
        else
        {
            userData.userScore -= score;
            return true;
        }
    }

    /// <summary>
    /// 增加对应等级商品数量
    /// </summary>
    /// <param name="level"></param>
    /// <param name="number"></param>
    public void AddLevelGoods(int level, int number)
    {
        List<UserItemLevelBean> listUserLevelData = userData.listUserLevelData;
        if (CheckUtil.ListIsNull(listUserLevelData))
            listUserLevelData = new List<UserItemLevelBean>();
        //增加数量
        for (int i = 0; i < listUserLevelData.Count; i++)
        {
            UserItemLevelBean itemData = listUserLevelData[i];
            //--------------BUG修复 正式版可以删除--------------
            if (itemData.itemGrow == 0)
            {
                LevelScenesBean levelScenes= GetScenesByLevel(itemData.level);
                itemData.itemGrow = levelScenes.item_grow;
            }
            //--------------BUG修复 正式版可以删除  --------------   
            if (itemData.level.Equals(level))
            {
                itemData.goodsNumber += number;
                //通知所有观察者
                List<IGameDataCallBack> listObserver = GetAllObserver();
                foreach (IGameDataCallBack itemObserver in listObserver)
                {
                    itemObserver.GoodsNumberChange(level, number, itemData.goodsNumber);
                }
            }
        }
        //增加成就
        AchievementItemLevelBean itemAch = GetAchItemLevelDataByLevel(level);
        if (itemAch == null)
        {
            itemAch = new AchievementItemLevelBean();
            itemAch.level = level;
            if (userData.userAchievement == null)
            {
                userData.userAchievement = new AchievementBean();
            }
            if (userData.userAchievement.listLevelData == null)
            {
                userData.userAchievement.listLevelData = new List<AchievementItemLevelBean>();
            }
            userData.userAchievement.listLevelData.Add(itemAch);
        }
        itemAch.totalNumber += number;
        RefreshData();
    }

    /// <summary>
    /// 增加场景数量
    /// </summary>
    /// <param name="level"></param>
    /// <param name="number"></param>
    public void AddLevelSpace(int level, int number)
    {
        List<UserItemLevelBean> listUserLevelData = userData.listUserLevelData;
        if (CheckUtil.ListIsNull(listUserLevelData))
            listUserLevelData = new List<UserItemLevelBean>();
        bool hasData = false;
        UserItemLevelBean tempData=null ;
        for (int i = 0; i < listUserLevelData.Count; i++)
        {
            tempData = listUserLevelData[i];
            if (tempData.level.Equals(level))
            {
                hasData = true;
                tempData.spaceNumber += number;
                break;
            }
        }
        if (!hasData)
        {
            tempData = new UserItemLevelBean();
            tempData.level = level;
            tempData.spaceNumber = number;
            LevelScenesBean levelScenesData = GetScenesByLevel(level);
            if (levelScenesData != null)
                tempData.itemGrow = levelScenesData.item_grow;
            userData.listUserLevelData.Add(tempData);
        }
        //排序
        userData.listUserLevelData.Sort(delegate (UserItemLevelBean x, UserItemLevelBean y) {
            return x.level.CompareTo(y.level);
        });
        //通知所有观察者
        List<IGameDataCallBack> listObserver = GetAllObserver();
        foreach (IGameDataCallBack itemObserver in listObserver)
        {
            itemObserver.SpaceNumberChange(level, number, tempData.spaceNumber);
        }
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    public void RefreshData()
    {
        List<UserItemLevelBean> listUserLevelData = userData.listUserLevelData;
        userData.userGrow = 0;
        if (CheckUtil.ListIsNull(listUserLevelData))
        {
            return;
        }
        for (int i = 0; i < listUserLevelData.Count; i++)
        {
            UserItemLevelBean itemData = listUserLevelData[i];
            if (itemData.level == 0)
            {

            }
            else
            {
                userData.userGrow += (itemData.itemGrow * itemData.goodsNumber * itemData.itemTimes);
            }
        }
    }

    #region  -----------------------------IUserDataView 回调--------------------------------------------
    public void GetUserDataSuccess(UserDataBean userData)
    {
        this.userData = userData;
    }

    public void GetUserDataFail(UserDataModel.UserDataFailEnum failEnum)
    {
    }

    public void CreateUserDataFail(UserDataModel.UserDataFailEnum failEnum)
    {

    }

    public void CreateUserDataSuccess(UserDataBean userData)
    {

    }

    public void DeleteUserDataFail(UserDataModel.UserDataFailEnum failEnum)
    {

    }

    public void DeleteUserDataSuccess(UserDataBean userData)
    {

    }

    public void SaveUserDataFail(UserDataModel.UserDataFailEnum failEnum)
    {

    }

    public void SaveUserDataSuccess(UserDataBean userData)
    {

    }

    public void ChangeUserDataSuccess(UserDataBean userData)
    {
        this.userData = userData;
        RefreshData();
        SaveUserData();
    }

    public void ChangeUserDataFail(UserDataModel.UserDataFailEnum failEnum)
    {

    }

    public void GetScenesDataSuccessByUserData(LevelScenesBean levelScenesData, UserItemLevelBean itemLevelData)
    {

    }

    public void GetAllScenesDataSuccess(List<LevelScenesBean> listScenesData)
    {
        this.listScenesData = listScenesData;
    }

    public void GetAllLevelSkillsDataSuccess(List<LevelSkillsBean> listSkillsData)
    {
        this.listSkillsData = listSkillsData;
    }

    public void GetAllLevelSkillsDataFail()
    {

    }

    public void GetTalentInfoDataSuccess(List<TalentInfoBean> listData)
    {
        this.listTalentData = listData;
    }

    public void GetTalentInfoDataFail()
    {

    }

    public void GetAllOilDataSuccess(List<OilInfoBean> listData)
    {
        this.listOilData = listData;
    }

    public void GetAllOilDataFail()
    {

    }

    #endregion



    [System.Serializable]
    public class IconKV
    {
        public string key;
        public Sprite value;
    }
}