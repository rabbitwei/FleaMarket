using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

using FleaMarket.Models;
using PagedList;
using FleaMarket.Util;
//using Webdiyer.WebControls.Mvc;


namespace FleaMarket.Controllers
{
    public class HomeController : Controller
    {

        #region 这个业务方法没有使用了
        /// <summary>
        /// 返回主页
        /// </summary>
        /// <param name="page"></param>
        /// <param name="categId"></param>
        /// <returns></returns>
        public ActionResult Index(int? page, int? categId)
        {
            if (IsLogined())
            {
                int userid;
                string userIdStr = Session["UserId"].ToString();
                if (!int.TryParse(userIdStr, out userid))
                {
                    return Content("<script>alert('参数不合法');window.location.href='/Users/Index'</script>");
                }
                ViewData["userId"] = userid;
            }
            else
            {
                ViewData["userId"] = null;
            }

            //if (!IsLogined())
            //{
            //    return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            //}
            //显示第几页
            int start = page ?? 1;  //如果page不为空，则赋值给start，否则赋值1给start
            int count = 10;
            int total = 0;
            int categoryId = categId ?? 0;
            ViewData["categoryId"] = categoryId;

            //分类查询
            var products = ListProduct(categoryId, ref start, count, ref total);
            //StaticPagedList<T>分页查询,某一页的数据、页码、每页数据的容量、和数据总条目传入
            //也就是说这时候StaticPagedList不再像PagedList一样承担数据的划分工作，而仅仅承担数据的绑定操作
            //var productAsIPagedList = new StaticPagedList<Product>(products, start, count, total);

            //通过Request.IsAjaxRequest()来判断是否要加载公共视图
            //if (Request.IsAjaxRequest())
            //{
            //    return PartialView("_ItemTable", productAsIPagedList);
            //}
            return View(products);


        }
        #endregion

        #region 通过Ajax和Json的方式，来渲染主页的产品数据
        /// <summary>
        /// 返回json数据，调用ListProduct方法
        /// </summary>
        /// <param name="page">开始页</param>
        /// <param name="categId">分页id</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult getProduct(int? page, int? categId)
        {
            //如果登录过的话，将用户名和用户id写到session
            if (IsLogined())
            {
                int userid;
                string userIdStr = Session["UserId"].ToString();
                if (!int.TryParse(userIdStr, out userid))
                {
                    return Content("<script>alert('参数不合法');window.location.href='/Users/Index'</script>");
                }
                ViewData["userId"] = userid;
            }
            else
            {
                ViewData["userId"] = null;
            }

            //显示第几页
            int start = page ?? 1;  //如果page不为空，则赋值给start，否则赋值1给start
            start = start < 1 ? 1 : start;
            int count = 10;
            int total = 0;
            int categoryId = categId ?? 0;

            //分类查询
            var products = ListProduct(categoryId, ref start, count, ref total);

            //构造Json数据
            Dictionary<String, Object> retJson = new Dictionary<string, object>();
            retJson["start"] = start;
            retJson["count"] = count;
            retJson["categId"] = categoryId;
            retJson["total"] = total;
            retJson["products"] = products;

            //返回结果
            return Json(retJson, JsonRequestBehavior.DenyGet);
        }
        #endregion

        #region 通过分类查询返回数据
        /// <summary>
        /// 返回指定数量的Product对象
        /// </summary>
        /// <param name="categoryId">分类id</param>
        /// <param name="start">第几条开始</param>
        /// <param name="count">获取多少条记录</param>
        /// <param name="total">数据库的记录总数</param>
        /// <returns></returns>
        private List<Dictionary<String, Object>> ListProduct(int categoryId, ref int start, int count, ref int total)
        {
            int lastPage = 0;
            using (ShoppingEntities entity = new ShoppingEntities())
            {
                List<Product> products = null;
                //若分类ID大于0时候，就查询对应分类ID的所有商品数据
                if (categoryId > 0)
                {
                    //返回分类后的产品总数，
                    total = entity.Product.Count(p => p.ProIsSell == false && p.ProCateg == categoryId);
                    lastPage = total / count + 1;

                    products = (from c in entity.Category
                                join p in entity.Product
                                on c.CategID equals p.ProCateg
                                where c.CategID == categoryId && p.ProIsSell == false
                                orderby p.ProID descending
                                select p
                                    ).Skip((start - 1) * count).Take(count).ToList();

                
                }
                //否则就查询所有商品显示在最新栏内容里面
                else {
                    //返回没有分类后的产品总数，
                    total = entity.Product.Count(p => p.ProIsSell == false);
                    lastPage = total / count + 1;

                    products = (from p in entity.Product
                                orderby p.ProID descending
                                where p.ProIsSell == false
                                select p).Skip((start - 1) * count).Take(count).ToList();
                    total = entity.Product.Count(p => p.ProIsSell == false);
                    lastPage = total / count + 1;
                    
                }

                //对start做了最大边界处理
                start = start > lastPage ? lastPage : start;

                List<Dictionary<String, Object>> proList = new List<Dictionary<String, Object>>();
               
                //将获取的到Product转成Dictionary对象
                foreach (Product p in products)
                    proList.Add(ProductObjectToJson.Convert(p, null));
                return proList;
            }
        }
        #endregion


        #region 判断用户是否登录了和登录过没有
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
                    return true;
                }

            }

            // 判断是否有 Session
            if (Session["LoginUser"] != null) return true;
            return false;
        }
        #endregion
    }
}
