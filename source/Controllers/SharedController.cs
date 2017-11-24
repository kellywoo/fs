using mtopt.logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mtopt.deal.web.Controllers
{
    public class SharedController : Controller
    {
        #region 支付成功
        /// <summary>
        /// 支付成功
        /// </summary>
        /// <param name="money"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public ActionResult payok(long dealid)
        {
            ViewBag.Deal = mtopt.logic.Business.MemberDeal.Get(dealid);

            return View();
        }
        #endregion

        #region 页面错误
        public ActionResult error()
        {
            return View();
        }
        public ActionResult error502()
        {
            return View();
        }
        public ActionResult error500()
        {
            return View();
        }
        public ActionResult error405()
        {
            return View();
        }
        public ActionResult error403()
        {
            return View();
        }
        public ActionResult error404()
        {
            return View();
        }
        #endregion

        #region 系统维护
        /// <summary>
        /// 维护信息
        /// </summary>
        /// <returns></returns>
        [OutputCache(Duration = 3600)]
        public ActionResult maintain()
        {
            return View();
        }
        #endregion

        #region 地图详情
        /// <summary>
        /// 地图详情
        /// </summary>
        /// <returns></returns>
        public ActionResult map()
        {
            return View();
        }
        #endregion


        #region 聊天窗口
        /// <summary>
        /// 聊天窗口
        /// </summary>
        /// <returns></returns>
        public ActionResult chat(string collid)
        {
            Basis.VerifyLogin(0);

            ViewBag.SendMember = Business.Member.Get(Basis.Session.Uid);
            ViewBag.CollMember = Business.Member.Get(collid);

            return View();
        }
        #endregion

        /***********************************公共方法***********************************/

        #region 发送聊天
        /// <summary>
        /// 发送聊天
        /// </summary>
        /// <param name="sendid"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public JsonResult api_chat_submit(short Head, string Branch, string collid, string content)
        {
            Basis.VerifyLogin(0);
            try
            {
                Basis.NotifyService.NoticeP2P(Branch, Head, Basis.Session.Uid, collid, content);

                return Json(new { Success = true, Error = 0, Data = "发送成功" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = -10000, Data = "发送失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}