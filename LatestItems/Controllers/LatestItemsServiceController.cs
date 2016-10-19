using Alchemy4Tridion.Plugins;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Xml.Linq;
using Tridion.ContentManager.CoreService.Client;
using Tridion.ContentManager.ImportExport;
using Tridion.ContentManager.ImportExport.Client;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;

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
        // Made static to persist across multiple POST and GET calls.
        // TODO: Look into a better way to do this.
        static RSACryptoServiceProvider csp;

        // Dummy method needed, as I could not figure out how to get JS running properly without it.
        // TODO: Find a way to remove this dummy method.
        [HttpGet]
        [Route("DummyItems")]
        public string GetDummyItems()
        {
            return "";
        }

        [HttpGet]
        [Route("ExportEndpointAndStreamDownloadAddresses")]
        public string[] GetExportEndpointAndStreamDownloadAddresses()
        {
            string[] exportEndpointAndStreamDownloadAddresses = {string.Empty, string.Empty};

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

                // App data is set up with the following parameters.
                ////string applicationId = "latestItemsApp";
                string exportEndpointId = "exportEndpointAddr";
                string streamDownloadId = "streamDownloadAddr";

                ApplicationData appData = client.ReadApplicationData(null, exportEndpointId);//// exportEndpointId, applicationId);
                if (appData != null)
                {
                    Byte[] data = appData.Data;
                    // exportEndpointId corresponds to the first element of the array return value.
                    exportEndpointAndStreamDownloadAddresses[0] = Encoding.Unicode.GetString(data);
                }

                appData = client.ReadApplicationData(null, streamDownloadId);// streamDownloadId, applicationId);
                if (appData != null)
                {
                    Byte[] data = appData.Data;
                    // streamDownloadId corresponds to the second element of the array return value.
                    exportEndpointAndStreamDownloadAddresses[1] = Encoding.Unicode.GetString(data);
                }

                // Explicitly abort to ensure there are no memory leaks.
                client.Abort();
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

            return exportEndpointAndStreamDownloadAddresses;
        }

        private void SetAppDataAddress(string id, string address)
        {
            // Create a new, null Core Service Client
            SessionAwareCoreServiceClient client = null;
            // TODO: Use Client, not client (then you don't have to worry about handling abort/dispose/etc.). <- Alchemy version 7.0 or higher
            // With Client, no need to call client.Abort();
            // Can we also remove catch block below if using Client?

            // TODO: Update to pass internal part of below snippet as a function (to reuse try-catch pattern)

            try
            {
                // Creates a new core service client
                client = new SessionAwareCoreServiceClient("netTcp_2013");
                // Gets the current user so we can impersonate them for our client
                string username = GetUserName();
                client.Impersonate(username);

                ////string applicationId = "latestItemsApp";

                Byte[] byteData = Encoding.Unicode.GetBytes(address);
                ApplicationData appData = new ApplicationData
                {
                    ApplicationId = id,////applicationId,
                    Data = byteData////,
                    ////TypeId = url.GetType().ToString()
                };
                client.SaveApplicationData(null, new[] { appData });////id, new[] { appData });

                // Explicitly abort to ensure there are no memory leaks.
                client.Abort();
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

        [HttpGet]
        [Route("PublicKeyModulusAndExponent")]
        public string[] GetPublicKeyModulusAndExponent()
        {
            csp = new RSACryptoServiceProvider(1024);

            var pubKey = csp.ExportParameters(false);

            // Convert byte[] to hex string.
            string modulusAsHexString = ConvertByteArrayToHexString(pubKey.Modulus);
            string exponentAsHexString = ConvertByteArrayToHexString(pubKey.Exponent);

            string[] publicKey = { modulusAsHexString, exponentAsHexString };

            return publicKey;
        }

        private string ConvertByteArrayToHexString(byte[] byteArray)
        {
            StringBuilder hex = new StringBuilder(byteArray.Length * 2);
            foreach (byte b in byteArray)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        private byte[] ConvertHexStringToByteArray(string hexString)
        {
            int NumberChars = hexString.Length;
            byte[] byteArray = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                byteArray[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return byteArray;
        }

        public class ExportConfigRequest
        {
            public string input { get; set; }
            public string outputFileWithPath { get; set; }
            public string encryptedPasswordAsHexString { get; set; }
            public string importExportEndpointAddress { get; set; }
            public string streamDownloadAddress { get; set; }
        }

        [HttpPost]
        [Route("ExportConfig")]
        public string GetExportConfig(ExportConfigRequest request)
        {
            string[] tcms = request.input.Split(',');

            string output = "";

            try
            {
                // Set up the app data for storing the endpoint addresses first, to ensure they get persisted.
                SetAppDataAddress("exportEndpointAddr", request.importExportEndpointAddress);
                SetAppDataAddress("streamDownloadAddr", request.streamDownloadAddress);

                // Retrieve credentials.
                string username = GetUserName();
                // Password comes in as a hex string from the request.
                string encryptedPWAsHexString = request.encryptedPasswordAsHexString;
                byte[] encryptedPWAsByteArray = ConvertHexStringToByteArray(encryptedPWAsHexString);
                // Decrypt and strip pkcs#1.5 padding.
                var bytesPlainTextDataTest = csp.Decrypt(encryptedPWAsByteArray, false);
                // Get our original plainText back.
                // Note: bytes are apparently representing a UTF8 version of the decrypted text.
                var password = System.Text.Encoding.UTF8.GetString(bytesPlainTextDataTest);
                var credentials = new NetworkCredential(username, password);

                // These settings are taken from Tridion.ContentManager.ImportExport.Common.dll. Normally, these would be entered in a web.config file, 
                // or similar, and pull in here simply by referencing the binding/endpoint in that config. But since there is no such file for this 
                // Alchemy plugin, we opt set these up here.
                var endpointAddress = new EndpointAddress(request.importExportEndpointAddress);
                var binding = new BasicHttpBinding();
                binding.Name = "ImportExport_basicHttpBinding";
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                var importExportClient = new ImportExportServiceClient(binding, endpointAddress);
                ////importExportClient.ClientCredentials.UseIdentityConfiguration = true;
                ////importExportClient.ChannelFactory.Credentials.UseIdentityConfiguration = true;
                importExportClient.ChannelFactory.Credentials.Windows.ClientCredential = credentials;

                // TODO: Work on these settings
                // e.g. June Test Comp 2016 goes in package twice for some reason, currently (even though it's not localized, etc.):
                var exportInstruction = new ExportInstruction()
                {
                    LogLevel = LogLevel.Normal,
                    BluePrintMode = BluePrintMode.ExportSharedItemsFromOwningPublication,
                    ExpandDependenciesOfTypes = {},
                    ErrorHandlingMode = ErrorHandlingMode.Skip
                };

                var selection = new Selection[] { new ItemsSelection(tcms) };

                var processId = importExportClient.StartExport(selection, exportInstruction);

                var processState = WaitForProcessFinish(importExportClient, processId);

                if (processState == ProcessState.Finished)
                {
                    // Use the same credentials as the above client
                    // TODO: validate outputFileWithPath
                    DownloadPackage(processId, request.outputFileWithPath, credentials, request.streamDownloadAddress);
                }

                output += "Success";
            }
            catch(Exception e)
            {
                // Get stack trace for the exception with source file information
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();

                output += e.ToString() + System.Environment.NewLine + e.StackTrace + System.Environment.NewLine + "Line: " + line;
            }

            return output;
        }

        private ProcessState WaitForProcessFinish(ImportExportServiceClient client, string processId)
        {
            do
            {
                Thread.Sleep(1000);
                ProcessState? processState = client.GetProcessState(processId);

                if (processState == ProcessState.Finished ||
                    processState == ProcessState.Aborted ||
                    processState == ProcessState.AbortedByUser)
                {
                    return processState.Value;
                }
            }
            while (true);
        }

        private void DownloadPackage(string processId, string packageLocation, NetworkCredential credentials, string streamDownloadAddress)
        {
            // These settings are taken from Tridion.ContentManager.ImportExport.Common.dll:
            var endpointAddress = new EndpointAddress(new Uri(streamDownloadAddress));
            var binding = new BasicHttpBinding();
            binding.Name = "streamDownload_basicHttp_2013";
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            binding.MessageEncoding = WSMessageEncoding.Mtom;
            binding.MaxReceivedMessageSize = int.MaxValue;

            var downloadClient = new ImportExportStreamDownloadClient(binding, endpointAddress);
            downloadClient.ClientCredentials.UseIdentityConfiguration = true; // TODO: this line needed?
            downloadClient.ChannelFactory.Credentials.UseIdentityConfiguration = true; // TODO: this line needed?
            downloadClient.ChannelFactory.Credentials.Windows.ClientCredential = credentials;

            using (var packageStream = downloadClient.DownloadPackage(processId, deleteFromServerAfterDownload: true))
            {
                using (var fileStream = File.Create(packageLocation))
                {
                    packageStream.CopyTo(fileStream);
                }
            }
        }

        public class LatestItemsRequest { public string tcmOfContainer { get; set; }
                                          public string publication { get; set; }
                                          public string user { get; set; }
                                          public string startTime { get; set; }
                                          public string endTime { get; set; } }

        [HttpPost]
        [Route("LatestItems")]
        public string GetLatestItems(LatestItemsRequest request)
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

                var filter = new SearchQueryData();

                // TODO: Add check for valid start and end times:

                if (String.IsNullOrEmpty(request.startTime))
                {
                    filter.ModifiedAfter = DateTime.Now.AddDays(-1);
                }
                else
                {
                    filter.ModifiedAfter = Convert.ToDateTime(request.startTime);
                }

                if (String.IsNullOrEmpty(request.endTime))
                {
                    filter.ModifiedBefore = DateTime.Now;
                }
                else
                {
                    filter.ModifiedBefore = Convert.ToDateTime(request.endTime);
                }

                filter.IncludeLocationInfoColumns = true;

                //TODO:Local doesn't seem to be working for 030C folders,
                // also, you want local in addition to localized, I think...
                //filter.BlueprintStatus = SearchBlueprintStatus.Local;

                if(!string.IsNullOrEmpty(request.publication) && !request.publication.Equals("(All)"))
                {
                    // Assume publication field is valid publication name
                    // TODO: Validate here
                    string pubTcm = client.GetTcmUri("/webdav/" + request.publication, null, null);
                    filter.SearchIn = new LinkToIdentifiableObjectData { IdRef = pubTcm };//"tcm:0-1006-1"/*tcmOfContainer*/ };
                }

                if (!string.IsNullOrEmpty(request.user) && !request.user.Equals("(All)"))
                {
                    // TODO: Validate here
                    //var users = client.GetSystemWideList(new UsersFilterData { BaseColumns = ListBaseColumns.IdAndTitle, IsPredefined = false });
                    //var userData = users.FirstOrDefault(item => item.Title == request.user);  //domain + "\\" + name);
                    //filter.Author = userData;
                }

                //filter.SearchInSubtree = true;

                filter.ItemTypes = new[]{ItemType.Schema,
                                         ItemType.Component,
                                         ItemType.TemplateBuildingBlock,
                                         ItemType.ComponentTemplate,
                                         ItemType.PageTemplate,
                                         ItemType.Category,
                                         ItemType.Folder,
                                         ItemType.Keyword,
                                         ItemType.Page,
                                         ItemType.StructureGroup,
                                         ItemType.VirtualFolder,
                                         ItemType.Publication};

                // TODO: Test localized instead of local; may need to process each one separately:
                filter.BlueprintStatus = SearchBlueprintStatus.Local;

                ////string extraTestHtml = "";// "<div>";

                // Make a copy of filter, but set BlueprintStatus to Localized.
                var filter2 = new SearchQueryData();
                filter2.ModifiedBefore = filter.ModifiedBefore;
                filter2.ModifiedAfter = filter.ModifiedAfter;
                filter2.IncludeLocationInfoColumns = filter.IncludeLocationInfoColumns;
                filter2.SearchIn = filter.SearchIn;
                filter2.ItemTypes = filter.ItemTypes;
                filter2.BlueprintStatus = SearchBlueprintStatus.Localized;

                var searchResults = client.GetSearchResults(filter);
                var searchResults2 = client.GetSearchResults(filter2);

                // Merge the two searchResults arrays (union goes into searchResults):
                int array1OriginalLength = searchResults.Length;
                Array.Resize<IdentifiableObjectData>(ref searchResults, array1OriginalLength + searchResults2.Length);
                Array.Copy(searchResults2, 0, searchResults, array1OriginalLength, searchResults2.Length);

                string extraTestHtml = "";
                
                ////foreach (IdentifiableObjectData item in client.GetSearchResults(filter))
                foreach (IdentifiableObjectData item in searchResults)
                {
                    string path = "";
                    if (item is RepositoryLocalObjectData)
                    {
                        path = ((RepositoryLocalObjectData)item).LocationInfo.Path;
                    }

                    bool outputItem = true;

                    // If user is not empty or set to (All), then run some logic to see if items match selected user within specified time range.
                    // If no specific user is specified, DO NOT run these checks as they are expensive and not necessary in that case!
                    if (!string.IsNullOrEmpty(request.user) && !request.user.Equals("(All)"))
                    {
                        // Set flag to false by default and set back to true if we find a match.
                        outputItem = false;

                        VersionsFilterData versionsFilter = new VersionsFilterData();
                        versionsFilter.IncludeRevisorDescriptionColumn = true;
                        IdentifiableObjectData[] versionList = client.GetList(item.Id, versionsFilter);

                        foreach (IdentifiableObjectData objectData in versionList)
                        {
                            var versionInfo = (FullVersionInfo)objectData.VersionInfo;

                            // Check 2 things:
                            // 1) that versionInfo.Revisor.Title == request.user
                            // 2) that versionInfo.RevisionDate.Value is between filter.ModifiedAfter and filter2.ModifiedBefore
                            // If we find a match, set outputItem to true and break the foreach loop.

                            if(versionInfo.Revisor.Title == request.user)
                            {
                                if ((objectData.VersionInfo.RevisionDate >= filter.ModifiedAfter) && (objectData.VersionInfo.RevisionDate <= filter.ModifiedBefore))
                                {
                                    outputItem = true;
                                    break;
                                }
                            }

                            //extraTestHtml += objectData.Id + " -- " + objectData.VersionInfo.RevisionDate.Value.ToString() + " -- " + versionInfo.Revisor.Title + System.Environment.NewLine;
                        }
                    }

                    if (outputItem)
                    {
                        string currItemHtml = "<div class=\"item\">";
                        // TODO: Look at Not_Used to see how icon is retrieved:
                        //currItemHtml += "<div class=\"icon\" style=\"background-image: url(/WebUI/Editors/CME/Themes/Carbon2/icon_v7.1.0.66.627_.png?name=" + item.Title + "&size=16)\"></div>";
                        currItemHtml += "<div class=\"name\">" + item.Title + "</div>";
                        currItemHtml += "<div class=\"path\">" + path + "</div>";
                        currItemHtml += "<div class=\"id\">" + item.Id + "</div>";
                        currItemHtml += "</div>";
                        html += currItemHtml;
                    }
                }

                // FOR TESTING / LOGGING ONLY!!! TODO: REMOVE:
                //html += "<div class=\"item\">";
                //html += "<div class=\"name\">" + extraTestHtml + "</div>";
                //html += "<div class=\"path\">" + request.publication + " -- " + request.user + "</div>";
                //html += "<div class=\"id\">" + filter.ModifiedAfter + " -- " + filter.ModifiedBefore + "</div>";
                //html += "</div>";

                ////System.IO.File.WriteAllText(@"C:\Users\Administrator\Desktop\WriteText.txt", extraTestHtml);
                ////System.IO.File.WriteAllText(@"C:\Users\Administrator\Desktop\WriteText.txt", filter.ModifiedAfter + " // " + filter.ModifiedBefore);
                ////extraTestHtml += "</div>";

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

            if ((HttpContext.Current != null) && (HttpContext.Current.User != null) && (HttpContext.Current.User.Identity != null))
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