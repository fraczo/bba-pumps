using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace EventReceivers.Helpers
{
    public class Folders
    {
        public static void FolderExists(SPWeb web, string folderName, SPList list, SPListItem listItem)
        {
            //Check folder exist in the SPWeb or not 

            SPListItem listFolder = null;
            bool folderExists = false;
            // Check to see if folder already exists, if not create it
            for (int i = 0; i < list.Folders.Count; i++)
            {
                if (list.Folders[i].Folder.Name == folderName)
                {
                    listFolder = list.Folders[i];
                    listItem = list.Items.Add(listFolder.Folder.ServerRelativeUrl, SPFileSystemObjectType.File, null);
                    web.AllowUnsafeUpdates = true;
                    listItem.Update();
                }
            }

            // The folder does not exist so we create it and add the item

            if (!folderExists)
            {
                listFolder = list.Items.Add(list.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, folderName);
                listFolder.Update();
                listFolder["Title"] = folderName;

                SPListItem newListItem = list.Items.Add(listFolder.Folder.ServerRelativeUrl, SPFileSystemObjectType.File, null);
                web.AllowUnsafeUpdates = true;
                foreach (SPField field in listItem.Fields)
                {
                    if (field.ReadOnlyField
                     || field.Id == SPBuiltInFieldId.Attachments)
                        continue;
                    newListItem[field.Id] = listItem[field.Id];
                }

                newListItem.Update();
                
                list.Update();
            }
        }
    }
}
