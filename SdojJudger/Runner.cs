﻿using System.Net;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using SdojJudger.Models;

namespace SdojJudger
{
    public class Runner
    {
        public async Task Run()
        {
            var authCookie = await AuthenticateUser(AppSettings.UserName, AppSettings.Password);
            _log = LogManager.GetLogger(typeof (Runner));
            
            if (authCookie == null)
            {
                _log.FatalFormat("error: login failed for user {0}", AppSettings.UserName);
            }
            else
            {
                _hub = new HubConnection(AppSettings.ServerUrl) { CookieContainer = new CookieContainer() };
                _hub.CookieContainer.Add(authCookie);
                _proxy = _hub.CreateHubProxy(AppSettings.HubName);
                _proxy.On<SolutionPushModel>(AppSettings.HubJudge, OnClientJudge);
                await _hub.Start();

                _log.InfoFormat("{0} online.", AppSettings.UserName);

                var client = GetClient();
                var all = await client.GetAll();
                foreach (var clientSolutionPushModel in all)
                {
                    await OnClientJudgeAsync(clientSolutionPushModel);
                }
            }
        }

        public async Task Restart()
        {
            _hub.Dispose();
            _log.Info("Connection stoped.");
            await Run();
        }

        public HubClient GetClient()
        {
            return new HubClient(_proxy);
        }

        // Details

        private async Task OnClientJudgeAsync(SolutionPushModel model)
        {
            _log.Debug(JsonConvert.SerializeObject(model));
            var ps = new JudgeProcess(model);
            await ps.ExecuteAsync();
        }

        private void OnClientJudge(SolutionPushModel model)
        {
            _log.Debug(JsonConvert.SerializeObject(model));
            var ps = new JudgeProcess(model);
            var task = ps.ExecuteAsync();
        }

        private async Task<Cookie> AuthenticateUser(string user, string password)
        {
            var request = WebRequest.CreateHttp(AppSettings.LoginUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();

            var authCredentials = string.Format("username={0}&password={1}",
                WebUtility.UrlEncode(user),
                WebUtility.UrlEncode(password));
            var bytes = Encoding.UTF8.GetBytes(authCredentials);
            request.ContentLength = bytes.Length;

            using (var requestStream = await request.GetRequestStreamAsync())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            using (var response = await request.GetResponseAsync() as HttpWebResponse)
            {
// ReSharper disable once PossibleNullReferenceException
                return response.Cookies[AppSettings.CookieName];
            }
        }

        // Fields & Properties.
        private IHubProxy _proxy;

        private HubConnection _hub;

        private ILog _log;
    }
}
