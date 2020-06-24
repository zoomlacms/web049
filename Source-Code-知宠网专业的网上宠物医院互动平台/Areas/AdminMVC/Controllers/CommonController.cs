using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ZoomLa.BLL;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Models.Field;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    public class CommonController : Ctrl_Admin
    {
        // GET: /AdminMVC/Common/
        B_User buser = new B_User();
        B_UserBaseField ubBll = new B_UserBaseField();
        public ActionResult UserBaseField(int uid,int mode)
        {
            //@Html.Partial("Fields/Index_User_AddContent", new VM_FieldModel(Model.ModelID, Model.NodeID, new ModelConfig() { Source = ModelConfig.SType.UserContent }, Model.GeneralID))
            DataTable valueDT = DBCenter.SelTop(1, "UserID", "*", "ZL_UserBase", "UserID=" + uid, "");
            ModelConfig modcfg = new ModelConfig() { Source = ModelConfig.SType.Admin, ValueDT = valueDT };
            modcfg.Mode = (ModelConfig.SMode)mode;
            VM_FieldModel model = new VM_FieldModel(ubBll.Select_All(), modcfg);
            return View("Fields/Index_User_AddContent", model);
        }
    }
}
