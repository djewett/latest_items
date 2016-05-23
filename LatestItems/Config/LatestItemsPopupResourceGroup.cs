using Alchemy4Tridion.Plugins.GUI.Configuration;
using Alchemy4Tridion.Plugins.GUI.Configuration.Elements;

namespace LatestItems.Config
{
    /// <summary>
    /// Represents the ResourceGroup element within the editor configuration that contains this plugin's files
    /// and references.
    /// </summary>
    public class LatestItemsPopupResourceGroup : ResourceGroup
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LatestItemsPopupResourceGroup()
        {
            // When adding files you only need to specify the filename and not full path
            AddFile("LatestItemsPopup.js");
            AddFile("LatestItemsPopup.css");
            // The above is just a convenient way of doing the following...
            // AddFile(FileTypes.Reference, "Alchemy.Plugins.HelloWorld.Commands.HelloCommandSet");

            // Since Alchemy comes with several libraries I can reference JQuery this way and avoid having
            // to add it myself
            Dependencies.AddLibraryJQuery();
            
            // If you want this resource group to contain the js proxies to call your webservice, call AddWebApiProxy()
            AddWebApiProxy();

            Dependencies.Add("Tridion.Web.UI.Editors.CME");
            Dependencies.Add("Tridion.Web.UI.Editors.CME.commands");

            // Let's add our resources to the WhereUsedPlusGroup.aspx page.  This will inject
            // the resources without us having to manually edit it.
            AttachToView("LatestItems.aspx");
        }
    }
}
