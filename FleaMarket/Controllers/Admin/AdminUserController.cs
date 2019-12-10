using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FleaMarket.Models;

namespace FleaMarket.Controllers.Admin
{
    public class AdminUserController : Controller
    {
        //
        // GET: /User/

        public ActionResult Index()
        {
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //获取所有的用户数据，是一个泛型集合
                List<Users> list = dc.Users.ToList();

                //使用ViewBag传递单数据给对应的视图
                ViewBag.TitleBar = "普通用户列表";
                //ViewData["TitleBar"] = "用户列表";

                //返回视图，同时传递数据给视图
                return View(list);
            }
        }

    }
}
