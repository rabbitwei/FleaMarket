using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

using FleaMarket.Models;
using FleaMarket.Util;
using System.Drawing;
using System.IO;
using System.Data;
using Newtonsoft.Json;

namespace FleaMarket.Controllers
{
    public class UsersController : Controller
    {


        #region 用户下架产品
        /// <summary>
        /// 用户下架产品
        /// </summary>
        /// <param name="pid">产品id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CancelRelease(string pid)
        {
            if (!IsLogined())
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

            int proId = 0;
            if (!int.TryParse(pid, out proId))
                return Content("<script>alert('查询查询错误');window.location.href='/Users/Me'</script>");

            //获取用户id
            int userId;
            if (!int.TryParse(Session["UserId"].ToString(), out userId))
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //查找product表，通过产品的id和产品的发布用户的id来查找
                var product = dc.Product.FirstOrDefault(p => p.ProID == proId && p.ProWhoUser == userId);
                if (null != product)
                {
                    //删除产品
                    dc.Product.Remove(product);
                    if (dc.SaveChanges() > 0)
                        return Content("<script>alert('下架成功');window.location.href='/Users/Me'</script>");
                    else
                        return Content("<script>alert('下架成功');window.location.href='/Users/Me'</script>");
                }
                else
                    return Content("<script>alert('该产品不是您的');window.location.href='/Users/Me'</script>");
            }
        }
        #endregion

