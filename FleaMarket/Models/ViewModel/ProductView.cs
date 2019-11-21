using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FleaMarket.Models.ViewModel;

namespace FleaMarket.Models
{
    public class ProductView
    {
        public string ProID { get; set; }

        [DisplayName("商品标题")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string ProName { get; set; }

        [DisplayName("商品新旧程度")]
        [Required(ErrorMessage = "{0}不能为空")]
        public int ProLevel { get; set; }

        [DisplayName("商品原价格")]
        [Required(ErrorMessage = "{0}不能为空")]
        [Range(typeof(decimal), "0.00", "9999.99")]
        public int ProOldPrice { get; set; }

        [DisplayName("商品现价格")]
        [Required(ErrorMessage = "{0}不能为空")]
        public int ProNewPrice { get; set; }

        [DisplayName("商品类别")]
        [Required(ErrorMessage = "{0}不能为空")]
        public int ProCateg { get; set; }

        [DisplayName("商品信息")]
        [Required(ErrorMessage = "请填写您的{0}")]
        public string ProIntro { get; set; }

        [DisplayName("商品图片")]
        [Required(ErrorMessage = "请上传您的{0}")]
        //[ValidateFile]
        public HttpPostedFileBase ProImgUrl { get; set; }
    }
}