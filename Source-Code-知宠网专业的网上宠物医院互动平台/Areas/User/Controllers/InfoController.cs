﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Field;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class InfoController : ZLCtrl
    {
        B_Group gpBll = new B_Group();
        B_ModelField fieldBll = new B_ModelField();
        B_UserBaseField ubfbll = new B_UserBaseField();
        B_History hisBll = new B_History();
        B_UserRecei receBll = new B_UserRecei();
        B_UserBaseField ubBll = new B_UserBaseField();
        B_Order_InvTlp invtBll = new B_Order_InvTlp();
        #region 用户信息管理
        public ActionResult Index()
        {
            ViewBag.gpMod = gpBll.SelReturnModel(mu.GroupID);
            return View();
        }
        public void UserInfo() { Response.Redirect("Index"); return; }
        public ActionResult UserBase()
        {
            M_Uinfo basemu = buser.GetUserBaseByuserid(mu.UserID);
            ViewBag.gpMod = gpBll.SelReturnModel(mu.GroupID);
            ViewBag.basemu = basemu;
            DataTable valueDT = DBCenter.SelTop(1, "UserID", "*", "ZL_UserBase", "UserID=" + mu.UserID, "");
            ModelConfig modcfg = new ModelConfig() { Source = ModelConfig.SType.Admin, ValueDT = valueDT };
            VM_FieldModel model = new VM_FieldModel(ubBll.Select_All(), modcfg);
            ViewBag.htmlMod = model;
            return View(mu);
        }
        public void UserBase_Edit()
        {
            DataTable dt = ubfbll.Select_All();
            Call commonCall = new Call();
            DataTable table;
            try
            {
                table = commonCall.GetDTFromMVC(dt, Request);
            }
            catch (Exception e)
            {
                function.WriteErrMsg(e.Message); return;
            }
            mu.UserFace = HttpUtility.HtmlEncode(Request.Form["UserFace_T"]);
            mu.HoneyName = HttpUtility.HtmlEncode(Request.Form["txtHonName"]);
            mu.CompanyName = HttpUtility.HtmlEncode(Request.Form["CompanyName"]);
            mu.TrueName = HttpUtility.HtmlEncode(Request.Form["tbTrueName"]);
            M_Uinfo binfo = buser.GetUserBaseByuserid(mu.UserID);
            binfo.Address = Request.Form["tbAddress"];
            binfo.BirthDay = Request.Form["tbBirthday"];
            binfo.FaceHeight = DataConverter.CLng(Request["tbFaceHeight"]);
            binfo.FaceWidth = DataConverter.CLng(Request["tbFaceWidth"]);
            binfo.UserFace = mu.UserFace;
            binfo.Fax = Request.Form["tbFax"];
            binfo.HomePage = Request.Form["tbHomepage"];
            //binfo.ICQ = Server.HtmlEncode(tbICQ.Text.Trim());
            binfo.HomePhone = Request.Form["tbHomePhone"];
            binfo.IDCard = Request.Form["tbIDCard"];
            //binfo.Mobile = Server.HtmlEncode(tbMobile.Text.Trim());
            binfo.OfficePhone = Request.Form["tbOfficePhone"];
            binfo.Privating = DataConvert.CLng(Request.Form["tbPrivacy"]);
            //binfo.PHS = Server.HtmlEncode(tbPHS.Text.Trim());
            binfo.QQ = Request.Form["tbQQ"];
            binfo.Sign = Request.Form["tbSign"];
            binfo.UC = Request.Form["tbUC"];
            binfo.UserSex = DataConverter.CBool(Request.Form["tbUserSex"]);
            //binfo.Yahoo = Server.HtmlEncode(tbYahoo.Text.Trim());
            binfo.ZipCode = HttpUtility.HtmlEncode(Request.Form["tbZipCode"]);
            binfo.HoneyName = mu.HoneyName;
            binfo.TrueName = mu.TrueName;
            string[] adrestr = Request.Form["address"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            binfo.Province = adrestr[0];
            binfo.City = adrestr[1];
            binfo.County = adrestr[2];
            binfo.Position = HttpUtility.HtmlEncode(Request.Form["Position"]);
            buser.UpDateUser(mu);
            if (binfo.IsNull)
            {
                binfo.UserId = mu.UserID;
                buser.AddBase(binfo);
            }
            else
            {
                buser.UpdateBase(binfo);//更新用户信息 
            }
            if (table.Rows.Count > 0)
            {
                buser.UpdateUserFile(binfo.UserId, table);
            }
            function.WriteSuccessMsg("修改成功", "UserBase"); return;
        }
        public ActionResult DredgeVip() { return View(); }
        public void DardgeVip_Open()
        {
        }
        #endregion
        #region 地址操作
        public ActionResult UserRecei()
        {
            PageSetting setting = receBll.SelByUid_SPage(CPage, PSize, mu.UserID);
            if (Request.IsAjaxRequest()) { return PartialView("UserRecel_List", setting); }
            return View(setting);
        }
        public int Recei_Del(int id)
        {
            receBll.DeleteByGroupID(id);
            return 1;
        }
        public int Recei_SetDef(int id)
        {
            receBll.SetDef(id);
            return 1;
        }
        public ActionResult AddUserAddress()
        {
            M_UserRecei model = new M_UserRecei();
            if (Mid > 0)
            {
                model = receBll.GetSelect(Mid, mu.UserID);
                if (model == null) { function.WriteErrMsg("修改的地址不存在"); return Content(""); }
                if (!model.phone.Contains("-")) { model.phone = "-"; }
            }
            return View(model);
        }
        public void Address_Add()
        {
            M_UserRecei model = new M_UserRecei();
            if (Mid > 0)
            {
                model = receBll.GetSelect(Mid, mu.UserID);
            }
            model.UserID = mu.UserID;
            model.Email = mu.Email;
            //model.Provinces = Request.Form["province_dp"] + "|" + Request.Form["city_dp"] + "|" + Request.Form["county_dp"];
            //model.CityCode = Request.Form["province_dp"] + "|" + Request.Form["city_dp"] + "|" + Request.Form["county_dp"];
            //model.CityCode = province_dp.SelectedValue + " " + city_dp.ValueSelectedValue + " " + county_dp.SelectedValue;
            model.Provinces = Request["pro_hid"];
            model.Street = Request["Street_T"];
            model.Zipcode = Request["ZipCode_T"];
            model.ReceivName = Request["ReceName_T"];
            model.MobileNum = Request["MobileNum_T"];
            //model.phone = Request.Form["Area_T"] + "-" + Request.Form["Phone_T"];
            model.isDefault = DataConvert.CLng(Request["Def_chk"]);
            if (Mid > 0)
            { receBll.GetUpdate(model); }
            else
            { model.ID = receBll.GetInsert(model); }
            if (model.isDefault == 1) { receBll.SetDef(model.ID); }
            function.WriteSuccessMsg("保存地址成功!", "UserRecei"); return;
        }
        #endregion
        #region 发票管理
        public ActionResult Invoice()
        {
            PageSetting setting = invtBll.SelPage(1, invtBll.MaxCount, mu.UserID);
            if (function.isAjax())
            {
                return PartialView("Invoice_List",setting);
            }
            return View(setting);
        }
        public ActionResult InvoiceAdd()
        {
            M_Order_Invoice invMod = new M_Order_Invoice();
            if (Mid > 0)
            {
                invMod = invtBll.SelReturnModel(Mid);
                if (invMod.UserID != mu.UserID)
                { function.WriteErrMsg("你无权操作该内容", "Invoice"); return null; }
            }
            return View(invMod);
        }
        public void Invoice_Add(M_Order_Invoice invMod)
        {
            if (Mid < 1)
            {
                invMod.UserID = mu.UserID;
                invtBll.Insert(invMod);
            }
            else
            {
                M_Order_Invoice oldMod = invtBll.SelReturnModel(Mid);
                if (oldMod.UserID != mu.UserID) { function.WriteErrMsg("你无权操作该内容","Invoice");return; }
                invMod.ID = oldMod.ID;
                invMod.UserID = oldMod.UserID;
                invMod.UserMobile = oldMod.UserMobile;
                invMod.UserEmail = oldMod.UserEmail;
                invtBll.UpdateByID(invMod);
            }
            function.WriteSuccessMsg("操作成功","Invoice");
        }
        public string Invoice_API()
        {
            string action = Request["action"];
            switch (action)
            {
                case "del":
                    {
                        invtBll.Del(Request["ids"],mu.UserID);
                    }
                    break;
            }
            return Success.ToString();
        }
        #endregion
        public void Listprofit() { Response.Redirect("ConsumeDetail?SType=1"); return; }
        public ActionResult ConsumeDetail()
        {
            int type = DataConvert.CLng(Request["SType"]);
            PageSetting setting = hisBll.SelPage(CPage, PSize, type, mu.UserID, Request["skey_t"], Request["stime_t"], Request["etime_t"]);
            if (Request.IsAjaxRequest())
            {
                return PartialView("ConsumeDetail_List", setting);
            }
            return View(setting);
        }
        public ActionResult CardView() { return View(); }
        public ActionResult MySubscription()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
    }
}
