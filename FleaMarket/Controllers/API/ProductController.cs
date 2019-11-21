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

    public class ProductController : Controller
    {

        /// <summary>
        /// 返回产品信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult getDetail(String id)
        {

            //参数有效性判断
            int pid = 0;
            if (!int.TryParse(id, out pid))
            {
                var ret = new { error = 400, errMessage = "参数错误" };
                return Json(ret, JsonRequestBehavior.AllowGet);
            }

            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //通过id和是否已出售来判断
                var product = dc.Product.FirstOrDefault(p => p.ProID == pid && p.ProIsSell == false);
                //用过产品的 id 来获取用户数据
                var user = dc.Users.FirstOrDefault(u => u.UserID == product.ProWhoUser);

                //将product对象转成json格式（包含用户信息）， 订单对象为null
                
                Dictionary<String, Object> jsonProduct = new Dictionary<String, Object>();

                jsonProduct.Add("product" ,ProductObjectToJson.Convert(product, null));

                //添加产品的用户
                jsonProduct.Add("user", UserObjectToJson.Convert(user));

                return Json(jsonProduct, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
