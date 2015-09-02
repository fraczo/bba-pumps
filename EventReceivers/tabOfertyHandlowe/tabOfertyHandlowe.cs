using System;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.Linq;
using System.IO;
using Helpers;

namespace EventReceivers.tabOfertyHandlowe
{
    public class tabOfertyHandlowe : SPItemEventReceiver
    {
        public override void ItemUpdated(SPItemEventProperties properties)
        {
            Execute(properties);
        }

        private void Execute(SPItemEventProperties properties)
        {
            this.EventFiringEnabled = false;

            try
            {

                SPListItem item = properties.ListItem;

                if (item["colStatus"] != null && item["colStatus"].ToString().Equals("Wysłana"))
                {
                    DateTime dataWyslania = item["colDataWyslania"] != null ? DateTime.Parse(item["colDataWyslania"].ToString()) : new DateTime();

                    if (dataWyslania != new DateTime())
                    {

                        if (item["selZadania"] != null)
                        {

                            SPFieldLookupValue zadanie = new SPFieldLookupValue(item["selZadania"].ToString());

                            if (zadanie.LookupId > 0)
                            {
                                //sprawdź czy którykolwiek z załączonych plików ma w nazwie oferta
                                const string fileNameMask = "OFERTA";

                                foreach (string attName in item.Attachments)
                                {
                                    if (attName.ToUpper().Contains(fileNameMask))
                                    {
                                        //sprawdź lokalizację piku źródłowego
                                        SPFile sFile = properties.Web.GetFile(Files.GetAttachmentUrl(item, attName));
                                        if (sFile.Exists)
                                        {

                                            //ustal nazwę docelową (nazwa bieżąca + id oferty + data wysyłki do klienta)
                                            string name = Path.GetFileNameWithoutExtension(sFile.Name);
                                            string ext = Path.GetExtension(sFile.Name);

                                            string targetFileName = String.Format("{0}_{1}_{2}{3}",
                                                name,
                                                item.ID.ToString(),
                                                dataWyslania.ToString(),
                                                ext);

                                            targetFileName = Files.CleanupFileName(targetFileName);

                                            //ustal lokalizację docelową
                                            SPList destList = properties.Web.Lists.TryGetList("Zadania");
                                            if (destList != null)
                                            {
                                                SPListItem destItem = destList.GetItemById(zadanie.LookupId);
                                                if (destItem != null)
                                                {
                                                    //prawdź czy istnieje
                                                    string targetUrl = SPUtility.ConcatUrls(destItem.Attachments.UrlPrefix, targetFileName);
                                                    SPFile targetFile = properties.Web.GetFile(targetUrl);
                                                    if (targetFile.Exists)
                                                    {
                                                        destItem.Attachments.DeleteNow(targetFileName);
                                                    }

                                                    //skopiuj plik do zadania poo nową nazwą docelową nadpisując istniejący
                                                    byte[] buffer = sFile.OpenBinary();
                                                    destItem.Attachments.Add(targetFileName, buffer);

                                                    //uzupełnij pozostałe pola jeżeli są puste

                                                    destItem["colDataWyslaniaOferty"] = dataWyslania;
                                                    destItem["colWartoscOferty"] = item["colWartoscOferty"] != null ? item["colWartoscOferty"] : 0;
                                                    destItem.Update();
                                                }
                                            }
                                        }

                                    }
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var result = ElasticMail.ReportError(ex, properties.WebUrl.ToString());
            }

            this.EventFiringEnabled = true;
        }




    }
}
