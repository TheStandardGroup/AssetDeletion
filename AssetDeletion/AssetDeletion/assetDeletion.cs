using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pageflex.Interfaces.Storefront;
using PageflexServices;
using System.Web;
using System.Web.UI;

namespace AssetDeletion
{
    public class assetDeletion : SXIExtension
    {
        public override string UniqueName
        {
            get
            {
                return "AssetDeletion.standardgroup.com";
            }
        }
        public override string DisplayName
        {
            get
            {
                
                return "TSG: Asset Deletion";
            }
        }

        public override int GetConfigurationHtml(KeyValuePair[] parameters, out string HTML_configString)
        {
            string movesel = base.Storefront.GetValue("ModuleField", "ExtensionCanDelete", "AssetDeletion.standardgroup.com");
            string first = "<br><br><strong>Extension Configuration:</strong><br><br><table><tr><td>Can Delete(Comma Seperated List):</td><td><input type='text' size='10' name='movall' value='" + movesel + "'>";
            string end = "</td></tr></table>";
            HTML_configString = null;
            if (parameters == null)
            {
                HTML_configString = first + end;
            }
            else
            {
                bool isMoveBad = false;
                foreach (KeyValuePair pair in parameters)
                {
                    if (pair.Name.Equals("movall") && (pair.Value.Length == 0))
                    {
                        isMoveBad = true;
                        //HTML_configString = first + "<font color=red><strong>Required.</strong></font>";
                    }
                    if (pair.Name.Equals("movall") && (pair.Value.Length != 0))
                    {
                        base.Storefront.SetValue("ModuleField", "ExtensionCanDelete", "AssetDeletion.standardgroup.com", pair.Value);
                    }
                }
                string firstReq = "";
                if (isMoveBad)
                {
                    firstReq = "<font color=red><strong>Required.</strong></font>";
                }
           
                if (isMoveBad)
                    HTML_configString = first + firstReq + end;
            }
            return 0;
        }

        public override int PageLoad(string pageBaseName, string eventName)
        {

            if ((pageBaseName == "usercontentlibrary_aspx"))
            {
                string url = HttpContext.Current.Request.Url.AbsoluteUri;
                string[] findVars = url.Split('?');
                var page = HttpContext.Current.CurrentHandler as Page;
                string userId = Storefront.GetValue("SystemProperty", "LoggedOnUserID", null);
                string canDel = base.Storefront.GetValue("ModuleField", "ExtensionCanDelete", "AssetDeletion.standardgroup.com");
                string[] listOfDelGroups = canDel.Split(',');
                string[] userGroups = Storefront.GetListValue("UserListProperty", "AllContainingGroups", userId);
                bool canIDel = false;

                for (int i = 0; i < userGroups.Length && !canIDel; i++) {
                    for (int c = 0; c < listOfDelGroups.Length && !canIDel; c++) { 
                        if(userGroups[i].Equals(listOfDelGroups[c]))
                            canIDel = true;
                    }
                }


                if(canIDel){
                    string[] imgAssets = Storefront.GetListValue("UserListProperty", "UserImages", userId);
                    if (findVars.Length > 1)
                    {
                        if (!findVars[1].Equals("mode=trash"))
                        {
                            //string[] imgAssets = Storefront.GetListValue("UserListProperty", "UserImages", userId);
                            string[] pdfAssets = Storefront.GetListValue("UserListProperty", "UserPdfDocuments", userId);
                            string[] dbAssets = Storefront.GetListValue("UserListProperty", "UserDatabases", userId);
                            string[] delIds = findVars[1].Split(',');
                            int totalCount = imgAssets.Length + pdfAssets.Length + dbAssets.Length;

                            for (int i = 1; i < delIds.Length - 1; i++)
                            {
                                int iDelId = Convert.ToInt32(delIds[i]);
                                if (iDelId < imgAssets.Length)
                                {
                                    if (delIds[0].Equals("del"))
                                        Storefront.DeleteAsset(imgAssets[iDelId]);
                                    else if (delIds[0].Equals("hide"))
                                        Storefront.SetValue("AssetProperty", "DoShowToUser", imgAssets[iDelId], "0");
                                }
                                else if (iDelId < (pdfAssets.Length + imgAssets.Length))
                                {
                                    if (delIds[0].Equals("del"))
                                        Storefront.DeleteAsset(pdfAssets[iDelId - imgAssets.Length]);
                                    else if (delIds[0].Equals("hide"))
                                        Storefront.SetValue("AssetProperty", "DoShowToUser", pdfAssets[iDelId - imgAssets.Length], "0");
                                }
                                else
                                {
                                    if (delIds[0].Equals("del"))
                                        Storefront.DeleteAsset(dbAssets[iDelId - imgAssets.Length - pdfAssets.Length]);
                                    else if (delIds[0].Equals("hide"))
                                        Storefront.SetValue("AssetProperty", "DoShowToUser", dbAssets[iDelId - imgAssets.Length - pdfAssets.Length], "0");
                                }
                            }
                        }
                    
                    }

                   
                    if (findVars.Length > 1)
                    {
                        if (!findVars[1].Equals("mode=trash"))
                            HttpContext.Current.Response.Redirect(findVars[0]);
                    }
                    else {
                        string js = createJavaScript(userId,imgAssets.Length);
                        page.ClientScript.RegisterStartupScript(this.GetType(), "Add_Check_Box", js);
                    }
                }
            }
            
            return eSuccess;
        }

