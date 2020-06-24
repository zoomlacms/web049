using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Page;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Page;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Common;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class PageController : Ctrl_Admin
    {
        B_Model modBll = new B_Model();
        B_PageReg regBll = new B_PageReg();
        B_PageStyle styleBll = new B_PageStyle();
        B_Content conBll = new B_Content();
        B_PageTemplate tempBll = new B_PageTemplate();
        B_Page pageBll = new B_Page();
        B_ModelField fieldBll=new B_ModelField();
        B_User buser=new B_User();
        public int StyleID
        {
            get { if (ViewBag.StyleID == null) { ViewBag.StyleID = DataConvert.CLng(Request["StyleID"]); }; return DataConvert.CLng(ViewBag.StyleID); }
            set { ViewBag.StyleID = value; }
        }
        public int UserID
        {
            get {if (ViewBag.StyleID == null) { ViewBag.StyleID = DataConvert.CLng(Request["UserID"]); }; return DataConvert.CLng(ViewBag.UserID); }
            set { ViewBag.UserID = value; }
        }
        public int RegID
        {
            get { if (ViewBag.RegID == null) { ViewBag.RegID = DataConvert.CLng(Request["RegID"]); }; return DataConvert.CLng(ViewBag.RegID); }
            set { ViewBag.RegID = value; }
        }
        public ActionResult ApplyAudit()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; };
            PageSetting setting = regBll.SelPage(CPage, PSize, DataConvert.CLng(Request["status"], -100), Request["skey"]);
            if (function.isAjax())
            {
                return PartialView("ApplyAudit_List", setting);
            }
            return View(setting);
        }
        public ActionResult ApplyInfo() 
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) {  return null; }
            M_PageReg regMod = regBll.SelReturnModel(Mid);
            ViewBag.valuedr = regBll.SelUserApplyInfo(regMod);
            DataTable styleDt = styleBll.Sel();
            styleDt.Columns["PageNodeid"].ColumnName = "TemplateID";
            styleDt.Columns["TemplateIndex"].ColumnName = "TemplateUrl";
            styleDt.Columns["TemplateIndexPic"].ColumnName = "TemplatePic";
            styleDt.Columns["PageNodeName"].ColumnName = "rname";
            ViewBag.styleDt = styleDt;
            ViewBag.applyModDT = modBll.GetListPage();
            return View(regMod);
        }
        public ActionResult PageStyle()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            PageSetting setting = styleBll.SelPage(CPage, PSize);
            if (function.isAjax())
            {
                return PartialView("PageStyle_List", setting);
            }
            return View(setting);
        }
        public ActionResult PageStyleAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            M_PageStyle styleMod = styleBll.SelReturnModel(Mid);
            if (styleMod == null) { styleMod = new M_PageStyle(); }
            return View(styleMod);
        }
        public ActionResult PageContent()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            int nodeID = DataConvert.CLng(Request.QueryString["TemplateID"]);
            PageSetting setting = conBll.Page_Sel(CPage, PSize, nodeID, DataConvert.CStr(Request.QueryString["status"]), Request["inputer_t"], Request["title_t"]);
            if (function.isAjax())
            {
                return PartialView("PageContent_List", setting);
            }
            return View(setting);
        }
        public ActionResult EditContent()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            M_CommonData Cdata = conBll.GetCommonData(Mid);
            if (Cdata == null) { function.WriteErrMsg("黄页内容不存在"); }
            M_ModelInfo model = modBll.SelReturnModel(Cdata.ModelID);
            M_Templata nodeMod = tempBll.SelReturnModel(Cdata.NodeID);
            ViewBag.nodeMod = nodeMod;
            return View(Cdata);
        }
        [HttpPost]
        [ValidateInput(false)]
        public void Content_Edit()
        {
            M_CommonData CData = conBll.SelReturnModel(Mid);
            Call commonCall = new Call();
            DataTable dt = fieldBll.SelByModelID(CData.ModelID, false);
            DataTable table;
            try
            {
                table = commonCall.GetDTFromMVC(dt, Request);
            }
            catch (Exception e)
            {
                function.WriteErrMsg(e.Message); return;
            }
            CData.Title = Request.Form["Title"];
            CData.UpDateTime = DateTime.Now;
            conBll.UpdateContent(table, CData);
            function.WriteSuccessMsg("修改成功", "PageContent?TemplateID=" + CData.NodeID); return;
        }
        public ActionResult PageConfig()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; };
            return View();
        }
        //根据传入的PageReg中的ID,列出用户所拥有的表(新用户申请通过,默认建立栏目中配置中处理)
        public ActionResult PageTemplate()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.page, "manage")) { return null; }
            if (Mid == 0) { function.WriteErrMsg("未指定参数"); return null; }
            //ViewBag.TempList = FileSystemObject.GetDTForTemplate();
            DataTable templist = new DataTable();
            if (Mid > 0)
            {
                M_PageReg regMod = regBll.SelReturnModel(Mid);
                templist = tempBll.Sel(regMod.UserID);
                StyleID = regMod.NodeStyle;
                UserID = regMod.UserID;
                ViewBag.title = regMod.UserName;
            }
            else if (Mid == -1)//公用栏目
            {
                templist = tempBll.Sel(Mid);
                ViewBag.title = "公用栏目";
            }
            //---------------------------------
            templist.DefaultView.RowFilter = "ParentID=0";
            VM_Recursion reMod = new VM_Recursion();
            reMod.alldt = templist;
            reMod.dt = templist.DefaultView.ToTable();
            ViewBag.reMod = reMod;
            return View();
        }
        public ActionResult PageTemplateAdd()
        {
            M_PageReg regMod = new M_PageReg();
            M_Templata tempMod = new M_Templata();
            regMod = regBll.SelReturnModel(RegID);
            if (Mid > 0)
            {
                tempMod = tempBll.SelReturnModel(Mid);
                regMod = regBll.SelModelByUid(tempMod.UserID);
                RegID = regMod.ID;
            }
            //用户所属RegID,公有为-1
            if (regMod == null) { function.WriteErrMsg("RegID参数错误"); return null; }
            DataTable templist = tempBll.Sel(regMod.UserID);
            DataTable styleDT = styleBll.Sel();
            B_Page pageBll = new B_Page(tempMod.Modelinfo);
            pageBll.moddt = modBll.SelByType("4");
            ViewBag.pageBll = pageBll;
            ViewBag.styleList = MVCHelper.ToSelectList(styleDT, "PageNodeName", "PageNodeName", tempMod.UserGroup);
            ViewBag.templist = templist;
            return View(tempMod);
        }
        public ActionResult SetPageOrder()
        {
            return View(tempBll.SelByStyleAndPid(-100, Mid));
        }
        public void PageConfig_Update()
        {
            SiteConfig.SiteOption.RegPageStart = !string.IsNullOrEmpty(Request.Form["RegPageStart_Chk"]);
            SiteConfig.YPage.IsAudit = !string.IsNullOrEmpty(Request.Form["IsAudit_Chk"]);
            SiteConfig.YPage.UserCanNode = !string.IsNullOrEmpty(Request.Form["UserCanNode_Chk"]);
            SiteConfig.Update();
            function.WriteSuccessMsg("配置保存成功", "PageConfig");
        }
        #region PageTemplate Logical
        public void PageTemplate_Add()
        {
            M_Templata tempMod = new M_Templata();
            M_PageReg regMod = new M_PageReg();
            if (Mid > 0) { tempMod = tempBll.SelReturnModel(Mid); regMod = regBll.SelModelByUid(tempMod.UserID); }
            if (RegID != 0) { regMod = regBll.SelReturnModel(RegID); }
            tempMod.TemplateName = Request.Form["TemplateName"];
            tempMod.TemplateUrl = Request.Form["TemplateUrl_hid"];
            tempMod.TemplateType = DataConvert.CLng(Request.Form["TemplateType"]);
            tempMod.OpenType = Request.Form["OpenType"];
            tempMod.UserGroup = Request.Form["UserGroup"];
            tempMod.Addtime = DataConverter.CDate(Request.Form["addtime"]);
            tempMod.IsTrue = DataConverter.CLng(Request.Form["IsTrue"]);
            tempMod.OrderID = DataConverter.CLng(Request.Form["OrderID"]);
            tempMod.Identifiers = Request.Form["Identifiers"];
            tempMod.NodeFileEx = Request.Form["NodeFileEx"];
            tempMod.ContentFileEx = Request.Form["ContentFileEx"];
            tempMod.Nodeimgurl = Request.Form["Nodeimgurl"];
            tempMod.Nodeimgtext = Request.Form["Nodeimgtext"];
            tempMod.Pagecontent = Request.Form["Pagecontent"];
            tempMod.PageMetakeyword = Request.Form["PageMetakeyword"];
            tempMod.PageMetakeyinfo = Request.Form["PageMetakeyinfo"];
            tempMod.Linkurl = Request.Form["linkurl.Text"];
            tempMod.Linkimg = Request.Form["linkimg.Text"];
            tempMod.Linktxt = Request.Form["linktxt.Text"];
            tempMod.Modelinfo = pageBll.GetSubmitModelChk(Request);
            if (tempMod.TemplateID > 0)
            {
                tempBll.UpdateByID(tempMod);
            }
            else
            {
                tempMod.ParentID = DataConvert.CLng(Request.QueryString["ParentID"]);
                tempMod.UserID = regMod.UserID;
                tempMod.Username = regMod.UserName;
                tempBll.insert(tempMod);
            }
            function.WriteSuccessMsg("操作成功", "PageTemplate?ID=" + regMod.ID);
        }
        public int Node_Del(string ids)
        {
            tempBll.DelByIDS(ids);
            return Success;
        }
        #endregion
        #region Order Logical
        public void SetPageOrder_Batch()
        {
            if (!string.IsNullOrEmpty(Request.Form["PageValue"]))
            {
                string[] idsarr = Request.Form["PageValue"].Split(',');
                for (int i = 0; i < idsarr.Length; i++)
                {
                    int orderid = DataConverter.CLng(Request.Form["OrderField" + idsarr[i]]);
                    M_Templata tempmod = tempBll.SelReturnModel(DataConverter.CLng(idsarr[i]));
                    tempmod.OrderID = orderid;
                    tempBll.UpdateByID(tempmod);
                }
            }
            Response.Redirect("SetPageOrder?id=" + Request["ID"]);
        }
        public void SetPageOrder_UpMove()
        {
            int tempid = DataConverter.CLng(Request["tempid"]);
            MovePage(tempid, true);
            Response.Redirect("SetPageOrder?id=" + Request["ID"]);
        }
        public void SetPageOrder_DownMove()
        {
            int tempid = DataConverter.CLng(Request["tempid"]);
            MovePage(tempid, false);
            Response.Redirect("SetPageOrder?id=" + Request["ID"]);
        }
        private void MovePage(int id, bool isup)
        {
            string[] SpecValues = Request.Form["PageValue"].Split(',');
            M_Templata tempmod = tempBll.SelReturnModel(id);
            for (int i = 0; i < SpecValues.Length; i++)
            {
                if (SpecValues[i].Equals(id.ToString()))
                {
                    if ((isup && i - 1 < 0) || (!isup && i + 1 >= SpecValues.Length)) { break; }//上移下移判断
                    int index = isup ? i - 1 : i + 1;
                    M_Templata targetmod = tempBll.SelReturnModel(DataConverter.CLng(SpecValues[index]));
                    int nodeorder = DataConverter.CLng(Request.Form["OrderField" + SpecValues[index]]);
                    targetmod.OrderID = tempmod.OrderID;
                    tempmod.OrderID = nodeorder;
                    tempBll.UpdateByID(tempmod);
                    tempBll.UpdateByID(targetmod);
                    break;
                }
            }
        }
        #endregion
        #region Content Logical
        public int Content_Audit(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.SetAuditByIDS(ids, (int)ZLEnum.ConStatus.Audited);
            }
            return Success;
        }
        public int Content_UnAudit(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.SetAuditByIDS(ids, (int)ZLEnum.ConStatus.UnAudit);
            }
            return Success;
        }
        public int Content_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.SetAuditByIDS(ids, (int)ZLEnum.ConStatus.Recycle);
            }
            return Success;
        }
        public int Content_RealDel(string ids)
        {
            conBll.DelByIDS(ids);
            return Success;
        }
        public int Content_Recovery(string ids)
        {
            conBll.RecByIDS(ids);
            return Success;
        }
        public int Content_Clear() 
        {
            conBll.Page_ClearRecycle();
            return Success;
        }
        #endregion
        #region Style Logical
        public void Style_Add()
        {
            M_PageStyle styleMod = new M_PageStyle();
            if (Mid > 0)
            {
                styleMod = styleBll.SelReturnModel(Mid);
            }
            styleMod.PageNodeName = Request.Form["PageNodeName"];
            styleMod.StylePath = Request.Form["StylePath"];
            styleMod.Orderid = DataConverter.CLng(Request.Form["orderid"]);
            styleMod.TemplateIndex = Request.Form["TemplateIndex_hid"];
            styleMod.TemplateIndexPic = Request.Form["TemplateIndexPic_t"];
            if (styleMod.PageNodeid > 0)
            {
                styleBll.UpdateByID(styleMod);
            }
            else
            {
                styleBll.insert(styleMod);
            }
            function.WriteSuccessMsg("操作成功", "PageStyle");
        }
        public int Style_Del(string ids)
        {
            styleBll.DelByIDS(ids);
            return Success;
        }
        #endregion
        #region Apply Logical
        public int Page_Audit(string ids)
        {
            regBll.UpdateByField("[Status]", ((int)ZLEnum.ConStatus.Audited).ToString(), ids);
            return Success;
        }
        public int Page_UnAudit(string ids)
        {
            regBll.UpdateByField("[Status]", ((int)ZLEnum.ConStatus.UnAudit).ToString(), ids);
            return Success;
        }
        public int Page_Recom(string ids)
        {
            regBll.UpdateByField("[Recommendation]", "1", ids);
            return Success;
        }
        public int Page_UnRecom(string ids)
        {
            regBll.UpdateByField("[Recommendation]", "0", ids);
            return Success;
        }
        public int Page_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                regBll.DelByIDS(ids);
            }
            return Success;
        }
        [ValidateInput(false)]
        public void Apply_Update()
        {
            M_PageReg regMod = regBll.SelReturnModel(Mid);
            M_UserInfo mu = buser.SelReturnModel(regMod.UserID);
            DataTable dt = fieldBll.GetModelFieldList(regMod.ModelID);
            Call commonCall = new Call();
            DataTable table = commonCall.GetDTFromMVC(dt, Request);
            regMod.CompanyName = Request.Form["CompanyName"];
            regMod.PageTitle = mu.UserName + "的黄页信息";
            regMod.PageInfo = Request.Form["PageInfo"];
            regMod.LOGO = Request.Form["LOGO_t"];
            regMod.NodeStyle = DataConverter.CLng(Request.Form["TempleID_Hid"]);//样式与首页模板??,首页模板可动态以styleMod中为准
            regMod.Template = Request.Form["TempleUrl_Hid"];
            //regMod.Status =//状态不变更
            //regMod.ModelID = applyMod.ModelID;
            //regMod.TableName = applyMod.TableName;
            conBll.Page_Update(table, regMod);
            function.WriteSuccessMsg("修改提交成功", "ApplyAudit"); return;
        }
        #endregion
    }
}
