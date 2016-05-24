using Alchemy4Tridion.Plugins.GUI.Configuration;

namespace LatestItems.Config
{
    /// <summary>
    /// Represents a ribbon tool bar
    /// </summary>
    public class LatestItemsRibbon : RibbonToolbarExtension
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LatestItemsRibbon()
        {
            // The id of the element (overridden b/c the ascx in the Group property contains the Id)
            AssignId = "LatestItemsRibbonToolbar";

            // The filename of the ascx user control that contains the button markup/controls.
            Group = "LatestItemsGroup.ascx";
            GroupId = Constants.GroupIds.HomePage.ManageGroup;
            InsertBefore = "WhereUsedBtn";
			// Try: InsertAfter = "Releasemanager";
			
            // The name of the extension.
            Name = "Latest Items";

            // Which Page tab the extension will go on.
            PageId = Constants.PageIds.HomePage;


            // Don't forget to add a dependency to the resource group that references the command set...
            Dependencies.Add<LatestItemsResourceGroup>();

            // And apply it to a view.
            Apply.ToView(Constants.Views.DashboardView, "DashboardToolbar");
        }
    }
}
