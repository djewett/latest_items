<!doctype html>
<html>
    <head runat="server">
        <title>Latest Items</title>
		<link rel='shortcut icon' type='image/x-icon' href='${ImgUrl}favicon.png' />
    </head>
    <body>
        <div class="tabs">
            <div class="active">Items:</div>
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
                <div class="button" id="refresh_items"><span class="text">Find Items</span></div>
                <div id="startDate" class="filterControl stack vertical">
                    <label class="timeLabel">Start Time:</label>
	                <input id="startDate_date" class="stack-calc" type="text" style="width: 99px;" value="" />
	                <input id="startDate_time" class="stack-calc" type="text" style="width: 100px;" value="">
                    <!--
	                <div id="startDate_selectbutton" class="tridion button stack-elem select" style="-webkit-user-select: none;">
		                <span class="text">Select Date</span>
	                </div>
                    -->
                </div>
                <div id="endDate" class=" filterControl date stack vertical">
                    <label class="timeLabel">End Time:</label>
	                <input id="endDate_date" class="stack-calc" type="text" style="width: 99px;" value="" />
	                <input id="endDate_time" class="stack-calc" type="text" style="width: 100px;" value="">
                    <!--
	                <div id="endDate_selectbutton" class="tridion button stack-elem select" style="-webkit-user-select: none;">
		                <span class="text">Select Date</span>
	                </div>
                    -->
                </div>
                <div class="filterControl stack vertical">
                    <label>Publication Name:</label>
	                <input id="publicationName" class="stack-calc" type="text" style="width: 170px;" value="(All)" />
                </div>
                <!-- TODO: remove display:none from CSS for this container (folder/structure group/etc.) field and implement: -->
                <div id="containerFields" class="filterControl stack vertical">
                    <label>Folder ID (partial TCM):</label>
	                <input id="folderId" class="stack-calc" type="text" style="width: 138px;" value="" />
                </div>
                <div class="filterControl stack vertical">
                    <label>User:</label>
	                <input id="userId" class="stack-calc" type="text" style="width: 242px;" value="" />
                </div>
	        </div>
            <div id="exportControls">
                <div class="button" id="export_config"><span class="text">Export Items</span></div>
                <div class="filterControl stack vertical">
                    <label>Output package:</label>
	                <input id="exportPackage" class="stack-calc" type="text" style="width: 178px;" value="C:\Packages\exported.zip" />
                </div>
                <div id="exportUserFields" class="filterControl stack vertical">
                    <label>Export User:</label>
	                <input id="exportUser" class="stack-calc" type="text" style="width: 201px;" value="" />
                </div>
                <div id="exportPasswordFields" class="filterControl stack vertical">
                    <label>Export Password:</label>
	                <input id="exportPassword" class="stack-calc" type="password" style="width: 173px;" value="" />
                </div>
                <textarea id="export_config_text" name="Text1" cols="38" rows="20" readonly="readonly"></textarea>
            </div>
        </div>
    </body>
</html>