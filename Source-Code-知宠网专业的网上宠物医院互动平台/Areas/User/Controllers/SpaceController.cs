﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.AppCode.Common;
using ZoomLa.BLL;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.User;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class SpaceController : Ctrl_Base
    {
        B_CreateHtml bll = new B_CreateHtml();
        B_User_BlogStyle bsBll = new B_User_BlogStyle();
        M_User_BlogStyle bsMod = null;
        B_User buser = new B_User();
        public int Uid { get { return DataConverter.CLng(Request.QueryString["id"]); } }
        public void Index()
        {
            Response.Redirect("SpaceManage");
        }
        public ActionResult SpaceManage() {
            string errtitle = "<h3 class='panel-title'><span class='zi zi_exclamationCircle'></span> 不正确的访问</h3>";
            int id = DataConverter.CLng(Request.QueryString["id"]);
            M_UserInfo mu = buser.SelReturnModel(Uid);
            if (mu.IsNull) { function.WriteErrMsg(errtitle, "主页ID违规，请使用/User/Space/SpaceManage?id=[uid]方式访问", ""); return null; }
            else if (mu.State != 2) { function.WriteErrMsg(errtitle, "未通过认证会员无法开启个人主页! !", ""); return null; }
            else if (mu.PageID < 1) { function.WriteErrMsg(errtitle, "用户未指定模板", ""); return null; }
            bsMod = bsBll.SelReturnModel(mu.PageID);
            if (bsMod == null) { function.WriteErrMsg(errtitle, "指定的风格不存在", ""); return null; }
            string tlp = function.VToP(SiteConfig.SiteOption.TemplateDir + bsMod.UserIndexStyle);
            if (!System.IO.File.Exists(tlp)) { function.WriteErrMsg(errtitle, "模板文件[" + bsMod.UserIndexStyle + "]不存在", ""); return null; }
            string html = SafeSC.ReadFileStr(tlp);
            ViewBag.conhtml = bll.CreateHtml(html);
            return View();
        }
    }
}
