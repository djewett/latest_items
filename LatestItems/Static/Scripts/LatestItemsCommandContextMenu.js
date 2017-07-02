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

        var item = this._getSelectedItem(selection);

        // Checks if a container item has been selected.

        var isValidItemType = ($models.getItemType(item) == $const.ItemType.PUBLICATION) ||
							  ($models.getItemType(item) == $const.ItemType.FOLDER) ||
							  ($models.getItemType(item) == $const.ItemType.STRUCTURE_GROUP);

        // TODO: add check and corresponding logic for const.ItemType.CATMAN and Business Process Types (Web 8 only)

        if ((item != null) && (item !== undefined) && isValidItemType) {
            return true;
        }
        else {
            return false;
        }
    },

    _getSelectedItem: function (selection) {
        $assert.isObject(selection);

        switch (selection.getCount()) {
            case 0: return (selection.getParentItemUri) ? selection.getParentItemUri() : null;
            case 1: return selection.getItem(0);
            default: return null;
        }
    },

    /**
     * Executes your command. You can use _execute or execute as the property name.
     */
    execute: function (selection) {
        // Gets the item id
        var itemId = selection.getItem(0);
        // Sets the url of a popup window, passing through params for the ID of the selected folder/item
        var url = "${ViewsUrl}LatestItems.aspx?uri=";
        if (itemId) {
            url += itemId;
        }
        else {
            // TODO: come up with a better way of handling case where nothing is selected in item list (right panel of CME).
            url += "tcm:0";
            // Find a way to get what's selected in left panel, as normally that only shows up if something is selected in the right panel as well.
        }
        // Creates a popup with the above URL
        var popup = $popup.create(url, "menubar=no,location=no,resizable=no,scrollbars=no,status=no,width=700,height=910,top=10,left=10", null);
        popup.open();
    }
});