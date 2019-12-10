using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FleaMarket.Models;

namespace FleaMarket.Controllers.Admin
{
    public class AdminProductController : Controller
    {
        //
        // GET: /AdminProduct/

        public ActionResult Index()
        {
            ShoppingEntities dc = new ShoppingEntities();
            //获取所有的用户数据，是一个泛型集合
            List<Product> list = dc.Product.ToList();

            //使用ViewBag传递单数据给对应的视图
            ViewBag.TitleBar = "产品管理列表";

            //返回视图，同时传递数据给视图
            return View(list);
        }

        //管理员删除产品操作
        public ActionResult Delete(String id)
        {
            //1、检查参数id的合法性  string -> int
            int productId;
            if (!int.TryParse(id, out productId))
            {
                return Content("<script>alert('参数不合法');window.location.href='/AdminProduct/Index'</script>");
            }
            //2、使用EF删除产品
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //1、先查询
                Product entity = dc.Product.FirstOrDefault(p => p.ProID == productId);
                //2、后删除
                dc.Product.Remove(entity);
                //3、保存回数据库中
                if (dc.SaveChanges() > 0)
                {
                    return Content("<script>alert('产品下架成功！');window.location.href='/AdminProduct/Index'</script>");
                }
                else
                {
                    return Content("<script>alert('产品下载失败！');window.location.href='/AdminProduct/Index'</script>");
                }
            }
        }

    }
}
