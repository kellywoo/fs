using mtopt.logic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mtopt.deal.web.Controllers
{
    public class NoticeController : Controller
    {
        #region 帮助列表
        /// <summary>
        /// 帮助列表
        /// </summary>
        /// <param name="classify">分类</param>
        /// <param name="page">页码</param>      
        /// <returns></returns>
        public ActionResult list(int? classify, int? page)
        {
            ViewBag.Page = page == null ? 0 : page.Value;
            ViewBag.Size = 20;
            int ClassifyId = classify == null ? -1 : classify.Value;
            //获取信息
            ViewBag.News = Business.News.GetNews(3, ClassifyId, ViewBag.Page, ViewBag.Size);
            ViewBag.ClassifyId = ClassifyId;
            ViewBag.Count = Business.News.NewCount(3, ClassifyId);

            return View();
        }
        #endregion

        #region 新闻详情
        /// <summary>
        /// 新闻详情
        /// </summary>
        /// <param name="newsid"></param>
        /// <returns></returns>
        public ActionResult detail(long? newsid, string codeid)
        {
            //获取数据
            if (newsid != null)
            {
                ViewBag.New = Business.News.Get(newsid.Value);
            }
            else
            {
                ViewBag.New = Business.News.Get(codeid);
            }
            return View();
        }
        #endregion
    }
}
