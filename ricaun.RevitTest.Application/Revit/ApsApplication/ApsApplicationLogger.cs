﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ricaun.RevitTest.Application.Revit.ApsApplication
{
    public static class ApsApplicationLogger
    {
        //private static string requestUri = "https://webfastminimal.azurewebsites.net/logger";
        //private static string requestUri = "https://localhost:7059/logger";
        private static string requestUri = "https://ricaun-aps-application.web.app/api/v1/aps/Logger/{0}";

        public static async Task<ApsResponse> Log(string type, string message, int appCount = 1)
        {
            ApsResponse result = null;
            Debug.WriteLine($"Log[{ApsApplication.IsConnected}]: {type} {message}");
            if (ApsApplication.IsConnected)
            {
                try
                {
                    var apsLog = ApsLogUtils.New(type, message, appCount);

                    var service = await ApsApplication.ApsService.ApsClient.GetRequestServiceAsync();

                    //var client = ApsApplication.ApsService.GetHttpClient();
                    //var service = new RequestService(client);
                    result = await service.PostAsync<ApsResponse>(string.Format(requestUri, type), apsLog);
                    service.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            return result;
        }

        //public static async Task Get()
        //{
        //    if (ApsApplication.IsConnected)
        //    {
        //        var str = await ApsApplication.ApsService.ApsClient.Get<string>(requestUri);
        //        System.Console.WriteLine(str);
        //    }
        //}
    }

    public static class ApsLogUtils
    {
        public static ApsLog New(string type, string message, int appCount = 1)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var revitApiAssembly = typeof(Autodesk.Revit.ApplicationServices.Application).Assembly;
            var apsLog = new ApsLog()
            {
                appId = assembly.GetName().Name.Split(new[] { ".Dev." }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(),
                appVersion = assembly.GetName().Version.ToString(3),
                appCount = appCount,
                productId = "Autodesk.Revit",
                productVersion = revitApiAssembly.GetName().Version.ToString(),
                productLanguage = System.Globalization.CultureInfo.CurrentUICulture.Name,
                userName = Environment.UserName,
                userMachine = Environment.MachineName,
                type = type,
                message = message
            };
            return apsLog;
        }
    }

    public class ApsLog
    {
        public string appId { get; set; }
        public string appVersion { get; set; }
        public int appCount { get; set; }
        public string productId { get; set; }
        public string productVersion { get; set; }
        public string productLanguage { get; set; }
        public string userName { get; set; }
        public string userMachine { get; set; }
        public string type { get; set; }
        public string message { get; set; }
    }

    public class ApsResponse
    {
        public string userId { get; set; }
        public string appId { get; set; }
        public bool isValid { get; set; }
        public string message { get; set; }

        public OtherResponse[] Other { get; set; }
        public class OtherResponse
        {
            public string Url { get; set; }
            public string Text { get; set; }
        }
        public override string ToString()
        {
            return $"[{userId}] {appId} {isValid} {message}";
        }
    }
}