        private string createJavaScript(string userId,int imgLen) {
            string js = "<script type='text/javascript' src='jSINI.js'></script>";
            js += "<style type = 'text/css'>";
            js += "a.disabledForButtons {";
            js += " opacity: 0.5;";
            js += " pointer-events: none;";
            js += " cursor: default;";
            js += "}";
            js += "</style>";
            js += "<script type = 'text/javascript'>";
            js += "Sys.WebForms.PageRequestManager.getInstance().add_endRequest(myJQueryRequestHandler);";
            js += "function myJQueryRequestHandler(sender,args){";
            //js += "alert('" + shouldRedir + "');";
            js += "addChecks();}";
            js += "$(document).ready(function(){";
            //js += "alert('" + imgLen + "');";
            js += "addChecks();";
            js += "});";
            js += "function addChecks(){";
            js += "     $('.assetOptions').each(function(i){";
            js += "         $(this).closest('td').append('<input type=\"checkbox\" class=\"aBox\" name=\"select\" value='+i+'>');";
            js += "     });";
            //js += "     $('#ctl01_div').append('<div id=\"ctl02_div\" class=\"siteButton\" style=\"margin-top: 10px; text-align: center; width: 15%\"  name=\"ctl02_div\">";
            js += "     $('#AssetDataList').parent().append('<div id=\"ctl02_div\" class=\"siteButton\" style=\"margin-top: 10px; text-align: center; width: 15%\"  name=\"ctl02_div\">";
            js += "     <div class=\"siteButton-t\"><div class=\"siteButton-b\"><div class=\"siteButton-l\"><div class=\"siteButton-r\">";
            js += "     <div class=\"siteButton-tl\"><div class=\"siteButton-tr\"><div class=\"siteButton-bl\"><div class=\"siteButton-br\">";
            js += "     <div class=\"siteButton-inner\"><a id=\"myDeleteButton\" class=\"siteButton\" href=\"javascript:checkConfirm(DeleteAssets)\">Delete Selected</a>";
            js += "     </div></div></div></div></div></div></div></div></div></div>');";
            //js += "     <div id=\"ctl02_div\" class=\"siteButton\" style=\"margin-top: 10px; margin-left: 10px; text-align: center; width: 15%\"  name=\"ctl02_div\">";
            //js += "     <div class=\"siteButton-t\"><div class=\"siteButton-b\"><div class=\"siteButton-l\"><div class=\"siteButton-r\">";
            //js += "     <div class=\"siteButton-tl\"><div class=\"siteButton-tr\"><div class=\"siteButton-bl\"><div class=\"siteButton-br\">";
            //js += "     <div class=\"siteButton-inner\"><a id=\"myHideButton\" class=\"siteButton\" href=\"javascript:checkConfirm(HideAssets)\">Hide Selected</a>";
            //js += "     </div></div></div></div></div></div></div></div></div></div>');";
            js += "     $('.assetOptions input:checkbox').click( function(e) {";
            js += "         checkIfChecked();});";
            js += "     checkIfChecked();";
            js += "}";
            js += "function checkConfirm(del){";
            js += "     if(confirm('Are You Sure?')){";
            js += "         del();";
            js += "     }";
            js += "}";
            js += "function DeleteAssets(){";
            js += "         var loc = document.location.toString();";
            //js += "         alert(myloc);";
            //js += "         alert(myloc[0]);";
            js += "         loc += '?';loc += 'del,';";
            js += "         var boxes =('.aBox:checked');";
            js += "         $(boxes).each(function(i){loc+=this.value;loc+=',';});";
            js += "         loc += ' ';";
            js += "         document.location=loc;}";
            js += "function HideAssets(){";
            js += "         var loc = document.location.toString();";
            //js += "         alert(myloc);";
            //js += "         alert(myloc[0]);";
            js += "         loc += '?';loc += 'hide,';";
            js += "         var boxes =('.aBox:checked');";
            js += "         $(boxes).each(function(i){loc+=this.value;loc+=',';});";
            js += "         loc += ' ';";
            js += "         document.location=loc;}";
            js += "function checkIfChecked(){";
            js += "     var boxes =$('.aBox:checked');";
            js += "     if(boxes.length <= 0){";
            js += "         $('#myDeleteButton').addClass('disabledForButtons');";
            js += "         $('#myDeleteButton').bind('click', function(e){";
            js += "             e.preventDefault();";
            js += "         });";
            js += "         $('#myHideButton').addClass('disabledForButtons');";
            js += "         $('#myHideButton').bind('click', function(e){";
            js += "             e.preventDefault();";
            js += "         });";
            js += "     }else{";
            js += "         $('#myDeleteButton').removeClass('disabledForButtons');";
            js += "         $('#myDeleteButton').unbind('click');";
            js += "         $('#myHideButton').removeClass('disabledForButtons');";
            js += "         $('#myHideButton').unbind('click');";
            js += "}}";
            js += "</script>";
            return js;
        }
    }
}

