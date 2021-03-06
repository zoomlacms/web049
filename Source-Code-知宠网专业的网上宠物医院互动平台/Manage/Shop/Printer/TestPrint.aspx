﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TestPrint.aspx.cs" Inherits="ZoomLaCMS.Manage.Shop.Printer.TestPrint"MasterPageFile="~/Manage/I/Index.master" %>


<asp:Content runat="server" ContentPlaceHolderID="head">
    <title>模拟打印</title>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Content">
<%=Call.SetBread(new Bread[] {
		new Bread("/{manage}/Main.aspx","工作台"),
        new Bread("ListDevice.aspx","智能硬件"),
        new Bread() {url="", text="模拟打印",addon="" }}
		)
    %>
<div class="container-fluid pr-0">
<div class="row sysRow list_choice">
    <table class="table table-striped table-bordered sys_table">
        <tr>
            <th class="w12rem_lg">选择设备</th>
            <td>
                <ZL:Repeater ID="RPT" runat="server">
                    <ItemTemplate>
                        <label style="margin-right: 15px;">
                            <input type="radio" name="Dev_R" value="<%#Eval("ID") %>" <%#Eval("IsDefault").ToString().Equals("1") ? "checked='true'":"" %> /><%#Eval("Alias") %></label>
                    </ItemTemplate>
                </ZL:Repeater>
            </td>
        </tr>
         <tr>
            <th>打印数量</th>
            <td>
                <ZL:TextBox ID="Num_T" runat="server" CssClass="form-control max20rem" AllowEmpty="false" Text="1" ValidType="Int" />
            </td>
        </tr>
        <tr>
            <th>快速指令</th>
            <td>
                <select ID="Tlp_DP" class="form-control max20rem">
                    <option Value="">请选择</option>
                    <option Value="下班时间到了，请大家做好准备关机下班。">通知下班</option>
                    <option Value="打印机订单纸已不多，请相关部门及时更换。">通知换纸</option>
                    <option Value="顺丰快递快件到了，请xxx下楼取货。">通知取货</option>
                    <option Value="半小时后例行安全检查，请各部门做好准备。">安全检查</option>
                    <option Value="半小时后例行卫生检查，请大家做好准备。">卫生检查</option>
                    <option Value="今天要清点库存，请相关部门做好准备工作。">库存盘算</option>
                    <option Value="因天气原因，今天可能会停电，大家做好相关准备，及时保存，以防数据丢失!">停电预警</option>
                    <option Value="端午节到了，祝大家节日快乐!">节日问候</option>
                    <option Value="今天要进行财务封存工作，请相关部门做好工作准备。">财务封存</option>
                </select>
            </td>
        </tr>
        <tr>
            <th>打印内容</th>
            <td>
                <asp:TextBox runat="server" ID="Msg_T" TextMode="MultiLine" cssclass="m50rem_50" />
                <asp:RequiredFieldValidator runat="server" ID="R1" ControlToValidate="Msg_T" ErrorMessage="打印内容不能为空" ForeColor="Red" />
            </td>
        </tr>
        <tr>
            <td></td>
            <td>
                <asp:Button runat="server" ID="Print_Btn" OnClick="Print_Btn_Click" Text="发送打印" CssClass="btn btn-outline-info" />
                <a href="MessageList.aspx" class="btn btn-outline-info">查看流水</a>
            </td>
        </tr>
    </table>
</div></div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="ScriptContent">
<script>
    $(function () {
        var devid = parseInt("<%:DevId%>");
        if (devid > 0) {
            $("input[value=" + devid + "]").attr("checked",true);
        }
    });
    $("#Tlp_DP").change(function(){
        $("#Msg_T").text($(this).val())
    })
</script>
</asp:Content>
