using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class iServerController : ZLCtrl
    {
        B_IServer isBll = new B_IServer();
        B_IServerReply repBll = new B_IServerReply();
        public void Index()
        {
            Response.Redirect("FiServer"); 
        }
        // 回答其他用户的提问,仅限于被@
        public ActionResult ISAnswer()
        {
            string title = Request.Form["title"];
            PageSetting setting = isBll.SelPage(CPage, PSize, new F_IServer()
            {
                ccuser = mu.UserID.ToString(),
                title = Request.Form["title"],
                state = Request.Form["state"]
            });
            if (function.isAjax())
            {
                return PartialView("ISAnswer_List",setting);
            }
            return View(setting);
        }
        public ActionResult FiServer()
        {
            //订单ID,有问必答与订单绑定
            int OrderID = DataConverter.CLng(Request.QueryString["orderid"]);
            int Type = DataConverter.CLng(Request.QueryString["type"],-100);
            string typeStr = "";
            if (Type == -100) { typeStr = ""; }
            else { typeStr = isBll.TypeArr[Type]; }
            string state = isBll.GetStatus(DataConverter.CLng(Request.QueryString["num"],-100));
            //----------------------------------
            PageSetting config = isBll.SelPage(CPage, PSize, new F_IServer()
            {
                uid = mu.UserID,
                state = state,
                type = typeStr,
                title = Request["skey_t"]
            });
            if (function.isAjax()) { return PartialView("FiServer_List",config); }
            ViewBag.allnum = isBll.getiServerNum("", mu.UserID, typeStr, OrderID);
            ViewBag.treatnum = isBll.getiServerNum("处理中", mu.UserID, typeStr, OrderID);
            ViewBag.nrslvnum = isBll.getiServerNum("未解决", mu.UserID, typeStr, OrderID);
            ViewBag.rslvnum = isBll.getiServerNum("已解决", mu.UserID, typeStr, OrderID);
            ViewBag.socknum = isBll.getiServerNum("已锁定", mu.UserID, typeStr, OrderID);
            ViewBag.typedt = isBll.GetSeachUserIdType(mu.UserID);
            return View(config);
        }
        public ActionResult AddQuestion()
        {
            return View();
        }
        [ValidateInput(false)]
        public void Question_Add()
        {
            M_IServer isMod = new M_IServer();
            isMod.UserId = mu.UserID;
            isMod.UserName = mu.UserName;
            isMod.Title = Request.Form["title_t"];
            isMod.Content = Request.Form["txtContent"];
            isMod.Priority = Request.Form["Priority"];
            isMod.Type = Request.Form["Type"];
            isMod.Root = "网页表单";
            isMod.State = "未解决";
            if (SafeSC.CheckIDS(Request.Form["CCUser_Hid"]))
            {
                isMod.CCUser = Request.Form["CCUser_Hid"];
            }
            isMod.RequestTime = DataConverter.CDate(Request.QueryString["mydate_t"]);
            if (!string.IsNullOrEmpty(Request["OrderID"])) { isMod.OrderType = DataConverter.CLng(Request["OrderID"]); }
            isMod.Path = Request.Form["attach_hid"];
            isMod.QuestionId = isBll.Insert(isMod);
            if (isMod.QuestionId > 0)
            {
                function.WriteSuccessMsg("提交成功", "FiServer?OrderID=" + isMod.OrderType); return;
            }
            else
            {
                function.WriteErrMsg("提交失败-可能是由于系统未开放功能所致"); return;
            }
        }
        //仅用于用户操作
        [HttpPost]
        public string IServer_API()
        {
            string action = Request["action"] ?? "";
            M_IServer isMod = isBll.SelReturnModel(Mid);
            if (isMod == null) { return ""; }
            if (isMod.UserId != mu.UserID) { return "你无权操作该内容"; }
            switch (action)
            {
                //case "state":
                //    {
                //        int state = DataConverter.CLng(Request["state"]);
                //        isBll.UpdateState(Mid,state);
                //    }
                //    break;
                case "solve":
                    {
                        isMod.State = isBll.GetStatus(3);
                        isMod.SolveTime = DateTime.Now;
                        isBll.UpdateByID(isMod);
                    }
                    break;
                case "close":
                    {
                        isBll.UpdateState(Mid, -1);
                    }
                    break;
                case "udel"://用户删除
                    {
                        isBll.DeleteById(Mid);
                    }
                    break;
            }
            return Success.ToString();
        }
        public ActionResult FiServerInfo()
        {
            string Menu = Request.QueryString["menu"] ?? "";
            string Path = Request.QueryString["filepath"];
            if (Menu.Equals("filedown") && !string.IsNullOrEmpty(Path))
            {
                FileInfo file = new FileInfo(function.VToP(Path));
                if (file.Exists) { SafeSC.DownFile(Path); }
                else { function.WriteErrMsg("文件不存在"); return null; }
            }
            M_IServer serverMod = isBll.SelReturnModel(Mid);
            if (serverMod == null) { function.WriteErrMsg("问题不存在"); return null; }
            //回复列表
            ViewBag.replydt = repBll.SeachById(serverMod.QuestionId);
            //更新已读状态
            repBll.GetUpdataState(1, serverMod.QuestionId);
            return View(serverMod);
        }
        public ActionResult SelectiServer()
        {
            string skey = DataConverter.CStr(Request["strTitle"]);
            int Type = DataConverter.CLng(Request["type"]);
            int OrderID = DataConverter.CLng(Request["orderid"]);
            string state = isBll.GetStatus(DataConverter.CLng(Request["num"]));
            F_IServer filter = new F_IServer()
            {
                uid = mu.UserID,
                title = skey,
                state = state,
                type = isBll.TypeArr[Type],
                oid = OrderID
            };
            //回答问题,不需要进行用户筛选
            if (DataConverter.CStr(Request["filter"]).Equals("answer"))
            {
                filter.uid = -100;
                filter.ccuser = mu.UserID.ToString();
            }
            PageSetting setting = isBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest()) { return PartialView("SelectServer_List", setting); }
            ViewBag.typedt = isBll.GetSeachUserIdType(mu.UserID);
            return View(setting);
        }
        #region 回复信息
        public ActionResult ISReplyAdd()
        {
            M_IServerReply repMod = new M_IServerReply();
            if (Mid > 0)
            {
                repMod = repBll.SelReturnModel(Mid);
                if (repMod.UserId != mu.UserID) { function.WriteErrMsg("你无权修改该回复");return null; }
            }
            else
            {
                repMod.QuestionId = DataConverter.CLng(Request.QueryString["Qid"]);
            }
            return View(repMod);
        }
        [ValidateInput(false)]
        public string QuestionReply_Add()
        {
            int Qid = DataConverter.CLng(Request["Qid"]);
            M_IServerReply repMod = new M_IServerReply();
            if (Mid > 0)
            {
                repMod = repBll.SelReturnModel(Mid);
                if (repMod.UserId != mu.UserID) { function.WriteErrMsg("你无权修改该回复"); return null; }
            }
            repMod.UserId = mu.UserID;
            repMod.UserName = mu.UserName;
            repMod.Title = Request.Form["Title_T"];
            repMod.Content = Request.Form["Content_T"];
            repMod.Path = Request.Form["Attach_Hid"];
            if (repMod.Id > 0)
            {
                repBll.UpdateByID(repMod);
            }
            else
            {
                repMod.QuestionId = Qid;
                repBll.Add(repMod);
            }
            return "<script>parent.location=parent.location;</script>";
        }
        #endregion
    }
}
