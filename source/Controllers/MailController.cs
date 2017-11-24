using mtopt.logic;
using mtopt.model;
using mtopt.rte.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mtopt.deal.web.Controllers
{
    public class MailController : Controller
    {
        #region 发表工单
        /// <summary>
        /// 发表工单
        /// </summary>
        /// <returns></returns>
        public ActionResult message()
        {
            Basis.VerifyLogin(0);

            return View();
        }
        #endregion

        #region 工单
        /// <summary>
        /// 工单
        /// </summary>
        /// <returns></returns>
        public ActionResult messagelists(int? Page)
        {
            Basis.VerifyLogin(0);

            ViewBag.Page = Page != null ? Page.Value : 0;
            ViewBag.Size = 20;
            ViewBag.Messages = Business.Message.Gets(Basis.Session.Uid, ViewBag.Page, ViewBag.Size);
            ViewBag.Count = Business.Message.Count(Basis.Session.Uid);
            return View();
        }
        #endregion

        #region 工单详情
        /// <summary>
        /// 工单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult messagedetail(int id)
        {
            Basis.VerifyLogin(0);
            ViewBag.Message = Business.Message.Get(Basis.Session.Uid, id);
            return View();
        }
        #endregion

        /**********************************外部接口************************************/

        #region 发表工单接口
        /// <summary>
        /// 发送邮件接口
        /// </summary>
        /// <returns></returns>
        public RedirectResult api_message_submit()
        {
            HttpResult Result = new HttpResult();

            short type = Convert.ToInt16(Request.Form["type"].ToString());
            string title = Request.Form["title"].ToString();
            string content = rte.Encrypt.HTML.Decode(Request.Form["content"].ToString());

            if (title.Length <= 0 || content.Length <= 0)
            {
                Result = HttpResult.GetResult(-10003, Basis.Parse.ToLanguage("输入不正确"));
            }
            else
            {
                if (Business.Message.Insert(Basis.Session.Uid, type, title, content, 0))
                {
                    Result = HttpResult.GetResult(0, Basis.Parse.ToLanguage("发表成功"));
                }
                else
                {
                    Result = HttpResult.GetResult(-10001, Basis.Parse.ToLanguage("发表失败"));
                }
            }

            return Redirect("~/Mail/message?data=" + new ConvertJson<HttpResult>().wrapToString(Result));
        }
        #endregion

        #region 回复工单
        /// <summary>
        /// 回复工单
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public JsonResult api_messagedetail_submit(long id,string content)
        {
            if (Basis.AstrictVisit("api_messagedetail_submit", 3) == true)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("操作频繁，请稍后再试...") }, JsonRequestBehavior.AllowGet);
            }
            Basis.VerifyLogin(0);

            //数据是否正确
            if (content.Length <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
            }
            if (Business.Message.MemberTalk(id,Basis.Session.Uid,1,content,0) == true)
            {
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("回复成功") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("回复失败") }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}