using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class CashCouponController : ZLCtrl
    {
        B_Arrive avBll = new B_Arrive();
        public void Index()
        {
           Response.Redirect("ArriveManage");
        }
        public ActionResult ArriveManage()
        {
            int type = DataConvert.CLng(Request["Type"], -100);
            int state = DataConvert.CLng(Request["State"], 1);
            int storeID = DataConvert.CLng(Request.QueryString["StoreID"], -100);
            string addon = Request["addon"] ?? "";
            if (state == 1 && string.IsNullOrEmpty(addon)) { addon = "noexp"; }
            //, mu.UserID, type, state, addon
            PageSetting setting = avBll.SelPage(CPage, PSize, new Filter_Arrive()
            {
                uid = mu.UserID,
                type = type,
                state = state,
                addon = addon,
                storeID = storeID
            });
            if (Request.IsAjaxRequest()) { return PartialView("ArriveManage_List",setting); }
            return View(setting);
        }
        //用户领取优惠券
        public ActionResult GetArrive()
        {
            int storeID = DataConvert.CLng(Request.QueryString["StoreID"], -100);
            PageSetting setting = avBll.U_SelForGet(CPage, PSize, mu.UserID,storeID);
            if (function.isAjax()) { return PartialView("GetArrive_List", setting); }
            return View(setting);
        }
        //领取优惠券
        [HttpPost]
        public void Arrive_Get(string flow)
        {
            avBll.U_GetArrive(mu.UserID, flow);
            string url = string.IsNullOrEmpty(Request["r_hid"]) ? "GetArrive" : Request["r_hid"];
            function.WriteSuccessMsg("优惠券领取成功",url);
        }
        public ActionResult ArriveJihuo()
        {
            return View();
        }
        public void Arrive_Act()
        {
            string ANo = Request.Form["ANo"];
            string APwd = Request.Form["APwd"];

            //优惠券的实例
            M_Arrive avMod = avBll.SelReturnModel(ANo, APwd);
            if (avMod == null) { function.WriteErrMsg("优惠券不存在"); return; }
            string str = "优惠券激活成功" + "！此优惠券的面值为[" + avMod.Amount + "]";
            function.WriteSuccessMsg(str,"ArriveJiHuo");
        }
    }
}
