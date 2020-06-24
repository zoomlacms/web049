using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class MessageController : ZLCtrl
    {
        B_Message msgBll = new B_Message();
        public void Index()
        {
            Response.Redirect("Message"); 
        }
        public ActionResult Message()
        {
            F_Message filter = new F_Message()
            {
                title = Request["skey_t"],
                uid = mu.UserID,
                filter = "rece"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (function.isAjax())
            {
                return PartialView("Message_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        //新建|修改邮件,草稿
        public ActionResult MessageSend()
        {
            M_Message msgMod = new M_Message();
            if (Mid > 0)
            {
                msgMod = msgBll.SelReturnModel(Mid);
                if (msgMod.Sender != mu.UserID) { function.WriteErrMsg("你无权修改该邮件"); return null; }
                if (msgMod.Savedata == 0) { function.WriteErrMsg("邮件已发送,不可修改"); return null; }
            }
            else
            {
                M_UserInfo tmu = new M_UserInfo(true);
                if (DataConverter.CLng(Request["uid"]) > 0)
                {
                    tmu = buser.SelReturnModel(DataConverter.CLng(Request["uid"]));
                }
                else if (!string.IsNullOrEmpty(Request["name"]))
                {
                    tmu = buser.GetUserByName(HttpUtility.UrlDecode(Request["name"]));
                }
                if (!tmu.IsNull) { msgMod.Incept = tmu.UserID.ToString(); }

                if (!string.IsNullOrEmpty(Request["content"])) { msgMod.Content = HttpUtility.UrlDecode(Request["content"]); }
                if (!string.IsNullOrEmpty(Request["title"])) { msgMod.Title = HttpUtility.UrlDecode(Request["title"]); }
            }
            return View(msgMod);
        }
        //阅读邮件
        public ActionResult MessageRead()
        {
            M_Message msgMod = msgBll.SelReturnModel(Mid);
            if (msgMod == null) { function.WriteErrMsg("邮件不存在"); }
            if (msgMod.Sender == mu.UserID||
                msgMod.Incept.Contains("," + mu.UserID + ",") ||
                msgMod.CCUser.Contains("," + mu.UserID + ",")) { }
            else { function.WriteErrMsg("你无权阅读该邮件"); }
            return View(msgMod);
        }
        //草稿箱
        public ActionResult MessageDraftbox()
        {
            F_Message filter = new F_Message()
            {
                //title = Request["skey_t"],
                uid = mu.UserID,
                filter = "draft"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (function.isAjax())
            {
                return PartialView("Message_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        //发件箱
        public ActionResult MessageOutbox()
        {
            F_Message filter = new F_Message()
            {
                title = Request["skey_t"],
                uid = mu.UserID,
                filter = "send"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (function.isAjax())
            {
                return PartialView("MessageOutbox_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        //回收站
        public ActionResult MessageRecycle()
        {
            F_Message filter = new F_Message()
            {
                title = Request["skey_t"],
                uid = mu.UserID,
                filter = "recycle"
            };
            PageSetting setting = msgBll.SelPage(CPage, PSize, filter);
            if (function.isAjax())
            {
                return PartialView("MessageRecycle_List", setting);
            }
            ViewBag.filter = filter;
            return View(setting);
        }
        public ActionResult Mobile() { return View(); }
        //------------------------------
        [HttpPost]
        [ValidateInput(false)]
        public void Message_Add()
        {
            M_Message msgMod = new M_Message();
            if (Mid > 0) { msgMod = msgBll.SelReturnModel(Mid); }
            msgMod.Savedata = 0;
            msgMod.status = 1;
            FillMsgModel(msgMod);
            if (msgMod.MsgID > 0)
            {
                msgBll.UpdateByID(msgMod);
            }
            else { msgMod.Sender = mu.UserID;msgMod.UserName = mu.UserName; msgBll.GetInsert(msgMod); }

            function.WriteSuccessMsg("发送成功", "MessageOutbox");
        }
        [HttpPost]
        [ValidateInput(false)]
        public void Message_Draft()
        {
            M_Message msgMod = new M_Message();
            if (Mid > 0) { msgMod = msgBll.SelReturnModel(Mid); }
            msgMod.Sender = mu.UserID;
            msgMod.Savedata = 1;
            msgMod.status = 0;
            FillMsgModel(msgMod);
            if (msgMod.MsgID > 0)
            {
                msgBll.UpdateByID(msgMod);
            }
            else { msgMod.Sender = mu.UserID;msgMod.UserName = mu.UserName; msgBll.GetInsert(msgMod); }
            function.WriteSuccessMsg("存为草稿成功", "MessageDraftbox");
        }
        [HttpPost]
        public int Message_Del(string ids)
        {
            msgBll.DelByIDS(ids, mu.UserID);
            return Success;
        }
        [HttpPost]
        public int Message_RealDel(string ids)
        {
         //msgBll.ReCoverByIDS
            msgBll.RealDelByIDS(ids, mu.UserID);
            return Success;
        }
        [HttpPost]
        public int Message_Recovery(string ids)
        {
         //msgBll.ReCoverByIDS
            msgBll.ReFromRecycle(ids, mu.UserID);
            return Success;
        }

        private M_Message FillMsgModel(M_Message msgMod)
        {
            msgMod.Title = HttpUtility.HtmlEncode(Request.Form["title_t"]);
            msgMod.Content = Request.Form["content_t"];
            msgMod.Incept = Request.Form["refer_hid"];
            msgMod.CCUser = Request.Form["ccuser_hid"];
            msgMod.Attachment = Request.Form["Attach_Hid"];
            return msgMod;
        }

    }
}
