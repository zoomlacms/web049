using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.Model;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class ComController : Controller
    {
        //
        // GET: /AdminMVC/Com/

        public ContentResult Logout()
        {
            if (DataConvert.CLng(Request["preload"]) == 1) { return null; }
            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    ZLLog.L(ZLEnum.Log.safe, "admin signout err:" + ex.Message);
            //}
            B_Admin.ClearLogin();
            string url = Request.QueryString["ReturnUrl"];
            if (string.IsNullOrEmpty(url)) { url = CustomerPageAction.customPath2 + "login"; }
            //回发清除cookies
            //Response.Write("<script></script>");
            return Content("<script>location='" + url + "';</script>");
        }
        public ActionResult BootLayout()
        {
            return View();
        }

    }
}
