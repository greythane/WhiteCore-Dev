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
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Servers.HttpServer.Implementation;

namespace WhiteCore.Modules.Web
{
    public class SimConsolePage : IWebInterfacePage
    {
        public string[] FilePath
        {
            get
            {
                return new[]
                           {
                               "html/admin/sim_console.html"
                           };
            }
        }

        public bool RequiresAuthentication
        {
            get { return true; }
        }

        public bool RequiresAdminAuthentication
        {
            get { return true; }
        }

        public Dictionary<string, object> Fill(WebInterface webInterface, string filename, OSHttpRequest httpRequest,
                                                OSHttpResponse httpResponse, Dictionary<string, object> requestParameters,
                                                ITranslator translator, out string response)
        {
            response = null;
            var vars = new Dictionary<string, object>();
            //IGenericsConnector connector = Framework.Utilities.DataManager.RequestPlugin<IGenericsConnector>();

            // Check if we're looking at the standard page or the submitted one
            if (requestParameters.ContainsKey("Submit"))
            {
                var command = "";
                if (httpRequest.Query.ContainsKey ("command")) {
                    command = httpRequest.Query ["command"].ToString ();
                    response = "Command in query";
                 } else {
                    if (requestParameters.ContainsKey ("command")) {
                        command = requestParameters ["command"].ToString ();
                        response = webInterface.UserMsg("Command in parameters", false);
                    } else {
                        response = webInterface.UserMsg("!Please enter a valid console command", false);
                    }
                }

                if (command != "") {
                    // RunCommandScript(command);
                    MainConsole.Instance.RunCommand(command);

                    response = webInterface.UserMsg("The sim console is not yet implemented<br/>Command: " + command, false);
                }
                return null;

            }
            else
            {
                //vars.Add("ErrorMessage", error);

                vars.Add("SimConsoleText", translator.GetTranslatedString("SimConsoleText"));
                vars.Add("SimAddressText", translator.GetTranslatedString("SimAddressText"));
                vars.Add("UserNameText", translator.GetTranslatedString("UserNameText"));
                vars.Add("PasswordText", translator.GetTranslatedString("PasswordText"));
                vars.Add("SendCommandText", translator.GetTranslatedString("SendCommandText"));

                vars.Add("Login", translator.GetTranslatedString("Login"));

            }
            return vars;
        }

        public bool AttemptFindPage(string filename, ref OSHttpResponse httpResponse, out string text)
        {
            text = "";
            return false;
        }
    }
}
