﻿/*
 * Copyright (c) Contributors, http://whitecore-sim.org/, http://aurora-sim.org
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the WhiteCore-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenMetaverse;
using WhiteCore.Framework.DatabaseInterfaces;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.Servers.HttpServer.Implementation;
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Services.ClassHelpers.Profile;

namespace WhiteCore.Modules.Web
{
    public class AgentGroupsPage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                           {
                               "html/webprofile/modal_groups.html",
                               "html/webprofile/groups.html"
                           };
            }
        }

        public bool RequiresAuthentication
        {
            get { return false; }
        }

        public bool RequiresAdminAuthentication
        {
            get { return false; }
        }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                               OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                               ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object>();

            string username = filename.Split('/').LastOrDefault();
            UserAccount userAcct = new UserAccount();
            var accountservice = webInterface.Registry.RequestModuleInterface<IUserAccountService>();

            if (accountservice == null) {
                return vars;
            }

            if (httpRequest.Query.ContainsKey("userid"))
            {
                string userid = httpRequest.Query["userid"].ToString();
                userAcct = accountservice.GetUserAccount(null, UUID.Parse(userid));
            }
            else if (httpRequest.Query.ContainsKey("name") || username.Contains('.'))
            {
                string name = httpRequest.Query.ContainsKey("name") ? httpRequest.Query["name"].ToString() : username;
                name = name.Replace('.', ' ');
                userAcct = accountservice.GetUserAccount(null, name);
            }
            else
            {
                username = username.Replace("%20", " ");
                userAcct = accountservice.GetUserAccount(null, username);
            }

            if (!userAcct.Valid)
                return vars;

            // User found...
            vars.Add("UserName", userAcct.Name);
            vars.Add("UserID", userAcct.PrincipalID);

            IUserProfileInfo profile = Framework.Utilities.DataManager.RequestPlugin<IProfileConnector>().
                                              GetUserProfile(userAcct.PrincipalID);
            IWebHttpTextureService webhttpService =
                webInterface.Registry.RequestModuleInterface<IWebHttpTextureService>();

            if (profile != null)
            {
                vars.Add("UserType", profile.MembershipGroup == "" ? "Resident" : profile.MembershipGroup);
                if (profile.Partner != UUID.Zero)
                {
                    var partnerAcct = accountservice.GetUserAccount(null, profile.Partner);
                    vars.Add("UserPartner", partnerAcct.Name);
                }
                else
                    vars.Add("UserPartner", "No partner");
                
                vars.Add("UserAboutMe", profile.AboutText == "" ? "Nothing here" : profile.AboutText);
                string url = "../static/icons/no_avatar.jpg";
                if (webhttpService != null && profile.Image != UUID.Zero)
                    url = webhttpService.GetTextureURL(profile.Image);
                vars.Add("UserPictureURL", url);
            } else
            {
                // no profile yet
                vars.Add ("UserType", "Guest");
                vars.Add ("UserPartner", "Not specified yet");
                vars.Add ("UserAboutMe", "Nothing here yet");
                vars.Add("UserPictureURL", "../static/icons/no_avatar.jpg");

            }

            vars.Add("UsersGroupsText", translator.GetTranslatedString("UsersGroupsText"));

            IGroupsServiceConnector groupsConnector =
                Framework.Utilities.DataManager.RequestPlugin<IGroupsServiceConnector>();
            List<Dictionary<string, object>> groups = new List<Dictionary<string, object>> ();

            if (groupsConnector != null)
            {
                foreach (var grp in groupsConnector.GetAgentGroupMemberships(userAcct.PrincipalID, userAcct.PrincipalID))
                {
                    var grpData = groupsConnector.GetGroupProfile (userAcct.PrincipalID, grp.GroupID);
                    string url = "../static/icons/no_groups.jpg";
                    if (webhttpService != null && grpData.InsigniaID != UUID.Zero)
                        url = webhttpService.GetTextureURL (grpData.InsigniaID);
                    groups.Add (new Dictionary<string, object> {
                        { "GroupPictureURL", url },
                        { "GroupName", grp.GroupName }
                    });

                }

                if (groups.Count == 0)
                {
                    groups.Add (new Dictionary<string, object> {
                        { "GroupPictureURL", "../static/icons/no_groups.jpg" },
                        { "GroupName", "None yet" }
                    });

                }

            }

            // Menus
            vars.Add("MenuProfileTitle", translator.GetTranslatedString("MenuProfileTitle"));
            vars.Add("TooltipsMenuProfile", translator.GetTranslatedString("TooltipsMenuProfile"));
            //vars.Add("MenuGroupTitle", translator.GetTranslatedString("MenuGroupTitle"));
            //vars.Add("TooltipsMenuGroups", translator.GetTranslatedString("TooltipsMenuGroups"));
            vars.Add("MenuPicksTitle", translator.GetTranslatedString("MenuPicksTitle"));
            vars.Add("TooltipsMenuPicks", translator.GetTranslatedString("TooltipsMenuPicks"));
            vars.Add("MenuRegionsTitle", translator.GetTranslatedString("MenuRegionsTitle"));
            vars.Add("TooltipsMenuRegions", translator.GetTranslatedString("TooltipsMenuRegions"));

            vars.Add("GroupNameText", translator.GetTranslatedString("GroupNameText"));
            vars.Add ("Groups", groups);
            vars.Add ("GroupsJoined", groups.Count-1);

            return vars;
        }

        public bool AttemptFindPage(string filename, ref OSHttpResponse httpResponse, out string text)
        {
            httpResponse.ContentType = "text/html";
            text = File.ReadAllText("html/webprofile/index.html");
            return true;
        }
    }
}