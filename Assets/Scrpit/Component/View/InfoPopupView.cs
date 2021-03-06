﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopupView : BaseMonoBehaviour
{
    public Image popupIcon;
    public Text popupTitle;
    public Text popupRemark;
    public CanvasGroup popupPriceCG;
    public Text popupPrice;
    public Text popupDescription;
    public Text popupOther;
    public Image line1;
    public Image line2;
    public Image ivPriceIcon;
    public List<Sprite> spPriceList;

    //屏幕(用来找到鼠标点击的相对位置)
    public RectTransform screenRTF;
    public RectTransform thisRTF;

    public bool isMoveX = true;
    public bool isMoveY = true;
    public float offsetX = 0;
    public float offsetY = 0;

    private void Update()
    {
        if (screenRTF == null)
            return;
        //如果显示Popup 则调整位置为鼠标位置
        if (gameObject.activeSelf)
        {
            Vector2 outPosition;
            //屏幕坐标转换为UI坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(screenRTF, Input.mousePosition, Camera.main, out outPosition);
            float moveX = outPosition.x;
            float moveY = outPosition.y;
            if (!isMoveX)
            {
                moveX = transform.localPosition.x;
            }
            if (!isMoveY)
            {
                moveY = transform.localPosition.y;
            }
            transform.localPosition = new Vector3(moveX+ offsetX, moveY+ offsetY, transform.localPosition.z);
        }
    }

    /// <summary>
    /// 设置弹窗框数据
    /// </summary>
    /// <param name="iconSp"></param>
    /// <param name="titleStr"></param>
    /// <param name="remarkStr"></param>
    /// <param name="priceStr"></param>
    /// <param name="descriptionStr"></param>
    /// <param name="other"></param>
    public void SetInfoData(Sprite iconSp, string titleStr, string remarkStr, string priceStr, string descriptionStr, string otherStr) {
        SetInfoData(iconSp, titleStr, remarkStr, priceStr, descriptionStr, otherStr, 0);
    }
    public void SetInfoData(Sprite iconSp,string titleStr,string remarkStr,string priceStr,string descriptionStr,string otherStr,int priceIconPosition)
    {
        //头像
        if (iconSp != null && popupIcon != null)
            popupIcon.sprite = iconSp;
        //标题
        if (CheckUtil.StringIsNull(titleStr))
        {
            titleStr = "---";
        }
        if (titleStr != null && popupTitle != null)
            popupTitle.text = titleStr;
        //备注
        if (CheckUtil.StringIsNull(remarkStr))
        {
            remarkStr = "---";
        }
        if (remarkStr != null && popupRemark != null)
            popupRemark.text = remarkStr;
        //价格
        if (CheckUtil.StringIsNull(priceStr))
        {
            popupPriceCG.alpha = 0;
        }
        else
        {
            popupPriceCG.alpha = 1;
        }
        if (priceStr != null && popupPrice != null)
            popupPrice.text = priceStr;
        //详情
        if (CheckUtil.StringIsNull(descriptionStr))
        {
            line1.gameObject.SetActive(false);
            popupDescription.gameObject.SetActive(false);
        }
        else
        {
            line1.gameObject.SetActive(true);
            popupDescription.gameObject.SetActive(true);
            popupDescription.text = descriptionStr;
        }
        //其他
        if (CheckUtil.StringIsNull(otherStr))
        {
            line2.gameObject.SetActive(false);
            popupOther.gameObject.SetActive(false);
        }
        else
        {
            line2.gameObject.SetActive(true);
            popupOther.gameObject.SetActive(true);
            popupOther.text = otherStr;
        }
        //价格图标
        if ( ivPriceIcon != null)
            ivPriceIcon.sprite = spPriceList[priceIconPosition];

    }


    /// <summary>
    /// 刷新控件大小
    /// </summary>
    public void RefreshViewSize()
    {
        GameUtil.RefreshRectViewHight(thisRTF,false);
    }


}
