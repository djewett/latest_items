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

        public class ExportConfigRequest { public string input { get; set; }
                                           public string outputFileWithPath { get; set; } }

        [HttpPost]
        [Route("ExportConfig")]
        public string GetExportConfig(ExportConfigRequest request)
        {
            string[] tcms = request.input.Split(',');

            string output = "";

            try
            {
                // Remove "BIN/":
                string assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                assemblyPath = assemblyPath.Substring(0, assemblyPath.Length - 4);
                string path = Path.Combine(assemblyPath, @"assets\export_credentials_and_config.txt");
                string[] files = File.ReadAllLines(path);

                // These settings are taken from Tridion.ContentManager.ImportExport.Common.dll. Normally, these would be entered in a web.config file, 
                // or similar, and pull in here simply by referencing the binding/endpoint in that config. But since there is no such file for this 
                // Alchemy plugin, we opt set these up here.
                var endpointAddress = new EndpointAddress(new Uri(files[2]));
                var binding = new BasicHttpBinding();
                binding.Name = "ImportExport_basicHttpBinding";
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
                //binding.MaxReceivedMessageSize = int.MaxValue;
                //binding.

                var importExportClient = new ImportExportServiceClient(binding, endpointAddress);
                importExportClient.ClientCredentials.UseIdentityConfiguration = true;
                importExportClient.ChannelFactory.Credentials.UseIdentityConfiguration = true;

                // Remove "user=" from files[0] and "password=" from files[1] to get the credentials.
                string user = files[0].Substring(5);
                string password = files[1].Substring(9);
                var credentials = new NetworkCredential(user, password);//"Administrator", "xxxxxxx");
                importExportClient.ChannelFactory.Credentials.Windows.ClientCredential = credentials;

                // No Impersonate() method available here:
                //string username = GetUserName();
                //importExportClient.ClientCredentials.Impersonate(username);

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
                    DownloadPackage(processId, request.outputFileWithPath, credentials); // @"C:\Packages\exported.zip", credentials);
                }



                //var cspParams = new CspParameters();
                //cspParams.
                //RSAParameters rsap = new RSAParameters();
                //BigInteger e = new BigInteger("3", 10);
                //byte[] exponentBytes = BitConverter.GetBytes(3);//Convert.FromBase64String("3");
                //rsap.Exponent = e.getBytes();
                //rsap.Exponent = exponentBytes;
                

                ////lets take a new CSP with a new 2048 bit rsa key pair
                //var csp = new RSACryptoServiceProvider(1024);
                ////csp.ImportParameters(rsap);


                ////how to get the private key
                //var privKey = csp.ExportParameters(true);

                ////and the public key ...
                //var pubKey = csp.ExportParameters(false);

                ////converting the public key into a string representation
                //string privKeyString;
                //{
                //    //we need some buffer
                //    var sw = new System.IO.StringWriter();
                //    //we need a serializer
                //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //    //serialize the key into the stream
                //    xs.Serialize(sw, privKey);
                //    //get the string from the stream
                //    privKeyString = sw.ToString();
                //}

                ////converting the public key into a string representation
                //string pubKeyString;
                //{
                //    //we need some buffer
                //    var sw = new System.IO.StringWriter();
                //    //we need a serializer
                //    var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //    //serialize the key into the stream
                //    xs.Serialize(sw, pubKey);
                //    //get the string from the stream
                //    pubKeyString = sw.ToString();
                //}




                ////we need some data to encrypt
                //var plainTextData = "foobar";

                ////for encryption, always handle bytes...
                //var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

                ////apply pkcs#1.5 padding and encrypt our data 
                //var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

                ////we might want a string representation of our cypher text... base64 will do
                //var cypherText = Convert.ToBase64String(bytesCypherText);


                /*
                 * some transmission / storage / retrieval
                 * 
                 * and we want to decrypt our cypherText
                 */

                ////first, get our bytes back from the base64 string ...
                //bytesCypherText = Convert.FromBase64String(cypherText);

                ////we want to decrypt, therefore we need a csp and load our private key
                //csp = new RSACryptoServiceProvider();
                //csp.ImportParameters(privKey);

                ////decrypt and strip pkcs#1.5 padding
                //bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

                ////get our original plainText back...
                //var plainTextData2 = System.Text.Encoding.Unicode.GetString(bytesPlainTextData);


                    //StringBuilder hex = new StringBuilder(pubKey.Exponent.Length * 2);
                    //foreach (byte b in pubKey.Exponent)
                    //{
                    //    hex.AppendFormat("{0:x2}", b);
                    //}
                    //string pubKeyExpAsHexString = hex.ToString();

                    //StringBuilder hex2 = new StringBuilder(pubKey.Modulus.Length * 2);
                    //foreach (byte b in pubKey.Modulus)
                    //{
                    //    hex2.AppendFormat("{0:x2}", b);
                    //}
                    //string pubKeyModAsHexString = hex2.ToString();

                    //StringBuilder hex3 = new StringBuilder(bytesCypherText.Length * 2);
                    //foreach (byte b in bytesCypherText)
                    //{
                    //    hex3.AppendFormat("{0:x2}", b);
                    //}
                    //string bytesCypherTextHexString = hex3.ToString();



                    // Use these public and private keys and make sure following string gets decrypted to "foober":
                    //
                    // <?xml version="1.0" encoding="utf-16"?>
                    // <RSAParameters xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                    //   <Exponent>AQAB</Exponent>
                    //   <Modulus>prSy4/Oe8iRgyWR649O7O2fSNYFUyZ8D+fUrHjXVbqAbOTitmiC15ZDdAueF6sW0hIjdcPVrmp5ZMnwmVU7whfHkAkNlDlT7PoLAU3/kGvtd0eLM125OxBqd1CHsEkjZjmPpWhoVcIn+G5hAIhBbalqUwNB+osK2lkkAvVpAkk8=</Modulus>
                    // </RSAParameters>
                    //
                    // <?xml version="1.0" encoding="utf-16"?>
                    // <RSAParameters xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
                    //   <Exponent>AQAB</Exponent>
                    //   <Modulus>prSy4/Oe8iRgyWR649O7O2fSNYFUyZ8D+fUrHjXVbqAbOTitmiC15ZDdAueF6sW0hIjdcPVrmp5ZMnwmVU7whfHkAkNlDlT7PoLAU3/kGvtd0eLM125OxBqd1CHsEkjZjmPpWhoVcIn+G5hAIhBbalqUwNB+osK2lkkAvVpAkk8=</Modulus>
                    //   <P>1uhvnIRWhHeOaw7/TeSeyLZneXeyY9AVwvOeUDGOpzXDWH4yghode9h4f0AVUrPpAgOChNVgv20TkFtrTwH6Rw==</P>
                    //   <Q>xpTPfsXk5/T4KxIlKLG5Bhtun1XDQOtXzt6NzyHBFIQEIzoYohxu5XMaMs8xwZFau+YVfkZflxMr7h4wAQ0juQ==</Q>
                    //   <DP>e06kc5bPGXSLx8u0GxpZLOrT1jMirPiA8/naVUMKCdDkQ8ss6c9YKW4cPU8krO5DfH9NDTBtMYjBV+vMV2nYEw==</DP>
                    //   <DQ>U2S25qQwhwCnH19VX4uTCe+HOz6G6sJqc6Oepfek3/q4yhphseKC57S4sdG1MXbbRcFQEWF4Tzdr4Wmn+ykLcQ==</DQ>
                    //   <InverseQ>LodX0DbWYS3fPDLhpWMf/qH3yMVkEoIkLItFXo4Up2eq02K5E9UbbCRLuW7RgPeHogpaRZOIAluHDMw32DsWrg==</InverseQ>
                    //   <D>gI2T7ejuRzf6UxNjGNEr7xGOrqf/JEO1o0mGaJOG9PoOREAKz3IuEst1Q0oaoQK4xANvEC6RPfiiPCY0wVBQdRtI63G/zah8diTIVFNOXhW6FP6eK2BweCihM/xcpJqDSLXBLxI3QaaIlr08K4J96mSiMKLBgG2GMcOYFY3auSE=</D>
                    // </RSAParameters>
                    //
                    // string tryDecryptingThisHex = "7883b11ab14c4b219a01fe193420d4595e4cf71eff8898d1f4aab0ca37ba84fe20b4288b8c7515885865a25050377b36026a55c045325cbaa949a5ff55441490a3f11088f1880d7ceef1c32b748124c19d1288447650abb548e6b1dd175c9aee09806b8c59769d52a5a5b1a9c1aff3448a795bae3c59223136bdd7322e1b2523";

                // hex:
                string tryDecryptingThisHex = "7883b11ab14c4b219a01fe193420d4595e4cf71eff8898d1f4aab0ca37ba84fe20b4288b8c7515885865a25050377b36026a55c045325cbaa949a5ff55441490a3f11088f1880d7ceef1c32b748124c19d1288447650abb548e6b1dd175c9aee09806b8c59769d52a5a5b1a9c1aff3448a795bae3c59223136bdd7322e1b2523";
                // "foobar" is the expected output when decrypting this input hex

                int NumberChars = tryDecryptingThisHex.Length;
                byte[] tryDecryptingThisBytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                    tryDecryptingThisBytes[i / 2] = Convert.ToByte(tryDecryptingThisHex.Substring(i, 2), 16);



                //string representation of byte[] to byte[]
                    //bytesCypherText = Convert.FromBase64String(cypherText);

                //RSAParameters privKeyTest = new RSAParameters();
                //privKeyTest.

                string privKeyAsXmlString = "<?xml version=\"1.0\"?><RSAParameters xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Exponent>AQAB</Exponent><Modulus>prSy4/Oe8iRgyWR649O7O2fSNYFUyZ8D+fUrHjXVbqAbOTitmiC15ZDdAueF6sW0hIjdcPVrmp5ZMnwmVU7whfHkAkNlDlT7PoLAU3/kGvtd0eLM125OxBqd1CHsEkjZjmPpWhoVcIn+G5hAIhBbalqUwNB+osK2lkkAvVpAkk8=</Modulus><P>1uhvnIRWhHeOaw7/TeSeyLZneXeyY9AVwvOeUDGOpzXDWH4yghode9h4f0AVUrPpAgOChNVgv20TkFtrTwH6Rw==</P><Q>xpTPfsXk5/T4KxIlKLG5Bhtun1XDQOtXzt6NzyHBFIQEIzoYohxu5XMaMs8xwZFau+YVfkZflxMr7h4wAQ0juQ==</Q><DP>e06kc5bPGXSLx8u0GxpZLOrT1jMirPiA8/naVUMKCdDkQ8ss6c9YKW4cPU8krO5DfH9NDTBtMYjBV+vMV2nYEw==</DP><DQ>U2S25qQwhwCnH19VX4uTCe+HOz6G6sJqc6Oepfek3/q4yhphseKC57S4sdG1MXbbRcFQEWF4Tzdr4Wmn+ykLcQ==</DQ><InverseQ>LodX0DbWYS3fPDLhpWMf/qH3yMVkEoIkLItFXo4Up2eq02K5E9UbbCRLuW7RgPeHogpaRZOIAluHDMw32DsWrg==</InverseQ><D>gI2T7ejuRzf6UxNjGNEr7xGOrqf/JEO1o0mGaJOG9PoOREAKz3IuEst1Q0oaoQK4xANvEC6RPfiiPCY0wVBQdRtI63G/zah8diTIVFNOXhW6FP6eK2BweCihM/xcpJqDSLXBLxI3QaaIlr08K4J96mSiMKLBgG2GMcOYFY3auSE=</D></RSAParameters>";

                var xs2 = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                MemoryStream stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(privKeyAsXmlString);
                writer.Flush();
                stream.Position = 0;
                RSAParameters privKeyTest = (RSAParameters)xs2.Deserialize(stream);


                //// For testing:
                //string privKeyStringTest;
                //{
                //    //we need some buffer
                //    var sw = new System.IO.StringWriter();
                //    //we need a serializer
                //    var xs3 = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
                //    //serialize the key into the stream
                //    xs3.Serialize(sw, privKeyTest);
                //    //get the string from the stream
                //    privKeyStringTest = sw.ToString();
                //}


                //var cspTest = new RSACryptoServiceProvider();
                //cspTest.ImportParameters(privKeyTest);

                ////we need some data to encrypt
                //var plainTextData4 = "foobar";

                ////for encryption, always handle bytes...
                //var bytesPlainTextData4 = System.Text.Encoding.Unicode.GetBytes(plainTextData4);

                ////apply pkcs#1.5 padding and encrypt our data 
                //var bytesCypherText4 = csp.Encrypt(bytesPlainTextData4, false);

                ////we might want a string representation of our cypher text... base64 will do
                //var cypherText4 = Convert.ToBase64String(bytesCypherText4);

                //output += cypherText4;

                
                try
                {

                    //we want to decrypt, therefore we need a csp and load our private key
                    var cspTest = new RSACryptoServiceProvider();
                    cspTest.ImportParameters(privKeyTest);

                    //decrypt and strip pkcs#1.5 padding
                    var bytesPlainTextDataTest = cspTest.Decrypt(tryDecryptingThisBytes, false);

                    //get our original plainText back...
                    // Note: bytes are apparently representing a UTF8 version of the decrypted text.
                    var plainTextData = System.Text.Encoding.UTF8.GetString(bytesPlainTextDataTest);

                    output += "*** " + plainTextData + " ***";// +System.Environment.NewLine + System.Environment.NewLine + privKeyStringTest;

                }
                catch (System.Security.Cryptography.CryptographicException e)
                {
                    output += tryDecryptingThisBytes + System.Environment.NewLine + System.Environment.NewLine +
                        e.ToString() + System.Environment.NewLine + System.Environment.NewLine +
                        e.StackTrace;
                }
                
                //foreach (byte b in tryDecryptingThisBytes)
                //    output += b + System.Environment.NewLine;

                //output += "Export complete - process state: " + processState.ToString();// + importExportClient.;

                //output += plainTextData + System.Environment.NewLine +
                //          cypherText + System.Environment.NewLine + System.Environment.NewLine +
                //          bytesCypherTextHexString + System.Environment.NewLine + System.Environment.NewLine +
                //          plainTextData2 + System.Environment.NewLine +
                //            System.Environment.NewLine + System.Environment.NewLine + 
                //            pubKeyString + System.Environment.NewLine + System.Environment.NewLine +
                //            pubKeyExpAsHexString + System.Environment.NewLine + System.Environment.NewLine +
                //            pubKeyModAsHexString + System.Environment.NewLine + System.Environment.NewLine +
                //            privKeyString;
                  //  output += "*** " + plainTextData + " ***" + System.Environment.NewLine + System.Environment.NewLine + privKeyStringTest;
            }
            catch(Exception e)
            {
                output += e.ToString() + System.Environment.NewLine + System.Environment.NewLine + e.StackTrace;
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

        private void DownloadPackage(string processId, string packageLocation, NetworkCredential credentials)
        {
            // These settings are taken from Tridion.ContentManager.ImportExport.Common.dll:
            var endpointAddress = new EndpointAddress(new Uri("http://localhost:81/webservices/ImportExportService2013.svc/streamDownload_basicHttp"));
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

        // GET /Alchemy/Plugins/HelloExample/api/LatestItemsService/LatestItems/tcm
        /// <summary>
        /// Finds the list of items not being used within a given Tridion folder (given by tcm).
        /// object.
        /// </summary>
        /// <param name="tcmOfContainer">
        /// The TCM ID of a Tridion container within which to find items that are latest items.
        /// Tridion
        /// <returns>
        /// Formatted HTML containing a list of unused items contained by the input folder.
        /// Tridion object
        /// </returns>
        [HttpGet]
        [Route("LatestItems/{tcmOfContainer}/{startTime}/{endTime}")]
        public string GetLatestItemsOld(string tcmOfContainer, string startTime, string endTime = "")
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

                // TODO: Add check for valid start and end times:

                if (String.IsNullOrEmpty(startTime))
                {
                    filter3.ModifiedAfter = DateTime.Now.AddDays(-1);
                }
                else
                {
                    filter3.ModifiedAfter = Convert.ToDateTime(startTime.Replace('-', '/').Replace('~', ':'));
                }

                if(String.IsNullOrEmpty(endTime))
                {
                    filter3.ModifiedBefore = DateTime.Now;
                }
                else
                {
                    filter3.ModifiedBefore = Convert.ToDateTime(endTime.Replace('-', '/').Replace('~', ':'));
                }

                filter3.IncludeLocationInfoColumns = true;
                //var results = client.GetSearchResults(filter);

                //filter3.FromRepository = new LinkToRepositoryData() { Title = "Building Blocks", IdRef = "tcm:0-1006-1" };
                filter3.SearchIn = new LinkToIdentifiableObjectData { IdRef = "tcm:0-1006-1"/*tcmOfFolder*/ };

                //////var items = client.GetSearchResults(filter3);

                //////if(items.Length > 0)
                //////{
                //////    html += CreateItemsHeading();
                //////}

                //filter2.ItemTypes = new[]{//ItemType.Schema,
                //                             ItemType.Component,
                //                             //ItemType.TemplateBuildingBlock,
                //                             //ItemType.ComponentTemplate,
                //                             //ItemType.PageTemplate,
                //                             ItemType.Publication,
                //                                ItemType.Folder,
                //ItemType.Category};

                foreach (IdentifiableObjectData item in client.GetSearchResults(filter3))
                {
                    string path = "";
                    if(item is RepositoryLocalObjectData)
                    {
                        path = ((RepositoryLocalObjectData)item).LocationInfo.Path;
                    }

                    string currItemHtml = "<div class=\"item\">";
                    //currItemHtml += "<div class=\"icon\" style=\"background-image: url(/WebUI/Editors/CME/Themes/Carbon2/icon_v7.1.0.66.627_.png?name=" + item.Title + "&size=16)\"></div>";
                    currItemHtml += "<div class=\"name\">" + item.Title + "</div>";
                    currItemHtml += "<div class=\"path\">" + path + "</div>"; // TODO: retrieve correct path
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