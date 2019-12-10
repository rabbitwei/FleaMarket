using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FleaMarket.Models
{
    public class UsersView
    {
        //[DisplayName("用户编号")]
        //[Required(ErrorMessage = "{0}不能为空")]
        //public int UserID { get; set; }

        [DisplayName("用户账号")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Remote("CheckUserAccount", "Users", ErrorMessage = "{0}账号已存在，请更换")]
        public string UserAccount { get; set; }

        [DisplayName("用户昵称")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string UserName { get; set; }

        [DisplayName("用户密码")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string UserPwd { get; set; }

        [DisplayName("用户性别")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string UserGender { get; set; }

        [DisplayName("用户地址")]
        [StringLength(50, ErrorMessage = "{0}不能超过{1}")]
        public string UserAddress { get; set; }

        [DisplayName("用户简介")]
        [StringLength(50, ErrorMessage = "{0}不能超过{1}")]
        public string UserIntro { get; set; }

        [DisplayName("用户电话")]
        [Required(ErrorMessage = "{0}不能为空")]
        [RegularExpression(@"^1[3458][0-9]{9}$", ErrorMessage = "手机号格式不正确")]
        public string UserPhone { get; set; }

        [DisplayName("用户QQ")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string UserQQ { get; set; }

        [DisplayName("用户头像")]
        [Required(ErrorMessage = "{0}不能为空")]
        public HttpPostedFileBase UserIcon { get; set; }
    }
}