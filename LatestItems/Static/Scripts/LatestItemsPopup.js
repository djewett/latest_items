/**
 * Handles all functionality of my popup window, including retrieving the unused items, refreshing the popup, etc.
 *
 * Note the self executing function wrapping the JS. This is to limit the scope of my variables and avoid
 * conflicts with other scripts.
 */
!(function () {
    // Alchemy comes with jQuery and several other libraries already pre-installed so assigning
    // it a variable here eliminates the redundancy of loading my own copy, and avoids any conflicts over
    // the $ character
    $j = Alchemy.library("jQuery");

    // Grabs the URL of the popup and gets the TCM of the item selected in Tridion from the querystring
    var url = location.href;
    var tcm = url.substring(url.indexOf("uri=tcm%3A") + 10, url.indexOf("#"));





    // DJ added:
    //$tcmutils.getPublicationIdFromItemId(tcm)
    //$j("#publicationUrl").val("tcm:" + tcm);





    // On page load I display the items not in use within the folder defined by tcm.
    updateLatestItems(tcm);

    /**
     * Takes a TCM ID for a Tridion folder and retrieves the a list of the contained items that are latest items.
     */
    function updateLatestItems(tcmInput) {

        //////$j("#progBar").remove().success(function (items) {
        //////})
        //////.error(function (type, error) {
        //////})
        //////.complete(function () {
        //////});;

        // This is the call to my controller where the core service code is used to gather the
        // latest items information. It is returned as a string of HTML.
        // NOTE: Dummy method needed here, as removing function call here stops JS from running properly.
        // TODO: Try to remove this dummy method call
        Alchemy.Plugins["${PluginName}"].Api.LatestItemsService.getDummyItems()
        .success(function (items) {

            // Currently, tcm:0 is entered in cases where no folder or publication is selected, when either the context menu or ribbon bar is used
            // to load the Latest Items popup:
            if ((tcmInput != "0") && (tcmInput != "")) {
                var pubTitle = $models.getItem("tcm:" + tcmInput).getPublication().getStaticTitle();
                $j("#publicationName").val(pubTitle);
            }

            if ($models.isContainerItemType($models.getItem("tcm:" + tcmInput).getItemType()))
            {
                $j("#folderId").val("");
            }

            var user = Tridion.UI.UserSettings.getJsonUserSettings(true).User.Data.Name;
            $j("#userId").val(user);

            $j("#exportUser").val(user);

            var now = new Date();
            var nowMonth = now.getMonth() + 1;
            $j("#endDate_date").val(nowMonth + "/" + now.getDate() + "/" + now.getFullYear());
            var nowHours = now.getHours();
            var ampm = nowHours >= 12 ? 'PM' : 'AM'; // Needs to be done before nowHours are manipulated
            nowHours = nowHours % 12;
            nowHours = nowHours ? nowHours : 12; // the hour '0' should be '12'
            var nowMinutes = now.getMinutes();
            nowMinutes = nowMinutes < 10 ? '0' + nowMinutes : nowMinutes;
            var nowSeconds = now.getSeconds();
            nowSeconds = nowSeconds < 10 ? '0' + nowSeconds : nowSeconds;
            var nowTime = nowHours + ":" + nowMinutes + ":" + nowSeconds + " " + ampm;
            $j("#endDate_time").val(nowTime);

            var yesterday = new Date();
            yesterday.setDate(now.getDate() - 1);
            var yestMonth = yesterday.getMonth() + 1;
            $j("#startDate_date").val(yestMonth + "/" + yesterday.getDate() + "/" + yesterday.getFullYear());
            // Time will always be the exact same as now:
            $j("#startDate_time").val(nowTime);


            $j("#exportPassword").val("XXX");

            // TODO: test encryption here and put encrypted password in text box

            //$j("#export_config_text").val($j("#exportPassword").val());


            //////function formatAMPM(date) {
            //////    var hours = date.getHours();
            //////    var minutes = date.getMinutes();
            //////    var ampm = hours >= 12 ? 'pm' : 'am';
            //////    hours = hours % 12;
            //////    hours = hours ? hours : 12; // the hour '0' should be '12'
            //////    minutes = minutes < 10 ? '0' + minutes : minutes;
            //////    var strTime = hours + ':' + minutes + ' ' + ampm;
            //////    return strTime;
            //////}

            // First arg in success is what's returned by your controller's action

            // Upon successful retrieval of latest items, we want to remove the progress bar and add the latest items to the markup
            // (there is a progress bar by default in the markup in LatestItems.aspx, as the search starts automatically when the 
            // popup is open).
            $j("#progBar").remove();
            $j(".tab-body.active").append(items);

            // We want to have an action when we click anywhere on the tab body
            // that isn't a used or using item
            $j(".tab-body").mouseup(function (e) {
                // To do this we first find the results item containing the latest items
                var results = $j(".results");
                if (!results.is(e.target) // if the target of the click isn't the results...
                && results.has(e.target).length === 0) // ... nor a descendant of the results
                {
                    // Call a function to deselect the current item
                    deselectItems();
                }
            });

            // The refresh button should always be enabled. It essentially re-runs the entire getLatestItems procedure, 
            // discarding any previously returned items. You may want to use this, for instance, in the scenario where you
            // have a component that is linked to. Once the linking component is deleted, the component linked to may no
            // longer be used, and so a refresh is required.
            // TODO: consider ways to make this more efficient (i.e. by only re-running the getLatestItems procedure for items
            // linked to from an item that is deleted). And then consider running this automatically after each deletion.
            $j("#refresh_items").click(function () {
                // When the refresh button is clicked, we want to clear out the markup for the list of items and add a progress bar, indicating 
                // a new search for the unused items has begun.
                $j(".tab-body.active").html("");
                $j(".tab-body.active").append("<progress id=\"progBar\"></progress>");
                // Call the same getLatestItems() Web API function that is used when the popup is first open.
                // TODO: Use POST, instead of string replacements to pass via query with GET:
                //Alchemy.Plugins["${PluginName}"].Api.LatestItemsService.getLatestItemsOld(tcmInput,
                //                                                                       $j("#startDate_date").val() + " " + $j("#startDate_time").val().replace(/:/g, "~"),
                //                                                                       $j("#endDate_date").val() + " " + $j("#endDate_time").val().replace(/:/g, "~"))
                // Note: tcmOfContainer can be for a publication, folder, structure group, etc.
                Alchemy.Plugins["${PluginName}"].Api.LatestItemsService.getLatestItems({tcmOfContainer: tcmInput,
                                                                                        publication: $j("#publicationName").val(),
                                                                                        user: $j("#userId").val(),
                                                                                        startTime: $j("#startDate_date").val() + " " + $j("#startDate_time").val(),
                                                                                        endTime: $j("#endDate_date").val() + " " + $j("#endDate_time").val()})
                .success(function (items) {
                    // Upon successful retrieval of latest items, we want to remove the progress bar and add the latest items to the markup.
                    $j("#progBar").remove();
                    $j(".tab-body.active").append(items);
                })
                .error(function (type, error) {
                    // First arg is a string that shows the type of error i.e. (500 Internal), 2nd arg is object representing
                    // the error.  For BadRequests and Exceptions, the error message will be in the error.message property.
                    console.log("There was an error", error.message);
                })
                .complete(function () {
                    // this is called regardless of success or failure.
                    deselectItems();
                    setupForItemClicked();
                });
            });

            $j("#export_config").click(function () {
                $j("#export_config_text").html("");
                // Convert an array of all tcms to a string:
                var theTcmString = $j(".item .id").text().toString();
                // Replace the "tcm:" substrings with commas, since we can't pass ":"
                theTcmString = theTcmString.replace(/tcm:/g, ",tcm:");
                // Remove the comma at the very beginning:
                theTcmString = theTcmString.substr(1);
                Alchemy.Plugins["${PluginName}"].Api.LatestItemsService.getExportConfig({input: theTcmString,
                                                                        outputFileWithPath: $j("#exportPackage").val()})
                .success(function (items) {
                    $j("#export_config_text").html(items);
                })
                .error(function (type, error) {
                    // First arg is a string that shows the type of error i.e. (500 Internal), 2nd arg is object representing
                    // the error.  For BadRequests and Exceptions, the error message will be in the error.message property.
                    console.log("There was an error", error.message);
                })
                .complete(function () {
                    // this is called regardless of success or failure.
                    deselectItems();
                    setupForItemClicked();
                });
            });

            setupForItemClicked();
        })
        .error(function (type, error) {
            // First arg is a string that shows the type of error i.e. (500 Internal), 2nd arg is object representing
            // the error.  For BadRequests and Exceptions, the error message will be in the error.message property.
            console.log("There was an error", error.message);
        })
        .complete(function () {
            // this is called regardless of success or failure.
        });
    }

    /**
     * Common routine that is used to specify what happens when an item in the list of unused items is clicked. 
     * This function should be called each time the latest items command is run, and in particular, each time the
     * "refresh" button is clicked.
     */
    function setupForItemClicked() {
        // An item is a Tridion item that is not being used by the current item (folder).
        // This is the click function for the items.
        $j(".item").click(function () {
            // When you click on an item we deselect any currently selected item
            $j(".item.selected").removeClass("selected")
            // And select the item you clicked on
            $j(this).addClass("selected");
            // We then use this function to enable the buttons since they are only enabled
            // when an item is selected
            enableButtons();

            // These are all the click functions for the buttons at the bottom of the plugin.
            // They get set when we click on an item because we only want them to happen when
            // the buttons are enabled and the buttons only get enabled when an item is selected.

            $j("#open_item.enabled").click(function () {
                // Gets the selected item TCM
                var selectedItemId = $j(".item.selected .id").html();
                // Checks if the selected item is a container and sets an appropriate command, either
                // "Properties" for containers or "Open" for other items
                var command = $models.isContainerItemType(selectedItemId) ? "Properties" : "Open";
                // Runs the Tridion command to open the selected item in the original CM window
                // Note that because this uses a $ rather than the $j assigned to JQuery this is actually
                // using the Sizzler library from the Tridion CME
                $cme.executeCommand(command, new Tridion.Cme.Selection(new Tridion.Core.Selection([selectedItemId])));

                // Added to fix issue where after deleting several items, opening an item was resulting in multiple
                // popups saying that the item is already open (even though it was only opened once)
                deselectItems();
            });

            $j("#go_to_item_location.enabled").click(function () {
                // Gets the selected item TCM
                var selectedItemId = $j(".item.selected .id").html();
                // Runs the Tridion command to go to the location of the selected item in the original CM window
                // Note that because this uses a $ rather than the $j assigned to JQuery this is actually
                // using the Sizzler library from the Tridion CME
                $cme.executeCommand("Goto", new Tridion.Cme.Selection(new Tridion.Core.Selection([selectedItemId])));

                // Added to fix issue where after deleting several items, opening an item was resulting in multiple
                // popups saying that the item is already open (even though it was only opened once)
                deselectItems();
            });
        });
    }

    /**
    ** Whenever we deactivate the current item we need to remove the selected class from the item
    ** and disable all buttons since they are dependent on an item being selected to have a meaning
    **/ 
    function deselectItems() {
        $j("#open_item").addClass("disabled");
        $j("#go_to_item_location").addClass("disabled");
        $j(".item.selected").removeClass("selected");
    }

    /**
    ** Enables all buttons by removing the disabled class and adding an enabled class.
    **/
    function enableButtons() {
        $j("#open_item").removeClass("disabled");
        $j("#open_item").addClass("enabled");
        $j("#go_to_item_location").removeClass("disabled");
        $j("#go_to_item_location").addClass("enabled");
    }
})();