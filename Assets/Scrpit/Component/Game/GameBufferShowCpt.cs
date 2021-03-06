﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

public class GameBufferShowCpt : BaseMonoBehaviour, IBufferInfoView
{
    public GameBufferListCpt bufferListCpt;

    public RectTransform listShowContent;
    public GameObject itemShowModel;
    public GameObject itemShowDetailsModel;

    public GameDataCpt gameDataCpt;
    public List<BufferInfoBean> listBufferInfoData;
    private BufferInfoController mBufferInfoController;

    public float waitTime = 180f;
    public float animDuration = 20f;
    public int addNumber = 1;
    public bool isShow = true;

    private RebirthTalentItemBean mTalentWaitTimeData;
    private RebirthTalentItemBean mTalentAddNumberData;
    private RebirthTalentItemBean mTalentAnimDurationData;

    private void Awake()
    {
        mBufferInfoController = new BufferInfoController(this, this);
    }

    private void Start()
    {
        mBufferInfoController.GetAllBufferInfo();
        //获取天赋-间隔时间
        mTalentWaitTimeData = gameDataCpt.GetRebirthTalentById(401);
        if (mTalentWaitTimeData != null && mTalentWaitTimeData.total_add > 0)
            waitTime = waitTime / (float)mTalentWaitTimeData.total_add;
        //获取天赋-礼物出现个数
        mTalentAddNumberData = gameDataCpt.GetRebirthTalentById(402);
        if (mTalentAddNumberData != null)
            addNumber += (int)mTalentAddNumberData.total_add;
        //获取天赋-礼物展示时间
        mTalentAnimDurationData = gameDataCpt.GetRebirthTalentById(403);
        if (mTalentAnimDurationData != null)
            animDuration += (int)mTalentAnimDurationData.total_add;

        StartCoroutine(StartShow());
    }

    private IEnumerator StartShow()
    {
        while (isShow)
        {
            yield return new WaitForSeconds(waitTime);
            if (CheckUtil.ListIsNull(listBufferInfoData))
                continue;
            for (int i = 0; i < addNumber; i++)
            {
                BufferInfoBean itemBufferData = GetOneBuffer();
                if (itemBufferData != null)
                {
                    CreateShowItem(itemBufferData);
                }
            }
        }
    }


