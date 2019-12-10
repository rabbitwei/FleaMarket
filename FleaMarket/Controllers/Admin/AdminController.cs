using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using FleaMarket.Models;
using FleaMarket.Util;
using System.Data;

namespace FleaMarket.Controllers
{
    public class AdminController : Controller
    {
        //后台用户显示页
        public ActionResult Index()
        {
            if (IsLogined())
            {

                using (ShoppingEntities dc = new ShoppingEntities())
                {
                    //获取所有的用户数据，是一个泛型集合
                    List<Administrators> list = dc.Administrators.ToList();

                    //使用ViewBag传递单数据给对应的视图
                    ViewBag.TitleBar = "后台用户列表";
                    //ViewData["TitleBar"] = "用户列表";

                    //返回视图，同时传递数据给视图
                    return View(list);
                }
            }
            return RedirectToAction("Login", "Admin");
        }

        //添加后台用户
        [HttpGet]
        public ActionResult Add()
        {
            ViewBag.TitleBar = "新增后台用户";
            return View();
        }

        //添加后台用户操作
        [HttpPost]
        public ActionResult Add(Administrators entity)
        {
            //实现新增用户功能
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                dc.Administrators.Add(entity);
                if (dc.SaveChanges() > 0)
                {
                    return Content("<script>alert('新增用户成功！');window.location.href='/Admin/Index'</script>");
                }
                else
                {
                    return Content("<script>alert('新增用户失败！');window.location.href='/Admin/Add'</script>");
                }
            }
        }

        //后台用户删除，没有判断删除的是否是当前登录的用户
        public ActionResult Delete(string id)
        {
            //1、检查参数id的合法性  string -> int
            int userId;
            if (!int.TryParse(id, out userId))
            {
                return Content("<script>alert('参数不合法');window.location.href='/Users/Index'</script>");
            }
            //2、使用EF删除用户
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //1、先查询
                Administrators entity = dc.Administrators.FirstOrDefault(u => u.AdminID == userId);
                //2、后删除
                dc.Administrators.Remove(entity);
                //3、保存回数据库中
                if (dc.SaveChanges() > 0)
                {
                    return Content("<script>alert('删除用户成功！');window.location.href='/Admin/Index'</script>");
                }
                else
                {
                    return Content("<script>alert('删除用户失败！');window.location.href='/Admin/Index'</script>");
                }
            }
        }

        //后台用户编辑
        [HttpGet]
        public ActionResult Edit(string id)
        {
            ViewBag.TitleBar = "编辑后台用户";
            //1.对参数id进行合法性验证
            int userid;
            if (!int.TryParse(id, out userid))
            {
                return Content("<script>alert('参数不合法');window.location.href='/Admin/Index'</script>");
            }
            //2.查询该id对应的用户信息
            using (ShoppingEntities dc = new ShoppingEntities())
            {
                Administrators entity = dc.Administrators.FirstOrDefault(u => u.AdminID == userid);
                if (entity != null)//查询到该用户
                {
                    return View(entity);
                }
                else//查询不到该用户
                {
                    return Redirect("/Admin/Index");
                }
            }
        }

        //后台用户编辑操作
        [HttpPost]
        public ActionResult Edit(Administrators entity)
        {

            using (ShoppingEntities dc = new ShoppingEntities())
            {
                //获取entity实体的状态
                var entry = dc.Entry(entity);

                //修改entity实体的状态
                //entry.State = System.Data.EntityState.Unchanged;
                //指定修改的属性
                //entry.Property("AdminName").IsModified = true;
                //entry.Property("AdminPwd").IsModified = true;
                
                dc.Entry<Administrators>(entity).State = EntityState.Modified;


                try
                {
                    //保存会数据库中
                    if (dc.SaveChanges() > 0)
                    {
                        return Content("<script>alert('修改成功');window.location.href='/Admin/Index'</script>");
                    }
                    else
                    {
                        return Content("<script>alert('修改失败');window.location.href='/Admin/Index'</script>");
                    }
                }
                catch (Exception e) 
                {
                    String str = e.Message;
                    return Content("<script>alert('" + str + "');window.location.href='/Admin/Index'</script>");

                }
            }
        }



        //Get: 返回登录页
        public ActionResult Login()
        {
            //判断是否有登录过，或者有保存cookie
            if (IsLogined()) return RedirectToAction("Index", "Admin");
            return View();
        }

        //Post: 登录判断
        /// <summary>
        /// 登录判断
        /// </summary>
        /// <param name="fc"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            string userName = fc["username"];
            string userPwd = fc["userpwd"];
            string vcode = fc["vcode"];
            string isremember = fc["isremember"];
            //string isremember = fc["isremember"];  //如果勾选了复选框，那么isremember的值为on；如果没有勾选，那么isremember的值为null

            //2、检查输入的有效性
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userPwd) || string.IsNullOrEmpty(vcode))
            {
                return Content("<script>alert('请输入所有数据!');window.location.href='/Admin/Login'</script>");
            }

            //3、判断Session是否已经过期
            if (Session["ValidateCode"] == null)
            {
                return Content("<script>alert('验证码已过期!');window.location.href='/Admin/Login'</script>");
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
                    Administrators model = dc.Administrators.FirstOrDefault(a => a.AdminName.Equals(userName)
                                                                                && a.AdminPwd.Equals(userPwd));
                    {
                        //判断用户是否存在
                        if (model != null)
                        {
                            //5、实现免登录功能
                            if (!string.IsNullOrEmpty(isremember))   //勾选了复选框
                            {
                                //将登录信息保存到Cookie中
                                HttpCookie cookie = new HttpCookie("adminIsremember", model.AdminName);
                                cookie.Expires = DateTime.Now.AddDays(10);
                                Response.Cookies.Add(cookie);
                            }
                            //6、将用户名字存储到Session中
                            Session["AdminUser"] = model.AdminName;

                            //判断是否用户类型：
                            return Content("<script>alert('登录成功!');window.location.href='/Admin/Index'</script>");
                        }
                        else
                        {
                            return Content("<script>alert('用户名或密码错误!');window.location.href='/Admin/Login'</script>");
                        }
                    }
                }
            }
            else
            {
                return Content("<script>alert('验证码输入错误!');window.location.href='/Admin/Login'</script>");
            }
        }

        public ActionResult Logout()
        {
            //清除Session
            Session.Abandon();
            //清除Cookie,不是删除，而是替换
            Response.Cookies["adminIsremember"].Expires = DateTime.Now.AddDays(-1);  //Cookie立刻失效
            //跳转到登录页
            return RedirectToAction("Login", "Admin");

        }

        /// <summary>
        /// 返回验证码图片
        /// </summary>
        /// <returns></returns>
        public ActionResult ValidateCode()
        {
            Dictionary<String, Object> dic = CodeUtil.GenerateCodeAndPic(4);
            Session["ValidateCode"] = dic["code"];
            //返回
            return File((byte[])dic["pic"], "image/jpeg");
        }

        /// <summary>
        /// 判断用户是否登录了
        /// </summary>
        /// <returns></returns>
        public Boolean IsLogined()
        {
            //判断是否有 cookie
            if (Request.Cookies["adminIsremember"] != null)
            {
                //将Cookie中保存的用户名读取出来，存储到Session中
                Session["AdminUser"] = Request.Cookies["AdminIsremember"].Value;
                return true;
            }

            // 判断是否Session
            if (Session["AdminUser"] != null) return true;
            return false;
        }
    }
}
