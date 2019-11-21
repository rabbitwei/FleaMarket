/*
 *CLR版本:      4.0.30319.42000
 *机器名称:     RABBITWEI
 *文件名:        ProductObjectToJson
 *命名空间:     FleaMarket.Util
 *创建时间:     2019/11/20 13:55:57
 *创建人:        DIWEI
 *描述: 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using FleaMarket.Models;

namespace FleaMarket.Util
{
    public class ProductObjectToJson
    {
        /// <summary>
        /// 将product对象和Trade对象转成Dictionary对象，方便返回json数据
        /// </summary>
        /// <param name="p">product对象</param>
        /// <param name="tra">订单对象</param>
        /// <returns></returns>
        public static Dictionary<String, Object> Convert(Product p, TradeRecord tra)
        {
            Dictionary<String, Object> dicProduct = new Dictionary<String, Object>();
            dicProduct["ProID"] = p.ProID;
            dicProduct["ProName"] = p.ProName;
            dicProduct["ProLevel"] = p.ProLevel;
            dicProduct["ProOldPrice"] = p.ProOldPrice;
            dicProduct["ProNewPrice"] = p.ProNewPrice;
            dicProduct["ProCategory"] = p.Category.CategName;
            dicProduct["ProWhoUser"] = p.Users.UserName;
            dicProduct["ProIntro"] = p.ProIntro;
            dicProduct["ProImgUrl"] = p.ProImgUrl;
            dicProduct["ProReleaseTime"] = p.ProReleaseTime.ToString();
            dicProduct["ProIsSell"] = p.ProIsSell;
            dicProduct["UserIcon"] = p.Users.UserIcon;
            dicProduct["UserQQ"] = p.Users.UserQQ;
            if (tra != null)
            {
                dicProduct["TraComment"] = tra.TraComment;
                dicProduct["ProSellUser"] = tra.Users1.UserName;
            }
            return dicProduct;
        }
    }
}