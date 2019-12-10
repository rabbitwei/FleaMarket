using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FleaMarket.Models;
using System.IO;

namespace FleaMarket.Controllers
{
    public class ProductController : Controller
    {

        #region 产品详细页（使用视图加控制的的业务方法），没有使用了
        //产品详细页
        /*
        public ActionResult Detail(int id)
        {
            //这里判断不是有没有登录，而是判断登录过没有
            IsLogined();

            ShoppingEntities se = new ShoppingEntities();
            Product data = se.Set<Product>().
                FirstOrDefault(p => p.ProID == id && p.ProIsSell == false);
            //获取数据库中的Product所有的项并存到list后传到前端
            ViewBag.Id = id;//将url中的id值传到后台页面用于匹配数据库中的哪一项               
            return View(data);
        }
         */
        #endregion

        #region 返回产品详细页
        /// <summary>
        /// 返回产品详细页
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Detail(int? id)
        {
            IsLogined();
            return View();
        }
        #endregion

        #region 订单提交的操作
        /// <summary>
        /// 订单提交的操作
        /// </summary>
        /// <param name="proId"></param>
        /// <param name="sellerId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Detail(string proId, string sellerId)
        {
            //1. 登录判断
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }

            
            int pid = 0;

            if (!int.TryParse(proId, out pid))
                return Content("<script>alert('购买失败！');window.location.href='/Home/Index'</script>");

            string backUrl = "/Product/Detail/" + pid; 

            //该产品的发布者
            int sellId = 0;
            if (!int.TryParse(sellerId, out sellId))
                return Content("<script>alert('购买失败！');window.location.href='" + backUrl + "'</script>");


            using (ShoppingEntities se = new ShoppingEntities())
            {
                //查询是否有指定的产品
                var proEntity = se.Product.FirstOrDefault(p => p.ProID == pid);
                if (proEntity == null)
                    return Content("<script>alert('没有该产品！');window.location.href='/Home/Index'</script>");

                //查询该产品是否是指定的发布用户
                var userEntity = se.Product.FirstOrDefault(p => p.ProWhoUser == sellId);
                if (userEntity == null)
                    return Content("<script>alert('卖家没有发布过这个产品！');window.location.href='/Home/Index'</script>");
                


                //修改product表中的字段，让该 product 下架
                var entry = se.Entry(proEntity);
                entry.State = System.Data.EntityState.Unchanged;
                entry.Property("ProIsSell").IsModified = true;
                proEntity.ProIsSell = true;

                //创建订单
                TradeRecord tradeEntity = new TradeRecord();

                //获取当前用户的id
                int buyUserId = 0;
                if (!int.TryParse(Session["UserId"].ToString(), out buyUserId))
                    return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

                //买家和卖家不能是同一个人
                if (sellId == buyUserId)
                    return Content("<script>alert('不能购买自己发布的商品');window.location.href='" + backUrl + "'</script>");


                //订单需要的字段：产品id， 买家id， 卖家id
                tradeEntity.UserBuyID = buyUserId;
                tradeEntity.UserSellID = sellId;
                tradeEntity.TraPID = pid;
                //需要指定购买时间（不设置的话有坑）
                tradeEntity.TraBuyTime = DateTime.Now;

                //添加到数据库
                se.TradeRecord.Add(tradeEntity);

                if (se.SaveChanges() > 0)
                {
                    return Content("<script>alert('购买成功！');window.location.href='/Order/Index';</script>");
                }
                else
                {
                    return Content("<script>alert('购买失败！');window.location.href='/Home/Index'</script>");
                }
            }
        }
        #endregion

        #region 发布产品的页面
        [HttpGet]
        public ActionResult Release()
        {
            //1. 登录判断
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }

            List<SelectListItem> selectOption = new List<SelectListItem>();
            selectOption.Add(new SelectListItem { Value = "10", Text = "十成新", Selected = true });
            selectOption.Add(new SelectListItem { Value = "9", Text = "九成新" });
            selectOption.Add(new SelectListItem { Value = "8", Text = "八成新" });
            selectOption.Add(new SelectListItem { Value = "7", Text = "七成新" });
            ViewData["selectOption"] = selectOption;

            BindCategory();
            return View();
        }
        #endregion


        #region 发布产品的操作
        [HttpPost]
        public ActionResult Release(Product entity, IEnumerable<HttpPostedFileBase> ProImgUrl)
        {
            //1. 登录判断
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }

            //通过Session获取id，不能通过url地址来获取
            string id = Session["UserId"].ToString();

            //2.对id进行合法性验证
            int userid;
            if (!int.TryParse(id, out userid))
            {
                return Content("<script>alert('参数不合法');window.location.href='/Users/Login'</script>");
            }

            using (ShoppingEntities dc = new ShoppingEntities())
            {
                if (ModelState.IsValid)
                {
                    //谁发布的商品（WhoUse）：
                    entity.ProWhoUser = userid;

                    //保存图片路径：
                    foreach (var file in ProImgUrl)
                    {
                        if (file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("/Content/productImg/images/"), fileName);
                            file.SaveAs(path);
                            entity.ProImgUrl = "images/" + fileName;
                        }
                    }

                    //发布时间：
                    entity.ProReleaseTime = DateTime.Now;

                    //保存实体到上下文对象中
                    dc.Product.Add(entity);

                    if (dc.SaveChanges() > 0)
                    {
                        return Content("<script>alert('发布商品成功！');window.location.href='/Home/Index'</script>");
                    }
                    else
                    {
                        return Content("<script>alert('发布商品失败！');window.location.href='/Product/Release'</script>");
                    }
                }
                else
                {
                    BindCategory();
                    return Content("<script>alert('发布商品失败！');window.location.href='/Product/Release'</script>");
                    //return View(entity);
                }

            }

        }
        #endregion

        #region 发布页的下拉列表的商品类别
        /// <summary>
        /// 下拉列表绑定商品类别
        /// </summary>
        private void BindCategory()
        {
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                List<Category> list = dc.Category.ToList();
                //ViewBag.category = list;
                //第一个参数，实现了IEnumerable接口的对象
                //第二个参数，实现了dataValueField:下拉列表框中选项对应的值字段
                //第三个参数dataTextField 下拉列表框中选项对应的文本字段
                //SelectList slist = new SelectList(list, "categid", "categname");
                //SelectList slist = new SelectList(dc.Category.ToList().Select(item => item.CategName));
                SelectList slist = new SelectList(list, "CategID", "Categname");
                ViewData["Category"] = slist;
            }
        }
        #endregion

        #region 判断是否有登录或登录过
        /// <summary>
        /// 判断用户是否登录或登录过
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
