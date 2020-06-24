<%@ Page Language="C#" AutoEventWireup="true" CodeFile="comp.aspx.cs" Inherits="Extend_cart_comp" %>
<%@ Import Namespace="ZoomLa.Common" %>
<table class="table table-bordered table-striped" style="margin-top:10px;color:#999;font-size:12px;">
    <tr>
        <td>类型：</td>
        <td><input type="text" name="<%:H_GetID("type") %>" class="dogItem form-control text_300" value="<%=addMod.type %>" maxlength="15"/></td>
    </tr>
    <tr>
        <td>年龄：</td>
        <td>
            <select class="dogItem form-control text_300" name="<%:H_GetID("age") %>">
                <%=H_GetDPOption("age","",addMod.age) %>
            </select>
        </td>
    </tr>
    <tr>
        <td>体重：</td>
        <td>
            <select class="dogItem form-control text_300" name="<%:H_GetID("weight") %>">
                <%=H_GetDPOption("weight","",addMod.weight) %>
            </select>
        </td>
    </tr>
</table>
