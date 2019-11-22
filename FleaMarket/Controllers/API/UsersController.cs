using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FleaMarket.Models;
using FleaMarket.Util;

namespace FleaMarket.Controllers.API
{
    //RESTful风格的api，参数 id 由 App_Start/RouteConfig.cs 文件来完成映射了

    public class UsersController : Controller
    {
        #region 这个业务方法没有使用
        /// <summary>
        /// 没有使用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult getUser(String id)
        {
            int uid = 0;
            if (!int.TryParse(id, out uid))
            {
                var ret = new { error = 400, errMessage = "参数错误" };
                return Json(ret, JsonRequestBehavior.AllowGet);
            }

            using (ShoppingEntities dc = new ShoppingEntities())
            {
                var user = dc.Users.FirstOrDefault(u => u.UserID == uid);

                return Json(UserObjectToJson.Convert(user), JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
