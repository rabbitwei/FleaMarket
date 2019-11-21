using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FleaMarket.Models
{
    [MetadataType(typeof(UsersView))]
    public partial class Users
    {
        [DisplayName("确认密码")]
        [Required(ErrorMessage = "{0}不能为空")]
        [System.Web.Mvc.Compare("UserPwd", ErrorMessage = "两次密码不一致")]
        public string ConfirmPwd { get; set; }

    
    }
}