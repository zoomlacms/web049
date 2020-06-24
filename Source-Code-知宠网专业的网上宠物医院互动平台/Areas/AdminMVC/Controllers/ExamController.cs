using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.AppCode.Controls;
using ZoomLa.BLL;
using ZoomLa.BLL.Exam;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Exam;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Exam;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class ExamController : Ctrl_Admin
    {
        B_Exam_Class classBll = new B_Exam_Class();
        B_Exam_Sys_Papers paperBll = new B_Exam_Sys_Papers();
        B_Exam_Sys_Questions questBll = new B_Exam_Sys_Questions();
        B_Questions_Knowledge knowBll = new B_Questions_Knowledge();
        B_User buser = new B_User();
        B_Exam_Answer answerBll = new B_Exam_Answer();
        B_Exam_Version verBll = new B_Exam_Version();
        B_School schBll = new B_School();
        B_ClassRoom croomBll = new B_ClassRoom();
        B_Student stuBll = new B_Student();
        B_ExTeacher teaBll = new B_ExTeacher();
        B_Course courBll = new B_Course();
        B_ExamPoint pointBll = new B_ExamPoint();
        public ActionResult Papers_System_Manage()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "paper")) { return null; }
            int PaperClass = DataConverter.CLng(Request.QueryString["type"]);
            int NodeID = DataConverter.CLng(Request.QueryString["NodeID"]);
            PageSetting setting = paperBll.SelAll_SPage(CPage, PSize, NodeID, Request["skey"]);
            if (Request.IsAjaxRequest()) { return PartialView("Papers_System_Manage_List", setting); }
            ViewBag.nodeid = NodeID;
            return View(setting);
        }
        public ActionResult Papers_System_Add()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "paper")) { return null; }
            int NodeID = DataConverter.CLng(Request.QueryString["NodeID"]);
            M_Exam_Sys_Papers paperMod = paperBll.SelReturnModel(Mid);
            if (paperMod != null)
            {
                ViewBag.title = "修改试卷";
            }
            else
            {
                paperMod = new M_Exam_Sys_Papers() { p_class = NodeID == 0 ? 1 : NodeID };
                ViewBag.title = "添加试卷";
                if (NodeID > 0)
                {
                    ViewBag.title += "[" + classBll.GetSelect(NodeID).C_ClassName + "]";
                }
            }
            C_TreeTlpDP treeMod = GetTreeMod();
            if (paperMod.p_class > 0) { treeMod.seled = paperMod.p_class.ToString(); }
            ViewBag.treeMod = treeMod;
            return View(paperMod);
        }
        public ActionResult Paper_QuestionManage()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "paper")) { return null; }
            int Pid = DataConverter.CLng(Request.QueryString["pid"]);
            int QType = DataConverter.CLng(Request.QueryString["qtype"]);
            M_Exam_Sys_Papers paperMod = paperBll.GetSelect(Pid);
            PageSetting setting = questBll.SelByIDS_SPage(CPage, PSize, paperMod.QIDS, QType);
            if (setting == null) { setting = new PageSetting() { itemCount = 0 }; }
            if (Request.IsAjaxRequest()) { return PartialView("Paper_QuestionManage_List", setting); }
            ViewBag.ptitle = StringHelper.SubStr(paperMod.p_name, 10);
            ViewBag.selids = "," + paperMod.QIDS + ",";
            return View(setting);
        }
        public ActionResult ViewPaperCenter()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "paper")) { return null; }
            if (Mid < 1) { function.WriteErrMsg("请输入试卷ID"); return null; }
            M_Exam_Sys_Papers paperMod = paperBll.SelReturnModel(Mid);
            ViewBag.title = paperMod.p_name;
            ViewBag.paperid = paperMod.id;
            ViewBag.desc = paperMod.p_Remark;
            //获取问题,自动组卷则筛选合适的IDS
            DataTable questDt = questBll.SelByIDSForExam(paperMod.QIDS, Mid) ?? new DataTable();
            DataTable typeDt = answerBll.GetTypeDT(questDt) ?? new DataTable();
            ViewBag.questDt = questDt;
            ViewBag.typeDt = typeDt;
            return View();
        }
        public ActionResult QuestList()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "paper")) { return null; }
            int NodeID = DataConverter.CLng(Request.QueryString["nodeid"]);
            int QType = DataConverter.CLng(Request.QueryString["qtype"], 99);
            int Diff = DataConverter.CLng(Request.QueryString["diff"]);
            int Version = DataConverter.CLng(Request.QueryString["version"]);
            string KeyWord = Request["keyWord"] ?? "";
            int Grade = DataConverter.CLng(Request["grade"]);
            PageSetting setting = questBll.SelPageByFilter(CPage, PSize, NodeID, QType, Grade, Diff, Version, KeyWord, 0);
            if (Request.IsAjaxRequest()) { return PartialView("QuestList_List", setting); }
            if (NodeID > 0) { ViewBag.nodename = classBll.GetSelect(NodeID).C_ClassName; }
            else if (Grade > 0) { ViewBag.nodename = B_GradeOption.GetGradeOption(Grade).GradeName; }
            else { ViewBag.nodename = "全部试题"; }
            ViewBag.nodeid = NodeID;
            ViewBag.grade = Grade;
            ViewBag.qtype = QType;
            DataTable gradeDt = B_GradeOption.GetGradeList(6, 0);
            DataRow dr = gradeDt.NewRow();
            dr["GradeName"] = "全部"; dr["GradeID"] = 0;
            gradeDt.Rows.InsertAt(dr, 0);
            ViewBag.gradelist = MVCHelper.ToSelectList(gradeDt, "GradeName", "GradeID");
            return View(setting);
        }
        public ActionResult AddEngLishQuestion()
        {
            int nodeid = DataConverter.CLng(Request.QueryString["NodeID"]);
            VM_Question model = new VM_Question();
            model.treeMod = GetTreeMod();
            if (model.questMod.p_Class > 0) { model.treeMod.seled = model.questMod.p_Class.ToString(); }
            else if (nodeid > 0) { model.treeMod.seled = nodeid.ToString(); }
            ViewBag.tagkey = knowBll.GetNamesByIDS(model.questMod.Tagkey);
            return View(model);
        }
        public ActionResult QuestShow()
        {
            M_Exam_Sys_Questions model = questBll.GetSelect(Mid);
            if (model == null) { function.WriteErrMsg("试题不存在"); return null; }
            DataTable dt = questBll.SelByIDSForExam(model.p_id.ToString());
            dt.DefaultView.RowFilter = "pid>0";
            dt.DefaultView.Sort = "order desc";
            ViewBag.questDt = dt.DefaultView.ToTable();
            return View(model);
        }
        public ActionResult Setting()
        {
            string stime = "", etime = "";
            DateHelper.GetWeekSE(DateTime.Now.AddDays(-7), ref stime, ref etime);
            ViewBag.stime = stime;
            ViewBag.etime = etime;
            return View();
        }
        public ActionResult Question_Class_Manage()
        {
            B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "paper");
            return View(classBll.GetSelectByC_ClassId(0));
        }
        public ActionResult AddQuestion_Class()
        {
            M_Exam_Class classMod = classBll.GetSelect(Mid);
            if (classMod.C_id < 1) { classMod.C_Classid = DataConverter.CLng(Request.QueryString["pid"]); }
            DataTable dt = classBll.Select_All();
            DataRow dr = dt.NewRow();
            dr["C_ClassName"] = "请选择"; dr["C_id"] = 0;
            dt.Rows.InsertAt(dr, 0);
            ViewBag.clist = MVCHelper.ToSelectList(dt, "C_ClassName", "C_id", classMod.C_Classid.ToString());
            return View(classMod);
        }
        public ActionResult QuestGrade()
        {
            int CateID = DataConverter.CLng(Request.QueryString["cate"]);
            string cateName = "题型";
            string paraName = "Grade";
            switch (CateID)
            {
                case 4: cateName = "题型"; break;
                case 5: cateName = "难度"; paraName = "diff"; break;
                case 6: cateName = "年级"; paraName = "Grade"; break;
                case 7: cateName = "教材版本"; paraName = "Version"; break;
            }
            ViewBag.cateName = cateName;
            ViewBag.paraName = paraName;
            PageSetting setting = B_GradeOption.GetGradeList_SPage(CPage, PSize, CateID, 0);
            if (Request.IsAjaxRequest()) { return PartialView("QuestGrade_List", setting); }
            return View(setting);
        }
        public ActionResult VersionList()
        {
            return View();
        }
        public ActionResult AddVersion()
        {
            int Pid = DataConverter.CLng(Request.QueryString["pid"]);
            int action = 0;
            if (Mid > 0)//修改
            {
                M_Exam_Version Mod = verBll.SelReturnModel(Mid);
                if (Mod.Pid > 0)
                {
                    M_Exam_Version pverMod = verBll.SelReturnModel(Mod.Pid);
                    if (pverMod.Pid > 0) { action = -3; }//修改课程、知识点
                    else { action = -2; }//修改章节
                }
                else { action = -1; }//修改版本
            }
            else//添加
            {
                if (Pid > 0)
                {
                    M_Exam_Version pverMod = verBll.SelReturnModel(Pid);
                    if (pverMod.Pid > 0) { action = 3; } //添加课程，知识点
                    else { action = 2; }//添加章节
                }
                else { action = 1; }//添加版本
            }
            M_Exam_Version verMod = verBll.SelReturnModel(Mid);
            if (verMod == null) { verMod = new M_Exam_Version(); }
            switch (action)
            {
                case 2:
                case 3:
                    LoadParent(Pid, verMod, action);
                    break;
                case -2:
                case -3:
                    LoadParent(verMod.Pid, verMod, action);
                    break;
            }

            ViewBag.action = action;
            ViewBag.tagkey = string.IsNullOrEmpty(verMod.Knows) ? "" : knowBll.GetNamesByIDS(verMod.Knows);
            ViewBag.inputer = verMod.ID > 0 ? verMod.Inputer : badmin.GetAdminLogin().UserName;
            C_TreeTlpDP treeMod = GetTreeMod();
            if (verMod.ID <= 0 && Pid > 0) { treeMod.seled = verBll.SelReturnModel(Pid).NodeID.ToString(); }
            else if(verMod.NodeID > 0){ treeMod.seled = verMod.NodeID.ToString(); }
            ViewBag.treeMod = treeMod;//TreeTlp_Hid
            ViewBag.gradedt = B_GradeOption.GetGradeList(6, 0);
            return View(verMod);
        }
        protected void LoadParent(int pid, M_Exam_Version verMod, int action = 0)
        {
            //当作为子级添加时，版本名称、科目、版本时间、册序、年级、价格不可编辑，值与父级相同
            M_Exam_Version pverMod = verBll.SelReturnModel(pid);
            verMod.VersionName = pverMod.VersionName;
            verMod.Inputer = pverMod.Inputer;
            verMod.VersionTime = pverMod.VersionTime;
            verMod.Grade = pverMod.Grade;
            verMod.Volume = pverMod.Volume;
            verMod.Price = pverMod.Price;
            if (action == 3 || action == -3)
            {
                verMod.Chapter = pverMod.Chapter;
            }
        }
        public ActionResult SchoolManage()
        {
            string sname = Request["sname_para"];
            string province = DataConverter.CBool(Request["nopro_chk"]) ? "" : Request["pro_para"];
            string city = DataConverter.CBool(Request["nocity_chk"]) ? "" : Request["city_para"];
            string county = DataConverter.CBool(Request["nocounty_chk"]) ? "" : Request["county_para"];
            PageSetting setting = schBll.SelPage(CPage, PSize, sname, province, city, county);
            if (Request.IsAjaxRequest()) { return PartialView("SchoolManage_List", setting); }
            return View(setting);
        }
        public ActionResult AddSchool()
        {
            M_School schMod = schBll.SelReturnModel(Mid);
            if (schMod == null) { schMod = new M_School() { SchoolType = 1, Visage = 1 }; }
            return View(schMod);
        }
        public ActionResult SchoolShow()
        {
            M_School schMod = schBll.SelReturnModel(Mid);
            return View(schMod);
        }
        public ActionResult ClassRoomManage()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.exam, "class")) { return null; }
            int status = DataConverter.CLng(Request["status"], -1);
            string skey = Request["skey"];
            PageSetting setting = croomBll.SelPage_All(CPage, PSize, 0, status, skey);
            if (Request.IsAjaxRequest()) { return PartialView("ClassRoomManage_List", setting); }
            return View(setting);
        }
        public ActionResult ClassRoomShow()
        {
            M_ClassRoom croomMod = croomBll.SelReturnModel(Mid);
            if (croomMod == null) { function.WriteErrMsg("班级不存在"); }
            string schoolName = "";
            if (croomMod.SchoolID > 0) { schoolName = schBll.SelReturnModel(croomMod.SchoolID).SchoolName; }
            ViewBag.schName = schoolName;
            return View(croomMod);
        }
        public ActionResult AddClassRoom()
        {
            M_ClassRoom croomMod = croomBll.SelReturnModel(Mid);
            if (croomMod == null) { croomMod = new M_ClassRoom(); }
            ViewBag.gradelist = MVCHelper.ToSelectList(B_GradeOption.GetGradeList(6, 0), "GradeName", "GradeID", croomMod.Grade.ToString());
            int sid = croomMod.RoomID > 0 ? croomMod.SchoolID : DataConverter.CLng(Request.QueryString["sid"]);
            M_School schMod = schBll.SelReturnModel(sid);
            ViewBag.schName = schMod == null ? "" : schMod.SchoolName;
            ViewBag.teaName = buser.SelReturnModel(croomMod.CreateUser).UserName;
            return View(croomMod);
        }
        public ActionResult StudentList()
        {
            int stuType = DataConverter.CLng(Request.QueryString["stutype"]);
            int status = DataConverter.CLng(Request["status"], -1);
            PageSetting setting = stuBll.SelByURid_SPage(CPage, PSize, Mid, status, stuType);
            if (Request.IsAjaxRequest()) { return PartialView("StudentList_List", setting); }
            return View(setting);
        }
        public ActionResult ExTeacherManage()
        {
            PageSetting setting = teaBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("ExTeacherManage_List", setting); }
            return View(setting);
        }
        public ActionResult AddExTeacher()
        {
            M_ExTeacher teaMod = teaBll.GetSelect(Mid);
            C_TreeTlpDP treeMod = GetTreeMod();
            if (teaMod.TClsss > 0) { treeMod.seled = teaMod.TClsss.ToString(); }
            ViewBag.treeMod = treeMod;
            return View(teaMod);
        }
        public ActionResult CourseManage()
        {
            PageSetting setting = courBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("CourseManage_List", setting); }
            return View(setting);
        }
        public ActionResult AddCourse()
        {
            M_Course courMod = courBll.GetSelect(Mid);
            C_TreeTlpDP treeMod = GetTreeMod();
            if (courMod.CoureseClass > 0) { treeMod.seled = courMod.CoureseClass.ToString(); }
            ViewBag.treeMod = treeMod;
            return View(courMod);
        }
        public ActionResult AddKnowledge()
        {
            int NodeID = DataConverter.CLng(Request.QueryString["nid"]);
            if (NodeID > 0)
            {
                ViewBag.cname = classBll.GetSelect(NodeID).C_ClassName;
            }
            M_Questions_Knowledge knowMod = knowBll.SelReturnModel(Mid);
            if (knowMod == null) { knowMod = new M_Questions_Knowledge(); }
            ViewBag.gradelist = MVCHelper.ToSelectList(B_GradeOption.GetGradeList(6, 0), "GradeName", "GradeID", knowMod.Grade.ToString());
            return View(knowMod);
        }
        public ActionResult KnowledgeManage()
        {
            int NodeID = DataConverter.CLng(Request.QueryString["nid"]);
            if (NodeID < 1) { function.WriteErrMsg("未指定科目"); return null; }
            PageSetting setting = knowBll.SelPage(CPage, PSize, NodeID);
            if (Request.IsAjaxRequest()) { return PartialView("KnowledgeManage_List", setting); }
            return View(setting);
        }
        public ActionResult ExamPointManage()
        {
            PageSetting setting = pointBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("ExamPointManage_List", setting); }
            return View(setting);
        }
        public ActionResult AddExamPoint()
        {
            M_ExamPoint pointMod = pointBll.GetSelect(Mid);
            return View(pointMod);
        }
        public ActionResult ToScore()
        {
            PageSetting setting = answerBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("ToScore_List", setting); }
            return View(setting);
        }
        //-----------------------------------------------------

        #region Papers Logical
        public int Papers_Del(string ids)
        {
            paperBll.DelByIDS(ids);
            return Success;
        }
        [HttpPost]
        public void Papers_Add()
        {
            int classid = DataConverter.CLng(Request.Form["TreeTlp_hid"]);
            if (classid < 1) { function.WriteErrMsg("请选择试卷科目"); return; }
            M_Exam_Sys_Papers paperMod = paperBll.SelReturnModel(Mid);
            if (paperMod == null) { paperMod = new M_Exam_Sys_Papers(); }
            paperMod.p_name = Request.Form["p_name"];
            paperMod.p_class = classid;
            paperMod.p_type = DataConverter.CLng(Request.Form["p_type"]);
            paperMod.p_rtype = DataConverter.CLng(Request.Form["p_rtype"]);
            paperMod.p_Remark = Request.Form["p_Remark"];
            paperMod.p_UseTime = DataConverter.CDouble(Request.Form["p_UseTime"]);
            paperMod.p_BeginTime = DataConverter.CDate(Request.Form["p_BeginTime"]);
            paperMod.p_endTime = DataConverter.CDate(Request.Form["p_endTime"]);
            paperMod.TagKey = Request.Form["tabinput"];
            if (Mid > 0)
            {
                paperBll.UpdateByID(paperMod);
            }
            else
            {
                paperMod.UserID = buser.GetLogin().UserID;
                paperBll.GetInsert(paperMod);
            }
            function.WriteSuccessMsg("操作成功", "Papers_System_Manage");
        }
        #endregion
        #region Question Logical
        public int Question_Del(string ids)
        {
            questBll.DelByIDS(ids);
            return Success;
        }
        [HttpPost]
        [ValidateInput(false)]
        public void Question_Add()
        {
            var model = new VM_Question();
            M_Exam_Sys_Questions questMod = Question_FillMod();
            if (Mid > 0)
            {
                questBll.GetUpdate(questMod);
            }
            else
            {
                questMod.p_id = questBll.GetInsert(questMod);
            }
            SafeSC.WriteFile(questMod.GetOPDir(), Request.Form["Optioninfo_Hid"]);
            function.WriteSuccessMsg("操作成功!", "QuestList?NodeID=" + model.NodeID); return;
        }
        private M_Exam_Sys_Questions Question_FillMod()
        {
            M_Exam_Sys_Questions questMod = null;
            if (Mid > 0)
            {
                questMod = questBll.GetSelect(Mid);
            }
            else
            {
                questMod = new M_Exam_Sys_Questions();
                M_UserInfo mu = buser.GetLogin();
                questMod.UserID = mu.UserID;
                questMod.p_Inputer = mu.UserName;
            }
            questMod.p_title = Request.Form["p_title"];
            questMod.p_Difficulty = DataConverter.CDouble(Request.Form["p_Difficulty"]);
            questMod.p_Class = DataConverter.CLng(Request.Form["TreeTlp_hid"]);
            questMod.p_Views = DataConverter.CLng(Request.Form["p_Views"]);
            questMod.p_Knowledge = DataConverter.CLng(Request.Form["knowname"]);
            string tagkey = Request.Form["tabinput"];
            if (string.IsNullOrEmpty(tagkey))
            {
                questMod.Tagkey = "";
            }
            else
            {
                int firstid = classBll.SelFirstNodeID(questMod.p_Class);
                questMod.Tagkey = knowBll.AddKnows(firstid, tagkey, 0);
            }
            questMod.p_Type = DataConverter.CLng(Request.Form["qtype_rad"]);
            questMod.p_shuming = Request.Form["p_shuming"] ?? Request.Form["p_Answer"];
            questMod.p_Answer = Request.Form["p_Answer"];
            if (questMod.p_Type == 10) { questMod.p_Content = Request.Form["Qids_Hid"]; questMod.LargeContent = Request.Form["p_Content"]; }
            else { questMod.p_Content = Request.Form["p_Content"]; }
            questMod.Qinfo = Request.Form["Qinfo_Hid"];
            questMod.p_ChoseNum = DataConverter.CLng(Request.Form["p_ChoseNum_DP"]);
            questMod.IsBig = 0;
            questMod.IsShare = string.IsNullOrEmpty(Request.Form["IsShare"]) ? 0 : 1;
            questMod.p_defaultScores = DataConverter.CFloat(Request.Form["p_defaultScores"]);
            questMod.Jiexi = Request.Form["Jiexi"];
            questMod.Version = DataConverter.CLng(Request.Form["Version"]);
            return questMod;
        }
        #endregion
        #region Question_Class Logical
        public string Question_Class_GetChild(int id)
        {
            return JsonHelper.JsonSerialDataTable(classBll.GetSelectByC_ClassId(id));
        }
        public int Question_Class_Del(int id)
        {
            classBll.DeleteByGroupID(id);
            return Success;
        }
        public void Question_Class_Add()
        {
            M_Exam_Class classMod = classBll.GetSelectByCName(Request.Form["C_ClassName"].Trim(' '));
            if (classMod.C_id > 0 && classMod.C_id != Mid) { function.WriteErrMsg("已存在该分类,请重新修改!"); return; }
            else if (classMod.C_id > 0 && Mid == 0) { function.WriteErrMsg("已存在该分类,请重新添加!"); return; }

            M_Exam_Class model = classBll.GetSelect(Mid);
            model.C_ClassName = Request.Form["C_ClassName"].Trim(' ');
            model.C_Classid = DataConverter.CLng(Request.Form["C_Classid"]);
            model.C_OrderBy = DataConverter.CLng(Request.Form["C_OrderBy"]);
            if (model.C_id > 0)
            {
                bool resu = classBll.GetUpdate(model);  //更新
                if (resu) { function.WriteSuccessMsg("更新成功!", "Question_Class_Manage"); return; }
                else { function.WriteErrMsg("更新失败!"); return; }
            }
            else
            {
                model.C_ClassType = DataConverter.CLng(Request.Form["C_ClassType"]);
                int resu = classBll.GetInsert(model);
                if (resu > 0) { function.WriteSuccessMsg("添加成功!", "Question_Class_Manage"); return; }
                else { function.WriteErrMsg("添加失败!"); return; }
            }
        }
        #endregion
        #region QuestGrade Logical
        public int QuestGrade_Del(int id)
        {
            B_GradeOption.DelGradeOption(id);
            return Success;
        }
        public void QuestGrade_Add()
        {
            int CateID = DataConverter.CLng(Request["cate"]);
            M_Grade GradeMod = new M_Grade();
            GradeMod.GradeID = DataConverter.CLng(Request.Form["GradeID_Hid"]);
            GradeMod.ParentID = 0;
            GradeMod.GradeName = Request.Form["GradeName_T"];
            GradeMod.Cate = CateID;
            if (GradeMod.GradeID > 0)
            {
                B_GradeOption.UpdateDic(GradeMod);
                function.WriteSuccessMsg("修改成功!", "QuestGrade?cate=" + CateID); return;
            }
            else
            {
                B_GradeOption.AddGradeOption(GradeMod);
                function.WriteSuccessMsg("添加成功!", "QuestGrade?cate=" + CateID);
            }
        }
        #endregion
        #region Student Logical
        public int Student_Del(string ids)
        {
            stuBll.DelByIDS(ids);
            return Success;
        }
        public int Student_Audit(string ids)
        {
            stuBll.AuditStatus(ids, true);
            return Success;
        }
        public int Student_UnAudit(string ids)
        {
            stuBll.AuditStatus(ids, false);
            return Success;
        }
        #endregion
        #region Version Logical
        public string Version_GetList(int pid)
        {
            DataTable dt = verBll.GetChildVersion(pid);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string ids = dt.Rows[i]["Knows"].ToString();
                string names = knowBll.GetNamesByIDS(ids).Replace(",", " | ").Trim('|');
                dt.Rows[i]["Knows"] = names;
            }
            return JsonConvert.SerializeObject(dt);
        }
        public int Version_Del(int id)
        {
            verBll.Del(id);
            return Success;
        }
        public int Version_Move(string oid, string nid)
        {
            string[] oids = oid.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] nids = nid.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            M_Exam_Version overMod = verBll.SelReturnModel(DataConverter.CLng(oids[0]));
            overMod.OrderID = DataConverter.CLng(nids[1]);
            M_Exam_Version nverMod = verBll.SelReturnModel(DataConverter.CLng(nids[0]));
            nverMod.OrderID = DataConverter.CLng(oids[1]);
            verBll.UpdateByID(overMod);
            verBll.UpdateByID(nverMod);
            return Success;
        }
        public void Version_Add()
        {
            M_Exam_Version verMod = verBll.SelReturnModel(Mid);
            if (verMod == null) { verMod = new M_Exam_Version(); }
            verMod.VersionName = Request.Form["VersionName"];
            verMod.VersionTime = Request.Form["VersionTime"];
            verMod.Inputer = Request.Form["Inputer"];
            verMod.NodeID = DataConverter.CLng(Request.Form["TreeTlp_hid"]);
            verMod.Grade = DataConverter.CLng(Request.Form["Grade"]);
            verMod.Volume = Request.Form["Volume"];
            verMod.Chapter = Request.Form["Chapter"];
            verMod.SectionName = Request.Form["SectionName"];
            verMod.CourseName = Request.Form["CourseName"];
            verMod.Price = DataConverter.CDouble(Request.Form["Price"]);
            string tagkey = Request.Form["Tabinput"];
            if (string.IsNullOrEmpty(tagkey))
            {
                verMod.Knows = "";
            }
            else
            {
                int firstid = classBll.SelFirstNodeID(verMod.Grade);
                verMod.Knows = knowBll.AddKnows(firstid, tagkey);
            }
            if (verMod.ID > 0)
            {
                verBll.UpdateByID(verMod);
            }
            else
            {
                M_UserInfo mu = buser.GetLogin();
                verMod.UserID = mu.UserID;
                verMod.Pid = DataConverter.CLng(Request.QueryString["pid"]);
                verBll.Insert(verMod);
            }
            function.WriteSuccessMsg("操作成功", "VersionList");
        }
        #endregion
        #region Knowledge Logical
        public void Knowledge_Add()
        {
            M_Questions_Knowledge knowMod = knowBll.SelReturnModel(Mid);
            if (knowMod == null) { knowMod = new M_Questions_Knowledge(); }

            int NodeID = DataConverter.CLng(Request.QueryString["nid"]);
            string name = Request.Form["K_name"].Trim();
            if ((knowMod.K_id > 0 && !knowMod.K_name.Equals(name))
                || knowMod.K_id <= 0)
            {
                DataTable tempdt = knowBll.SelByName(NodeID, name);
                if (tempdt.Rows.Count > 0) { function.WriteErrMsg("同级下知识点名称不能重复!"); return; }
            }
            knowMod.K_name = name;
            knowMod.Status = string.IsNullOrEmpty(Request.Form["Status"]) ? 0 : 1;
            knowMod.IsSys = string.IsNullOrEmpty(Request.Form["IsSys"]) ? 0 : 1;
            knowMod.Grade = DataConverter.CLng(Request.Form["Grade"]);
            knowMod.K_class_id = NodeID;
            if (knowMod.K_id > 0) { knowBll.GetUpdate(knowMod); }
            else
            {
                knowMod.CUser = badmin.GetAdminLogin().AdminId;
                knowMod.CDate = DateTime.Now;
                knowBll.insert(knowMod);
            }
            function.WriteSuccessMsg("操作成功", "KnowledgeManage?nid=" + NodeID);
        }
        public int Knowledge_Del(string ids)
        {
            knowBll.DelByIDS(ids);
            return Success;
        }
        public string Knowledge_GetChild(int nid)
        {
            DataTable dt = knowBll.Select_All(nid);
            return JsonConvert.SerializeObject(dt);
        }
        #endregion
        #region School Logical
        public int School_Del(string ids)
        {
            schBll.DelByIDS(ids);
            return Success;
        }
        public void School_Add()
        {
            M_School schMod = schBll.SelReturnModel(Mid);
            if (schMod == null) { schMod = new M_School(); }
            schMod.SchoolName = Request.Form["SchoolName"];
            schMod.SchoolType = DataConverter.CLng(Request.Form["SchoolType"]);
            schMod.Addtime = DateTime.Now;
            schMod.Province = Request.Form["province_dp"];
            schMod.City = Request.Form["city_dp"];
            schMod.County = Request.Form["county_dp"];
            schMod.Visage = DataConverter.CLng(Request.Form["Visage"]);
            string saveurl = ZLHelper.GetUploadDir_Admin(adminMod, "Exam");
            schMod.Country = Request.Form["Country"];
            schMod.SchoolInfo = Request.Form["SchoolInfo"];
            if (schMod.ID > 0)
            {
                schBll.GetUpdate(schMod);
            }
            schMod.ID = schBll.GetInsert(schMod);
            function.WriteSuccessMsg("操作成功!", "SchoolShow?id=" + schMod.ID);
        }
        #endregion
        #region ClassRoom Logical
        public int ClassRoom_Del(string ids)
        {
            croomBll.DelByIDS(ids);
            return Success;
        }
        public int ClassRoom_Audit(string ids)
        {
            croomBll.UpdateByState(ids, 1);
            return Success;
        }
        public int ClassRoom_CancelAudit(string ids)
        {
            croomBll.UpdateByState(ids, 0);
            return Success;
        }
        public void ClassRoom_Add()
        {
            M_ClassRoom roomMod = croomBll.SelReturnModel(Mid);
            if (roomMod == null) { roomMod = new M_ClassRoom(); }
            int olderuser = roomMod.CreateUser;//记录原教师id
            roomMod.Classinfo = Request.Form["Classinfo"];
            M_UserInfo mu = buser.SelReturnModel(DataConverter.CLng(Request.Form["Manager_Hid"]));
            if (mu.IsNull) { mu = buser.GetLogin(); }
            roomMod.CreateUser = mu.UserID;
            roomMod.Creation = DateTime.Now;
            roomMod.Grade = DataConverter.CLng(Request.Form["Grade"]);
            roomMod.Integral = DataConverter.CLng(Request.Form["Integral"]);
            roomMod.IsTrue = string.IsNullOrEmpty(Request.Form["IsTrue"]) ? 0 : 1;
            string saveurl = ZLHelper.GetUploadDir_Admin(adminMod, "Exam");
            roomMod.Monitor = Request.Form["Monitor"];
            roomMod.ClassStar = DataConverter.CLng(Request.Form["star_hid"]);
            roomMod.IsDone = string.IsNullOrEmpty(Request.Form["IsDone"]) ? 0 : 1;
            roomMod.RoomName = Request.Form["RoomName"];
            string schName = Request.Form["SchoolName_T"].Trim();
            DataTable tempschools = schBll.SelByName(schName);
            //添加或选择学校操作
            if (string.IsNullOrEmpty(schName)) { roomMod.SchoolID = 0; }
            else if (tempschools.Rows.Count > 0) { roomMod.SchoolID = DataConverter.CLng(tempschools.Rows[0]["ID"]); }//选择学校
            else { roomMod.SchoolID = InsertSchool(schName); }//添加学校
            if (roomMod.RoomID > 0)
            {
                if (olderuser != roomMod.CreateUser)//更改了班主任
                {
                    ChangeTearch(olderuser, mu, roomMod.CreateUser);
                }
                croomBll.GetUpdate(roomMod);
                function.WriteSuccessMsg("修改成功!", "ClassRoomShow?id=" + roomMod.RoomID); return;
            }
            int roomid = croomBll.GetInsert(roomMod);
            InsertTearcher(mu, roomid);
            function.WriteSuccessMsg("添加成功!", "ClassRoomShow?id=" + roomid); return;
        }

        private int InsertSchool(string name)
        {
            M_School schoolmod = new M_School();
            schoolmod.SchoolName = name;
            schoolmod.SchoolType = 1;
            schoolmod.Visage = 1;
            schoolmod.Addtime = DateTime.Now;
            schoolmod.SchoolInfo = "自定义学校";
            return schBll.insert(schoolmod);
        }
        //更改班主任时，该班级教师成员随之变化
        private void ChangeTearch(int oldeuser, M_UserInfo newuser, int roomid)
        {
            DataTable dt = stuBll.SelByURid(roomid, -1, 2, oldeuser);
            stuBll.UpDateStatus(dt.Rows[0]["Noteid"].ToString(), 1);//将原教师成员的状态改为已审核
            dt = stuBll.SelByURid(roomid, -1, 2, newuser.UserID);
            if (dt.Rows.Count > 0)//如果新任教师已存在该班级，将其状态改为无需审核状态
            {
                stuBll.UpDateStatus(dt.Rows[0]["Noteid"].ToString(), -1);
            }
            else//如果不存在则将该教师加入该班级成员
            {
                InsertTearcher(newuser, roomid);
            }
        }
        //添加班级时,创建者也是教师成员
        private void InsertTearcher(M_UserInfo mu, int roomid)
        {
            M_Student stuMod = new M_Student();
            stuMod.Addtime = DateTime.Now;
            stuMod.UserID = mu.UserID;
            stuMod.UserName = mu.UserName;
            stuMod.StudentType = 2;
            stuMod.Auditing = -1;
            stuMod.AuditingContext = "班主任";
            stuMod.RoomID = roomid;
            stuBll.insert(stuMod);
        }
        #endregion
        #region Tracher Logical
        public int Teacher_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                if (ids.IndexOf(',') > -1)
                {
                    string[] itemarr = ids.Split(',');
                    for (int i = 0; i < itemarr.Length; i++)
                    {
                        teaBll.DeleteByGroupID(DataConverter.CLng(itemarr[i]));
                    }
                }
                else
                {
                    teaBll.DeleteByGroupID(DataConverter.CLng(ids));
                }
            }
            return Success;
        }
        [ValidateInput(false)]
        public void Teacher_Add()
        {
            M_ExTeacher teaMod = teaBll.GetSelect(Mid);
            teaMod.TName = HttpUtility.HtmlEncode(Request.Form["TName"]);
            teaMod.TClsss = DataConverter.CLng(Request.Form["TreeTlp_Hid"]);
            teaMod.Teach = HttpUtility.HtmlEncode(Request.Form["Teach"]);
            teaMod.Post = HttpUtility.HtmlEncode(Request.Form["Post"]);
            teaMod.Remark = HttpUtility.HtmlEncode(Request.Form["Remark"]);
            teaMod.AddUser = badmin.GetAdminLogin().AdminId;
            if (teaMod.ID > 0)
            {
                if (teaBll.GetUpdate(teaMod)) { function.WriteSuccessMsg("修改成功！", "ExTeacherManage"); return; }
                else { function.WriteErrMsg("修改失败！"); return; }
            }
            else
            {
                teaMod.CreatTime = DateTime.Now;
                if (teaBll.GetInsert(teaMod) > 0) { function.WriteSuccessMsg("添加成功！", "ExTeacherManage"); return; }
                else { function.WriteErrMsg("添加失败！"); return; }
            }
        }
        #endregion
        #region Course Logical
        public int Course_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                if (ids.IndexOf(',') > -1)
                {
                    string[] itemarr = ids.Split(',');
                    for (int i = 0; i < itemarr.Length; i++)
                    {
                        courBll.DeleteByGroupID(DataConverter.CLng(itemarr[i]));
                    }
                }
                else
                {
                    courBll.DeleteByGroupID(DataConverter.CLng(ids));
                }
            }
            return Success;
        }
        public void Course_Add()
        {
            M_Course courMod = courBll.GetSelect(Mid);
            courMod.CoureseClass = DataConverter.CLng(Request.Form["TreeTlp_hid"]);
            courMod.CoureseCode = Request.Form["CoureseCode"];
            courMod.CoureseCredit = DataConverter.CDouble(Request.Form["CoureseCredit"]);
            courMod.CoureseRemark = Request.Form["CoureseRemark"];
            courMod.CoureseThrun = Request.Form["CoureseThrun"];
            courMod.CourseName = Request.Form["CourseName"];
            courMod.Hot = string.IsNullOrEmpty(Request.Form["Hot"]) ? 0 : 1;
            if (courMod.id > 0)
            {
                courBll.GetUpdate(courMod);
                function.WriteSuccessMsg("修改成功", "CourseManage"); return;
            }
            else
            {
                courMod.AddUser = badmin.GetAdminLogin().AdminId;
                courMod.AddTime = DateTime.Now;
                courBll.GetInsert(courMod);
                function.WriteSuccessMsg("添加成功", "CourseManage"); return;
            }
        }
        #endregion
        #region ExamPoint Logical
        public int ExamPoint_Del(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                if (ids.IndexOf(',') > -1)
                {
                    string[] itemarr = ids.Split(',');
                    for (int i = 0; i < itemarr.Length; i++)
                    {
                        pointBll.DeleteByGroupID(DataConverter.CLng(itemarr[i]));
                    }
                }
                else
                {
                    pointBll.DeleteByGroupID(DataConverter.CLng(ids));
                }
            }
            return Success;
        }
        public void ExamPoint_Add()
        {
            M_ExamPoint pointMod = pointBll.GetSelect(Mid);
            pointMod.TestPoint = Request.Form["TestPoint"];
            pointMod.OrderBy = DataConverter.CLng(Request.Form["OrderBy"]);
            if (pointMod.ID > 0)
            {
                pointBll.GetUpdate(pointMod);
            }
            else
            {
                pointMod.AddTime = DateTime.Now;
                pointMod.AddUser = badmin.GetAdminLogin().AdminId;
                pointBll.GetInsert(pointMod);
            }
            function.WriteSuccessMsg("操作成功", "ExamPointManage");
        }
        #endregion
        //获取科目下拉模型
        private C_TreeTlpDP GetTreeMod()
        {
            DataTable dt = classBll.Select_All();
            return new C_TreeTlpDP()
            {
                F_ID = "C_id",
                F_Name = "C_ClassName",
                F_Pid = "C_Classid",
                dt = dt,
                seled = dt != null && dt.Rows.Count > 0 ? dt.Rows[0][0].ToString() : "0"
            };
        }
        public void Setting_Update()
        {
            questBll.CountDiffcult(Request.Form["stime"], Request.Form["etime"]);
            function.WriteSuccessMsg("难度更新成功", "Setting");
        }
    }
}
