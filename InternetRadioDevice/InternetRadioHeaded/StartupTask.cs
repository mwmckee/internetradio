﻿using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetRadio
{
    class StartupTask
    {
        private static TelemetryClient s_telemetryClient;
        internal static RadioManager s_radioManager;
        private static HttpServer s_webServer;

        public static async void Start()
        {
            if (null == s_telemetryClient)
            {
                s_telemetryClient = new TelemetryClient();
            }

            if (null == s_radioManager)
            {
                s_radioManager = new RadioManager();
                await s_radioManager.Initialize();
            }

            if (null == s_webServer)
            {
                s_webServer = new HttpServer(8000);
                s_webServer.StartServer();
            }
        }

        public static async void stop()
        {
            await s_radioManager.Dispose();
            s_webServer.Dispose();
        }

        public static void WriteTelemetryEvent(string eventName)
        {
            WriteTelemetryEvent(eventName, new Dictionary<string, string>());
        }

        public static void WriteTelemetryEvent(string eventName, IDictionary<string, string> properties)
        {
            if (null == s_telemetryClient)
            {
                s_telemetryClient = new TelemetryClient();
            }

            s_telemetryClient.TrackEvent(eventName, properties);
        }

        public static void WriteTelemetryException(Exception e)
        {
            WriteTelemetryException(e, new Dictionary<string, string>());
        }

        public static void WriteTelemetryException(Exception e, IDictionary<string, string> properties)
        {
            if (null == s_telemetryClient)
            {
                s_telemetryClient = new TelemetryClient();
            }

            s_telemetryClient.TrackException(e, properties);
        }
    }
}