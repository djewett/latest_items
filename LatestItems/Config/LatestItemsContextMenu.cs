using Alchemy4Tridion.Plugins.GUI.Configuration;

namespace LatestItems.Config
{
    /// <summary>
    /// Represents an extension element in the editor configuration for creating a context menu extension.
    /// </summary>
    public class LatestItemsContextMenu : ContextMenuExtension
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LatestItemsContextMenu()
        {
            // This is the id which gets put on the html element for this menu (to target with css/js).
            AssignId = "LatestItems";

            // The name of the extension menu
            Name = "LatestItemsdMenu";

            // Where to add the new menu in the current context menu.
            InsertBefore = Constants.ContextMenuIds.MainContextMenu.Separator7;
            
            // Generate all of the context menu items...
            AddItem("cm_latestitems", "Latet Items...", "LatestItems");

            // We need to add our resource group as a dependency to this extension
            Dependencies.Add<LatestItemsResourceGroup>();

            // Actually apply our extension to a particular view.  You can have multiple.
            Apply.ToView(Constants.Views.DashboardView);
        }
    }
}
