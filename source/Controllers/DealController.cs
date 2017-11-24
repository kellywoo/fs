using mtopt.logic;
using mtopt.logic.Extend_;
using mtopt.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace mtopt.deal.web.Controllers
{
    public class DealController : Controller
    {
        #region 交易列表
        /// <summary>
        /// 交易列表
        /// </summary>
        /// <returns></returns>
        public ActionResult list(int? type, int? state, string timestart, string timeend, int? page)
        {
            Basis.VerifyLogin(0);

            ViewBag.Page = page == null ? 0 : page.Value;
            ViewBag.Size = 15;
            ViewBag.Type = type == null ? -1 : type.Value;
            ViewBag.State = state == null ? -1 : state.Value;
            ViewBag.TimeStart = timestart != null ? timestart : "";
            ViewBag.TimeEnd = timeend != null ? timeend : "";
            short Type = (short)ViewBag.Type;
            short State = (short)ViewBag.State;
            DateTime? TimeStart = null;
            DateTime? TimeEnd = null;
            if (ViewBag.TimeStart.Length > 0) { TimeStart = Convert.ToDateTime(timestart); }
            if (ViewBag.TimeEnd.Length > 0) { TimeEnd = Convert.ToDateTime(timeend).AddDays(1); }
            //获取信息
            ViewBag.Deals = Business.MemberDeal.Gets(Type, State, Basis.Session.Uid, null, TimeStart, TimeEnd, ViewBag.Page, ViewBag.Size);
            ViewBag.Count = Business.MemberDeal.Count(Type, State, Basis.Session.Uid, null, TimeStart, TimeEnd);

            return View();
        }
        #endregion

        #region 交易详情
        /// <summary>
        /// 交易详情
        /// </summary>
        /// <param name="dealid"></param>
        /// <returns></returns>
        public ActionResult detail(long dealid)
        {
            Basis.VerifyLogin(0);

            ViewBag.Deal = Business.MemberDeal.Get(Basis.Session.Uid, dealid);
            return View();
        }
        #endregion

        #region 充值
        /// <summary>
        /// 充值
        /// </summary>
        /// <returns></returns>
        public ActionResult deposit()
        {
            Basis.VerifyLogin(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);
            ViewBag.BankAccounts = Business.BankAccount.Gets();

            return View();
        }
        #endregion

        #region 提现
        /// <summary>
        /// 提现
        /// </summary>
        /// <returns></returns>
        public ActionResult withdraw()
        {
            Basis.VerifyLogin(0);
            Basis.VerifyReal(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);

            return View();
        }
        #endregion

        #region 转账
        /// <summary>
        /// 转账
        /// </summary>
        /// <returns></returns>
        public ActionResult transfer()
        {
            Basis.VerifyLogin(0);
            Basis.VerifyReal(0);

            ViewBag.Member = Business.Member.Get(Basis.Session.Uid);

            if (Convert.ToBoolean(Config.GetDeploy("config.member.transfer")) == false)
            {
                return Redirect("~/");
            }
            else
            {
                return View();
            }
        }
        #endregion

        #region 支付宝充值
        /// <summary>
        /// 支付宝充值
        /// </summary>
        /// <param name="money"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public ActionResult alipaypay(decimal money, string remarks)
        {
            Basis.VerifyLogin(0);
            //创建充值申请
            mtopt.model.EntitySales.member Memer = Business.Member.Get(Basis.Session.Uid);
            long DealId = Business.MemberDeal.InsertDeposit(Basis.Session.Uid, Basis.Parse.ToLanguage("支付宝充值"), "", "", "", "", "", remarks, money, Em_CurrencyType.Money);

            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", "alipay.wap.create.direct.pay.by.user");
            sParaTemp.Add("partner", Config.GetDeploy("config.alipay.pay.partner"));
            sParaTemp.Add("seller_id", Config.GetDeploy("config.alipay.pay.sellerid"));
            sParaTemp.Add("_input_charset", "utf-8");
            sParaTemp.Add("payment_type", "1");
            sParaTemp.Add("notify_url", Config.GetDeploy("config.url.mall.wap").Trim() + "/Service/paynotice_alipay.ashx");
            sParaTemp.Add("return_url", Config.GetDeploy("config.url.mall.wap").Trim() + "/Shared/payok?dealid=" + DealId);
            sParaTemp.Add("anti_phishing_key", "");
            sParaTemp.Add("exter_invoke_ip", Config.GetDeploy("config.alipay.pay.ip"));
            sParaTemp.Add("out_trade_no", "0001" + DealId.ToString());
            sParaTemp.Add("subject", Basis.Parse.ToLanguage("支付宝充值"));
            sParaTemp.Add("total_fee", money.ToString("0.00"));
            sParaTemp.Add("show_url", Config.GetDeploy("config.url.sales.wap").Trim());
            sParaTemp.Add("body", remarks);
            //建立请求
            ViewBag.Content = AlipaySubmit.BuildRequest(sParaTemp, "get", Basis.Parse.ToLanguage("确认"));
            return View();
        }
        #endregion

        #region 易通支付
        /// <summary>
        /// 易通支付
        /// </summary>
        /// <param name="money"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public ActionResult yitongpay(decimal money, string remarks)
        {
            Basis.VerifyLogin(0);
            //创建充值申请
            mtopt.model.EntitySales.member Memer = Business.Member.Get(Basis.Session.Uid);
            long DealId = Business.MemberDeal.InsertDeposit(Basis.Session.Uid, Basis.Parse.ToLanguage("在线支付"), "", "", "", "", "", remarks, money, Em_CurrencyType.Money);
            //准备必要参数
            string version = "1.0.0";
            string transCode = "8888";
            string merchantId = Config.GetDeploy("config.yitong.paymerchant");//商户编号
            string merOrderNum = DealId.ToString();
            string bussId = "888453";//业务代码
            string tranAmt = (money * 100).ToString("0");
            string sysTraceNum = DealId.ToString();
            string tranDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");
            string currencyType = "156";
            string merUrl = Config.GetDeploy("config.url.deal.web").Trim() + "/Shared/payok?dealid=" + DealId;
            string backUrl = Config.GetDeploy("config.url.deal.web").Trim() + "/Service/paynotice_yitong.ashx";
            string orderInfo = "";
            string userId = "";
            string datakey = Config.GetDeploy("config.yitong.datakey");
            string StringtxnString = version + "|" + transCode + "|" + merchantId + "|" + merOrderNum + "|" + bussId + "|" + tranAmt + "|" + sysTraceNum + "|" + tranDateTime + "|" + currencyType + "|" + merUrl + "|" + backUrl + "|" + orderInfo + "|" + userId;
            string StringsignValue = rte.Encrypt.MD5.Encrypt(StringtxnString + datakey).ToLower();

            ViewBag.PostUrl = string.Format("{0}/NetPay/BankSelect.action?version={1}&transCode={2}&merchantId={3}&merOrderNum={4}&bussId={5}&tranAmt={6}&sysTraceNum={7}&tranDateTime={8}&currencyType={9}&merURL={10}&orderInfo=&bankId=&stlmId=&userId=&userIp=&backURL={11}&reserver1=&reserver2=&reserver3=&reserver4=&entryType=1&signValue={12}",
                 Config.GetDeploy("config.yitong.payhost"), version, transCode, merchantId, merOrderNum, bussId, tranAmt, sysTraceNum, tranDateTime, currencyType, rte.Encrypt.URL.Encode(merUrl), rte.Encrypt.URL.Encode(backUrl), StringsignValue);

            return View();
        }
        #endregion

        /***********************************公共方法***********************************/

        #region 充值申请
        /// <summary>
        /// 充值申请
        /// </summary>
        /// <param name="cardtype"></param>
        /// <param name="cardno"></param>
        /// <param name="name"></param>
        /// <param name="time"></param>
        /// <param name="money"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public JsonResult api_deposit_submit(string cardtype, string cardno, string name, string time, decimal money, string remarks)
        {
            Basis.VerifyLogin(0);

            //数据是否正确
            if (cardtype.Length <= 0 || cardno.Length <= 0 || name.Length <= 0 || time.Length <= 0 || money <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = "输入不正确" }, JsonRequestBehavior.AllowGet);
            }
            if (Basis.AstrictVisit("deal_api_deposit_submit", 3) == true)
            {
                return Json(new { Success = false, Error = -1, Data = "操作频繁，请稍后再试..." }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                //提交汇款申请
                long DealId = Business.MemberDeal.InsertDeposit(
                    Basis.Session.Uid,
                    string.Format("{0}[{1}]", cardtype, cardno),
                    cardtype,
                        cardno,
                        name,
                        "",
                        "",
                    remarks,
                    money,
                    Em_CurrencyType.Money
                );
                if (DealId > 0)
                {
                    return Json(new { Success = true, Error = 0, Data = DealId }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, Error = -10000, Data = "申请失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 提现申请
        /// <summary>
        /// 提现申请
        /// </summary>
        /// <param name="bankid"></param>
        /// <param name="bankno"></param>
        /// <param name="bankuser"></param>
        /// <param name="bankname"></param>
        /// <param name="money"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public JsonResult api_withdraw_submit(short bankid, string bankno, string bankuser, string bankname,
            decimal money, string remarks)
        {
            Basis.VerifyLogin(0);
            Basis.VerifyReal(0);

            if (Basis.AstrictVisit("deal_api_withdraw_submit", 3) == true)
            {
                return Json(new { Success = false, Error = -1, Data = "操作频繁，请稍后再试..." }, JsonRequestBehavior.AllowGet);
            }
            //数据是否正确
            if (bankid <= 0 || bankno.Length <= 0 || bankuser.Length <= 0 || bankname.Length <= 0|| money <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = "输入不正确" }, JsonRequestBehavior.AllowGet);
            }
            string bankidname = Basis.Sales_ADO.DataBase.GetTable("select * from bank where id = " + bankid).Rows[0]["name"].ToString();
            //提交提现申请
            try
            {
                long DealId = Business.MemberDeal.InsertWithdraw(
                       Basis.Session.Uid,
                       string.Format("{0}[{1}]", bankidname, bankno),
                       bankidname,
                       bankno,
                       bankuser,
                       bankname,
                       "",
                       remarks,
                       money,
                       Em_CurrencyType.Money);
                if (DealId > 0)
                {
                    return Json(new { Success = true, Error = 0, Data = DealId }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, Error = -10000, Data = "申请失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 内部转账
        /// <summary>
        /// 内部转账
        /// </summary>
        /// <param name="memberid"></param>
        /// <param name="money"></param>
        /// <param name="type"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public JsonResult api_transfer_submit(string memberid, short type, decimal money, string remarks)
        {
            Basis.VerifyLogin(0);
            Basis.VerifyReal(0);

            //数据是否正确
            if (memberid.Length <= 0 || money <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = "输入不正确" }, JsonRequestBehavior.AllowGet);
            }
            if (Basis.AstrictVisit("deal_api_transfer_submit", 3) == true)
            {
                return Json(new { Success = false, Error = -1, Data = "操作频繁，请稍后再试..." }, JsonRequestBehavior.AllowGet);
            }
            //提交汇款申请
            try
            {
                long DealId = Business.MemberDeal.InsertTransfer(
                        Basis.Session.Uid,
                        memberid,
                        string.Format("收款人编号：{0}", memberid),
                        string.Format("【收款人编号：{0}】", memberid),
                        remarks,
                        money,
                        type == 1 ? Em_CurrencyType.Money : Em_CurrencyType.Inside
                    );
                if (DealId > 0)
                {
                    return Json(new { Success = true, Error = 0, Data = DealId }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, Error = -10000, Data = "申请失败" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}
