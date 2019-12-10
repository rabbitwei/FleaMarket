using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FleaMarket.Models;

namespace FleaMarket.Controllers.Admin
{
    public class AdminTradeController : Controller
    {
        //
        // GET: /AdminTrade/

        public ActionResult Index()
        {
            ShoppingEntities dc = new ShoppingEntities();
                //获取所有的用户数据，是一个泛型集合
                List<TradeRecord> list = dc.TradeRecord.ToList();

                //使用ViewBag传递单数据给对应的视图
                ViewBag.TitleBar = "交易管理列表";

                //返回视图，同时传递数据给视图
                return View(list);
        }

    }
}
