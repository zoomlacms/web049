using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class SaleController : Ctrl_Admin
    {
        private string ActionPath = "/AdminMVC/Sale/";
        F_Order_Sale filter = new F_Order_Sale();
        private F_Order_Sale GetFilter()
        {
            F_Order_Sale filter = new F_Order_Sale();

            filter.stime = Request["stime"];
            filter.etime = Request["etime"];
            if (string.IsNullOrEmpty(filter.stime)) { filter.stime = DateTime.Now.AddMonths(-1).ToString("yyyy/MM/dd"); }
            if (string.IsNullOrEmpty(filter.etime)) { filter.etime = DateTime.Now.ToString("yyyy/MM/dd"); }
            filter.storeIds = Request["storeIDS"];
            filter.fast = Request["fast"];
            return filter;
        }
        public ActionResult Index()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "Index";
            return View(filter);
        }
        public ActionResult SaleByDay()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByDay";
            return View(filter);
        }
        public ActionResult SaleByProduct()
        {
            //DataTable dt = srpBll.GetSalesByProduct(new F_Shop_SaleReport()
            //{
            //    ProName = Request["proname"],
            //    SDate = Request["sdate"],
            //    EDate = Request["edate"],
            //    NodeIDS = Request["NodeID"]
            //});
            //if (function.isAjax())
            //{
            //    return PartialView("SaleByProduct_List", dt);
            //}
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByProduct";
            return View(filter);
        }
        public ActionResult SaleByClass()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByClass";
            return View(filter);
        }
        public ActionResult SaleByUser()
        {
            F_Order_Sale filter = GetFilter();
            filter.action = ActionPath + "SaleByUser";
            return View(filter);
        }
    }
}
