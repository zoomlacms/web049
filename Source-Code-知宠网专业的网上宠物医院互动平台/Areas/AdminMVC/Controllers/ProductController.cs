using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.API;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.CreateJS;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Controls;
using ZoomLa.Model;
using ZoomLa.Model.Content;
using ZoomLa.Model.Shop;
using ZoomLa.Model.User;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Product;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class ProductController : Ctrl_Admin
    {
        B_User_BindPro bindBll = new B_User_BindPro();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_Node nodeBll = new B_Node();
        B_Product proBll = new B_Product();
        B_Group gpBll = new B_Group();
        B_Stock stockBll = new B_Stock();
        B_Shop_FareTlp fareBll = new B_Shop_FareTlp();
        B_Shop_RegionPrice regionBll = new B_Shop_RegionPrice();
        B_KeyWord keyBll = new B_KeyWord();
        B_Shop_GroupPro sgpBll = new B_Shop_GroupPro();
        B_Content_VerBak verBll = new B_Content_VerBak();
        private int NodeID { get { return DataConverter.CLng(Request.QueryString["NodeID"]); } }
        private int ModelID { get { return DataConverter.CLng(Request.QueryString["ModelID"]); } }
        public ActionResult AddProduct()
        {
            VM_Product vm = new VM_Product();
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop,"product")) { return null; }
            if (Mid < 1)
            {
                if (ModelID < 1) { function.WriteErrMsg("没有指定要添加内容的模型ID!"); return null; }
                if (NodeID < 1) { function.WriteErrMsg("没有指定要添加内容的栏目节点ID!"); return null; }
                vm.proMod = new M_Product() { Stock = 10, Rateset = 1, Dengji = 3 };
                vm.NodeID = NodeID;
                vm.ModelID = ModelID;
                vm.proMod.ProCode = B_Product.GetProCode();
                vm.ProGuid = Guid.NewGuid().ToString();
            }
            else
            {
                int VerID = DataConvert.CLng(Request.QueryString["Ver"]);
                if (VerID > 0)
                {
                    M_Content_VerBak verMod = verBll.SelReturnModel(VerID);
                    vm.proMod = JsonConvert.DeserializeObject<M_Product>(verMod.ContentBak);
                    if (vm.proMod.ID != Mid) { function.WriteErrMsg("加载的版本与商品不匹配"); return null; }
                    vm.ValueDT = JsonConvert.DeserializeObject<DataTable>(verMod.TableBak);
                }
                else
                {
                    vm.proMod = proBll.GetproductByid(Mid);
                    vm.ValueDT = proBll.GetContent(vm.proMod.TableName, vm.proMod.ItemID);
                }
                vm.ProGuid = vm.proMod.ID.ToString();
                if (vm.proMod.Class == 2) { Response.Redirect(CustomerPageAction.customPath + "Shop/Arrive/SuitProAdd.aspx?ID=" + vm.proMod.ID); }
                vm.NodeID = vm.proMod.Nodeid;
                vm.ModelID = vm.proMod.ModelID;
                if (!string.IsNullOrEmpty(vm.proMod.BindIDS))//捆绑商品
                {
                    DataTable dt = proBll.SelByIDS(vm.proMod.BindIDS, "id,Thumbnails,Proname,LinPrice");
                    vm.bindList = JsonConvert.SerializeObject(dt);
                }
                //多区域价格
                vm.regionMod = regionBll.SelModelByGuid(vm.ProGuid);
                if (vm.regionMod == null) { vm.regionMod = new M_Shop_RegionPrice(); }
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
            if (vm.nodeMod.IsNull) { function.WriteErrMsg("节点[" + vm.NodeID + "]不存在"); return null; }
            return View(vm);
        }
        [ValidateInput(false)]
        public void Product_Add()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) { return; }
            DataTable table = new DataTable();
            M_Product proMod = FillProductModel(ref table, proBll.GetproductByid(Mid));
            if (Mid < 1)
            {
                proMod.ID = proBll.Add(table, proMod);
                IsAddStock(proMod, DataConvert.CLng(Request.Form["Stock"]));
                //多区域价格
                SqlParameter[] sp = new SqlParameter[] { new SqlParameter("guid", Request.Form["ProGuid"]) };
                SqlHelper.ExecuteSql("UPDATE ZL_Shop_RegionPrice SET [ProID]=" + proMod.ID + " WHERE [Guid]=@guid", sp);
            }
            else
            {
                proBll.Update(table, proMod);
            }
            IsUserProduct(proMod, Request.Form["uprouids_old_hid"], Request.Form["uprouids_hid"]);
            IsNeedVerBak(proMod);
            IsHaveMaterial(proMod);
            IsHavePresent(proMod);
            Response.Redirect(CustomerPageAction.customPath + "Shop/ProductShow.aspx?ID=" + proMod.ID);
        }
        [ValidateInput(false)]
        public void Product_AddToNew()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "product")) { return ; }
            DataTable table = new DataTable();
            M_Product proMod = proBll.GetproductByid(Mid);
            proMod = FillProductModel(ref table, proMod);
            //------------------
            proMod.ProCode = B_Product.GetProCode();
            proMod.AddUser = B_Admin.GetLogin().AdminName;
            proMod.Stock = 0;
            proMod.AddTime = DateTime.Now;
            proMod.UpdateTime = DateTime.Now;
            proMod.ID = 0;
            proMod.ID = proBll.Add(table, proMod);
            //------------------
            IsNeedVerBak(proMod);
            IsHaveMaterial(proMod);
            IsAddStock(proMod, DataConvert.CLng(Request.Form["Stock"]));
            IsUserProduct(proMod, Request.Form["uprouids_old_hid"], Request.Form["uprouids_hid"]);
            IsHavePresent(proMod);
            Response.Redirect(CustomerPageAction.customPath + "Shop/ProductShow.aspx?ID=" + proMod.ID);
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
        private void IsNeedVerBak(M_Product proMod)
        {
            if (!string.IsNullOrEmpty(Request.Form["verbak_chk"]))
            {
                M_Content_VerBak verMod = new M_Content_VerBak();
                DataTable valueDT = proBll.GetContent(proMod.TableName,proMod.ItemID);
                verMod.GeneralID = proMod.ID;
                verMod.ContentBak = JsonConvert.SerializeObject(proMod);
                verMod.TableBak = JsonConvert.SerializeObject(valueDT);
                verMod.Title = proMod.Proname;
                verMod.Inputer = adminMod.AdminName;
                verMod.ZType = "product";
                verBll.Insert(verMod);
            }
        }
        //更新商品所用原料信息
        private void IsHaveMaterial(M_Product proMod)
        {
            B_Shop_Material matBll = new B_Shop_Material();
            B_CodeModel codeBll = new B_CodeModel("ZL_Shop_ProMaterial");
            try
            {
                DataTable dt = DBCenter.SelWithField(matBll.TbName, "ID");
                foreach (DataRow dr in dt.Rows)
                {
                    int id = Convert.ToInt32(dr["ID"]);
                    int matnum = DataConvert.CLng(Request.Form["mat_" + id + "_num"]);
                    if (matnum < 0) { matnum = 0; }
                    DataTable codeMod = codeBll.SelByWhere("ProID=" + proMod.ID + " AND MatID=" + id, "ID DESC");
                    if (codeMod.Rows.Count < 1)
                    {
                        codeMod.Rows.Add(codeMod.NewRow());
                    }
                    codeMod.Rows[0]["ProID"] = proMod.ID;
                    codeMod.Rows[0]["MatID"] = id;
                    codeMod.Rows[0]["MatNum"] = matnum;
                    codeMod.Rows[0]["Remind"] = "";
                    if (DataConvert.CLng(codeMod.Rows[0]["ID"]) > 0) { codeBll.UpdateByID(codeMod.Rows[0]); }
                    else { codeBll.Insert(codeMod.Rows[0]); }
                }
            }
            catch { }
        }
        //会员特选商品逻辑
        private void IsUserProduct(M_Product proMod, string olduids, string newuids)
        {
            #region 特选商品
            //有更改则执行特选商品
            string uids = StrHelper.IDS_GetChange(olduids, newuids);//uprouids_old_hid.Value, uprouids_hid.Value
            if (!string.IsNullOrEmpty(uids))
            {
                //1,目标表中可能无数据
                //2,只对变更部分操作
                //目的：会员可知道自己有哪些商品,商品处也可知道可有哪些特选
                string[] add = uids.Split('|')[0].Split(',');
                string[] remove = uids.Split('|')[1].Split(',');
                foreach (string id in add)
                {
                    int uid = DataConvert.CLng(id);
                    if (uid < 1) { continue; }
                    M_User_BindPro bindMod = bindBll.SelModelByUid(uid);
                    if (bindMod == null) { bindMod = new M_User_BindPro() { UserID = uid }; }
                    bindMod.ProIDS = StrHelper.AddToIDS(bindMod.ProIDS, proMod.ID.ToString());
                    if (bindMod.ID > 0) { bindBll.UpdateByID(bindMod); }
                    else { bindBll.Insert(bindMod); }
                }
                foreach (string id in remove)
                {
                    int uid = DataConvert.CLng(id);
                    if (uid < 1) { continue; }
                    M_User_BindPro bindMod = bindBll.SelModelByUid(uid);
                    if (bindMod == null) { bindMod = new M_User_BindPro() { UserID = uid }; }
                    bindMod.ProIDS = StrHelper.RemoveToIDS(bindMod.ProIDS, proMod.ID.ToString());
                    if (bindMod.ID > 0) { bindBll.UpdateByID(bindMod); }
                    else { bindBll.Insert(bindMod); }
                }
            }
            #endregion
        }
        //是否有赠品逻辑
        private void IsHavePresent(M_Product proMod)
        {
            B_Shop_Present ptBll = new B_Shop_Present();
            ptBll.BatInsert(proMod.ID, Request.Form["present_hid"]);
        }
        private M_Product FillProductModel(ref DataTable table, M_Product proMod = null)
        {
            if (proMod == null) { proMod = new M_Product(); }
            string adminname = badmin.GetAdminLogin().AdminName;
            //DataTable gpdt = gpBll.GetGroupList();
            /*--------------proMod------------*/
            if (proMod.ID < 1)
            {
                proMod.Nodeid = NodeID;
                proMod.FirstNodeID = nodeBll.SelFirstNodeID(proMod.Nodeid);
                proMod.ModelID = ModelID;
                proMod.TableName = modBll.SelReturnModel(ModelID).TableName;
                proMod.AddUser = adminname;
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
            proMod.PointVal = DataConverter.CLng(Request.Form["PointVal"]);
            proMod.Proinfo = Request.Form["Proinfo"];
            proMod.Procontent = Request.Form["Procontent"];
            proMod.Clearimg = Request.Form["txt_Clearimg"];
            proMod.Thumbnails = Request.Form["txt_Thumbnails"];
            proMod.Producer = Request.Form["Producer"];
            proMod.Brand = Request.Form["Brand"];
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
            proMod.UserShopID = DataConvert.CLng(Request.Form["UserShopID"]);//店铺页面应该禁止此项
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
        #region 库存管理
        public ActionResult StockList()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "stock")) { return null; }
            F_Stock filter = new F_Stock()
            {
                NodeID = DataConvert.CLng(Request["NodeID"]),
                ProID = DataConvert.CLng(Request["ProID"]),
                StockType = DataConvert.CLng(Request["StockType"]),
                ProName = Request["ProName"],
                AddUser = Request["AddUser"],
                SDate = Request["SDate"],
                EDate = Request["EDate"]
            };
            PageSetting setting = stockBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjaxRequest())
            {
                return PartialView("~/Areas/User/Views/UserShop/StockList_List.cshtml", setting);
            }
            return View(setting);
        }
        public ActionResult StockAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.shop, "stock")) { return null; }
            int ProID = DataConvert.CLng(Request.QueryString["ProID"]);
            string action = DataConverter.CStr(Request.QueryString["action"]);
            M_Product proMod = proBll.GetproductByid(ProID);
            return View(proMod);
        }
        public string Stock_Add()
        {
            M_AdminInfo adminMod = B_Admin.GetLogin();
            int ProID = DataConvert.CLng(Request["ProID"]);
            string action = DataConverter.CStr(Request.QueryString["action"]);
            M_Product proMod = proBll.GetproductByid(ProID);
            M_Stock stockMod = new M_Stock();
            stockMod.proid = ProID;
            stockMod.proname = proMod.Proname;
            stockMod.Pronum = DataConverter.CLng(Request.Form["Pronum"]);
            stockMod.stocktype = DataConverter.CLng(Request.Form["stocktype_rad"]);
            string code = 0 + DateTime.Now.ToString("yyyyMMddHHmmss");
            stockMod.danju = (stockMod.stocktype == 0 ? "RK" : "CK") + code;
            stockMod.UserID = adminMod.AdminId;
            stockMod.adduser = adminMod.AdminName;
            if (stockMod.Pronum < 1) { function.WriteErrMsg("出入库数量不能小于1"); return ""; }
            switch (stockMod.stocktype)
            {
                case 0:
                    proMod.Stock += stockMod.Pronum;
                    break;
                case 1:
                    proMod.Stock -= stockMod.Pronum;
                    if (proMod.Stock < 0) { function.WriteErrMsg("出库数量不能大于库存!"); return ""; }
                    break;
                default:
                    throw new Exception("出入库操作错误");
            }
            stockBll.AddStock(stockMod);
            proBll.UpdateByID(proMod);
            if (action.Equals("addpro"))
            {
                int num = stockMod.stocktype == 0 ? stockMod.Pronum : -stockMod.Pronum;
                return "<script>parent.addStock(" + num + ");</script>";
            }
            function.WriteSuccessMsg("库存操作成功", "StockList"); return "";
        }
        #endregion
        #region 版本备份
        public ActionResult Addon_VerBak()
        {
            PageSetting setting = verBll.SelPage(CPage, PSize, DataConvert.CLng(Request["GeneralID"]),"product");
            return View("Addon/VerBak", setting);
        }
        public int Addon_VerBak_Del(string ids)
        {
            verBll.Del(ids);
            return M_APIResult.Success;
        }
        #endregion
    }
}
