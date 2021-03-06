﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DownServer.aspx.cs" Inherits="ZoomLaCMS.Manage.FtpFile.DownServer" EnableViewStateMac="false" MasterPageFile="~/Manage/I/Index.master" %>
<asp:Content runat="server" ContentPlaceHolderID="head"><title>下载服务器</title></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Content">
    <%=Call.SetBread(new Bread[] {
		new Bread("/{manage}/Main.aspx","工作台"),
        new Bread("/{manage}/plus/ADManage.aspx","扩展功能"),	
        new Bread("","文件管理"),	
        new Bread() {url="DownServerManage.aspx", text="下载服务器",addon="" }}
		)
    %>
	<div class="container-fluid pr-0">
	<div class="row sysRow list_choice">
    <table class="table table-striped table-bordered table-hover sys_table">
        <tr align="center">
            <td colspan="2" class="spacingtitle">
                <strong>
                    <asp:Label ID="LblTitle" runat="server" Text="添加服务器" Font-Bold="True"></asp:Label>
                </strong>
            </td>
        </tr>
        <tr>
            <th class="w12rem_lg">
                服务器名称<br />
				<small class="text-muted">在此输入在前台显示的镜像服务器名，如广东下载、上海下载等。</small>
            </th>
            <td>
                <asp:TextBox ID="TxtServerName" class="form-control max20rem" runat="server" ></asp:TextBox>
                <asp:RequiredFieldValidator ID="ValrServerName" runat="server" ErrorMessage="下载服务器名称不能为空"
                    ControlToValidate="TxtServerName"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <th>
                服务器LOGO<br />
				<small class="text-muted">输入服务器LOGO的绝对地址，如http://www.z01.com/Images/ServerLogo.gif</small>
            </th>
            <td>
                <asp:TextBox ID="TxtServerLogo" class="form-control max20rem" runat="server"></asp:TextBox>
            </td>
        </tr>
        <tr>
            <th>
                <strong>服务器地址</strong><br />
				<small class="text-muted">请认真输入正确的服务器地址。如http://www.z01.com/这样的地址</small>
            </th>
            <td>
                <asp:TextBox ID="TxtServerUrl" class="form-control max20rem" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="ValrServerUrl" runat="server" ErrorMessage="下载服务器地址不能为空"
                    ControlToValidate="TxtServerUrl"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr >
            <th>链接地址加密方式</th>
            <td>
                <asp:DropDownList ID="Encrypttype" CssClass="form-control max20rem" runat="server">
                    <asp:ListItem Value="0">不加密</asp:ListItem>
                    <asp:ListItem Value="1">Base64加密</asp:ListItem>
                    <asp:ListItem Value="2">DES加密</asp:ListItem>
                    <asp:ListItem Value="3">RSA加密</asp:ListItem>
                </asp:DropDownList>
                <asp:Label ID="Label2" runat="server"></asp:Label>
            </td>
        </tr>

        <tr  id="Encrypt" runat="server" visible="false" >
            <th>链接地址加密密钥</th>
            <td>
                <asp:TextBox ID="EncryptKey" class="form-control max20rem" runat="server" TextMode="MultiLine"></asp:TextBox>
            </td>
        </tr>

        <tr  id="Tr1" >
            <th>附加时间戳加密</th>
            <td>
                <asp:CheckBox ID="TimeEncrypt" runat="server" Text="使用" AutoPostBack="true" />
				<small class="text-muted">说明:此时间戳是经过了MD5+SHA1加密,在设定时间内更新一次</small>
            </td>
        </tr>
        <tr  id="Tr2" runat="server">
            <th>
               更新时间戳间隔时间
                单位：分钟, 0 为不更新</th>
            <td>
                <asp:DropDownList ID="UpTimeutiList" CssClass="form-control max20rem" runat="server">
                    <asp:ListItem Value="1">每分钟</asp:ListItem>
                    <asp:ListItem Value="10">每十分钟</asp:ListItem>
                    <asp:ListItem Value="30">每三十分钟</asp:ListItem>
                    <asp:ListItem Value="60">每小时</asp:ListItem>
                    <asp:ListItem Value="360">每六小时</asp:ListItem>
                    <asp:ListItem Value="720">每十二小时</asp:ListItem>
                    <asp:ListItem Value="1440">每天</asp:ListItem>
                    <asp:ListItem Value="2880">每二天</asp:ListItem>
                    <asp:ListItem Value="7200">每五天</asp:ListItem>
                    <asp:ListItem Value="10080">每七天</asp:ListItem>
                    <asp:ListItem Value="44640">每月</asp:ListItem>
                    <asp:ListItem Value="133920">每季度</asp:ListItem>
                    <asp:ListItem Value="535680">每年</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr >
            <th>显示方式</th>
            <td>
                <asp:DropDownList ID="DropShowType" CssClass="form-control max20rem" runat="server">
                    <asp:ListItem Value="0">显示名称</asp:ListItem>
                    <asp:ListItem Value="1">显示LOGO</asp:ListItem>
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <th>允许访问用户组(权限设置)</th>
            <td>
                <asp:CheckBoxList ID="ReadRoot" runat="server">
                </asp:CheckBoxList>
            </td>
        </tr>
        <tr>
            <td></td>
            <td>
                <asp:Button ID="EBtnSubmit" class="btn btn-outline-info mt-2" Text="保存信息" OnClick="EBtnSubmit_Click" runat="server" />
                <input type="button" class="btn btn-outline-info mt-2" onclick="javascript: window.location.href = 'DownServerManage.aspx'" value="返回列表 " />
            </td>
        </tr>
    </table>
	</div></div>
</asp:Content>
