# latest_items

See this plugin in the Alchemy Webstore at: http://www.alchemywebstore.com/plugins/Latest-Items

Alchemy plugin for Tridion that filters a list of items edited within some range of time, and exports them to a config file for content porting.

IMPORTANT:

Depending on your Tridion installation, you may need to take the following steps to ensure the seach functionality is working as needed:

http://tridion.kb.sdl.com/#tab:homeTab:crumb:7:artId:5353

I noticed I had to redo those steps as well when my license expired and I needed to copy over the new Tridion licenses.

(see also: http://docs.sdl.com/LiveContent/content/en-US/SDL%20Tridion%20Connector%20for%20Media%20Manager-v2/GUID-49001ADE-8661-43E5-B906-237D6EF44793)



****IMPORTANT:

To get this working, under the current implementation, you need to add a file called "export_credentials_and_config.txt" to the assets folder (by temporarily changing the .a4t file to a .zip). See attached example (but it needs to go in the assets folder).
