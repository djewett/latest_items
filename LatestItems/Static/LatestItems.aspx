﻿<!doctype html>
<html>
    <head runat="server">
        <title>Latest Items</title>
		<link rel='shortcut icon' type='image/x-icon' href='${ImgUrl}favicon.png' />
    </head>
    <body>
        <div class="tabs">
            <div class="active">These are the latest updated items:</div>
        </div>
        <div class="tab-body active">
            <progress id="progBar"></progress>
        </div>
		<div class="controls">
			<div class="button disabled" id="open_item"><span class="text">Open</span></div>
			<div class="button disabled" id="go_to_item_location"><span class="text">Go To Location</span></div>
		</div>
        <div class="controls2">
            <div id="searchFilters">
                <div class="button" id="refresh_items"><span class="text">Find Latest</span></div>
                <div id="startDate" class="filterControl stack vertical">
                    <label class="timeLabel">Start Time:</label>
	                <input id="startDate_date" class="stack-calc" type="text" style="width: 99px;" value="5-21-2016" />
	                <input id="startDate_time" class="stack-calc" type="text" style="width: 100px;" value="12:00:00 AM">
                    <!--
	                <div id="startDate_selectbutton" class="tridion button stack-elem select" style="-webkit-user-select: none;">
		                <span class="text">Select Date</span>
	                </div>
                    -->
                </div>
                <div id="endDate" class=" filterControl date stack vertical">
                    <label class="timeLabel">End Time:</label>
	                <input id="endDate_date" class="stack-calc" type="text" style="width: 99px;" value="5-24-2016" />
	                <input id="endDate_time" class="stack-calc" type="text" style="width: 100px;" value="12:00:00 AM">
                    <!--
	                <div id="endDate_selectbutton" class="tridion button stack-elem select" style="-webkit-user-select: none;">
		                <span class="text">Select Date</span>
	                </div>
                    -->
                </div>
                <div class="filterControl stack vertical">
                    <label>Publication:</label>
	                <input id="publicationUrl" class="stack-calc" type="text" style="width: 207px;" value="070 Local Site" />
                </div>
                <div class="filterControl stack vertical">
                    <label>User:</label>
	                <input id="userId" class="stack-calc" type="text" style="width: 242px;" value="WIN-DFMAJQHT95L\Administrator" />
                </div>
	        </div>
            <div id="exportControls">
                <div class="button" id="export_config"><span class="text">Export Config</span></div>
                <textarea id="export_config_text" name="Text1" cols="38" rows="20" readonly="readonly"></textarea>
            </div>
        </div>
    </body>
</html>