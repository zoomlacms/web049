using Newtonsoft.Json;
using System;
using System.Data;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Controls;
using ZoomLa.Model;
using ZoomLa.Model.User;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Product;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class ContentController : ZLCtrl
    {
        B_Favorite favBll = new B_Favorite();
        B_Content conBll = new B_Content();
        B_Node nodeBll = new B_Node();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_UserPromotions upBll = new B_UserPromotions();
        B_Comment cmtBll = new B_Comment();
        B_Product proBll = new B_Product();
        B_Stock stockBll = new B_Stock();
        B_User_BindPro bindBll = new B_User_BindPro();
        B_Group gpBll = new B_Group();
        B_KeyWord keyBll = new B_KeyWord();
        public int NodeID
        {
            get
            {
                if (ViewBag.NodeID == null) { ViewBag.NodeID = DataConvert.CLng(Request["NodeID"]); }
                return DataConvert.CLng(ViewBag.NodeID);
            }
            set { ViewBag.NodeID = value; }
        }
        public int ModelID
        {
            get
            {
                if (ViewBag.ModelID == null) { ViewBag.ModelID = DataConvert.CLng(Request["ModelID"]); }
                return DataConvert.CLng(ViewBag.ModelID);
            }
            set { ViewBag.ModelID = value; }
        }
        public void Index() { Response.Redirect("MyContent"); }
        public void Default() { Response.Redirect("MyContent"); }
        public ActionResult MyContent()
        {
            string Status = Request.QueryString["Status"] ?? "";
            DataTable nodeDT = nodeBll.SelByPid(0, true);
            string nodeids = upBll.GetNodeIDS(mu.GroupID);
            if (!string.IsNullOrEmpty(nodeids))
            {
                nodeDT.DefaultView.RowFilter = "NodeID in(" + nodeids + ")";
            }
            else
            {
                nodeDT.DefaultView.RowFilter = "1>2";//无权限，则去除所有
            }
            nodeDT = nodeDT.DefaultView.ToTable();
            C_TreeView treeMod = new C_TreeView()
            {
                NodeID = "NodeID",
                NodeName = "NodeName",
                NodePid = "ParentID",
                DataSource = nodeDT,
                liAllTlp = "<a href='MyContent'>全部内容</a>",
                LiContentTlp = "<a href='MyContent?NodeID=@NodeID'>@NodeName</a>",
                SelectedNode = NodeID.ToString()
            };
            if (NodeID != 0)
            {
                M_Node nod = nodeBll.GetNodeXML(NodeID);
                if (nod.NodeListType == 2)
                {
                    return RedirectToAction("ProductList", new { NodeID = NodeID });//跳转到商城
                }
                string ModeIDList = nod.ContentModel;
                string[] ModelID = ModeIDList.Split(',');
                string AddContentlink = "";
                for (int i = 0; i < ModelID.Length; i++)
                {
                    M_ModelInfo infoMod = modBll.SelReturnModel(DataConverter.CLng(ModelID[i]));
                    if (infoMod == null) continue;
                    if (infoMod.ModelType != 5)
                    {
                        AddContentlink += "<a href='AddContent?NodeID=" + NodeID + "&ModelID=" + infoMod.ModelID + "' class='btn btn-info' style='margin-right:5px;'><i class='zi zi_plus'></i> 添加" + infoMod.ItemName + "</a>";
                    }
                }
                ViewBag.addhtml = AddContentlink;
            }
            PageSetting setting = conBll.SelContent(CPage, PSize, NodeID, Status, mu.UserName, Request["skey"]);
            ViewBag.Status = Status;
            ViewBag.treeMod = treeMod;
            return View(setting);
        }
        public ActionResult ShowContent()
        {
            M_CommonData conMod = conBll.SelReturnModel(Mid);
            if (conMod == null) { function.WriteErrMsg("内容不存在"); return Content(""); }
            if (!mu.UserName.Equals(conMod.Inputer)) { function.WriteErrMsg("你无权查看该内容"); return Content(""); }
            M_Node nodeMod = nodeBll.SelReturnModel(conMod.NodeID);
            DataTable contentDT = conBll.GetContentByItems(conMod.TableName, conMod.GeneralID);
            ViewBag.nodeMod = nodeMod;
            ViewBag.modelhtml = fieldBll.InputallHtml(conMod.ModelID, conMod.NodeID, new ModelConfig()
            {
                ValueDT = contentDT,
                Mode = ModelConfig.SMode.PreView
            });
            return View(conMod);
        }
        public ActionResult AddContent()
        {
            M_CommonData Cdata = new M_CommonData();
            if (Mid > 0)
            {
                Cdata = conBll.GetCommonData(Mid);
            }
            else { Cdata.NodeID = DataConvert.CLng(Request.QueryString["NodeID"]); Cdata.ModelID = DataConvert.CLng(Request.QueryString["ModelID"]); }

            if (Cdata.ModelID < 1 && Cdata.NodeID < 1) { function.WriteErrMsg("参数错误"); return null; }
            M_ModelInfo model = modBll.SelReturnModel(Cdata.ModelID);
            M_Node nodeMod = nodeBll.SelReturnModel(Cdata.NodeID);
            if (Mid > 0)
            {
                if (mu.UserName != Cdata.Inputer) { function.WriteErrMsg("不能编辑不属于自己输入的内容!"); return Content(""); }
                //DataTable dtContent = conBll.GetContent(Mid);
                //ViewBag.modelhtml = fieldBll.InputallHtml(Cdata.ModelID, Cdata.NodeID, new ModelConfig()
                //{
                //    Source = ModelConfig.SType.UserContent,
                //    ValueDT = dtContent
                //});
            }
            //else
            //{
            //    ViewBag.modelhtml = fieldBll.InputallHtml(ModelID, NodeID, new ModelConfig()
            //    {
            //        Source = ModelConfig.SType.UserContent
            //    });
            //}
            ViewBag.ds = fieldBll.SelByModelID(Cdata.ModelID, true);
            ViewBag.op = (Mid < 1 ? "添加" : "修改") + model.ItemName;
            ViewBag.tip = "向 <a href='MyContent?NodeId=" + nodeMod.NodeID + "'>[" + nodeMod.NodeName + "]</a> 节点" + ViewBag.op;
            return View(Cdata);
        }
        #region product

        public ActionResult ProductList()
        {
            int NodeID = DataConverter.CLng(Request.QueryString["NodeID"]);
            int Recycler = DataConverter.CLng(Request.QueryString["Recycler"]);
            PageSetting setting = proBll.U_SPage(CPage, PSize, mu.UserID, NodeID, Recycler);
            if (Request.IsAjaxRequest()) { return PartialView("ProductList_List", setting); }
            DataTable nodeDT = nodeBll.SelByPid(0, true);
            nodeDT = nodeDT.DefaultView.ToTable();
            C_TreeView treeMod = new C_TreeView()
            {
                NodeID = "NodeID",
                NodeName = "NodeName",
                NodePid = "ParentID",
                DataSource = nodeDT,
                liAllTlp = "<a href='MyContent'>全部内容</a>",
                LiContentTlp = "<a href='MyContent?NodeID=@NodeID'>@NodeName</a>",
                SelectedNode = NodeID.ToString()
            };
            ViewBag.treeMod = treeMod;
            string AddContentlink = "";
            if (NodeID > 0)
            {
                M_Node nodeMod = nodeBll.GetNodeXML(NodeID);
                string[] ModelID = nodeMod.ContentModel.Split(',');
                for (int i = 0; i < ModelID.Length; i++)
                {

                    AddContentlink = AddContentlink + "<input name=\"btn" + i.ToString() + "\" class=\"btn btn-primary\" type=\"button\" value=\"添加" + modBll.GetModelById(DataConverter.CLng(ModelID[i])).ItemName + "\" onclick=\"javascript:window.location.href='AddProduct?ModelID=" + ModelID[i] + "&NodeID=" + this.NodeID + "';\" />&nbsp;&nbsp;";
                    if (modBll.GetModelById(DataConverter.CLng(ModelID[i])).Islotsize)
                    {
                        AddContentlink = AddContentlink + "<input name=\"btn" + i.ToString() + "\" class=\"btn btn-primary\"  type=\"button\" value=\"批量添加" + modBll.GetModelById(DataConverter.CLng(ModelID[i])).ItemName + "\" onclick=\"javascript:window.location.href='Release?ModelID=" + ModelID[i] + "&NodeID=" + this.NodeID + "';\" />&nbsp;&nbsp;";
                    }
                }
            }
            ViewBag.addlink = AddContentlink;
            ViewBag.Recycler = Recycler;
            return View(setting);
        }
        public ActionResult AddProduct()
        {
            VM_Product vm = new VM_Product();
            if (Mid < 1)
            {
                if (ModelID < 1) { function.WriteErrMsg("没有指定要添加内容的模型ID!"); return null; }
                if (NodeID < 1) { function.WriteErrMsg("没有指定要添加内容的栏目节点ID!"); return null; }
                vm.proMod = new M_Product() { Stock = 10, Rateset = 1, Dengji = 3 };
                vm.NodeID = NodeID;
                vm.ModelID = ModelID;
                vm.proMod.ProCode = B_Product.GetProCode();
            }
            else
            {
                vm.proMod = proBll.GetproductByid(Mid);
                vm.NodeID = vm.proMod.Nodeid;
                vm.ModelID = vm.proMod.ModelID;
                vm.ValueDT = proBll.GetContent(vm.proMod.TableName, vm.proMod.ItemID);
                if (!string.IsNullOrEmpty(vm.proMod.BindIDS))//捆绑商品
                {
                    DataTable dt = proBll.SelByIDS(vm.proMod.BindIDS, "id,Thumbnails,Proname,LinPrice");
                    vm.bindList = JsonConvert.SerializeObject(dt);
                }
                #region 特选商品
                {
                    string where = string.Format("(ProIDS LIKE '%,{0},%' OR ProIDS LIKE '{0},%' OR ProIDS LIKE '%,{0}')", vm.proMod.ID.ToString());
                    DataTable dt = DBCenter.SelWithField("ZL_User_BindPro", "UserID", where);
                    string uids = StrHelper.GetIDSFromDT(dt, "UserID");
                    ViewBag.prouids = uids;
                }
                #endregion
            }
            //------------------------------------------------------------------------------------------------
            vm.nodeMod = nodeBll.SelReturnModel(vm.NodeID);
            if (vm.nodeMod.IsNull) { function.WriteErrMsg("节点[" + NodeID + "]不存在"); return null; }
            return View(vm);
        }
        [ValidateInput(false)]
        public void Product_Add()
        {
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table, proBll.GetproductByid(Mid));
            if (Mid < 1)
            {
                proMod.ID = proBll.Add(table, proMod);
                IsAddStock(proMod, DataConvert.CLng(Request.Form["Stock"]));
                //多区域价格
                //SqlParameter[] sp = new SqlParameter[] { new SqlParameter("guid", Request.Form["ProGuid"]) };
                //SqlHelper.ExecuteSql("UPDATE ZL_Shop_RegionPrice SET [ProID]=" + proMod.ID + " WHERE [Guid]=@guid", sp);
            }
            else
            {
                proBll.Update(table, proMod);
            }
            function.WriteSuccessMsg("操作成功", "ProductList?NodeID=" + proMod.Nodeid); return;
        }
        [ValidateInput(false)]
        public void Product_AddToNew()
        {
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table);
            //------------------
            proMod.AddUser = mu.UserName;
            proMod.Stock = 0;
            proMod.AddTime = DateTime.Now;
            proMod.UpdateTime = DateTime.Now;
            proMod.ID = proBll.Add(table, proMod);
            //------------------
            IsAddStock(proMod, DataConvert.CLng(Request.Form["Stock"]));
            function.WriteSuccessMsg("操作成功", "ProductList?NodeID=" + proMod.Nodeid); return;
        }
        private M_Product FillProductModel(ref DataTable table, M_Product proMod = null)
        {
            if (proMod == null) { proMod = new M_Product(); }
            //DataTable gpdt = gpBll.GetGroupList();
            /*--------------proMod------------*/
            if (proMod.ID < 1)
            {
                proMod.Nodeid = NodeID;
                proMod.FirstNodeID = nodeBll.SelFirstNodeID(proMod.Nodeid);
                proMod.ModelID = ModelID;
                proMod.TableName = modBll.SelReturnModel(ModelID).TableName;
                proMod.AddUser = mu.UserName;
                proMod.UserID = mu.UserID;
            }
            DataTable dt = fieldBll.GetModelFieldList(proMod.ModelID);
            table = new Call().GetDTFromMVC(dt, Request);
            //-------------------------------------------
            proMod.ProCode = Request.Form["ProCode"];
            proMod.BarCode = Request.Form["BarCode"];
            proMod.Proname = Request.Form["Proname"];
            proMod.ShortProName = Request.Form["ShortProName"];
            proMod.Kayword = Request.Form["tabinput"];
            keyBll.AddKeyWord(proMod.Kayword, 1);
            proMod.ProUnit = Request.Form["ProUnit"];
            proMod.AllClickNum = DataConverter.CLng(Request.Form["AllClickNum"]);
            proMod.Weight = DataConvert.CDouble(Request.Form["Weight_T"]);
            proMod.ProClass = DataConverter.CLng(Request.Form["proclass_rad"]);
            proMod.IDCPrice = Request.Form["IDC_Hid"];
            proMod.PointVal = DataConverter.CLng(Request.Form["PointVal_T"]);
            proMod.Proinfo = Request.Form["Proinfo"];
            proMod.Procontent = Request.Form["Procontent"];
            proMod.Clearimg = Request.Form["txt_Clearimg"];
            proMod.Thumbnails = Request.Form["txt_Thumbnails"];
            //proMod.Producer = Request.Form["Producer"];
            //proMod.Brand = Request.Form["Brand"];
            //proMod.Quota = DataConverter.CLng(Quota.Text);
            //proMod.DownQuota = DataConverter.CLng(DownQuota.Text);
            proMod.StockDown = DataConverter.CLng(Request.Form["StockDown"]);
            proMod.Rate = DataConverter.CDouble(Request.Form["Rate"]);
            proMod.Rateset = DataConverter.CLng(Request.Form["Rateset"]);
            proMod.Dengji = DataConverter.CLng(Request.Form["Dengji"]);
            proMod.ShiPrice = DataConverter.CDouble(Request.Form["ShiPrice"]);
            proMod.LinPrice = DataConverter.CDouble(Request.Form["LinPrice"]);
            //proMod.Preset = (OtherProject.SelectedValue == null) ? "" : OtherProject.SelectedValue;  //促销
            //proMod.Integral = DataConverter.CLng(Integral.Text);
            //proMod.Propeid = DataConverter.CLng(Propeid.Text);
            proMod.Recommend = DataConverter.CLng(Request.Form["Recommend"]);
            //proMod.Largesspirx = DataConverter.CLng(Largesspirx.Text);
            proMod.Largess = DataConvert.CLng(Request.Form["Largess"]);
            //更新时间，若没有指定则为当前时间
            proMod.UpdateTime = DataConverter.CDate(Request.Form["UpdateTime"]);
            proMod.AddTime = DataConverter.CDate(Request.Form["AddTime"]);
            proMod.ModeTemplate = Request.Form["ModeTemplate_hid"];
            //proMod.bookDay = DataConverter.CLng(BookDay_T.Text);
            proMod.BookPrice = DataConverter.CDouble(Request.Form["BookPrice"]);
            proMod.FarePrice = Request.Form["FareTlp_Rad"];
            //proMod.UserShopID = DataConvert.CLng(Request.Form["UserShopID"]);//店铺页面应该禁止此项
            proMod.UserType = DataConverter.CLng(Request.Form["UserPrice_Rad"]);
            proMod.Quota = DataConvert.CLng(Request.Form["Quota_Rad"]);
            proMod.DownQuota = DataConvert.CLng(Request.Form["DownQuota_Rad"]);
            switch (proMod.UserType)
            {
                case 1:
                    proMod.UserPrice = Request.Form["UserPrice"];
                    break;
                case 2:
                    proMod.UserPrice = Request.Form["Price_Group_Hid"];
                    break;
            }
            switch (proMod.Quota)
            {
                case 0:
                    break;
                case 2:
                    proMod.Quota_Json = Request.Form["Quota_Group_Hid"];
                    break;
            }
            switch (proMod.DownQuota)
            {
                case 0:
                    break;
                case 2:
                    proMod.DownQuota_Json = Request.Form["DownQuota_Group_Hid"];
                    break;
            }
            proMod.Sales = DataConverter.CLng(Request.Form["Sales_Chk"]);
            proMod.Istrue = DataConverter.CLng(Request.Form["istrue_chk"]);
            proMod.Ishot = DataConverter.CLng(Request.Form["ishot_chk"]);
            proMod.Isnew = DataConverter.CLng(Request.Form["isnew_chk"]);
            proMod.Isbest = DataConverter.CLng(Request.Form["isbest_chk"]);
            proMod.Allowed = DataConverter.CLng(Request.Form["Allowed"]);
            proMod.GuessXML = Request.Form["GuessXML"];
            proMod.Wholesalesinfo = Request.Form["ChildPro_Hid"];
            proMod.DownCar = DataConvert.CLng(Request.Form["DownCar"]);
            ////捆绑商品
            if (!string.IsNullOrEmpty(Request.Form["Bind_Hid"]))
            {
                //获取绑定商品
                DataTable binddt = JsonHelper.JsonToDT(Request.Form["Bind_Hid"]);
                proMod.BindIDS = StrHelper.GetIDSFromDT(binddt, "ID");
            }
            else
            {
                proMod.BindIDS = "";
            }
            return proMod;
        }
        //新建商品,添加库存
        private void IsAddStock(M_Product proMod, int proStock)
        {
            if (proStock == 0) { return; }
            string danju = proMod.UserShopID + DateTime.Now.ToString("yyyyMMddHHmmss");
            M_Stock stockMod = new M_Stock()
            {
                proid = proMod.ID,
                proname = proMod.Proname,
                adduser = proMod.AddUser,
                StoreID = proMod.UserShopID,
            };
            //int proStock = DataConverter.CLng(Stock.Text);
            stockMod.proid = proMod.ID;
            stockMod.stocktype = 0;
            stockMod.Pronum = proStock;
            stockMod.danju = "RK" + danju;
            stockMod.content = "添加商品:" + proMod.Proname + "入库";
            stockBll.AddStock(stockMod);
        }
        public int Product_Del(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.setproduct(13, ids);
            }
            return Success;
        }
        public int Product_Recover(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.setproduct(12, ids);
            }
            return Success;
        }
        public int Product_RealDel(string ids)
        {
            if (!String.IsNullOrEmpty(ids))
            {
                proBll.RealDelByIDS(ids);
            }
            return Success;
        }
        #endregion
        public void EditContent()
        {
            int gid = DataConvert.CLng(Request.QueryString["GeneralID"]);
            if (gid < 1) { gid = DataConvert.CLng(Request.QueryString["ID"]); }
            if (gid < 1) { function.WriteErrMsg("未指定需要修改的内容"); return; }
            Response.Redirect("AddContent?ID=" + gid); return;
        }
        //其不是根据名称,而是根据顺序来取值
        public PartialViewResult Content_Data()
        {
            string Status = Request["Status"] ?? "";
            PageSetting setting = conBll.SelContent(CPage, PSize, NodeID, Status, mu.UserName, Request["skey"]);
            return PartialView("MyContent_List", setting);
        }
        [HttpPost]
        [ValidateInput(false)]
        public void Content_Add()
        {
            M_CommonData CData = new M_CommonData();
            if (Mid > 0)
            {
                CData = conBll.GetCommonData(Mid);
                if (!CData.Inputer.Equals(mu.UserName)) { function.WriteErrMsg("你无权修改该内容"); return; }
            }
            else
            {
                CData.NodeID = NodeID;
                CData.ModelID = ModelID;
                CData.TableName = modBll.SelReturnModel(CData.ModelID).TableName;
            }
            M_Node nodeMod = nodeBll.SelReturnModel(CData.NodeID);
            DataTable dt = fieldBll.SelByModelID(CData.ModelID, false);
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
            CData.Title = Request.Form["title"];
            CData.Subtitle = Request["Subtitle"];
            CData.PYtitle = Request["PYtitle"];
            CData.TagKey = Request.Form["tabinput"];
            CData.Status = nodeMod.SiteContentAudit;
            string parentTree = "";
            CData.TitleStyle = Request.Form["TitleStyle"];
            CData.TopImg = Request.Form["topimg_hid"];//首页图片
            if (Mid > 0)//修改内容
            {
                conBll.UpdateContent(table, CData);
            }
            else
            {
                CData.FirstNodeID = nodeBll.SelFirstNodeID(CData.NodeID, ref parentTree);
                CData.ParentTree = parentTree;
                CData.Inputer = mu.UserName;
                CData.SuccessfulUserID = mu.UserID;
                CData.GeneralID = conBll.AddContent(table, CData);//插入信息给两个表，主表和从表:CData-主表的模型，table-从表
            }
            if (Mid < 1 && SiteConfig.UserConfig.InfoRule > 0)//添加时增加积分
            {
                buser.AddMoney(mu.UserID, SiteConfig.UserConfig.InfoRule, M_UserExpHis.SType.Point, "添加内容:" + CData.Title + "增加积分");
            }
            function.WriteSuccessMsg("操作成功!", "MyContent?NodeID=" + CData.NodeID); return;
        }
        public int Content_Del(string ids)
        {
            conBll.UpdateStatus(ids, (int)ZLEnum.ConStatus.Recycle, mu.UserName);
            return Success;
        }
        public int Content_RealDel(string ids)
        {
            conBll.DelContent(ids, mu.UserName);
            return Success;
        }
        public int Content_Recover(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                conBll.UpdateStatus(ids, 0);
            }
            return Success;
        }
        public ActionResult FileBuyManage()
        {
            B_Content_FileBuy buyBll = new B_Content_FileBuy();
            PageSetting setting = buyBll.SelPage(CPage, PSize, mu.UserID);
            if (Request.IsAjaxRequest())
            {
                return PartialView("FileBuyManage_List", setting);
            }
            else { return View(setting); }
        }
        #region 收藏
        public ActionResult AddToFav()
        {
            M_Favorite favMod = new M_Favorite();
            favMod.InfoID = DataConvert.CLng(Request["infoID"]);
            favMod.Owner = mu.UserID;
            favMod.AddDate = DateTime.Now;
            favMod.FavoriType = DataConvert.CLng(Request.Form["type"], 1);
            //---------------------------
            favMod.FavItemID = "";
            favMod.Title = HttpUtility.HtmlEncode(Request["title"]);
            favMod.FavUrl = Request.Form["url"];
            switch (favMod.FavoriType)
            {
                case 1:
                case 3:
                    {
                        M_CommonData conMod = conBll.SelReturnModel(favMod.InfoID);
                        if (conMod == null) { err = "内容ID[" + favMod.InfoID + "]不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = conMod.Title;
                        }
                    }
                    break;
                case 2:
                    {
                        M_Product proMod = proBll.GetproductByid(favMod.InfoID);
                        if (proMod == null) { err = "商品不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = proMod.Proname;
                        }
                    }
                    break;
                case 4:
                    {
                        B_Ask askBll = new B_Ask();
                        M_Ask askMod = askBll.SelReturnModel(favMod.InfoID);
                        if (askMod == null) { err = "问题不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = StringHelper.SubStr(askMod.Qcontent);
                        }
                    }
                    break;
                case 5:
                    {
                        B_Baike bkBll = new B_Baike();
                        M_Baike bkMod = bkBll.SelReturnModel(favMod.InfoID);
                        if (bkMod == null) { err = "百科不存在";break; }
                        if (string.IsNullOrEmpty(favMod.Title))
                        {
                            favMod.Title = bkMod.Tittle;
                        }
                    }
                    break;
            }
            if (string.IsNullOrEmpty(favMod.Title)) { favMod.Title = "无标题"; }
            if (favMod.InfoID < 1) { err = "未指定内容ID"; }
            else if (favMod.Owner < 1) { err = "用户未登录"; }
            else { favBll.insert(favMod); }
            ViewBag.err = err;
            return View();
        }
        public ActionResult MyFavori()
        {
            PageSetting setting = favBll.SelPage(CPage, PSize, mu.UserID, DataConvert.CLng(Request["type"], -100));
            return View(setting);
        }
        public PartialViewResult Favour_Data()
        {
            PageSetting setting = favBll.SelPage(CPage, PSize, mu.UserID, DataConvert.CLng(Request["type"], -100));
            return PartialView("MyFavori_List", setting);
        }
        public int Favour_Del(string ids)
        {
            favBll.DelByIDS(ids, mu.UserID);
            return Success;
        }
        #endregion
        #region 评论
        public ActionResult MyComment()
        {
            PageSetting setting = cmtBll.SelPage(CPage, PSize, NodeID, 0, mu.UserID);
            return View(setting);
        }
        public int Comment_Del(string ids)
        {
            cmtBll.U_Del(mu.UserID, ids);
            return Success;
        }
        #endregion
        #region MarkDown
        B_Content_MarkDown mdBll = new B_Content_MarkDown();
        public ActionResult MarkDown()
        {
            PageSetting setting = new PageSetting();
            setting = mdBll.SelPage(CPage, PSize,new Com_Filter() {uids=mu.UserID.ToString() });
            if (function.isAjax()) { return PartialView("Markdown_List", setting); }
            return View(setting);
        }
        public string MarkDown_Del(string ids)
        {
            mdBll.Del(ids);
            return Success.ToString();
        }
        #endregion
    }
}
