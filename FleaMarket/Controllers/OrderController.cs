using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FleaMarket.Models;

namespace FleaMarket.Controllers
{
    public class OrderController : Controller
    {
        //
        // GET: /Order/

        #region 返回订单页面
        public ActionResult Index()
        {
            //1. 登录判断
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }

            //通过Session 获取用户的id
            int userId = 0;
            if (!int.TryParse(Session["UserId"].ToString(), out userId))
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

            ShoppingEntities se = new ShoppingEntities();
            Users user = se.Users.FirstOrDefault(u => u.UserID== userId);
            userId = user.UserID;

            //通过用户的ID查到产品
            var order = se.TradeRecord.Where(tra => tra.UserBuyID == userId).ToList();
            ViewBag.title = "我的订单";
            ViewBag.Id = userId;
            return View(order);
        }
        #endregion

        #region 提交订单评价
        /// <summary>
        /// 提交订单评价
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Submit(int id)
        {
            //1. 登录判断
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }

            //往数据库的表中修改内容
            using (ShoppingEntities se = new ShoppingEntities())
            {
                string text = Request.Form["text"];
                TradeRecord order = se.TradeRecord.FirstOrDefault(u => u.TraPID == id);
                //判断是否提交过评论
                if (order.TraComment == null)
                {
                    var entry = se.Entry(order);
                    entry.State = System.Data.EntityState.Unchanged;
                    entry.Property("TraComment").IsModified = true;
                    order.TraComment = text;
                    if (se.SaveChanges() > 0)
                    {
                        return Content("<script>alert('提价评价成功！');window.location.href='/Order/Index';</script>");
                    }
                    else
                    {
                        return Content("<script>alert('提价评价失败！');window.location.href='/Order/Index';</script>");
                    }
                }
                else
                {
                    return Content("<script>alert('已经提交过评论了，请勿重复提交！');window.location.href='/Order/Index';</script>");
                }
            }
        }
        #endregion

        #region 删除订单
        /// <summary>
        /// 删除订单，因为数据量少，暂时不给操作
        /// </summary>
        /// <param name="Pid"></param>
        /// <returns></returns>
        public ActionResult Delete(int Pid)
        {

            ShoppingEntities se = new ShoppingEntities();
            TradeRecord entity = se.TradeRecord.FirstOrDefault(u => u.TraPID == Pid);
            se.TradeRecord.Remove(entity);

            //修改买卖状态
            Product pro = se.Product.FirstOrDefault(u => u.ProID == Pid);
            var entry = se.Entry(pro);
            entry.State = System.Data.EntityState.Unchanged;
            entry.Property("ProIsSell").IsModified = true;
            pro.ProIsSell = false;

            if (se.SaveChanges() > 0)
            {
                return Content("<script>alert('删除订单成功！');window.location.href='/Order/Index'</script>");
            }
            else
            {
                return Content("<script>alert('删除订单失败！');window.location.href='/Order/Index'</script>");
            }

        }
        #endregion


        #region 判断是否有登录或登录过
        /// <summary>
        /// 判断用户是否登录了
        /// </summary>
        /// <returns></returns>
        public Boolean IsLogined()
        {
            //判断是否有 cookie
            if (Request.Cookies["isremember"] != null)
            {
                //将Cookie中保存的用户id读取出来，存储到Session中
                string idStr = Request.Cookies["isremember"].Value;
                int userid;
                if (!int.TryParse(idStr, out userid))
                {
                    return false;
                }
                //通过id查询数据库获取用户名
                using (ShoppingEntities dc = new ShoppingEntities())
                {
                    Users model = dc.Users.FirstOrDefault(u => u.UserID == userid);
                    Session["LoginUser"] = model.UserName;
                    Session["UserId"] = model.UserID;
                    //返回登录后的用户头像地址
                    Session["userIcon"] = model.UserIcon;
                }
                return true;
            }

            // 判断是否有 Session
            if (Session["LoginUser"] != null) return true;
            return false;
        }
        #endregion

    }
}
