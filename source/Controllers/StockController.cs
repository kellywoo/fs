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
    public class StockController : Controller
    {
        #region KLINE图
        /// <summary>
        /// KLINE图
        /// </summary>
        /// <returns></returns>
        public ActionResult kline()
        {
            return View();
        }
        #endregion

        #region 交易列表
        /// <summary>
        /// 交易列表
        /// </summary>
        /// <returns></returns>
        public ActionResult list(int? order, string opera)
        {
            ViewBag.Opera = opera == null ? "main" : opera;
            ViewBag.Order = order == null ? 0 : order.Value;
            //获取信息
            ViewBag.StockmarketTypes = Business.StockMarketTrend.GetTypes(ViewBag.Order);

            return View();
        }
        #endregion

        #region 交易首页
        /// <summary>
        /// 交易首页
        /// </summary>
        /// <returns></returns>
        public ActionResult main(short? type)
        {
            if(Basis.Session.IsLogin == true)
            {
                ViewBag.Account = Business.StockMarket.GetAccount(Basis.Session.Uid, type.Value);
            }
            ViewBag.StockmarketType = Business.StockMarketTrend.GetType(type.Value);

            return View();
        }
        #endregion

        #region 虚拟币充值
        /// <summary>
        /// 虚拟币充值
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult deposit(short? type)
        {
            if (type == null)
            {
                return Redirect("~/Stock/list?opera=deposit");
            }
            Basis.VerifyLogin(0);

            ViewBag.Account = Business.StockMarket.GetAccount(Basis.Session.Uid, type.Value);
            Business.StockMarket.GetAddress(ViewBag.Account.id);//获取钱包地址
            ViewBag.Account = Business.StockMarket.GetAccount(Basis.Session.Uid, type.Value);
            ViewBag.StockmarketType = Business.StockMarketTrend.GetType(type.Value);

            return View();
        }
        #endregion

        #region 虚拟币提现
        /// <summary>
        /// 虚拟币提现
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult withdraw(short? type)
        {
            if (type == null)
            {
                return Redirect("~/Stock/list?opera=withdraw");
            }
            Basis.VerifyLogin(0);
            Basis.VerifyReal(0);

            ViewBag.Account = Business.StockMarket.GetAccount(Basis.Session.Uid, type.Value);
            ViewBag.StockmarketType = Business.StockMarketTrend.GetType(type.Value);

            return View();
        }
        #endregion

        #region 交易记录
        /// <summary>
        /// 交易记录
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult deallists(short? type, int? page)
        {
            if (type == null)
            {
                return Redirect("~/Stock/list?opera=deallists");
            }
            Basis.VerifyLogin(0);

            ViewBag.Type = type.Value;
            ViewBag.Account = Business.StockMarket.GetAccount(Basis.Session.Uid, type.Value);
            ViewBag.Page = page == null ? 0 : page.Value;
            ViewBag.Size = 15;
            //获取信息
            ViewBag.Deals = Business.StockMarketDeal.Gets(0, ViewBag.Account.id, null, null, ViewBag.Page, ViewBag.Size);
            ViewBag.Count = Business.StockMarketDeal.Count(0, ViewBag.Account.id, null, null);

            return View();
        }
        #endregion

        #region 成交记录
        /// <summary>
        /// 成交记录
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public ActionResult turnoverlists(short? type, int? page)
        {
            if (type == null)
            {
                return Redirect("~/Stock/list?opera=turnoverlists");
            }
            Basis.VerifyLogin(0);

            ViewBag.Type = type.Value;
            ViewBag.Account = Business.StockMarket.GetAccount(Basis.Session.Uid, type.Value);
            ViewBag.Page = page == null ? 0 : page.Value;
            ViewBag.Size = 15;
            //获取信息
            ViewBag.Turnovers = Business.StockMarket.GetTurnovers(ViewBag.Account.id, ViewBag.Type, ViewBag.Page, ViewBag.Size);
            ViewBag.Count = Business.StockMarket.TurnoverCount(ViewBag.Account.id, ViewBag.Type);

            return View();
        }
        #endregion

        /**********************************外部接口************************************/

        #region 委托更新
        /// <summary>
        /// 委托更新
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [OutputCache(Duration = 1)]
        public ContentResult get_main_entrusts(short type, long? accountid)
        {
            dynamic Data = Business.StockMarketTrend.GetDeals(type, accountid, 20);

            return Content(new ConvertJson<dynamic>().wrapToString(Data));
        }
        #endregion

        #region KLINE图
        /// <summary>
        /// K线走势
        /// </summary>
        /// <param name="mark"></param>
        /// <returns></returns>
        [OutputCache(Duration = 1)]
        public JsonResult api_main_kline_period(short symbol, int step)
        {
            return Json(Business.StockMarketTrend.GetTrends(symbol, step, 100), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 新KLINE
        /// <summary>
        /// 新KLINE
        /// </summary>
        /// <param name="mark"></param>
        /// <returns></returns>
        [OutputCache(Duration = 1)]
        public ContentResult api_main_kline_depth(short symbol, int step)
        {
            dynamic Data = Business.StockMarketTrend.GetTrendFirst(symbol, step);
            return Content(new ConvertJson<dynamic>().wrapToString(Data));
        }
        #endregion

        #region 币种价格
        /// <summary>
        /// K线走势
        /// </summary>
        /// <param name="mark"></param>
        /// <returns></returns>
        [OutputCache(Duration = 3)]
        public JsonResult api_main_types()
        {
            return Json(Business.StockMarketTrend.GetStockTypes(), JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 我要买入
        /// <summary>
        /// /我要买入
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public JsonResult api_main_buy(long accountid, decimal amount, decimal price)
        {
            Basis.VerifyLogin(0);
            try
            {
                //数据是否正确
                if (amount <= 0 || amount <= 0)
                {
                    return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
                }
                Business.StockMarket.BuysAccount(accountid, price, amount);
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("买入成功") }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 我要卖出
        /// <summary>
        /// /我要卖出
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="price"></param>
        /// <returns></returns>
        public JsonResult api_main_sell(long accountid, decimal amount, decimal price)
        {
            Basis.VerifyLogin(0);
            try
            {
                //数据是否正确
                if (amount <= 0 || amount <= 0)
                {
                    return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
                }
                Business.StockMarket.SellAccount(accountid, price, amount);
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("卖出成功") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 我要撤单
        /// <summary>
        /// /我要撤单
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public JsonResult api_main_cancel(long accountid, long id, short type)
        {
            Basis.VerifyLogin(0);
            try
            {
                //数据是否正确
                if (accountid <= 0 || id <= 0 || type <= 0)
                {
                    return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
                }
                if (type == 1)
                {
                    Business.StockMarket.BuysCancel(accountid, id);
                }
                else
                {
                    Business.StockMarket.SellCancel(accountid, id);
                }
                return Json(new { Success = true, Error = 0, Data = Basis.Parse.ToLanguage("撤单成功") }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage(e.Message) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 虚拟币提现
        /// <summary>
        /// 虚拟币提现
        /// </summary>
        /// <param name="accountid"></param>
        /// <param name="address"></param>
        /// <param name="money"></param>
        /// <param name="remarks"></param>
        /// <returns></returns>
        public JsonResult api_withdraw_submit(long accountid, string address, decimal money, string remarks)
        {
            Basis.VerifyLogin(0);
            Basis.VerifyReal(0);

            if (Basis.AstrictVisit("deal_api_withdraw_submit", 3) == true)
            {
                return Json(new { Success = false, Error = -1, Data = Basis.Parse.ToLanguage("操作频繁，请稍后再试...") }, JsonRequestBehavior.AllowGet);
            }
            //数据是否正确
            if (address.Length <= 0 || money <= 0)
            {
                return Json(new { Success = false, Error = -10002, Data = Basis.Parse.ToLanguage("输入不正确") }, JsonRequestBehavior.AllowGet);
            }
            //提交提现申请
            try
            {
                long DealId = Business.StockMarketDeal.InsertWithdraw(
                       accountid,
                       Basis.Parse.ToLanguage("提币申请"),
                       "-",
                       address,
                       "",
                       money);
                if (DealId > 0)
                {
                    return Json(new { Success = true, Error = 0, Data = DealId }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { Success = false, Error = -10000, Data = Basis.Parse.ToLanguage("申请失败") }, JsonRequestBehavior.AllowGet);
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