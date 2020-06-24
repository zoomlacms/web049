using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.AppCode.Filter;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class UserShopSaleController : ZLCtrl
    {
        private string ActionPath = "/User/UserShopSale/";
        F_Order_Sale filter = new F_Order_Sale();
        private F_Order_Sale GetFilter()
        {
            F_Order_Sale filter = new F_Order_Sale();
            M_Store_Info storeMod = storeBll.SelModelByUser(mu.UserID);
            filter.stime = Request["stime"];
            filter.etime = Request["etime"];
            if (string.IsNullOrEmpty(filter.stime)) { filter.stime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd"); }
            if (string.IsNullOrEmpty(filter.etime)) { filter.etime = DateTime.Now.ToString("yyyy/MM/dd"); }
            filter.storeIds = storeMod.ID.ToString();
            filter.fast = Request["fast"];

            ViewBag.storeMod = storeMod;
            return filter;
        }
        B_Store_Info storeBll = new B_Store_Info();
        [AF_StoreCheck]
        public ActionResult Index()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "Index";
            return View(filter);
        }
        [AF_StoreCheck]
        public ActionResult SaleByProduct()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByProduct";
            return View(filter);
        }
        [AF_StoreCheck]
        public ActionResult SaleByClass()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByClass";
            return View(filter);
        }
        [AF_StoreCheck]
        public ActionResult SaleByDay()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByDay";
            return View(filter);
        }
        [AF_StoreCheck]
        public ActionResult SaleByUser()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByUser";
            return View(filter);
        }
    }
}
