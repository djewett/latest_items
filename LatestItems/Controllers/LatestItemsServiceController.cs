using Alchemy4Tridion.Plugins;
using System;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace LatestItems.Controllers
{
    /// <summary>
    /// A WebAPI web service controller that can be consumed by your front end.
    /// </summary>
    /// <remarks>
    /// The following conditions apply:
    ///     1.) Must have AlchemyRoutePrefix attribute. You pass in the type of your AlchemyPlugin (the one that inherits AlchemyPluginBase).
    ///     2.) Must inherit AlchemyApiController.
    ///     3.) All Action methods must have an Http Verb attribute on it as well as a RouteAttribute (otherwise it won't generate a js proxy).
    /// </remarks>
    [AlchemyRoutePrefix("LatestItemsService")]
    public class LatestItemsServiceController : AlchemyApiController
    {
        // Dummy method needed, as I could not figure out how to get JS running properly without it.
        // TODO: Find a way to remove this dummy method.
        [HttpGet]
        [Route("DummyItems")]
        public string GetDummyItems()
        {
            return "";
        }

        [HttpGet]
        [Route("ExportConfig/{listHtml}")]
        public string GetExportConfig(string listHtml)
        {
            //string n = String.Format("{0}", Request.Form["latestItemsList"]);
            string[] tcms = listHtml.Split(',');
            string output = "";
            foreach(var tcm in tcms)
            {
                output += "tcm:" + tcm + System.Environment.NewLine;
            }
            return output;
        }

        // GET /Alchemy/Plugins/HelloExample/api/LatestItemsService/LatestItems/tcm
        /// <summary>
        /// Finds the list of items not being used within a given Tridion folder (given by tcm).
        /// object.
        /// </summary>
        /// <param name="tcmOfFolder">
        /// The TCM ID of a Tridion folder within which to find items that are latest items.
        /// Tridion
        /// <returns>
        /// Formatted HTML containing a list of unused items contained by the input folder.
        /// Tridion object
        /// </returns>
        [HttpGet]
        [Route("LatestItems/{tcmOfFolder}/{startTime}/{endTime}")]
        public string GetLatestItems(string tcmOfFolder, string startTime, string endTime = "")
        {
            // Create a new, null Core Service Client
            SessionAwareCoreServiceClient client = null;
            // TODO: Use Client, not client (then you don't have to worry about handling abort/dispose/etc.). <- Alchemy version 7.0 or higher
            // With Client, no need to call client.Abort();
            // Can we also remove catch block below if using Client?

            try
            {
                // Creates a new core service client
                client = new SessionAwareCoreServiceClient("netTcp_2013");
                // Gets the current user so we can impersonate them for our client
                 string username = GetUserName();
                client.Impersonate(username);

                // Start building up a string of html to return, including headings for the table that the html will represent.
                string html = "<div class=\"usingItems results\" id=\"latestItemsList\">";
                html += CreateItemsHeading();

                // Create a filter to recursively search through a folder and find all schemas, components, templates, etc. that are latest items.
                // TODO: Add support for ItemType.Keyword.
                var filterData = new OrganizationalItemItemsFilterData();
                filterData.ItemTypes = new[]{ItemType.Schema,
                                             ItemType.Component,
                                             ItemType.TemplateBuildingBlock,
                                             ItemType.ComponentTemplate,
                                             ItemType.PageTemplate};
                // When using OrganizationalItemItemsFilterData, we need to explicitly set a flag to include paths in resultXml.
                filterData.IncludePathColumn = true;
                filterData.Recursive = true;




                //////IdentifiableObjectData xxx = new FolderData();
                //////xxx.Id = tcmOfFolder;

                var filter2 = new SearchQueryData();
                filter2.BaseColumns = ListBaseColumns.IdAndTitle;
            //$folderLink = New-Object Tridion.ContentManager.CoreService.Client.LinkToIdentifiableObjectData
            //$folderLink.IdRef = $folder.Id
            //$filter.SearchIn = $folderLink
                //tcmOfFolder
                filter2.SearchIn = new LinkToIdentifiableObjectData { Title = "Copy 2 of Folder3", IdRef = "tcm:1006-2119-2"/*tcmOfFolder*/ };
                //filter2.FromRepository = new LinkToPublicationData() { IdRef = "tcm:0-1006-1" };
                //filter2.FromRepository = new LinkToRepositoryData { IdRef = "tcm:0-1006-1" };
                //filter2.BlueprintStatus = SearchBlueprintStatus.Local; // or anything else
                //filter2.FromRepository = new LinkToPublicationData { IdRef = tcmOfFolder };
                filter2.SearchInSubtree = true; /////// ??????
                filter2.ItemTypes = new[]{//ItemType.Schema,
                                             ItemType.Component,
                                             //ItemType.TemplateBuildingBlock,
                                             //ItemType.ComponentTemplate,
                                             //ItemType.PageTemplate,
                                             ItemType.Publication,
                                                ItemType.Folder};
                //filter2.IncludeLocationInfoColumns = true; /////// ??????
                filter2.ModifiedAfter = new System.DateTime(2016, 05, 21, 1, 0, 0, DateTimeKind.Unspecified); //DateTime.Today;
                //filter2.ModifiedAfter.Value.Subtract(System.TimeSpan.FromHours(3.0));
                filter2.ModifiedBefore = DateTime.Now; //new System.DateTime(2016, 05, 23, 1, 0, 0, 0); //DateTime.Now;


                SearchQueryData filter3 = new SearchQueryData();
                //string id3 = "tcm:1006-2119-2";
                //folder.IdRef = id3;
                //filter3.SearchIn = new LinkToIdentifiableObjectData { Title = "Copy 2 of Folder3", IdRef = "tcm:1006-2119-2"/*tcmOfFolder*/ }; //folder;
                //filter3.ModifiedAfter = Convert.ToDateTime("05/20/16");
                //filter3.ModifiedAfter = DateTime.Now.AddDays(-1);

                if (String.IsNullOrEmpty(startTime))
                {
                    filter3.ModifiedAfter = DateTime.Now.AddDays(-1);
                }
                else
                {
                    filter3.ModifiedAfter = Convert.ToDateTime(startTime.Replace('-', '/'));
                }

                if(String.IsNullOrEmpty(endTime))
                {
                    filter3.ModifiedBefore = DateTime.Now;
                }
                else
                {
                    filter3.ModifiedBefore = Convert.ToDateTime(endTime.Replace('-','/'));
                }
                //var results = client.GetSearchResults(filter);

                //////var items = client.GetSearchResults(filter3);

                //////if(items.Length > 0)
                //////{
                //////    html += CreateItemsHeading();
                //////}

                foreach (IdentifiableObjectData item in client.GetSearchResults(filter3))
                {
                    string currItemHtml = "<div class=\"item\">";
                    //currItemHtml += "<div class=\"icon\" style=\"background-image: url(/WebUI/Editors/CME/Themes/Carbon2/icon_v7.1.0.66.627_.png?name=" + item.Title + "&size=16)\"></div>";
                    currItemHtml += "<div class=\"name\">" + item.Title + "</div>";
                    currItemHtml += "<div class=\"path\">" + "TODO" + "</div>"; // TODO: retrieve correct path
                    currItemHtml += "<div class=\"id\">" + item.Id + "</div>";
                    currItemHtml += "</div>";

                    html += currItemHtml;
                }





                // Use the filter to get the list of ALL items contained in the folder represented by tcmOfFolder.
                // We have to add "tcm:" here because we can't pass a full tcm id (with colon) via a URL.
                //////XElement resultXml = client.GetListXml("tcm:" + tcmOfFolder, filterData);

                //////// Iterate over all items returned by the above filtered list returned.
                //////foreach (XElement currentItem in resultXml.Nodes())
                //////{
                //////    var id = currentItem.Attribute("ID").Value;

                //////    // Retrieve the list (as an XElement) of items that currently use currentItem.
                //////    var usingFilter = new UsingItemsFilterData();
                //////    usingFilter.IncludedVersions = VersionCondition.OnlyLatestVersions;
                //////    XElement usingItemsXElement = client.GetListXml(id, usingFilter);

                //////    // If there are no using elements, then currentItem is not in used and should be added to the html to return.
                //////    if (!usingItemsXElement.HasElements)
                //////    {
                //////        html += CreateItem(currentItem) + System.Environment.NewLine;
                //////        html += CreateItem(currentItem) + System.Environment.NewLine;
                //////    }
                //////}

                // Close the div we opened above
                html += "</div>";

                // Explicitly abort to ensure there are no memory leaks.
                client.Abort();

                // Return the html we've built.
                return html;
            }
            catch (Exception ex)
            {
                // Proper way of ensuring that the client gets closed... we close it in our try block above,
                // then in a catch block if an exception is thrown we abort it.
                if (client != null)
                {
                    client.Abort();
                }

                // We are rethrowing the original exception and just letting webapi handle it.
                throw ex;
            }
        }

        /// <summary>
        /// Borrowed from Tridion.Web.UI.Core.Utils, this gets the current username to be used in 
        /// core service impersonation
        /// </summary>
        /// <returns>
        /// String containing the username
        /// </returns>
        public string GetUserName()
        {
            string text = string.Empty;
            if (HttpContext.Current != null && HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
            {
                text = HttpContext.Current.User.Identity.Name;
            }
            else if (ServiceSecurityContext.Current != null && ServiceSecurityContext.Current.WindowsIdentity != null)
            {
                text = ServiceSecurityContext.Current.WindowsIdentity.Name;
            }
            if (string.IsNullOrEmpty(text))
            {
                text = Thread.CurrentPrincipal.Identity.Name;
            }
            return text;
        }

        /// <summary>
        /// Creates an HTML string containing the headings explaining our representation of a Tridion
        /// object's key information
        /// </summary>
        /// <returns>
        /// Formatted HTML presentation of the headings for key information for Tridion items
        /// </returns>
        public string CreateItemsHeading()
        {
            string html = "<div class=\"headings\">";
            html += "<div class=\"icon\">&nbsp</div>";
            html += "<div class=\"name\">Name</div>";
            html += "<div class=\"path\">Path</div>";
            html += "<div class=\"id\">ID</div></div>";

            return html;
        }

        /// <summary>
        /// Creates an HTML representation of a Tridion object, including its title, path and TCM ID
        /// </summary>
        /// <param name="item">An XElement containing all information on a Tridion item</param>
        /// <returns>
        /// Formatted HTML presentation of key information for Tridion items
        /// </returns>
        public string CreateItem(XElement item)
        {
            string html = "<div class=\"item\">";
            html += "<div class=\"icon\" style=\"background-image: url(/WebUI/Editors/CME/Themes/Carbon2/icon_v7.1.0.66.627_.png?name=" + item.Attribute("Icon").Value + "&size=16)\"></div>";
            html += "<div class=\"name\">" + item.Attribute("Title").Value + "</div>";
            html += "<div class=\"path\">" + item.Attribute("Path").Value + "</div>";
            html += "<div class=\"id\">" + item.Attribute("ID").Value + "</div>";
            html += "</div>";
            return html;
        }
    }
}