    /// <summary>
    /// 创建一个展示
    /// </summary>
    /// <param name="itemBufferData"></param>
    public void CreateShowItem(BufferInfoBean itemBufferData)
    {
        if (itemShowModel == null || listShowContent == null)
            return;
        GameObject showObj = Instantiate(itemShowModel, itemShowModel.transform);
        showObj.transform.SetParent(listShowContent.transform);
        showObj.SetActive(true);
        RectTransform showItemRT = showObj.GetComponent<RectTransform>();
        CanvasGroup showItemCG = showObj.GetComponent<CanvasGroup>();
        Vector2 showPosition = new Vector2(Random.Range(showItemRT.rect.width / 2f, listShowContent.rect.width - showItemRT.rect.width / 2f), Random.Range(-(listShowContent.rect.height - showItemRT.rect.height / 2f), -showItemRT.rect.height / 2f));
        showItemRT.anchoredPosition = showPosition;
        Button btShow = showObj.GetComponent<Button>();
        if (btShow != null)
        {
            btShow.onClick.AddListener(delegate ()
            {
                CreateDetailsItem(itemBufferData, showPosition);
                showObj.transform.DOKill();
                if (showObj)
                    Destroy(showObj);
            });
        }

        //设置本身动画
        showObj.transform.localScale = new Vector3(0, 0, 0);
        showObj.transform.DOScale(new Vector3(1, 1, 1), 1).OnComplete(delegate ()
        {
            showObj.transform.DOScale(new Vector3(0, 0), animDuration / 2f).SetDelay(animDuration / 2f);
        });
        showObj.transform.DOPunchRotation(new Vector3(0, 0, 10), 1, 5, 1f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
        showItemCG.DOFade(0, animDuration / 2f).SetDelay(animDuration / 2f).OnComplete(delegate ()
        {
            showObj.transform.DOKill();
            if (showObj)
                Destroy(showObj);
        });
        Image ivIcon = CptUtil.GetCptInChildrenByName<Image>(showObj, "Icon");
        ivIcon.sprite = gameDataCpt.GetIconByKey(itemBufferData.icon_key);
    }

    /// <summary>
    /// 创建展示的详情
    /// </summary>
    /// <param name="itemBufferData"></param>
    public void CreateDetailsItem(BufferInfoBean itemBufferData, Vector2 showPostion)
    {
        GameObject itemDetailsObj = Instantiate(itemShowDetailsModel, itemShowDetailsModel.transform);
        itemDetailsObj.SetActive(true);
        itemDetailsObj.transform.SetParent(listShowContent.transform);
        RectTransform detailsItemRT = itemDetailsObj.GetComponent<RectTransform>();
        CanvasGroup detailsItemCG = itemDetailsObj.GetComponent<CanvasGroup>();
        Text detailsContent = itemDetailsObj.GetComponentInChildren<Text>();
        detailsItemRT.anchoredPosition = showPostion;
        //设置内容
        string contentStr = "";
        if (itemBufferData.add_grow != 0)
        {
            if (!contentStr.Equals(""))
                contentStr += "\n";
            if (itemBufferData.level == -1)
            {
                contentStr += ("+ " + itemBufferData.add_grow * 100 + "%" + GameCommonInfo.GetTextById(50) + GameCommonInfo.GetTextById(54));
            }
            else
            {
                LevelScenesBean levelScenesBean = gameDataCpt.GetScenesByLevel(itemBufferData.level);
                if (levelScenesBean != null)
                    contentStr += ("+ " + itemBufferData.add_grow * 100 + "%" + levelScenesBean.goods_name + GameCommonInfo.GetTextById(54));
            }
            contentStr += "\n" + GameCommonInfo.GetTextById(55) + itemBufferData.time + GameCommonInfo.GetTextById(56);
        }
        if (itemBufferData.add_number != 0)
        {
            if (!contentStr.Equals(""))
                contentStr += "\n";
            double addNumber = itemBufferData.add_number;
            contentStr += ("+ " + GameCommonInfo.GetPriceStr(addNumber) + GameCommonInfo.GetTextById(40));
        }
        if (itemBufferData.add_percentage != 0)
        {
            if (!contentStr.Equals(""))
                contentStr += "\n";
            double addNumber = gameDataCpt.userData.userScore * itemBufferData.add_percentage;
            contentStr += ("+ " + GameCommonInfo.GetPriceStr(addNumber) + GameCommonInfo.GetTextById(40));
            gameDataCpt.userData.userScore += addNumber;
        }
        detailsContent.text = contentStr;
        //设置动画
        Vector3 oldScale = itemDetailsObj.transform.localScale;
        Vector3 positionScale = itemDetailsObj.transform.localPosition;
        itemDetailsObj.transform.localScale = new Vector3(0, 0, 0);
        itemDetailsObj.transform.DOScale(oldScale, 1);

        detailsItemCG.DOFade(0, 10);
        itemDetailsObj.transform.DOLocalMoveY(positionScale.y + 100, 10).OnComplete(delegate ()
        {
            Destroy(itemDetailsObj);
        });

        bufferListCpt.AddBuffer(itemBufferData);
    }

    private BufferInfoBean GetOneBuffer()
    {
        if (listBufferInfoData == null)
            return null;
        int level = Random.Range(1, gameDataCpt.userData.goodsLevel + 1);
        List<BufferInfoBean> tempList = new List<BufferInfoBean>();
        for (int i = 0; i < listBufferInfoData.Count; i++)
        {
            BufferInfoBean itemInfo = listBufferInfoData[i];
            if (itemInfo.level == level)
            {
                tempList.Add(itemInfo);
            }
        }
        if (CheckUtil.ListIsNull(tempList))
            return null;
        return RandomUtil.GetRandomDataByList(tempList);
    }

    #region 数据回调
    public void GetBufferInfoFail()
    {

    }

    public void GetBufferInfoSuccess(List<BufferInfoBean> listData)
    {
        listBufferInfoData = listData;
    }
    #endregion
}