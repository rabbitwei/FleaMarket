/*
 *CLR版本:      4.0.30319.42000
 *机器名称:     RABBITWEI
 *文件名:        CodeUtil
 *命名空间:     FleaMarket.Util
 *创建时间:     2019/11/16 20:28:08
 *创建人:        DIWEI
 *描述: 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.IO;

namespace FleaMarket.Util
{
    public class CodeUtil
    {
        /// <summary>
        /// 生出验证码和验证码图片
        /// </summary>
        /// <param name="count">验证码的字符数</param>
        /// <returns>Dictionary 集合，包含验证码和字节流</returns>
        public static Dictionary<String, Object> GenerateCodeAndPic(int count)
        {
            Object code = GenerateCode(count);
            Object picImg = GeneratePic();
            Dictionary<String, Object> dir = new Dictionary<String, Object>();
            dir.Add("code", code);
            dir.Add("pic", picImg);
            return dir;
        }

        /// <summary>
        /// 生成验证码的字节流
        /// </summary>
        /// <returns></returns>
        private static Object GeneratePic()
        {
            //调用方法,生成随机验证码字符串
            string code = GenerateCode(4);

            //定义画布的宽度和高度
            int width = code.Length * 12;
            int height = 20;
            //使用GDI  Graphics Device Interface 图形设备接口
            //创建一个Image图片对象
            using (Image img = new Bitmap(width, height))
            {
                //创建画布
                using (Graphics g = Graphics.FromImage(img))
                {
                    //将背景色清空为白色，默认黑色
                    g.Clear(Color.White);
                    //绘制矩形的边框
                    g.DrawRectangle(Pens.Red, 0, 0, width - 1, height - 1);
                    //定义字体对象
                    Font f = new Font("黑体", 14, FontStyle.Bold);
                    //绘制验证码字符串
                    g.DrawString(code, f, Brushes.Blue, 0, 0);

                }
                //创建内存流
                MemoryStream stream = new MemoryStream();
                //保存回图片
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                //转换为二进制
                byte[] bytes = stream.ToArray();
                //返回
                return bytes;
            }
        }

        /// <summary>
        /// 生成验证码
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private static String GenerateCode(int count)
        {
            //声明一个空的验证码字符串
            string code = string.Empty;
            //备选的验证码字符串
            string str = "0123456789ABCDEFGHIJKLMNPQRSTUVWXYZabcdefghijklmnpqrstuvwxyz";
            int length = str.Length;
            //创建随机数对象
            Random r = new Random();
            //循环遍历
            for (int i = 0; i < count; i++)
            {
                int index = r.Next(length);  //获取随机的索引值0~length-1
                //获取该索引的字符，添加到验证码字符串中
                code += str[index];
            }
            //返回
            return code;
        }

    }
}