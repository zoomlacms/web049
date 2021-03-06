﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LeftMenu.aspx.cs" Inherits="ZoomLaCMS.MIS.OA.Menu.LeftMenu" MasterPageFile="~/Office/OAMain.master" %>
<asp:Content runat="server" ContentPlaceHolderID="head">
<title>OA左边栏</title>
<link type="text/css" rel="stylesheet" href="/Plugins/Third/Calendar/Calendar.css" />
<script src="/JS/calendar-brown.js" type="text/javascript"></script>   
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Content">
<div class="oaleftmenu">
<ul class="list-unstyled" id="userinfo_ul">
<div class="left_user">
		<div class="userimg pull-left">
			<img src="" onerror="shownoface(this)" runat="server" id="faceImg" /></div>
		<div class="userinfo" >
			<span>名称：<asp:Label runat="server" ID="userNameL" /></span><br />
			<span>部门：<asp:Label runat="server" ID="userGNL" /></span><br />
			<span>工号：<asp:Label runat="server" ID="Work_L" /></span>
		</div>
		<div class="clearfix"></div>
		<div id="CalendarMain"></div>
		<div id="calendarDiv" runat="server">
		</div>
		<div class="clearfix"></div>
	</div>
	<div class="commonuser" runat="server" id="leftChk3">
		<div class="common_t">
			<div class="duty_tp d-flex">
				<span>常用联系人</span>
			<div class="ml-auto mr-3">
			<i class="zi zi_compress oaarrow oaarrow" onclick="hideC(this);" style="display: none;"></i>
			<i class="zi zi_expand oaarrow oaarrowDown" onclick="showC(this);"></i>
			</div>
			</div>
		</div>
		<div class="content common_c clearfix" style="display: none;">
			<ul>
				<li><a href="#" style="display: block; width: 40px; height: 40px; float: left;">
					<img src="/Office/images/cuser.jpg" alt="" /></a><a class="name" href="#"> 暴风杨立东</a></li>
			</ul>
			<div class="clear"></div>
		</div>
	</div>
	<div class="commonuser" runat="server" id="leftChk5">
		<div class="history_t">
			<div class="duty_tp d-flex">
			<span>便捷访问</span>
			<div class="ml-auto mr-3">
				<i class="zi zi_compress oaarrow oaarrow" onclick="hideC(this);"></i>
				<i class="zi zi_expand oaarrowDown" style="display: none;" onclick="showC(this);"></i>
			</div>
<!--
				<i class="zi zi_compress oaarrow oaarrow" onclick="hideC(this);" style="display: none;"></i>
				<i class="zi zi_expand oaarrow oaarrowDown" onclick="showC(this);"></i>
-->
				
			</div>
		</div>
		<div class="content history_c clearfix">
		</div>
	</div> 
</ul>
<ul class="list-unstyled" id="draftdoc_ul">
<li><span><i class="zi zi_plusSquareblack"></i>公文拟稿</span></li>
<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>代收公文</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>公文发送</a></li>
	<li><a href="javascript:;" data-url="Drafting.aspx" class="oaleftul"><i class="zi zi_circleRights"></i>撰写公文</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>已发公文</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>已收公文</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>已收组配置</a></li>

</ul>
<ul class="list-unstyled" id="agencydoc_ul">
	<li><span><i class="zi zi_plusSquareblack"></i>代办公文</span></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>今日公文</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>历史公文</a></li> 
</ul>
<ul class="list-unstyled" id="office_ul">
	<li><span><i class="zi zi_plusSquareblack"></i>已办事务</span></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>事务管理</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>待办事务</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>已办事务</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>回收站</a></li>
</ul>
<ul class="list-unstyled" id="senddoc_ul">
	<li><span><i class="zi zi_plusSquareblack"></i>发文模板</span></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>发文代字</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>主题词</a></li> 
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>主送</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>抄送</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>抄报</a></li>
</ul>
<ul class="list-unstyled" id="newdoc_ul">    
	<li><span><i class="zi zi_plusSquareblack"></i>新建发文</span></li>   
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>发文拟稿</a></li>
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>发文追踪</a></li> 
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>收文等登记</a></li> 
	<li><a href="javascript:;" data-url="" class="oaleftul"><i class="zi zi_circleRights"></i>收文追踪</a></li>   
</ul>
<ul class="list-unstyled" id="doclist_ul">    
	<li><span><i class="zi zi_plusSquareblack"></i>已归档文件</span></li>
	<li><a href="javascript:;" data-url="doc/FiledList.aspx" class="oaleftul"><i class="zi zi_circleRights"></i>归档公文</a></li>
	<li><a href="javascript:;" data-url="doc/BorrowList.aspx" class="oaleftul"><i class="zi zi_circleRights"></i>借阅列表</a></li>
</ul>
</div> 
</asp:Content>
<asp:Content runat="server" ContentPlaceHolderID="Script">
<script src="/Plugins/Third/Calendar/Calendar.js"></script>
<script src="/JS/ICMS/OAMain.js"></script>
<script>
$(function () {
	//初始化日历
	CalendarHandler.initialize();
	//日历点击事件
	$("#CalendarMain #context .item:not(.lastItem)").on("click", function (e) {
		var year = $("#CalendarMain #title .selectYear").data("year");
		var month = $("#CalendarMain #title .selectMonth").data("month");
		var day = $(this).data("day");
		window.open("/Plat/Daily/AddDaily.aspx?name=admin&Date=" + year + "-" + month + "-" + day, "main_right");
	});
})
function hideC(obj) {
	$(obj).hide().siblings().show();
	$p = $(obj).parent().parent().parent().parent();
	$p.find("div.content").hide("fast");
}
function showC(obj) {
	$(obj).hide().siblings().show();
	$p = $(obj).parent().parent().parent().parent();
	$p.find("div.content").show("fast");
}
$(".oaleftul").click(function (e) {
	$(".oaleftul").removeClass("active");
	$(this).addClass("active");
	var right = $(this).attr("data-url");
	if (right != "")
		parent.showRightCnt($(this).attr("data-url"));
})
</script>
</asp:Content>