using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZoomLa.BLL.Helper;
using ZoomLa.SQLDAL;

public partial class Extend_cart_comp : System.Web.UI.Page
{
    public M_Cart_Addition addMod = null;
    public DataRowView item = null;
    //public M_Product proMod = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        item = (DataRowView)PageHelper.Aspx_GetModel(Request);
        //proMod = new B_Product().GetproductByid(DataConvert.CLng(item["ProID"]));
        addMod = JsonConvert.DeserializeObject<M_Cart_Addition>(item["Additional"].ToString());
        if (addMod == null) { addMod = new M_Cart_Addition(); }
        //Additional {"age":"","weight":"","type":""}
    }
    public string H_GetDPOption(string type, string emptyText, string selvalue = "")
    {
        DataTable dt = new DataTable();
        dt.Columns.Add(new DataColumn("name",typeof(string)));
        dt.Columns.Add(new DataColumn("value",typeof(string)));
        string[] options = null;
        switch (type)
        {
            case "age":
                {
                    options = "1岁,2岁,3岁,4岁,5-8岁,9-10岁,10岁以上".Split(',');
                }
                break;
            case "weight":
                {
                    options = "1.5公斤以下,1.5—3.5公斤,10—15公斤,20—25公斤,25公斤以上".Split(',');
                }
                break;
        }
        foreach (string option in options)
        {
            DataRow dr = dt.NewRow();
            dr["name"] = option;
            dr["value"] = option;
            dt.Rows.Add(dr);
        }
        if (!string.IsNullOrEmpty(emptyText))
        {
            DataRow dr = dt.NewRow();
            dr["name"] = emptyText;
            dr["value"] = "";
            dt.Rows.InsertAt(dr, 0);
        }

        string html = "";
        foreach (DataRow dr in dt.Rows)
        {
            string name = DataConvert.CStr(dr["name"]);
            string value = dr["value"].ToString();
            string selected = value.Equals(selvalue) ? "selected=\"selected\"" : "";
            html += "<option value=\"" + value + "\" " + selected + ">" + name + "</option>";
        }
        return html;
    }
    public string H_GetID(string type)
    {
        return type + "_" + item["ID"];
    }
    public class M_Cart_Addition
    {
        //年龄
        public string age = "";
        public string weight = "";
        //狗狗类型
        public string type = "";
    }
}