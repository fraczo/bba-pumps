using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.SharePoint.Linq;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace Helpers
{
    public class Files
    {
        public static string CleanupFileName(string nazwaPliku)
        {
            //string illegal = "\"M\"\\a/ry/ h**ad:>> a\\/:*?\"| li*tt|le|| la\"mb.?";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(nazwaPliku, "");
        }

        /// <summary>
        /// How to get attachement URL programatically
        /// </summary>
        /// <param name="oItem"></param>
        /// <returns></returns>
        //public static string GetAttachmentUrls(Microsoft.SharePoint.SPListItem oItem)
        //{

        //    string path = string.Empty;

        //    try
        //    {

        //        path = (from string file in oItem.Attachments
        //                orderby file
        //                select SPUrlUtility.CombineUrl(oItem.Attachments.UrlPrefix, file)).FirstOrDefault();
        //        return path;
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}

        public static string GetAttachmentUrl(Microsoft.SharePoint.SPListItem oItem, string attName)
        {
            string url = string.Empty;

            try
            {
                url = SPUrlUtility.CombineUrl(oItem.Attachments.UrlPrefix, attName);
            }
            catch (Exception)
            {}

            return url;
        }

    }
}
