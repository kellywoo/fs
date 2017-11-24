using mtopt.logic;
using mtopt.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mtopt.deal.web.Controllers
{
    public class HomeController : Controller
    {
        #region 系统首页
        /// <summary>
        /// 系统首页
        /// </summary>
        /// <returns></returns>
        public ActionResult main()
        {
            ViewBag.StockmarketTypes = Business.StockMarketTrend.GetTypes(0);
            ViewBag.News = Business.News.GetBills(3, 8);

            return View();
        }
        #endregion

        #region APP下载
        /// <summary>
        /// APP下载
        /// </summary>
        /// <returns></returns>
        public ActionResult app()
        {
            return View();
        }
        #endregion

        #region 文件下载
        /// <summary>
        /// 文件下载
        /// </summary>
        /// <returns></returns>
        public ActionResult download()
        {
            return View();
        }
        #endregion
    }
}