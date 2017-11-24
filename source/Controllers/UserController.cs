using mtopt.logic;
using mtopt.model;
using mtopt.rte.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace mtopt.deal.web.Controllers
{
    public class UserController : Controller
    {
        #region 登录系统
        /// <summary>
        /// 登录系统
        /// </summary>
        /// <returns></returns>
        public ActionResult login()
        {
            return View();
        }
        #endregion

        #region 实名认证
        /// <summary>
        /// 实名认证
        /// </summary>
        /// <returns></returns>
        public ActionResult real()
        {
            Basis.VerifyLogin(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);

            return View();
        }
        #endregion

        #region 注册账户
        /// <summary>
        /// 登录系统
        /// </summary>
        /// <returns></returns>
        public ActionResult register(string recomid)
        {
            ViewBag.RecomId = recomid;

            return View();
        }
        public ActionResult register_emil(string recomid)
        {
            ViewBag.RecomId = recomid;
            return View();
        }
        #endregion

        #region SKEY登录
        /// <summary>
        /// SKEY登录
        /// </summary>
        /// <param name="skey"></param>
        /// <returns></returns>
        public RedirectResult skey(string skey)
        {
            if (skey == null || skey.Length <= 0)
            {
                return Redirect("~/user/login");
            }
            else
            {
                if (Business.Member.SKeyLogin(skey) == true)
                {
                    return Redirect("~/");
                }
                else
                {
                    return Redirect("~/user/login");
                }
            }
        }
        #endregion

        #region 找回密码
        /// <summary>
        /// 登录系统
        /// </summary>
        /// <returns></returns>
        public ActionResult find()
        {
            return View();
        }
        #endregion

        #region 账户信息
        /// <summary>
        /// 账户信息
        /// </summary>
        /// <returns></returns>
        public ActionResult basicinfo()
        {
            Basis.VerifyLogin(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);
            ViewBag.Accounts = Business.StockMarket.GetAccounts(Basis.Session.Uid);

            return View();
        }
        #endregion

        #region 修改资料
        /// <summary>
        /// 修改资料
        /// </summary>
        /// <returns></returns>
        public ActionResult basicedit()
        {
            Basis.VerifyLogin(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);
            return View();
        }
        #endregion

        #region 修改头像
        /// <summary>
        /// 修改资料
        /// </summary>
        /// <returns></returns>
        public ActionResult basicface()
        {
            Basis.VerifyLogin(0);
            

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);

            return View();
        }
        #endregion

        #region 修改登录密码
        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <returns></returns>
        public ActionResult wordedit()
        {
            Basis.VerifyLogin(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);

            return View();
        }
        #endregion


        /**********************************外部接口************************************/

        #region 验证码发送-任意会员
        /// <summary>
        /// 验证码发送-任意会员
        /// </summary>
        /// <returns></returns>
        public JsonResult everyonesend(string moblie)
        {
            if (Basis.Verify.IsSendCheckCode(moblie) == true)
            {
                AsyncManager.OutstandingOperations.Increment(1);
                Task.Factory.StartNew(() =>
                {
                    Basis.Verify.SendToPhone("",moblie);
                    AsyncManager.OutstandingOperations.Decrement();
                });
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("发送成功") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("发送失败") }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 邮件验证码
        /// </summary>
        /// <param name="moblie">手机号</param>
        /// <returns></returns>
        public JsonResult everyonesend_Emil(string moblie)
        {
            if (Basis.Verify.IsSendCheckCode(moblie) == true)
            {
                AsyncManager.OutstandingOperations.Increment(1);
                Task.Factory.StartNew(() =>
                {
                    Basis.Verify.SendToEmil(moblie);
                    AsyncManager.OutstandingOperations.Decrement();
                });
                return Json(new { Success = true, Error = 0, Data = "发送成功" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = -1, Data = "发送失败" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 验证码发送-指定会员
        /// <summary>
        /// 验证码发送-指定会员
        /// </summary>
        /// <returns></returns>
        public JsonResult fixedsend(string memberid)
        {
            model.EntitySales.member Member = Business.Member.Get(memberid);
            if (Member == null)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("发送失败") }, JsonRequestBehavior.AllowGet);
            }
            if (Basis.Verify.IsSendCheckCode(Member.moblie) == true)
            {
                AsyncManager.OutstandingOperations.Increment(1);
                Task.Factory.StartNew(() =>
                {
                    Basis.Verify.SendToPhone(Member.id , Member.moblie);
                    AsyncManager.OutstandingOperations.Decrement();
                });
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("发送成功") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("发送失败") }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 实名认证
        /// <summary>
        /// 实名认证
        /// </summary>
        /// <param name="idcard"></param>
        /// <param name="checkcode"></param>
        /// <param name="username"></param>
        /// <param name="idcardz"></param>
        /// <param name="idcardf"></param>
        /// <returns></returns>
        public JsonResult api_real_submit(string idcard, string username,string idcardf,string idcardz)
        {
            try
            {
                MemoryStream idcardzs = new MemoryStream(Basis.Parse.GetBase64File(idcardz));
                MemoryStream idcardfs = new MemoryStream(Basis.Parse.GetBase64File(idcardf));
                Basis.Gatekeeper.FileDefense(idcardzs);
                Basis.Gatekeeper.FileDefense(idcardfs);
                //进行验证
                model.EntitySales.member Member = Business.Member.Get(Basis.Session.Uid);
                //if (!Basis.Verify.EqualCheckCode(Member.moblie, checkcode))
                //{
                //    return Json(new { Success = true, Error = -1, Data = "验证码错误" }, JsonRequestBehavior.AllowGet);
                //}
                string idcardz_key = Basis.ContentDB.Insert(idcardzs, Basis.Session.Uid.ToString());
                string idcardf_key = Basis.ContentDB.Insert(idcardfs, Basis.Session.Uid.ToString());
                if (Business.MemberReal.Submit(Basis.Session.Uid, username, idcard, idcardz_key, idcardf_key))
                {
                    return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("申请成功") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = true, Error = -5000, Data = Basis.Parse.ToLanguage("申请失败") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 修改头像
        /// <summary>
        /// 修改头像
        /// </summary>
        /// <returns></returns>
        public RedirectResult api_basicface_submit()
        {
            HttpResult Result = new HttpResult();

            if (Request.Files.Count == 0 || Request.Files[0].InputStream.Length <= 0)
            {
                Result = HttpResult.GetResult(-1, Basis.Parse.ToLanguage("请选择上传图片"));
            }
            else
            {
                Stream file = Request.Files[0].InputStream;
                Basis.Gatekeeper.FileDefense(file);//安全检测
                string filekey = Basis.ContentDB.Insert(file, Basis.Session.Uid.ToString());
                //进行设置
                if (Business.Member.Update_Head(Basis.Session.Uid, filekey))
                {
                    Result = HttpResult.GetResult(0, Basis.Parse.ToLanguage("确定成功"));
                }
                else
                {
                    Result = HttpResult.GetResult(-10001, Basis.Parse.ToLanguage("确定失败"));
                }
            }

            return Redirect("~/User/basicface?data=" + new ConvertJson<HttpResult>().wrapToString(Result));
        }
        #endregion

        #region 找回密码
        /// <summary>
        /// 找回密码
        /// </summary>
        /// <param name="moblie"></param>
        /// <param name="id"></param>
        /// <param name="recomid"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public JsonResult api_find_submit(string memberid, string password, string checkcode)
        {
            model.EntitySales.member Member = Business.Member.Get(memberid);
            if (Member == null)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("用户不存在") }, JsonRequestBehavior.AllowGet);
            }
            if (!Basis.Verify.EqualCheckCode(Member.moblie, checkcode))
            {
                return Json(new { Success = false, Error = -10005, Data = Basis.Parse.ToLanguage("验证码错误") }, JsonRequestBehavior.AllowGet);
            }
            //数据是否正确
            if (password.Length <= 0 || memberid.Length <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                Business.Member.Update_PassWord(Member.id, password);

                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("新增成功") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 注册会员
        /// <summary>
        /// 注册会员
        /// </summary>
        /// <param name="moblie"></param>
        /// <param name="id"></param>
        /// <param name="recomid"></param>
        /// <param name="password"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public JsonResult api_register_submit(string email, string password, string recomid, string checkcode)
        {
            if (email.Length <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
            }

            if (!Basis.IsPasswordValid(password))
            {
                return Json(new { Success = false, Error = -10003, Data = Basis.Parse.Localization("insufficient_password_strength") }, JsonRequestBehavior.AllowGet);
            }

            if (!Basis.Verify.EqualCheckCode(email, checkcode))
            {
                return Json(new { Success = false, Error = -1, Data = "验证码错误" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                if (Business.Member.Insert(email, email, recomid, recomid, null, 1, 1, 1, password, email, null, null, null, null, null,
                    null, email, null, null, null, null, null, "", false) == true)
                {
                    return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("新增成功") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("新增失败") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 用户登录
        /// <summary>
        /// 用户登录
        /// </summary>
        /// <param name="memberid"></param>
        /// <param name="password"></param>
        /// <param name="checkcode"></param>
        /// <returns></returns>
        public JsonResult api_login_submit(string memberid, string password)
        {
            if (memberid.Length <= 0 || password.Length <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                if (Business.Member.WordLogin(memberid, password) == true)
                {
                    return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("登录成功") }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, Error = -10001, Data = Basis.Parse.ToLanguage("登录失败") }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) { return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet); }
        }
        #endregion

        #region 退出登录
        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        public RedirectResult api_cancel()
        {
            Basis.VerifyLogin(0);

            Basis.Session.IsLogin = false;
            Basis.Session.Uid = "";
            Basis.Session.Utp = 1;
            Basis.Session.VerifyAccount.IsAuthentication = false;

            return Redirect("~/");
        }
        #endregion

        #region 修改资料
        /// <summary>
        /// 修改资料
        /// </summary>
        /// <param name="bankid"></param>
        /// <param name="bankno"></param>
        /// <param name="bankname"></param>
        /// <param name="email"></param>
        /// <param name="zipcode"></param>
        /// <param name="address"></param>
        /// <param name="place"></param>
        /// <param name="checkcode"></param>
        /// <returns></returns>
        public JsonResult api_basicedit_submit(short bankid, string bankno, string bankname, string email)

        {
            Basis.VerifyLogin(0);
            //数据是否正确
            if (bankid <= 0 || bankname.Length <= 0 || bankno.Length <= 0 || email.Length <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
            }
            if (Business.Member.Update(Basis.Session.Uid, bankid, bankno, bankname, email, "", "", "", null, null) == true)
            {
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("修改成功") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("修改失败") }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 修改登录密码
        /// <summary>
        /// 修改登录密码
        /// </summary>
        /// <param name="npassword"></param>
        /// <returns></returns>
        public JsonResult api_wordedit_submit(string checkcode, string npassword)
        {
            Basis.VerifyLogin(0);
            if (Basis.AstrictVisit("api_wordedit_submit", 3) == true)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("操作频繁，请稍后再试...") }, JsonRequestBehavior.AllowGet);
            }

            //数据是否正确
            model.EntitySales.member Member = Business.Member.Get(Basis.Session.Uid);
            if (!Basis.Verify.EqualCheckCode(Member.moblie, checkcode))
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("验证码错误") }, JsonRequestBehavior.AllowGet);
            }
            if (Business.Member.Update_PassWord(Basis.Session.Uid, npassword) == true)
            {
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("修改成功") }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("修改失败") }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}