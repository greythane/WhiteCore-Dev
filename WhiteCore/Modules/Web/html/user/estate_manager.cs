﻿/*
 * Copyright (c) Contributors, http://whitecore-sim.org/
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
using WhiteCore.Framework.DatabaseInterfaces;
using WhiteCore.Framework.Servers.HttpServer.Implementation;

namespace WhiteCore.Modules.Web
{
    public class UserEstateManagerPage : IWebInterfacePage
    {
        public string [] FilePath {
            get {
                return new []
                           {
                               "html/user/estate_manager.html"
                           };
            }
        }

        public bool RequiresAuthentication {
            get { return true; }
        }

        public bool RequiresAdminAuthentication {
            get { return false; }
        }

        public Dictionary<string, object> Fill (WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                               OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                               ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object> ();
            var estateListVars = new List<Dictionary<string, object>> ();
            var estateConnector = Framework.Utilities.DataManager.RequestPlugin<IEstateConnector> ();
            var user = Authenticator.GetAuthentication (httpRequest);

            var estates = estateConnector.GetEstates (user.PrincipalID);

            if (estates.Count > 0) {
                vars.Add("HaveData", true);
                vars.Add("NoData", false);

                foreach (var estate in estates) {
                    var regions = estateConnector.GetRegions ((int)estate.EstateID);

                    estateListVars.Add (new Dictionary<string, object> {
                        {"EstateID", estate.EstateID.ToString()},
                        {"EstateName", estate.EstateName},
                        {"PublicAccess", WebHelpers.YesNo(translator, estate.PublicAccess)},
                        {"AllowVoice", WebHelpers.YesNo(translator, estate.AllowVoice)},
                        {"TaxFree", WebHelpers.YesNo(translator, estate.TaxFree)},
                        {"AllowDirectTeleport", WebHelpers.YesNo (translator, estate.AllowDirectTeleport)},
                        {"RegionCount", regions.Count.ToString()}
                    });
                }
            } else {
                vars.Add("HaveData", false);
                vars.Add("NoData", true);
                /*
                estateListVars.Add (new Dictionary<string, object> {
                    {"EstateName", translator.GetTranslatedString("NoDetailsText")},
                    {"PublicAccess", ""},
                        {"AllowVoice", ""},
                        {"TaxFree", ""},
                        {"AllowDirectTeleport", ""},
                        {"RegionCount", ""}
                    });
                */
            }

            vars.Add ("EstateList", estateListVars);

            // labels
            vars.Add ("UserName", user.Name);
            vars.Add ("AddEstateText", translator.GetTranslatedString ("AddEstateText"));
            vars.Add ("EditText", translator.GetTranslatedString ("EditText"));
            vars.Add ("EstateListText", translator.GetTranslatedString ("EstatesText"));
            vars.Add ("EstateText", translator.GetTranslatedString ("EstateText"));
            vars.Add ("PublicAccessText", translator.GetTranslatedString ("PublicAccessText"));
            vars.Add ("AllowVoiceText", translator.GetTranslatedString ("AllowVoiceText"));
            vars.Add ("TaxFreeText", translator.GetTranslatedString ("TaxFreeText"));
            vars.Add ("AllowDirectTeleportText", translator.GetTranslatedString ("AllowDirectTeleportText"));
            vars.Add ("RegionsText", translator.GetTranslatedString ("MenuRegionsTitle"));
            return vars;
        }

        public bool AttemptFindPage (string filename, ref OSHttpResponse httpResponse, out string text)
        {
            text = "";
            return false;
        }
    }

}
