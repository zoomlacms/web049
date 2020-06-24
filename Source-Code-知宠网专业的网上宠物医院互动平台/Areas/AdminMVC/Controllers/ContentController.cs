using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Common.Addon;
using ZoomLa.Components;
using ZoomLa.HtmlLabel;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Content;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class ContentController : Ctrl_Admin
    {
        M_Node nodeMod = new M_Node();
        B_Content contentBll = new B_Content();
        B_Model modelBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_Node nodeBll = new B_Node();
        B_Spec spbll = new B_Spec();
        B_Content_ScheTask taskBll = new B_Content_ScheTask();
        B_KeyWord keyBll = new B_KeyWord();
        B_Process proBll = new B_Process();
        B_Content_VerBak verBll = new B_Content_VerBak();
        ContentHelper conhelp = new ContentHelper();
        B_Content_ScheTask scheBll = new B_Content_ScheTask();
        Call commonCall = new Call();
        public int NodeID { get { return DataConvert.CLng(Request["NodeID"]); } }
        public int ModelID { get { return DataConvert.CLng(Request["ModelID"]); } }
        public ActionResult AddContent()
        {
            if (ModelID < 1 || NodeID < 1) { function.WriteErrMsg("参数错误"); return null; }
            if (!B_NodeRole.CheckNodeAuth(adminMod, NodeID, "addto")) { function.WriteErrMsg("你无权在该节点下添加内容页"); return null; }
            DataTable ddlDT = proBll.SelByNodeID2(NodeID);
            VM_Content vm = new VM_Content();
            vm.fieldDT = fieldBll.SelByModelID(ModelID, true);
            vm.nodeMod = nodeBll.SelReturnModel(NodeID);
            vm.modelMod = modelBll.SelReturnModel(ModelID);
            vm.conMod.Status = adminMod.DefaultStart;
            vm.conMod.Inputer = adminMod.UserName;
            ViewBag.ddlDT = ddlDT;
            return View(vm);
        }
        public ActionResult EditContent()
        {
            int Mid = Convert.ToInt32(Request.QueryString["GeneralID"]);
            int VerID = DataConvert.CLng(Request.QueryString["Ver"]);
            VM_Content vm = new VM_Content();
            if (VerID > 0)
            {
                M_Content_VerBak verMod = verBll.SelReturnModel(VerID);
                vm.conMod = JsonConvert.DeserializeObject<M_CommonData>(verMod.ContentBak);
                if (vm.conMod.GeneralID != Mid) { function.WriteErrMsg("加载的版本与内容不匹配"); return null; }
                vm.ValueDT = JsonConvert.DeserializeObject<DataTable>(verMod.TableBak);
            }
            else
            {
                vm.conMod = contentBll.SelReturnModel(Mid);
                vm.ValueDT = contentBll.GetContent(vm.conMod);
            }
            vm.fieldDT = fieldBll.SelByModelID(vm.conMod.ModelID, true);
            vm.nodeMod = nodeBll.SelReturnModel(vm.conMod.NodeID);
            if (!B_NodeRole.CheckNodeAuth(adminMod, vm.nodeMod.NodeID, "addto")) { function.WriteErrMsg("你无权修改该内容"); return null; }
            vm.modelMod = modelBll.SelReturnModel(vm.conMod.ModelID);
            if (!string.IsNullOrEmpty(vm.conMod.SpecialID))
            {
                vm.SpecialDT = spbll.SelByIDS(vm.conMod.SpecialID);
                if (vm.SpecialDT == null) { vm.SpecialDT = new DataTable(); }
            }
            DataTable ddlDT = proBll.SelByNodeID2(vm.NodeID);
            ViewBag.ddlDT = ddlDT;
            //M_Content_ScheTask exaTask = scheBll.GetModel(vm.conMod.BidType);
            //if (exaTask != null) { vm.ExamineTime = exaTask.ExecuteTime; }
            //M_Content_ScheTask expTask = scheBll.GetModel(vm.conMod.IsBid);
            //if (expTask != null) { vm.ExpiredTime = expTask.ExecuteTime; }
            //---------------
            return View("AddContent", vm);
        }
        [ValidateInput(false)]
        public void Content_Add()
        {
            DataTable table = new DataTable();
            M_CommonData CData = FillContentModel(ref table, null, ref err);
            if (!string.IsNullOrEmpty(err)) { function.WriteErrMsg(err); return; }
            if (!B_NodeRole.CheckNodeAuth(adminMod, CData.NodeID, "addto")) { function.WriteErrMsg("你无权在该节点下添加内容"); return; }
            CData.GeneralID = contentBll.AddContent(table, CData);
            IsPushContent(Request.Form["pushcon_hid"], CData, table);
            IsCreateHtml(CData, table);
            IsNeedVerBak(CData);
            IsAutoTask(CData);
            Response.Redirect(CustomerPageAction.customPath + "Content/ContentShow.aspx?gid=" + CData.GeneralID);
        }
        [ValidateInput(false)]
        public void Content_AddToNew()
        {
            int Mid = Convert.ToInt32(Request.QueryString["GeneralID"]);
            M_CommonData CData = contentBll.SelReturnModel(Mid);
            DataTable table = new DataTable();
            CData = FillContentModel(ref table, CData, ref err);
            if (!string.IsNullOrEmpty(err)) { function.WriteErrMsg(err); return; }
            if (!B_NodeRole.CheckNodeAuth(adminMod, CData.NodeID, "addto")) { function.WriteErrMsg("你无权在该节点下增加内容"); return; }
            CData.GeneralID = contentBll.AddContent(table, CData);
            IsPushContent(Request.Form["pushcon_hid"], CData, table);
            IsCreateHtml(CData, table);
            IsNeedVerBak(CData);
            IsAutoTask(CData);
            Response.Redirect(CustomerPageAction.customPath + "Content/ContentShow.aspx?gid=" + CData.GeneralID);
        }
        [ValidateInput(false)]
        public void Content_Update()
        {
            int Mid = Convert.ToInt32(Request.QueryString["GeneralID"]);
            M_CommonData CData = contentBll.SelReturnModel(Mid);
            DataTable table = new DataTable();
            CData = FillContentModel(ref table, CData, ref err);
            if (!string.IsNullOrEmpty(err)) { function.WriteErrMsg(err); return; }
            if (!B_NodeRole.CheckNodeAuth(adminMod, CData.NodeID, "modify")) { function.WriteErrMsg("你无权修改该内容"); return; }
            contentBll.UpdateContent(table, CData);
            IsPushContent(Request.Form["pushcon_hid"], CData, table);
            IsCreateHtml(CData, table);
            IsNeedVerBak(CData);
            IsAutoTask(CData);
            Response.Redirect(CustomerPageAction.customPath + "Content/ContentShow.aspx?gid=" + CData.GeneralID);
        }
        [ValidateInput(false)]
        public void Content_Draft()
        {
            DataTable table = new DataTable();
            M_CommonData CData = FillContentModel(ref table, null, ref err);
            if (!string.IsNullOrEmpty(err)) { function.WriteErrMsg(err); return; }
            CData.Status = (int)ZLEnum.ConStatus.Draft;
            CData.GeneralID = contentBll.AddContent(table, CData);
            Response.Redirect(CustomerPageAction.customPath + "Content/ContentShow.aspx?gid=" + CData.GeneralID);
        }
        public string Content_API()
        {
            string action = Request["action"];
            switch (action)
            {
                case "duptitle":
                    DataTable dt = contentBll.GetByDupTitle(Request["value"]);
                    return Newtonsoft.Json.JsonConvert.SerializeObject(dt);
                default:
                    return "";
            }
        }
        public string Content_PdfAPI(int gid, string op)
        {
            M_CommonData conMod = contentBll.SelReturnModel(gid);
            M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
            if (conMod == null || conMod.GeneralID < 1) { retMod.result = "内容[" + gid + "]不存在"; return retMod.ToString(); }
            switch (op)
            {
                case "create":
                    {
                        string pdfdir = "";
                        if (string.IsNullOrEmpty(conMod.PdfLink))
                        {
                            conMod.PdfLink = ZLHelper.GetUploadDir_System("Content", "PDF") + conMod.GeneralID + ".pdf";
                        }
                        pdfdir = Path.GetDirectoryName(Server.MapPath(conMod.PdfLink));
                        if (!Directory.Exists(pdfdir)) { Directory.CreateDirectory(pdfdir); }
                        //------------------------
                        string html = GetContentHtml(conMod);
                        PdfHelper.HtmlToPdf(html, "", conMod.PdfLink);
                        contentBll.UpdateByID(conMod);
                        retMod.result = conMod.PdfLink;
                        retMod.retcode = M_APIResult.Success;
                    }
                    break;
                case "del":
                    if (!string.IsNullOrEmpty(conMod.PdfLink))
                    {
                        if (System.IO.File.Exists(Server.MapPath(conMod.PdfLink))) { System.IO.File.Delete(Server.MapPath(conMod.PdfLink)); }
                        conMod.PdfLink = "";
                        contentBll.UpdateByID(conMod);
                        retMod.retcode = M_APIResult.Success;
                    }
                    break;
                default:
                    retMod.retmsg = "[" + op + "]不存在";
                    break;
            }
            return retMod.ToString();
        }
        public string Content_WordApi(int gid, string op)
        {
            M_CommonData conMod = contentBll.SelReturnModel(gid);
            M_APIResult retMod = new M_APIResult(M_APIResult.Failed);
            if (conMod == null || conMod.GeneralID < 1) { retMod.result = "内容[" + gid + "]不存在"; return retMod.ToString(); }
            switch (op)
            {
                case "create":
                    {
                        string pdfdir = "";
                        if (string.IsNullOrEmpty(conMod.Rtype))
                        {
                            conMod.Rtype = ZLHelper.GetUploadDir_System("Content", "Word") + conMod.GeneralID + ".docx";
                        }
                        pdfdir = Path.GetDirectoryName(Server.MapPath(conMod.Rtype));
                        if (!Directory.Exists(pdfdir)) { Directory.CreateDirectory(pdfdir); }
                        //------------------------
                        string html = GetContentHtml(conMod);
                        OfficeHelper.W_HtmlToWord(html, conMod.Rtype);
                        contentBll.UpdateByID(conMod);
                        retMod.result = conMod.Rtype;
                        retMod.retcode = M_APIResult.Success;
                    }
                    break;
                case "del":
                    if (!string.IsNullOrEmpty(conMod.Rtype))
                    {
                        if (System.IO.File.Exists(Server.MapPath(conMod.Rtype))) { System.IO.File.Delete(Server.MapPath(conMod.Rtype)); }
                        conMod.Rtype = "";
                        contentBll.UpdateByID(conMod);
                        retMod.retcode = M_APIResult.Success;
                    }
                    break;
                default:
                    retMod.retmsg = "[" + op + "]不存在";
                    break;
            }
            return retMod.ToString();
        }
        /// <summary>
        /// 将指定内容,推送给指定节点
        /// </summary>
        /// <param name="ids">内容IDS</param>
        /// <param name="nids">节点IDS</param>
        /// <returns></returns>
        public string Content_Push(string ids,string nodes)
        {
            if (string.IsNullOrEmpty(ids) || string.IsNullOrEmpty(nodes)) { return "节点与内容不能为为空"; }

            string[] idsArr = ids.Split(',');
            for (int i = 0; i < idsArr.Length; i++)
            {
                M_CommonData cdata = null;
                DataTable dt = null;
                contentBll.GetContent(Convert.ToInt32(idsArr[i]), ref cdata, ref dt);
                if (cdata != null && dt != null)
                {
                    PushConToNodes(nodes, cdata, dt);
                }
            }
            return Success.ToString();
        }
        //填充共通部分
        private M_CommonData FillContentModel(ref DataTable table, M_CommonData CData, ref string err)
        {
            if (CData == null) { CData = new M_CommonData(); }
            //if (SiteConfig.SiteOption.FileRj == 1 && contentBll.SelHasTitle(Request.Form["txtTitle"])) { function.WriteErrMsg(Resources.L.该内容标题已存在 + "!", "javascript:history.go(-1);"); }
            if (CData.GeneralID < 1)
            {
                CData.Inputer = adminMod.AdminName;
                CData.NodeID = NodeID;
                CData.ModelID = ModelID;
                CData.TableName = modelBll.SelReturnModel(ModelID).TableName;
                string parentTree = "";
                CData.FirstNodeID = nodeBll.SelFirstNodeID(CData.NodeID, ref parentTree);
                CData.ParentTree = parentTree;
            }
            DataTable dt = fieldBll.SelByModelID(CData.ModelID, false);
            try
            {
                table = commonCall.GetDTFromMVC(dt, Request);
            }
            catch (Exception ex)
            {
                err = ex.Message; return null;
            }
            CData.Title = Request.Form["txtTitle"];
            CData.EliteLevel = DataConvert.CLng(Request.Form["EliteLevel"]);
            CData.Hits = DataConvert.CLng(Request.Form["Hits"]);
            CData.TagKey = Request.Form["tabinput"];
            CData.Status = DataConvert.CLng(Request.Form["ddlFlow"]);
            CData.Template = Request.Form["TemplateUrl_hid"];
            CData.CreateTime = DataConverter.CDate(Request.Form["CreateTime"]);
            CData.UpDateTime = DataConverter.CDate(Request.Form["UpDateTime"]);
            CData.SpecialID = "," + string.Join(",", DataConvert.CStr(Request.Form["Spec_Hid"]).Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)) + ",";
            CData.TitleStyle = Request.Form["ThreadStyle"];
            CData.TopImg = Request.Form["ThumImg_Hid"];//首页图片
            CData.Subtitle = Request.Form["Subtitle"];
            CData.PYtitle = Request.Form["PYtitle"];
            CData.RelatedIDS = Request.Form["RelatedIDS_Hid"];
            CData.IsComm = DataConvert.CLng(Request.Form["IsComm_Radio"]);
            CData.UGroupAuth = Request.Form["UGroupAuth"];
            CData.IsTop = DataConvert.CLng(Request.Form["IsTop"]);
            CData.IsTopTime = DataConvert.CStr(Request.Form["IsTopTime"]);
            #region  关键词
            {
                string[] ignores = DataConvert.CStr(Request.Form["Keywords"]).Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string[] newKeys = CData.TagKey.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                string keys = StrHelper.RemoveRepeat(newKeys, ignores);
                if (!string.IsNullOrEmpty(keys))
                {
                    keyBll.AddKeyWord(keys, 1);
                }
            }
            #endregion
            //积分
            //if (SiteConfig.UserConfig.InfoRule > 0)
            //{
            //    B_User buser = new B_User();
            //    M_UserInfo muser = buser.GetUserByName(adminMod.AdminName);
            //    if (!muser.IsNull)
            //    {
            //        buser.ChangeVirtualMoney(muser.UserID, new M_UserExpHis()
            //        {
            //            UserID = muser.UserID,
            //            detail = "添加内容:" + txtTitle.Text + "增加积分",
            //            score = SiteConfig.UserConfig.InfoRule,
            //            ScoreType = (int)M_UserExpHis.SType.Point
            //        });
            //    }
            //}
            return CData;
        }
        //进行版本备份
        private void IsNeedVerBak(M_CommonData conMod)
        {
            if (!string.IsNullOrEmpty(Request.Form["verbak_chk"]))
            {
                M_Content_VerBak verMod = new M_Content_VerBak();
                DataTable valueDT = contentBll.GetContent(conMod);
                verMod.GeneralID = conMod.GeneralID;
                verMod.ContentBak = JsonConvert.SerializeObject(conMod);
                verMod.TableBak = JsonConvert.SerializeObject(valueDT);
                verMod.Title = conMod.Title;
                verMod.Inputer = adminMod.AdminName;
                verMod.ZType = "content";
                verBll.Insert(verMod);
            }
        }
        //将内容推送至其他节点,附表中数据只需要一份
        private void IsPushContent(string nodes, M_CommonData CData, DataTable table)
        {
            if (string.IsNullOrEmpty(nodes)) { return; }
            string[] nodeArr = nodes.Trim(',').Split(',');
            for (int i = 0; i < nodeArr.Length; i++)
            {
                CData.NodeID = Convert.ToInt32(nodeArr[i]);
                contentBll.AddContent(table, CData);
            }
        }
        //是否需要生成或重新生成html
        private void IsCreateHtml(M_CommonData CData, DataTable table)
        {
            string ischk = Request["quickmake"];
            if (!string.IsNullOrEmpty(ischk))
            {
                CreateHtmlDel createHtml = CreateHtmlFunc;
                createHtml.BeginInvoke(HttpContext.ApplicationInstance.Context.Request, CData, table, null, null);
            }
        }
        //添加计划任务(自动审核、过期),
        private void IsAutoTask(M_CommonData CData)
        {
            JObject taskInfo = null;
            if (!string.IsNullOrEmpty(CData.TaskInfo))
            {
                taskInfo = JsonConvert.DeserializeObject<JObject>(CData.TaskInfo);
            }
            else
            {
                taskInfo = new JObject();
            }
            taskInfo = AddContentTask(CData, taskInfo, "audit", Request.Form["ExamineTime"]);
            taskInfo = AddContentTask(CData, taskInfo, "expire", Request.Form["ExpiredTime"]);
            string taskInfoStr = JsonConvert.SerializeObject(taskInfo);
            if (!CData.TaskInfo.Equals(taskInfoStr))
            {
                CData.TaskInfo = taskInfoStr;
                contentBll.UpdateByID(CData);
            }
        }
        /// <summary>
        /// 根据判断,增加自动审核|过期任务
        /// </summary>
        /// <param name="CData">当前内容模型</param>
        /// <param name="taskInfo">任务信息</param>
        /// <param name="name">audit|expire</param>
        /// <param name="timeStr">日期时间字符串</param>
        /// <returns></returns>
        private JObject AddContentTask(M_CommonData CData, JObject taskInfo, string name, string timeStr)
        {
            //audit
            if (taskInfo[name] == null)
            {
                taskInfo.Add(name, 0);
                taskInfo.Add(name + "_time", "");
            }
            int taskId = DataConvert.CLng(taskInfo[name]);
            //如果未设置任务,并且指定了时间字符串,则新建
            if (taskId == 0 && !string.IsNullOrEmpty(timeStr))
            {
                DateTime time = DateTime.Now;
                if (DateTime.TryParse(timeStr, out time))
                {
                    taskInfo[name] = AddTask(CData.GeneralID.ToString(), name, time);
                    taskInfo[name + "_time"] = timeStr;
                }
            }
            //对比时间是否修改,如果修改,则创建任务
            else if (taskId > 0)
            {
                string taskTimeStr = taskInfo[name + "_time"].ToString();
                M_Content_ScheTask taskMod = taskBll.GetModel(taskId);
                //不匹配则修改
                if (!taskTimeStr.Equals(timeStr))
                {
                    if (string.IsNullOrEmpty(timeStr))
                    {
                        taskInfo[name] = 0;
                        taskInfo[name + "_time"] = "";
                        taskBll.DelByID(taskId.ToString());
                    }
                    else
                    {
                        taskMod.ExecuteTime = timeStr;
                        taskBll.Update(taskMod);
                        taskInfo[name + "_time"] = timeStr;
                    }
                }
            }
            return taskInfo;
        }
        //添加的同时,异步生成静态页
        private delegate void CreateHtmlDel(HttpRequest r, M_CommonData cdate, DataTable table);
        private void CreateHtmlFunc(HttpRequest r, M_CommonData cdata, DataTable table)
        {
            M_Node noinfo = nodeBll.GetNodeXML(cdata.NodeID);
            if (noinfo.ListPageHtmlEx < 3)
            {
                B_Create CreateBll = new B_Create(r);
                CreateBll.createann(cdata.GeneralID.ToString());//发布内容页
                CreateBll.CreateColumnByID(cdata.NodeID.ToString());//发布栏目页
                CreateBll.CreatePageIndex(); //发布首页
            }
            cdata = contentBll.SelReturnModel(cdata.GeneralID);
            contentBll.UpdateContent(table, cdata);
        }
        ZoomLa.BLL.Helper.HtmlHelper htmlHelp = new ZoomLa.BLL.Helper.HtmlHelper();
        //用于生成pdf与word
        private string GetContentHtml(M_CommonData conMod)
        {
            string html = "", conhtml = "";
            DataTable dt = contentBll.GetContent(conMod.GeneralID);
            if (dt != null && dt.Rows.Count > 0 && dt.Columns.Contains("content")) { conhtml = DataConvert.CStr(dt.Rows[0]["content"]); }
            html = "<div style='text-align:center;font-size:30px;font-weight:bolder;'><h1>" + conMod.Title + "</h1></div>";
            html += "<div style='text-align:center;'>作者：" + conMod.Inputer + "</div>";
            html = html + conhtml;
            html = htmlHelp.ConvertImgUrl(html, SiteConfig.SiteInfo.SiteUrl);
            html = "<html><body>" + html + "</body></html>";
            return html;
        }
        private void PushConToNodes(string nodes, M_CommonData cdata, DataTable dt)
        {
            nodes = nodes.Trim(',').Replace(" ", "");
            //if (string.IsNullOrEmpty(nodes)) { return; }
            int startNode = cdata.NodeID;
            string[] nodeArr = nodes.Split(',');
            for (int i = 0; i < nodeArr.Length; i++)
            {
                cdata.NodeID = Convert.ToInt32(nodeArr[i]);
                if (startNode == cdata.NodeID) { continue; }//不能将文章推送到自己节点
                cdata.FirstNodeID = nodeBll.SelFirstNodeID(cdata.NodeID);
                PushAddCon(dt, cdata);
            }
        }
        private void PushAddCon(DataTable dt, M_CommonData cdata)
        {
            //nsert into 库B.dbo.AA select * from 库A.dbo.AA where 库A.dbo.AA.C = ’‘
            DataRow dr = dt.Rows[0];
            string sql = "INSERT INTO {1} SELECT {0} FROM {1} WHERE [ID] = " + dr["ID"] + ";SELECT @@identity;";
            string columns = "";
            foreach (DataColumn dc in dt.Columns)
            {
                if (dc.ColumnName.ToLower().Equals("id")) { continue; }
                columns += dc.ColumnName + ",";
            }
            columns = columns.Trim(',');
            cdata.ItemID = Convert.ToInt32(SqlHelper.ExecuteScalar(CommandType.Text, string.Format(sql, columns, cdata.TableName)));
            contentBll.insert(cdata);
        }
        /*************************************************************************************************/
        #region 内容版本管理
        public ActionResult Addon_VerBak()
        {
            PageSetting setting = verBll.SelPage(CPage, PSize, DataConvert.CLng(Request["GeneralID"]));
            return View("Addon/VerBak", setting);
        }
        public int Addon_VerBak_Del(string ids)
        {
            verBll.Del(ids);
            return M_APIResult.Success;
        }
        #endregion
        #region ContentManage
        public ActionResult ContentManage()
        {
            //if (!adminMod.IsSuperAdmin() && NodeID < 1) { function.WriteErrMsg("没有指定节点ID"); return null; }
            VM_ContentManage vm = FillVMContentManage(ref err);
            if (!string.IsNullOrEmpty(err)) { function.WriteErrMsg(err); return null; }
            //测试是否可直接收值
            if (vm.nodeMod != null && vm.nodeMod.NodeListType == 2) { Response.Redirect(CustomerPageAction.customPath2 + "Shop/ProductManage.aspx?NodeID=" + NodeID); return null; }
            if (function.isAjax()) { return PartialView("ContentManage_List", vm); }
            else { return View(vm); }
        }
        public string ContentManage_API()
        {
            string result = "";
            switch (Request.Form["action"])
            {
                case "move":
                    string direct = Request.Form["direct"];
                    int curid = DataConvert.CLng(Request.Form["curid"]), tarid = DataConvert.CLng(Request.Form["tarid"]);

                    M_CommonData curMod = contentBll.GetCommonData(curid);
                    M_CommonData tarMod = contentBll.GetCommonData(tarid);
                    if (curMod.OrderID == tarMod.OrderID)
                    {
                        switch (direct)
                        {
                            case "up":
                                curMod.OrderID++;
                                break;
                            case "down":
                                curMod.OrderID--;
                                break;
                        }
                    }
                    else
                    {
                        int temp = curMod.OrderID;
                        curMod.OrderID = tarMod.OrderID;
                        tarMod.OrderID = temp;
                    }
                    contentBll.UpdateByID(curMod); contentBll.UpdateByID(tarMod);
                    result = "true";
                    break;
                default:
                    break;
            }
            return result;
        }
        public int ContentManage_Html(int id, string action)
        {
            B_Create createBll = new B_Create();
            M_CommonData conMod = contentBll.SelReturnModel(id);
            switch (action)
            {
                case "create":
                    {
                        conMod.IsCreate = 1;
                        contentBll.UpdateByID(conMod);
                        createBll.CreateInfo(conMod.GeneralID, conMod.NodeID, conMod.ModelID);
                        createBll.CreatePageIndex();
                    }
                    break;
                case "del":
                    {
                        if (!string.IsNullOrEmpty(conMod.HtmlLink))
                        {
                            conMod.HtmlLink = "";
                            SafeSC.DelFile(conMod.HtmlLink);
                        }
                        conMod.IsCreate = 0;
                        createBll.CreatePageIndex();
                        contentBll.UpdateByID(conMod);
                    }
                    break;
            }
            return M_APIResult.Success;
        }
        public int ContentManage_Elite(string ids, int elite)
        {
            contentBll.UpdateElite(ids, elite);
            return M_APIResult.Success;
        }
        public string ContentManage_Status(string ids, int status)
        {
            //if (!B_NodeRole.CheckNodeAuth(adminMod, NodeID, "state")) { return "你没有修改状态的权限"; }
            //删除权限单独限验证
            if (status == (int)ZLEnum.ConStatus.Recycle) { return "该API不允许删除"; }
            contentBll.UpdateStatus(ids, status);
            return M_APIResult.Success.ToString();
        }
        public string ContentManage_Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return "未指定需要删除的内容"; }
            //用户只能删除自己有管理权限的节点下的内容
            string[] nids = badmin.GetNodeAuthStr("modify").Split(',');
            bool isSuper = adminMod.IsSuperAdmin();
            foreach (string item in ids.Split(','))
            {
                int id = DataConvert.CLng(item);
                if (id < 1) { continue; }
                M_CommonData conMod = contentBll.GetCommonData(id);
                //超管或其角色有对应的操作权限
                if (isSuper || nids.FirstOrDefault(p => p.Equals(conMod.NodeID.ToString())) != null)
                {
                    if (!string.IsNullOrEmpty(conMod.HtmlLink)) { SafeSC.DelFile(conMod.HtmlLink); }
                    conMod.Status = (int)ZLEnum.ConStatus.Recycle;
                    conMod.IsCreate = 0;
                    conMod.HtmlLink = "";
                    contentBll.UpdateByID(conMod);
                }
            }
            return M_APIResult.Success.ToString();
        }
        //刷新，将内容添加时间和最后更新时间改为当前时间
        public string ContentManage_Refresh(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return "未指定需要刷新的内容"; }
            foreach (string item in ids.Split(','))
            {
                M_CommonData conMod = contentBll.GetCommonData(DataConverter.CLng(item));
                if (conMod != null)
                {
                    conMod.CreateTime = DateTime.Now;
                    conMod.UpDateTime = DateTime.Now;
                    contentBll.UpdateByID(conMod);
                }
            }
            return M_APIResult.Success.ToString();
        }
        public void ContentManage_ToExcel()
        {
            VM_ContentManage vm = FillVMContentManage(ref err);
            if (!string.IsNullOrEmpty(err)) { function.WriteErrMsg(err); return; }
            DataTable dt = vm.setting.dt;
            dt.Columns.Add("ShowTitle");
            dt.Columns.Add("ShowElite");
            dt.Columns.Add("ShowStatus");
            foreach (DataRow dr in dt.Rows)
            {
                dr["ShowTitle"] = NodeID == 0 ? "[" + dr["NodeName"] + "]" + dr["Title"] : dr["Title"];
                dr["ShowElite"] = conhelp.GetElite(dr["EliteLevel"].ToString());
                dr["ShowStatus"] = conhelp.GetStatus(DataConvert.CLng(dr["Status"]));
            }
            DataTable newDt = dt.DefaultView.ToTable(false, "GeneralID,ShowTitle,Inputer,Hits,ShowElite,ShowStatus,CreateTime,UpDateTime".Split(','));
            string columnames = "ID,标题,录入者,点击数,推荐,状态,添加时间,修改时间";
            M_Node nodeMod = nodeBll.SelReturnModel(NodeID);
            string nodename = nodeMod == null ? "" : "[" + nodeMod.NodeName + "]";
            SafeSC.DownStr(OfficeHelper.ExportExcel(newDt, columnames), DateTime.Now.ToString("yyyyMMdd") + nodename + "内容管理表.xls");
        }
        public string ContentManage_TimeRelease(string ids, string retime)
        {
            if (!B_NodeRole.CheckNodeAuth(adminMod, NodeID, "state")) { return "你没有修改状态的权限"; }
            if (string.IsNullOrEmpty(ids)) { return "未选择文章"; }
            DateTime executeTime = DataConverter.CDate(retime);
            if (executeTime <= DateTime.Now) { return "执行时间无效"; }
            AddTask(ids, "定时发布", executeTime);
            return M_APIResult.Success.ToString();
        }
        //创建任务
        private int AddTask(string content, string title, DateTime etime, ZLEnum.ConStatus status = ZLEnum.ConStatus.Audited)
        {
            M_Content_ScheTask scheModel = new M_Content_ScheTask();
            scheModel.TaskName = title;
            scheModel.TaskContent = content;//ID
            scheModel.TaskType = (int)M_Content_ScheTask.TaskTypeEnum.Content;//根据这个调用不同的方法处理TaskContent
            scheModel.ExecuteType = (int)M_Content_ScheTask.ExecuteTypeEnum.JustOnce;
            scheModel.ExecuteTime = etime.ToString();
            scheModel.Remind = ((int)status).ToString();
            scheModel.AdminID = adminMod.AdminId;
            scheModel.ID = scheBll.Add(scheModel);
            TaskCenter.AddTask(scheModel);
            return scheModel.ID;
        }
        private VM_ContentManage FillVMContentManage(ref string err)
        {
            PSize = 20;
            Filter_Content filter = new Filter_Content();
            VM_ContentManage vm = new VM_ContentManage();
            vm.nodeMod = nodeBll.SelReturnModel(NodeID);
            vm.filter.NodeID = vm.NodeID;
            vm.filter.ModelID = ModelID;
            vm.filter.KeyType = DataConvert.CLng(Request["KeyType"]);
            vm.filter.KeyWord = HttpUtility.UrlDecode(Request["KeyWord"] ?? "");
            vm.filter.Status = DataConvert.CStr(Request["Status"]);
            //----------------工作流,其与角色绑定，不分是否超管(需将其改为视图)
            //DataTable dts = new DataTable();
            //if (SelType == 5)//工作流审批
            //{
            //    if (vm.NodeID > 0)
            //    {
            //        dts = bll.GetDTByAuth(adminMod.RoleList, CNodeID);
            //    }
            //    else//获取全部
            //    {
            //        dts = bll.GetDTByAuth(adminMod.RoleList);
            //    }
            //    btnUnAudit.Visible = false;
            //    audit_Div.Visible = true;
            //    return dts;
            //}
            if (!adminMod.IsSuperAdmin())//非超级管理员(用视图,组合权限表)
            {
                //筛选数据,如何筛选
                DataTable authDT = badmin.GetNodeAuthList("look");
                if (authDT == null || authDT.Rows.Count < 1) { err = "你尚未配置内容管理角色"; return null; };//没有分配角色,或权限为空
                string nodes = "";
                foreach (DataRow dr in authDT.Rows)
                {
                    nodes += dr["NID"].ToString() + ",";
                }
                vm.filter.authNodes = nodes;
                if (vm.NodeID == 0 && !string.IsNullOrEmpty(nodes)) //如果是全部文章,则筛选后输出
                {
                    vm.setting = contentBll.SelPage(CPage, PSize, vm.filter);
                }
                else //如果是节点,判断用户权限,避免其直接通过URL地址进入
                {
                    if (("," + nodes).Contains("," + vm.NodeID + ","))
                    {
                        vm.setting = contentBll.SelPage(CPage, PSize, vm.filter);
                    }
                    else
                    {
                        err = "你没有该节点的访问权限，请联系[系统管理员]!";
                        return null;
                    }
                }
            }
            else //超级管理员
            {
                vm.setting = contentBll.SelPage(CPage, PSize, vm.filter);
            }
            vm.Count_WZS = vm.setting.itemCount;
            vm.Count_DJS = DataConvert.CLng(vm.setting.addon);
            return vm;
        }
        #endregion
        #region 导入数据
        string excel_headers = "标题,审核状态,添加时间,更新时间,点击数";
        //根据csv或xlsx,将数据导入数据库
        public void ContentManage_Import()
        {
            HttpPostedFileBase file = Request.Files["fileImp"];
            if (file == null) { function.WriteErrMsg("没有上传数据文件"); return; }
            string exName = Path.GetExtension(file.FileName).ToLower();
            if (!exName.Equals(".csv") && !exName.Equals(".xls") && !exName.Equals(".xlsx"))//判断扩展名
            {
                function.WriteErrMsg("上传的文件不是符合扩展名csv,请重新选择!"); return;
            }
            string vpath = SafeSC.SaveFile(ZLHelper.GetUploadDir_System("Content", "Import", "yyyyMMdd"), function.GetRandomString(6) + exName, IOHelper.StreamToBytes(file.InputStream));
            //导入文件到数据集对象                   
            DataTable dt = null;
            if (exName.Equals(".csv"))
            {
                OfficeHelper office = new OfficeHelper();
                dt = office.OpenCSV(vpath);
            }
            else
            {
                SqlBase excel = SqlBase.CreateHelper("excel");
                excel.ConnectionString = vpath;
                dt = excel.ExecuteTable(new SqlModel() { tbName = excel.Table_List().Rows[0]["name"].ToString() });
            }
            SafeSC.DelFile(vpath);
            DataSet newDs = Import_CreateTable(dt);
            Import_SaveDB(newDs);//保存到数据库    
        }
        //生成Excel模板
        public void Import_ToTemplate()
        {
            //根据模型ID查询字段
            DataTable dt = fieldBll.GetModelFieldList(ModelID);
            // 生成Excel模板表头,并回发
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                excel_headers += "," + dt.Rows[i]["FieldAlias"].ToString();
            }
            string fname = DateTime.Now.ToString("yyyyMMdd") + modelBll.SelReturnModel(ModelID).ItemName + "导入模板";
            Response.AddHeader("Content-Disposition", "attachment; filename=" + Server.UrlEncode(fname) + ".csv");
            Response.ContentEncoding = System.Text.Encoding.GetEncoding("GB2312");
            Response.Clear(); Response.Write(excel_headers); Response.Flush(); Response.End();
        }
        // 生成新的主从表
        private DataSet Import_CreateTable(DataTable dt)
        {
            dt.Columns.Add(new DataColumn("No",typeof(string)));
            dt.Columns.Add(new DataColumn("Up",typeof(string)));
            dt.Columns["No"].SetOrdinal(0);
            dt.Columns["Up"].SetOrdinal(2);
            for (int j = 0; j < dt.Rows.Count; j++)
            {
                dt.Rows[j]["No"] = j + 1;
                dt.Rows[j]["Up"] = 0;
            }

            int index = 3, sp_no = 2;

            DataTable t1 = new DataTable();
            DataColumn c1 = dt.Columns[0];
            t1.Columns.Add(new DataColumn(c1.ColumnName, c1.DataType));
            for (int i = 1; i < index; i++)
            {
                DataColumn col = dt.Columns[i];
                t1.Columns.Add(new DataColumn(col.ColumnName, col.DataType));
            }

            DataTable t2 = new DataTable();
            DataColumn c2 = dt.Columns[0];
            t2.Columns.Add(new DataColumn(c2.ColumnName, c2.DataType));
            for (int i = index; i < dt.Columns.Count; i++)
            {
                DataColumn col = dt.Columns[i];
                t2.Columns.Add(new DataColumn(col.ColumnName, col.DataType));
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow r = t1.NewRow();
                r[0] = dt.Rows[i][0];
                for (int j = 1; j < index; j++)
                {
                    r[j] = dt.Rows[i][j];
                }
                //t1.ImportRow(r);
                t1.Rows.Add(r);
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow r = t2.NewRow();
                r[0] = dt.Rows[i][0];
                for (int j = index; j < dt.Columns.Count; j++)
                {
                    r[j - sp_no] = dt.Rows[i][j];
                }
                t2.Rows.Add(r);
            }
            DataSet newDs = new DataSet();
            newDs.Tables.Add(t1);
            newDs.Tables.Add(t2);
            return newDs;
        }
        // 导入excel数据到数据库
        private void Import_SaveDB(DataSet ds)
        {
            if (ds.Tables[1].Rows.Count < 1) { function.WriteErrMsg("没有要导入的数据"); }
            //根据模型号查询字段
            DataTable dt = fieldBll.GetModelFieldList(ModelID);
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("FieldName", typeof(string)));
            table.Columns.Add(new DataColumn("FieldType", typeof(string)));
            table.Columns.Add(new DataColumn("FieldValue", typeof(string)));
            int colCount = ds.Tables[1].Columns.Count;
            foreach (DataRow childRow in ds.Tables[1].Rows)
            {
                table.Rows.Clear();
                #region 构建从表内容表
                int colNo = excel_headers.Split(',').Length;//从第几列开始
                foreach (DataRow dr in dt.Rows)
                {
                    DataRow row = table.NewRow();
                    row["FieldName"] = dr["FieldName"].ToString();
                    row["FieldType"] = dr["FieldType"].ToString();
                    row["FieldValue"] = childRow[colNo].ToString();//excel或csv中的数据
                    table.Rows.Add(row);
                    colNo++;
                    if (colNo == colCount) { break; }
                }
                #endregion
                #region 构建主表
                ds.Tables[0].DefaultView.RowFilter = "No=" + childRow["No"].ToString();
                ds.Tables[1].DefaultView.RowFilter = "No=" + childRow["No"].ToString();
                DataRow mainRow = ds.Tables[0].DefaultView[0].Row;
                DataRow mainRow2 = ds.Tables[1].DefaultView[0].Row;
                M_CommonData CData = new M_CommonData();
                CData.NodeID = NodeID;
                CData.ModelID = ModelID;
                CData.TableName = modelBll.SelReturnModel(ModelID).TableName;
                CData.Title = mainRow[1].ToString();
                CData.Inputer = badmin.GetAdminLogin().AdminName;
                CData.EliteLevel = 0;
                CData.Hits = DataConvert.CLng(mainRow2["点击数"]);
                CData.CreateTime = DataConvert.CDate(mainRow2["添加时间"].ToString());
                CData.UpDateTime = DataConvert.CDate(mainRow2["更新时间"].ToString());
                CData.Status = DataConverter.CLng(mainRow2[1].ToString());
                #endregion
                contentBll.AddContent(table, CData);
            }
            function.WriteSuccessMsg("导入成功", "ContentManage"); return;
        }
        #endregion
        #region MarkDown
        B_Content_MarkDown mdBll = new B_Content_MarkDown();
        public ActionResult MarkDown()
        {
            PageSetting setting = new PageSetting();
            setting = mdBll.SelPage(CPage, PSize, new Com_Filter() {  });
            if (function.isAjax()) { return PartialView("Comment/Markdown_List", setting); }
            return View("Comment/MarkDown",setting);
        }
        public string MarkDown_Del(string ids)
        {
            mdBll.Del(ids);
            return Success.ToString();
        }
        #endregion

        public string Node_API()
        {
            string action = Request.QueryString["action"];
            string ids = Request.Form["ids"];
            switch (action)
            {
                case "del":
                    B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.system, "node");
                    nodeBll.DelToRecycle(ids);
                    break;
            }
            return Success.ToString();
        }
        public ActionResult NodeManage()
        {
            B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.system, "node");
            return View();
        }
    }
}
