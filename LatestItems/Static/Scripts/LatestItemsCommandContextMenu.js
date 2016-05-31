Alchemy.command("${PluginName}", "LatestItemsContextMenu", {

    /**
     * If an init function is created, this will be called from the command's constructor when a command instance
     * is created.
     */
    init: function () {
        console.log("INIT CALLED FROM LatestItems");
    },

    /**
     * Whether or not the command is enabled for the user (will usually have extensions displayed but disabled).
     * @returns {boolean}
     */
    isEnabled: function (selection) {
        return true;
    },

    /**
     * Whether or not the command is available to the user. This impacts the context menu option.
     * @returns {boolean}
     */
    isAvailable: function (selection) {
        // Gets the selected item in Tridion GUI
        var items = selection.getItems();
        var item = $models.getItem(selection.getItem(0));
        // Checks if a container item has been selected.
        if ($models.isContainerItemType(item.getItemType())) {
            return true;
        }
        else {
            return false;
        }
    },

    /**
     * Executes your command. You can use _execute or execute as the property name.
     */
    execute: function (selection) {
        // Gets the item id
        var itemId = selection.getItem(0);
        // Sets the url of a popup window, passing through params for the ID of the selected folder/item
        var url = "${ViewsUrl}LatestItems.aspx?uri="
        if (itemId) {
            url += itemId;
        }
        else {
            url += "tcm:0"; // TODO: come up with a better way of handling case where nothing is selected in item list (right panel of CME)
            // Find a way to get what's selected in left panel, as normally that only shows up if something is selected in the right panel as well...
        }
        // Creates a popup with the above URL
        var popup = $popup.create(url, "menubar=no,location=no,resizable=no,scrollbars=no,status=no,width=700,height=910,top=10,left=10", null);
        popup.open();
    }
});