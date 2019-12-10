/*
 *CLR版本:      4.0.30319.42000
 *机器名称:     RABBITWEI
 *文件名:        UserToJson
 *命名空间:     FleaMarket.Util
 *创建时间:     2019/11/21 16:39:22
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
    public class UserObjectToJson
    {
        public static Dictionary<String, Object> Convert(Users u) {
            Dictionary<String, Object> dic = new Dictionary<String, Object>();

            dic["UserId"] = u.UserID;
            dic["UserAccount"] = u.UserAccount;
            dic["UserName"] = u.UserName;
            dic["UserGender"] = u.UserGender;
            dic["UserAddress"] = u.UserAccount;
            dic["UserIntro"] = u.UserIntro;
            dic["UserPhone"] = u.UserPhone;
            dic["UserIcon"] = u.UserIcon;
            dic["UserQQ"] = u.UserQQ;

            return dic; 
        }
    }
}