        #region 用户取消收藏
        /// <summary>
        /// 用户取消收藏
        /// </summary>
        /// <param name="pid">产品id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CancelFavorite(string pid)
        {
            if (!IsLogined())
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

            int proId = 0;
            if (!int.TryParse(pid, out proId))
                return Content("<script>alert('查询查询错误');window.location.href='/Users/Me'</script>");

            //获取用户id
            int userId;
            if (!int.TryParse(Session["UserId"].ToString(), out userId))
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //查找Favorite表是否有该产品的收藏，通过登录用户的id和产品id
                var favoriteEntiry = dc.Favorite.FirstOrDefault(
                    fav => fav.FavPID == proId && fav.FavUID == userId);

                //判断是否有该收藏
                if (null != favoriteEntiry)
                {
                    //删除产品
                    dc.Favorite.Remove(favoriteEntiry);
                    if (dc.SaveChanges() > 0)
                        return Content("<script>alert('取消收藏成功');window.location.href='/Users/Me'</script>");
                    else
                        return Content("<script>alert('取消收藏失败');window.location.href='/Users/Me'</script>");
                }
                else
                    return Content("<script>alert('您没有该产品的收藏');window.location.href='/Users/Me'</script>");
            }
        }
        #endregion

        #region 用户添加收藏
        /// <summary>
        /// 用户添加收藏，需要登录
        /// </summary>
        /// <param name="pid">产品id</param>
        /// <returns></returns>
        /// AddFavorite
        public ActionResult AddFavorite(string pid)
        {
            if (!IsLogined())
                return Content("<script>alert('请登录才可以收藏');window.location.href='/Users/Login'</script>");

            int proId = 0;
            if (!int.TryParse(pid, out proId))
                return Content("<script>alert('查询查询错误');window.location.href='/Users/Me'</script>");

            //获取用户id
            int userId;
            if (!int.TryParse(Session["UserId"].ToString(), out userId))
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");

            //添加收藏
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                var favoriteEntity = new Favorite();
                favoriteEntity.FavUID = userId;
                favoriteEntity.FavPID = proId;
                //收藏前先判断是否有重复收藏
                var favRet = dc.Favorite.FirstOrDefault(fav => fav.FavPID == proId && fav.FavUID == userId);
                if (favRet != null)
                    return Content("<script>alert('不可以重复收藏');window.location.href='/Product/Detail/" + pid + "'</script>");
                else
                {
                    dc.Favorite.Add(favoriteEntity);
                    if (dc.SaveChanges() > 0)
                        return Content("<script>alert('收藏成功');window.location.href='/Product/Detail/" + pid + "'</script>");
                    else
                        return Content("<script>alert('收藏失败');window.location.href='/Product/Detail" + pid + "'</script>");
                }



            }
        }
        #endregion



        #region 返回用户发布的产品数据（返回Json）

        /// <summary>
        //返回该用户发布的产品，（做了产品被卖了之后不显示的操作）
        /// </summary>
        /// <returns>json集合</returns>
        [HttpPost]
        public ActionResult MyReleaseProduct()
        {
            //1. 先判断用户是否登录了

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

            //3. 获取该用户的发布产品的数据
            /*
             * 遇到的bug：Entity Framework对象序列化出错：检测到循环引用
             * 原因： 错误是EF的导航属性导致的，Product对象的ProWhoUser属性引用了Product对象导致无限循环，EF下很多问题ToList后通常能解决，但这次不行：
            
             * 解决： 方法一：关闭导航功能(不能再使用导航属性):   dc.Configuration.ProxyCreationEnabled = false;
             *        方法二：转为匿名对象：dc.Product.Where(p => p.ProWhoUser == userid).ToList();
             */
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //手动将Product对象转成Dictionary对象
                List<Dictionary<String, Object>> proList = new List<Dictionary<String, Object>>();
                var retList = dc.Product.Where(p => p.ProWhoUser == userid && p.ProIsSell == false).ToList();
                foreach (Product p in retList)
                    proList.Add(ProductObjectToJson.Convert(p, null));

                //var dataList = dc.Product.Where(p => p.ProWhoUser == userid && p.ProIsSell == false).Select((p =>
                //    new { p.ProID, p.ProName, p.ProLevel, p.ProOldPrice, p.ProNewPrice, p.ProIntro, p.ProImgUrl, p.ProReleaseTime }
                //    )).ToList();
                Dictionary<String, Object> retJson = new Dictionary<String, Object>();
                retJson.Add("total", proList.Count());
                retJson.Add("products", proList);
                return Json(retJson, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region 返回用户收藏的产品（返回Json）
        /// <summary>
        /// 返回用户收藏的产品
        /// </summary>
        /// <returns>json集合</returns>
        [HttpPost]
        public ActionResult MyFavoriteProduct()
        {
            //1. 先判断用户是否登录了
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

            //查询用户的收藏，这里不需要判断该产品是否出售了
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //用于返回给前端的集合
                List<Dictionary<String, Object>> favList = new List<Dictionary<String, Object>>();
                //查询用户收藏的集合
                var retList = dc.Favorite.Where(fav => fav.FavUID == userid).ToList();
                foreach (var ret in retList)
                    favList.Add(ProductObjectToJson.Convert(ret.Product, null));

                Dictionary<String, Object> retJson = new Dictionary<String, Object>();
                retJson.Add("total", favList.Count());
                retJson.Add("products", favList);
                return Json(retJson, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region 返回用户买到的产品（返回Json）
        /// <summary>
        /// 返回该用户买到的产品，通过订单表中的买家id来判断
        /// </summary>
        /// <returns>json集合</returns>
        [HttpPost]
        public ActionResult MyBuyProduct()
        {
            //1. 先判断用户是否登录了
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
            //查询用户买了的产品，通过查询订单表中的买家id来判断
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                List<Dictionary<String, Object>> buyList = new List<Dictionary<String, Object>>();
                var tradeList = dc.TradeRecord.Where(tra => tra.UserBuyID == userid).ToList();
                foreach (var trade in tradeList)
                    buyList.Add(ProductObjectToJson.Convert(trade.Product, trade));

                Dictionary<String, Object> retJson = new Dictionary<String, Object>();
                retJson.Add("total", buyList.Count());
                retJson.Add("products", buyList);
                return Json(retJson, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region 返回用户卖出的产品（返回Json）
        /// <summary>
        /// 返回该用户卖出的产品，通过订单表中的卖家id来判断，还添加了评价
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult MySellProduct()
        {
            //1. 先判断用户是否登录了
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

            //3. 查询订单表，通过卖家id来判断，还添加了评价
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                List<Dictionary<String, Object>> sellList = new List<Dictionary<String, Object>>();
                var tradeList = dc.TradeRecord.Where(tra => tra.UserSellID == userid).ToList();
                foreach (var trade in tradeList)
                    sellList.Add(ProductObjectToJson.Convert(trade.Product, trade));

                Dictionary<String, Object> retJson = new Dictionary<String, Object>();
                retJson.Add("total", sellList.Count());
                retJson.Add("products", sellList);
                return Json(retJson, JsonRequestBehavior.DenyGet);
            }
        }
        #endregion


        #region 用户编辑页面
        /// <summary>
        ///用户编辑页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Edit()
        {
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }
            ViewBag.TitleBar = "编辑个人信息";

            //通过Session获取id，不能通过url地址来获取
            string id = Session["UserId"].ToString();

            //1.对id进行合法性验证
            int userid;
            if (!int.TryParse(id, out userid))
            {
                return Content("<script>alert('参数不合法');window.location.href='/Users/Login'</script>");
            }

            //2. 查询该用户的信息
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                Users entity = dc.Users.FirstOrDefault(u => u.UserID == userid);
                if (entity != null)//查询到该用户
                {
                    return View(entity);
                }
                else//查询不到该用户
                {
                    return Redirect("/Users/Index");
                }
            }
        }
        #endregion

        #region 用户编辑操作
        ///<summary>
        /// 用户编辑页面的操作
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(Users entity)
        {
            string intro = entity.UserIntro;
            entity.UserIntro = intro.Trim();


            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //获取entity实体的状态
                var entry = dc.Entry(entity);
                entry.State = System.Data.EntityState.Unchanged;
                //指定修改的属性
                entry.Property("UserName").IsModified = true;
                entry.Property("UserGender").IsModified = true;
                entry.Property("UserAddress").IsModified = true;
                entry.Property("UserIntro").IsModified = true;
                entry.Property("UserPhone").IsModified = true;
                //dc.Entry<Users>(entity).State = EntityState.Modified;
                //try
                {
                    dc.Configuration.ValidateOnSaveEnabled = false;
                    //保存回数据库中
                    if (dc.SaveChanges() > 0)
                    {
                        dc.Configuration.ValidateOnSaveEnabled = true;
                        return Content("<script>alert('修改成功');window.location.href='/Users/Me'</script>");
                    }
                    else
                    {
                        return Content("<script>alert('修改失败');window.location.href='/Users/Edit'</script>");
                    }
                }
                //catch (Exception e)
                //{
                //    String str = e.Message;
                //    return Content("<script>alert('" + str + "');window.location.href='/Users/Me'</script>");

                //}

            }
        }
        #endregion

        #region 用户详细页面
        /// <summary>
        /// 用户的详细信息页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Me()
        {
            if (!IsLogined())
            {
                return Content("<script>alert('请登录');window.location.href='/Users/Login'</script>");
            }
            ViewBag.TitleBar = "我的";

            //通过Session获取id，不能通过url地址来获取
            string id = Session["UserId"].ToString();

            ////1.对id进行合法性验证
            int userid;
            if (!int.TryParse(id, out userid))
            {
                return Content("<script>alert('参数不合法');window.location.href='/Users/Login'</script>");
            }
            //2.查询该id对应的用户信息
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                Users entity = dc.Users.FirstOrDefault(u => u.UserID == userid);
                if (entity != null)//查询到该用户
                {
                    return View(entity);
                }
                else//查询不到该用户
                {
                    return Redirect("/Users/Index");
                }
            }
        }
        #endregion


        #region 返回登录页面
        public ActionResult Login()
        {
            //1、免登录判断
            if (IsLogined())
                return RedirectToAction("Index", "Home");
            return View();
        }
        #endregion

        #region 该业务方法没有再使用了

        /// <summary>
        /// 登录功能使用全局刷新的， 注意不是使用用户名登录的，而是使用用户的账号， 对应数据库的 UserAccount 字段
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            string userAccount = fc["useraccount"];
            string userPwd = fc["userpwd"];
            string vcode = fc["vcode"];
            string isremember = fc["isremember"];  //如果勾选了复选框，那么isremember的值为on；如果没有勾选，那么isremember的值为null

            //2、检查输入的有效性
            if (string.IsNullOrEmpty(userAccount) || string.IsNullOrEmpty(userPwd) || string.IsNullOrEmpty(vcode))
            {
                return Content("<script>alert('请输入所有数据!');window.location.href='/Users/Login'</script>");
            }

            //3、判断Session是否已经过期
            if (Session["ValidateCode"] == null)
            {
                return Content("<script>alert('验证码已过期!');window.location.href='/Users/Login'</script>");
            }

            //4、判断用户输入的验证码 与 产生的验证码字符串 是否一致

            //获取存入到Session中的验证码字符串
            string validateCode = Session["ValidateCode"].ToString();   //转换为字符串
            //判断是否一致（StringComparison.OrdinalIgnoreCase忽略大小写）
            if (validateCode.Equals(vcode, StringComparison.OrdinalIgnoreCase))
            {
                //验证码验证成功
                //验证用户名和密码的正确性
                using (ShoppingEntities dc = new ShoppingEntities())
                {
                    //使用EF查询用户
                    Users model = dc.Users.FirstOrDefault(u => u.UserAccount.Equals(userAccount)
                                                            && u.UserPwd.Equals(userPwd)
                                                            && u.UserStatus == 1);

                    {
                        //判断用户是否存在
                        if (model != null)
                        {
                            //5、实现免登录功能
                            if (!string.IsNullOrEmpty(isremember))   //勾选了复选框
                            {
                                //将登录信息保存到Cookie中
                                HttpCookie cookie = new HttpCookie("isremember", model.UserID.ToString());
                                cookie.Expires = DateTime.Now.AddDays(10);
                                Response.Cookies.Add(cookie);
                            }
                            //6、将用户名字存储到Session中
                            Session["LoginUser"] = model.UserName;
                            // 将用户的id存储到Session中， 用于判断是哪个用户
                            Session["UserId"] = model.UserID;

                            //判断是否用户类型：
                            return Content("<script>alert('登录成功!');window.location.href='/Home/Index'</script>");
                        }
                        else
                        {
                            return Content("<script>alert('用户名或密码错误!');window.location.href='/Users/Login'</script>");
                        }
                    }
                }
            }
            else
            {
                Dictionary<String, Object> ret = new Dictionary<String, Object>();
                ret.Add("error", true);
                ret.Add("errorMessage", "验证码错误");
                return Json(ret, JsonRequestBehavior.DenyGet);
                //return Content("<script>alert('验证码输入错误!');window.location.href='/Users/Login'</script>");
            }
        }
        #endregion

        #region 登出
        public ActionResult Logout()
        {
            //String userAccount = Session["LoginUser"].ToString();
            ////清除本地cookie
            //HttpCookie cookie = new HttpCookie("isremember", userAccount);
            //cookie.Expires = DateTime.Now.AddDays(10);
            //Response.Cookies.Add(cookie);
            //Session["LoginUser"] = null;
            //return Content("<script>window.location.href='/home/Index'</script>");

            //清除Session
            Session.Abandon();
            //清除Cookie,不是删除，而是替换
            Response.Cookies["isremember"].Expires = DateTime.Now.AddDays(-1);  //Cookie立刻失效
            //跳转到登录页
            return RedirectToAction("Index", "Home");
        }
        #endregion



        #region 返回注册页面
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        #endregion

        #region 注册 有头像
        [HttpPost]
        public ActionResult Register(Users entity, IEnumerable<HttpPostedFileBase> UserIcon)
        {
            if (ModelState.IsValid)
            {
                using (ShoppingEntities dc = new ShoppingEntities())
                {
                    entity.UserStatus = 1;
                    entity.UserRole = 2;

                    //保存用户上传的头像
                    foreach (var file in UserIcon)
                    {
                        if (file.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(file.FileName);
                            var path = Path.Combine(Server.MapPath("/Content/userImg/"), fileName);
                            file.SaveAs(path);
                            entity.UserIcon = "userImg/" + fileName;
                        }
                    }



                    dc.Users.Add(entity);

                    if (dc.SaveChanges() > 0)
                    {
                        return Content("<script>alert('注册成功！');window.location.href='/Users/Login'</script>");
                        //return Success("新增用户成功！");
                    }
                    else
                    {
                        return Content("<script>alert('注册失败！');window.location.href='/Users/Register'</script>");
                        //return Failure("新增用户失败！")；
                    }
                }
            }
            return View(entity);
        }
        #endregion

        #region 检查用户账号的唯一性
        /// <summary>
        /// 检查用户账号的唯一性
        /// </summary>
        /// <param name="useraccount">传入的账号参数名必须与模型类的属性名保持一致</param>
        /// <returns>JSON数据,值为true,表示用户账号可以使用,值为false,表示用户名不可以使用</returns>
        public ActionResult CheckUserAccount(string useraccount)
        {
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //在Users表中，查询UserAccount字段等于输入的参数值的记录个数，个数为0则表示true,个数不为0则表示false
                bool result = dc.Users.Count(u => u.UserAccount == useraccount) == 0;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region 返回验证码
        /// <summary>
        /// 返回验证码图片
        /// </summary>
        /// <returns></returns>
        public ActionResult GetValidateCode()
        {
            Dictionary<String, Object> dic = CodeUtil.GenerateCodeAndPic(4);
            Session["ValidateCode"] = dic["code"];
            //返回
            return File((byte[])dic["pic"], "image/jpeg");
        }
        #endregion

        #region 验证码验证(返回Json)
        /// <summary>
        /// 判断前端验证码输入是否正确
        /// </summary>
        /// <returns>Json对象</returns>
        [HttpPost]
        public ActionResult ValidateCode()
        {
            String vcode = Request.Form["vcode"];
            //3、判断Session是否已经过期
            if (Session["ValidateCode"] == null)
            {
                return Content("<script>alert('验证码已过期!');window.location.href='/Users/Login'</script>");
            }
            //4、判断用户输入的验证码 与 产生的验证码字符串 是否一致
            //获取存入到Session中的验证码字符串
            string validateCode = Session["ValidateCode"].ToString();   //转换为字符串
            //判断是否一致（StringComparison.OrdinalIgnoreCase忽略大小写）
            if (validateCode.Equals(vcode, StringComparison.OrdinalIgnoreCase))
            {
                Dictionary<String, Object> ret = new Dictionary<String, Object>();
                ret.Add("success", true);
                ret.Add("errorMessage", "验证码正确，请进行账号和密码判断");
                return Json(ret, JsonRequestBehavior.DenyGet);
            }
            else
            {
                Dictionary<String, Object> ret = new Dictionary<String, Object>();
                ret.Add("error", true);
                ret.Add("errorMessage", "验证码错误");
                return Json(ret, JsonRequestBehavior.DenyGet);
                //return Content("<script>alert('验证码输入错误!');window.location.href='/Users/Login'</script>");
            }
        }
        #endregion


        #region 用户账号和密码验证（返回Json）
        [HttpPost]
        public ActionResult LoginIsSuccess()
        {
            string userAccount = Request.Form["useraccount"];
            string userPwd = Request.Form["userpwd"];
            //如果勾选了复选框，那么isremember的值为on；如果没有勾选，那么isremember的值为null
            string isremember = Request.Form["isremember"];

            Dictionary<String, Object> ret = new Dictionary<String, Object>();
            //2、检查输入的有效性
            if (string.IsNullOrEmpty(userAccount) || string.IsNullOrEmpty(userPwd))
            {
                ret.Add("error", true);
                ret.Add("errorMessage", "请输入所有数据");
                return Json(ret, JsonRequestBehavior.DenyGet);
                //return Content("<script>alert('请输入所有数据!');window.location.href='/Users/Login'</script>");

            }

            //3、判断Session是否已经过期
            if (Session["ValidateCode"] == null)
            {
                ret.Add("error", true);
                ret.Add("errorMessage", "验证码已过期");
                return Json(ret, JsonRequestBehavior.DenyGet);
                //return Content("<script>alert('验证码已过期!');window.location.href='/Users/Login'</script>");
            }
            //验证用户名和密码的正确性
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //使用EF查询用户
                Users model = dc.Users.FirstOrDefault(u => u.UserAccount.Equals(userAccount)
                                                        && u.UserPwd.Equals(userPwd)
                                                        && u.UserStatus == 1);

                {
                    //判断用户是否存在
                    if (model != null)
                    {
                        //5、实现免登录功能
                        if (!string.IsNullOrEmpty(isremember))   //勾选了复选框
                        {
                            //将登录信息保存到Cookie中
                            HttpCookie cookie = new HttpCookie("isremember", model.UserID.ToString());
                            cookie.Expires = DateTime.Now.AddDays(10);
                            Response.Cookies.Add(cookie);
                        }
                        //6、将用户名字存储到Session中
                        Session["LoginUser"] = model.UserName;
                        // 将用户的id存储到Session中， 用于判断是哪个用户
                        Session["UserId"] = model.UserID;
                        Session["UserIcon"] = model.UserIcon;

                        ret.Add("success", true);
                        ret.Add("succMessage", "登录成功");
                        return Json(ret, JsonRequestBehavior.DenyGet);
                        //return Content("<script>alert('登录成功!');window.location.href='/Home/Index'</script>");
                    }
                    else
                    {
                        ret.Add("error", true);
                        ret.Add("errorMessage", "用户名或密码错误");
                        return Json(ret, JsonRequestBehavior.DenyGet);
                        //return Content("<script>alert('用户名或密码错误!');window.location.href='/Users/Login'</script>");
                    }
                }
            }
        }
        #endregion

        #region 判断用户是否登录了和登录过没有
        /// <summary>
        /// 判断用户是否登录过了，判断是否有cookie先，在判断是否有session
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
                ShoppingEntities dc = new ShoppingEntities();
                Users model = dc.Users.FirstOrDefault(u => u.UserID == userid);
                Session["LoginUser"] = model.UserName;
                Session["UserId"] = model.UserID;
                Session["UserIcon"] = model.UserIcon;
                return true;
            }

            // 判断是否有 Session
            if (Session["LoginUser"] != null) return true;
            return false;
        }
        #endregion
    }
}
