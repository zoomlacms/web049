using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Shop;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Shop;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Areas.User.Models;

namespace ZoomLaCMS.Areas.User.Controllers
{
    public class OrderController : ZLCtrl
    {
        OrderViewModel viewMod = null;
        B_OrderList orderBll = new B_OrderList();
        B_Order_Repair repBll = new B_Order_Repair();
        B_Order_Share shareBll = new B_Order_Share();
        B_Order_Back backBll = new B_Order_Back();
        B_CartPro cartProBll = new B_CartPro();
        B_Group groupBll = new B_Group();
        B_Admin badmin = new B_Admin();
        B_Payment payBll = new B_Payment();
        OrderCommon orderCom = new OrderCommon();
        B_CartPro cartBll = new B_CartPro();
        B_Product proBll = new B_Product();
        public void Index() { Response.Redirect("OrderList"); }
        public ActionResult OrderList()
        {
            viewMod = new OrderViewModel(CPage, PSize, mu, Request);
            if (function.isAjax())
            {
                return PartialView("OrderList_List", viewMod);
            }
            else
            {
                return View(viewMod);
            }
        }
        public PartialViewResult Order_Data()//兼容之前,后期移除
        {
            viewMod = new OrderViewModel(CPage, PSize, mu, Request);
            return PartialView("OrderList_List", viewMod);
        }
        public ActionResult TripOrder()
        {
            PageSetting setting = orderBll.U_SelPage(new Filter_Order()
            {
                cpage = CPage,
                psize = PSize,
                uids = mu.UserID.ToString(),
                orderType = ((int)M_OrderList.OrderEnum.Trval).ToString()

            });
            return View(setting);
        }
        public PartialViewResult Trip_Data()
        {
            //CPage, PSize, mu.UserID, "", "", (int)M_OrderList.OrderEnum.Trval
            PageSetting setting = orderBll.U_SelPage(new Filter_Order()
            {
                cpage = CPage,
                psize = PSize,
                uids = mu.UserID.ToString(),
                orderType = ((int)M_OrderList.OrderEnum.Trval).ToString()

            });
            return PartialView("TripOrder_List", setting);
        }
        public ActionResult HotelOrder()
        {
            PageSetting setting = orderBll.U_SelPage(new Filter_Order()
            {
                cpage = CPage,
                psize = PSize,
                uids = mu.UserID.ToString(),
                orderType = ((int)M_OrderList.OrderEnum.Hotel).ToString()

            });
            return View(setting);
        }
        public PartialViewResult Hotel_Data()
        {
            PageSetting setting = orderBll.U_SelPage(new Filter_Order()
            {
                cpage = CPage,
                psize = PSize,
                uids = mu.UserID.ToString(),
                orderType = ((int)M_OrderList.OrderEnum.Hotel).ToString()

            });
            return PartialView("HotelOrder_List", setting);
        }
        public ActionResult OrderProList()
        {
            return View(new VM_OrderPro(mu, Request));
        }
        public int Order_API()
        {
            int oid = Convert.ToInt32(Request.Form["oid"]);
            string action = Request.Form["action"];
            int result = Failed;
            //-----
            M_OrderList orderMod = orderBll.SelReturnModel(oid);
            if (mu.UserID != orderMod.Userid) { return result; }
            switch (action)
            {
                case "del":
                    {
                        orderBll.DelByIDS_U(oid.ToString(), mu.UserID);
                        orderBll.CancelOrder(orderMod);
                        result = Success;
                    }
                    break;
                case "receive":
                    {
                        if (orderMod.Paymentstatus < (int)M_OrderList.PayEnum.HasPayed) break;
                        orderBll.UpdateByField("StateLogistics", "2", oid);
                        if (DataConvert.CLng(orderMod.ExpressNum) > 0)
                        {
                            B_Order_Exp expBll = new B_Order_Exp();
                            M_Order_Exp expMod = expBll.SelReturnModel(DataConvert.CLng(orderMod.ExpressNum));
                            if (expMod != null)
                            {
                                expMod.SignDate = DateTime.Now.ToString();
                                expBll.UpdateByID(expMod);
                            }
                        }
                        result = Success;
                    }
                    break;
                case "reconver"://还原
                    {
                        orderBll.UpdateByField("Aside", "0", oid);
                        result = Success;
                    }
                    break;
                case "realdel"://彻底删除
                    {
                        orderBll.UpdateByField("Aside", "2", oid);
                        result = Success;
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
        public int Order_Del(string ids)
        {
            orderBll.DelByIDS_U(ids, mu.UserID);
            return Success;
        }
        //----------------------------Share
        private int ProID { get { return DataConvert.CLng(Request.QueryString["ProID"]); } }
        //admin
        public string Mode { get { return Request.QueryString["Mode"] ?? ""; } }
        public ActionResult AddShare()
        {
            int OrderID = DataConvert.CLng(Request.QueryString["OrderID"]);
            M_OrderList orderMod = orderBll.SelReturnModel(OrderID);
            if (orderMod.Userid != mu.UserID) { function.WriteErrMsg("你无权访问该订单"); return Content(""); }
            if (orderMod.OrderStatus < (int)M_OrderList.StatusEnum.OrderFinish) { function.WriteErrMsg("订单未完结,不允许评价"); return null; }
            ViewBag.dt = cartProBll.SelForRPT(OrderID, "comment");
            return View(orderMod);
        }
        public ActionResult ShareList()
        {
            if (Mode.ToLower().Equals("admin")) { B_Admin.CheckIsLogged(); }
            PageSetting setting = shareBll.SelPage(CPage, PSize, new F_Order_Share()
            {
                uid = DataConvert.CLng(Request["uid"]),
                proid = DataConvert.CLng(Request["proid"])
            });
            return View(setting);
        }
        public PartialViewResult Share_Data()
        {
            int pid = DataConvert.CLng(Request["pid"]);
            int proid = DataConvert.CLng(Request["proid"]);
            PageSetting setting = shareBll.SelPage(CPage, PSize, pid, proid);
            if (pid == 0) { return PartialView("ShareList_List", setting); }
            else { return PartialView("ShareList_Reply", setting); }
        }
        [HttpPost]
        public string Share_API()
        {
            string action = Request.Form["action"];
            string result = "-1";
            switch (action)
            {
                case "reply"://回复,不需要购买也可,但必须登录
                    {
                        string msg = Request.Form["msg"];
                        int pid = DataConvert.CLng(Request.Form["ID"]);
                        int rid = DataConvert.CLng(Request.Form["rid"]);
                        int proid = DataConvert.CLng(Request.Form["proid"]);
                        if (pid < 1 || rid < 1 || string.IsNullOrEmpty(msg)) break;
                        M_Order_Share replyMod = shareBll.SelReturnModel(rid);
                        M_Order_Share shareMod = new M_Order_Share();
                        shareMod.UserID = mu.UserID;
                        shareMod.Pid = pid;
                        shareMod.MsgContent = msg;
                        shareMod.ReplyID = rid;
                        shareMod.ProID = proid;
                        shareMod.ReplyUid = replyMod.UserID;
                        shareBll.Insert(shareMod);
                        result = "1";
                    }
                    break;
                case "del":
                    {
                        int id = Convert.ToInt32(Request.Form["id"]);
                        shareBll.Del(id);
                        result = "1";
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
        [HttpPost]
        [ValidateInput(false)]
        public void Share_Add()
        {
            int OrderID = DataConvert.CLng(Request.QueryString["OrderID"]);
            int cartid = DataConverter.CLng(Request.Form["cart_rad"]);
            //购买时间与商品信息也需要写入
            M_CartPro cartProMod = cartProBll.SelReturnModel(cartid);
            cartProMod.AddStatus = StrHelper.AddToIDS(cartProMod.AddStatus, "comment");//换为枚举
            M_Order_Share shareMod = new M_Order_Share();
            shareMod.Title = Request.Form["Title_T"];
            shareMod.MsgContent = Request.Form["MsgContent_T"];
            shareMod.UserID = mu.UserID;
            shareMod.IsAnonymous = string.IsNullOrEmpty(Request.Form["IsHideName"]) ? 0 : 1;
            shareMod.Score = DataConverter.CLng(Request.Form["star_hid"]);
            if (shareMod.Score > 5) { shareMod.Score = 5; }
            shareMod.Imgs = Request.Form["Attach_Hid"];
            shareMod.Labels = "";
            shareMod.OrderID = cartProMod.Orderlistid;
            shareMod.ProID = cartProMod.ProID;
            shareMod.OrderDate = cartProMod.Addtime;
            shareBll.Insert(shareMod);
            cartProBll.UpdateByID(cartProMod);
            DataTable dt = cartProBll.SelForRPT(OrderID, "comment");
            if (dt.Rows.Count < 1)
            {
                function.WriteSuccessMsg("评价成功,将跳转至商品页", OrderHelper.GetShopUrl(cartProMod.StoreID, cartProMod.ProID)); return; //返回商品页,或对应的商品页
            }
            else { function.WriteErrMsg("评价成功", "AddShare?OrderID="+OrderID); }
        }
        //----------------------------订单申请退款
        public ActionResult DrawBack()
        {
            M_OrderList orderMod = orderBll.SelReturnModel(Mid);
            if (!DrawBackCheck(orderMod, ref err)) { function.WriteErrMsg(err); return null; }
            M_Payment payMod = payBll.SelModelByPayNo(orderMod.PaymentNo);
            orderMod.Ordersamount = payMod.MoneyTrue;
            return View(orderMod);
        }
        public ActionResult DrawBack_Add()
        {
            M_OrderList orderMod = orderBll.SelReturnModel(Mid);
            if (!DrawBackCheck(orderMod, ref err)) { function.WriteErrMsg(err); return null; }
            string text = Request["Back_T"];
            if (text.Length < 10) { function.WriteErrMsg("退款说明最少需十个字符"); return null; }
            //------------------------
            M_Order_Back backMod = new M_Order_Back();
            backMod.OrderID = orderMod.id;
            backMod.OrderBak = JsonConvert.SerializeObject(orderMod);
            backMod.UserRemind = text;
            backMod.UserID = mu.UserID;
            backMod.ID = backBll.Insert(backMod);
            DBCenter.UpdateSQL(orderMod.TbName, "BackID=" + backMod.ID + ",OrderStatus=" + (int)M_OrderList.StatusEnum.DrawBack, "ID=" + orderMod.id);
            return Content("<script>top.location=top.location;</script>");
        }
        //检测订单是否允许退货,true:通过
        private bool DrawBackCheck(M_OrderList orderMod,ref string err)
        {
            err = "";
            if (orderMod == null) { err = "订单不存在"; }
            else if (orderMod.Userid != mu.UserID) { err = "订单不属于你,拒绝操作"; }
            //只有已付款订单,并且未退款才可申请
            else if (orderMod.Paymentstatus != (int)M_OrderList.PayEnum.HasPayed) { err = "该订单当前支付状态无法退款"; }
            else if (orderMod.OrderStatus != (int)M_OrderList.StatusEnum.OrderFinish) { err = "订单属于不可退款状态"; }
            else if (SiteConfig.SiteOption.THDate != 0 && (DateTime.Now - orderMod.AddTime).TotalDays > SiteConfig.SiteOption.THDate) { err = "订单已超过" + SiteConfig.SiteOption.THDate + "天,无法申请退款"; }
            return string.IsNullOrEmpty(err);
        }
        //--------------------------------商品申请退货
        public ActionResult MyOrderRepair()
        {
            PageSetting setting = repBll.U_SelAll(CPage, PSize, mu.UserID);
            if (Request.IsAjaxRequest())
            {
                return View("MyOrderRepair_List", setting);
            }
            return View(setting);
        }
        public ActionResult ReqRepair()
        {
            int Cid = DataConverter.CLng(Request.QueryString["cid"]);//购物车ID
            if (repBll.SelByCartID(Cid) != null) { function.WriteErrMsg("该商品已申请售后!","MyOrderRepair"); return null; }
            M_Order_Repair repairMod = repBll.SelReturnModel(Mid);
            M_Product proMod = null;
            M_CartPro cartMod = null;
            M_OrderList orderMod = null;
            if (repairMod != null)//修改
            {
                proMod = proBll.GetproductByid(repairMod.ProID);
                cartMod = cartBll.SelReturnModel(repairMod.CartID);
                orderMod = orderBll.GetOrderListByid(cartMod.Orderlistid);
            }
            else//申请
            {
                repairMod = new M_Order_Repair() { ProNum = 1, ReProType = 1 };
                cartMod = cartBll.SelReturnModel(Cid);
                proMod = proBll.GetproductByid(cartMod.ProID);
                if (proMod == null) { function.WriteErrMsg("需要操作的商品不存在"); return null; }
                if (cartMod == null) { function.WriteErrMsg("购物数据不存在"); return null; }
                orderMod = orderBll.GetOrderListByid(cartMod.Orderlistid);
                if (orderMod.Userid != mu.UserID) { function.WriteErrMsg("你无权申请该商品退货"); return null; }
                repairMod.TakeProCounty = orderMod.Shengfen;
                repairMod.ReProCounty = orderMod.Shengfen;
                repairMod.TakeProAddress = orderMod.Jiedao;
                repairMod.ReProAddress = orderMod.Jiedao;
                repairMod.UserName = orderMod.Rename;
                repairMod.Phone = orderMod.MobileNum;
            }


            //该商品支持的服务类型
            string[] services = proMod.GuessXML.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string ServiceType = "";
            for (int i = 0; i < services.Length; i++)
            {
                switch (services[i])
                {
                    case "drawback":
                        ServiceType += "<div data-value='drawback' class='type_div'>退货</div>";
                        break;
                    case "exchange":
                        ServiceType += "<div data-value='exchange' class='type_div'>换货</div>";
                        break;
                    case "repair":
                        ServiceType += "<div data-value='repair' class='type_div'>维修</div>";
                        break;
                }
            }
            if (services.Length == 0 || string.IsNullOrEmpty(ServiceType)) { function.WriteErrMsg("该商品不能返修或退货!", "OrderList"); return null; }
            if (repairMod.ID == 0) { repairMod.ServiceType = services[0]; }
            DataRow cartDR= DBCenter.Sel(cartMod.TbName, "ID=" + cartMod.ID).Rows[0];
            cartDR["CartID"] = cartDR["ID"];
            ViewBag.cartDR = cartDR;
            ViewBag.orderMod = orderMod;
            ViewBag.ServiceType = ServiceType;
            ViewBag.cartMod = cartMod;
            ViewBag.proMod = proMod;
            return View(repairMod);
        }
        public void ReqRepair_Add()
        {
            int Cid = DataConverter.CLng(Request.QueryString["cid"]);
            M_CartPro cartMod = cartBll.SelReturnModel(Cid);
            if (repBll.SelByCartID(Cid) != null) { function.WriteErrMsg("该商品已申请售后,不可重复申请!", "MyOrderRepair"); return; }
            //----------------------
            M_Order_Repair repairMod = new M_Order_Repair();
            repairMod.Deailt = Request.Form["Deailt"];
            repairMod.CretType = Request.Form["cret"];
            repairMod.ReProType = DataConverter.CLng(Request.Form["ReProType_Hid"]);
            repairMod.TakeProCounty = Request.Form["province_dp"] + " " + Request.Form["city_dp"] + " " + Request.Form["county_dp"];
            repairMod.TakeProAddress = Request.Form["TakeProAddress"];
            repairMod.ReProCounty = Request.Form["reprovince_dp"] + " " + Request.Form["recity_dp"] + " " + Request.Form["recounty_dp"];
            repairMod.ReProAddress = Request.Form["ReProAddress"];
            repairMod.UserName = Request.Form["UserName"];
            repairMod.Phone = Request.Form["Phone"];
            repairMod.ProImgs = Request.Form["Attach_Hid"];
            repairMod.ProID = cartMod.ProID;
            repairMod.OrderNO = orderBll.SelReturnModel(cartMod.Orderlistid).OrderNo;
            repairMod.ServiceType = Request.Form["ServicesType_Hid"];
            repairMod.UserID = mu.UserID;
            repairMod.CartID = cartMod.ID;
            repairMod.ProNum = DataConverter.CLng(Request.Form["ProNum"]);
            //----------------------
            if (repairMod.ProNum > cartMod.Pronum) { function.WriteErrMsg("退换商品数量不能大于购买数量"); return; }
            repBll.Insert(repairMod);
            cartMod.AddStatus = StrHelper.AddToIDS(cartMod.AddStatus, repairMod.ServiceType);
            cartBll.UpdateByID(cartMod);
            function.WriteSuccessMsg("添加成功!", "MyOrderRepair");
        }
        public ActionResult RepairInfo()
        {
            M_Order_Repair repairMod = repBll.SelReturnModel(Mid);
            if (repairMod == null) { function.WriteErrMsg("退货/返修记录不存在"); return null; }
            M_Product proMod = proBll.GetproductByid(repairMod.ProID);
            M_CartPro cartMod = cartBll.SelReturnModel(repairMod.CartID);
            ViewBag.cartMod = cartMod;
            ViewBag.proMod = proMod;
            return View(repairMod);
        }
    }
}
