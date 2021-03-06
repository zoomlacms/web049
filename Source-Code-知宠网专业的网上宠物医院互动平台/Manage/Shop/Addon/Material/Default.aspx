﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="ZoomLaCMS.Manage.Shop.Addon.Material.Default" MasterPageFile="~/Manage/I/Index.Master"%>
<asp:Content runat="server" ContentPlaceHolderID="head"><title>原料列表</title></asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Content">
<%=Call.SetBread( new Bread[] {
	new Bread("/{manage}/I/Main.aspx","工作台"),
	new Bread("../../ProductManage.aspx","商城管理"),
	new Bread("/{manage}/Shop/Addon/exp/DeliverType.aspx","商城设置"),
	new Bread(){url="/{manage}/Shop/Addon/Material/Default.aspx", text="原料列表<a href='MaterialAdd.aspx'> [添加原料]</a>"}
	}) %>
<ZL:ExGridView ID="EGV" runat="server" AutoGenerateColumns="False" PageSize="10" IsHoldState="false" 
    OnPageIndexChanging="EGV_PageIndexChanging" AllowPaging="True" AllowSorting="True" OnRowCommand="EGV_RowCommand" OnRowDataBound="EGV_RowDataBound"
    CssClass="table table-striped table-bordered list_choice" EnableTheming="False" EnableModelValidation="True" EmptyDataText="数据为空">
    <Columns>
        <asp:TemplateField ItemStyle-CssClass="td_xs">
            <ItemTemplate>
                <input type="checkbox" name="idchk" value="<%#Eval("ID") %>" />
            </ItemTemplate>
        </asp:TemplateField>
        <asp:BoundField HeaderText="ID" DataField="ID" ItemStyle-CssClass="td_s" />
        <asp:BoundField HeaderText="名称" DataField="Name"  />
        <asp:BoundField HeaderText="单位" DataField="Unit" ItemStyle-CssClass="td_m"/>
        <asp:BoundField DataField="Remind" HeaderText="备注" />
        <asp:TemplateField HeaderText="操作" ItemStyle-CssClass="td_l">
            <ItemTemplate>
                <a class="option_style" href="MaterialAdd.aspx?id=<%#Eval("ID") %>"><i class="zi zi_pencilalt" title="修改"></i></a>
                <asp:LinkButton runat="server" class="option_style" CommandName="del2" CommandArgument='<%#Eval("ID") %>' OnClientClick="return confirm('确定要删除吗');"><i class="zi zi_trashalt" title="删除"></i></asp:LinkButton>
            </ItemTemplate>
        </asp:TemplateField>
    </Columns>
</ZL:ExGridView>
<div class="sysBtline">
<asp:Button runat="server" ID="BatDel_Btn" Text="批量删除" OnClick="BatDel_Btn_Click" class="btn btn-outline-danger" OnClientClick="return confirm('确定要删除吗?');"/>
</div>
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="ScriptContent">

</asp:Content>