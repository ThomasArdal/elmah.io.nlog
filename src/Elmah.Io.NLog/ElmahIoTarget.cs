﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elmah.Io.Client;
using Elmah.Io.Client.Models;
using NLog;
using NLog.Config;
using NLog.LayoutRenderers;
using NLog.Layouts;
using NLog.Targets;

namespace Elmah.Io.NLog
{
    [Target("elmah.io")]
    public class ElmahIoTarget : TargetWithLayout
    {
        private IElmahioAPI _client;

        [RequiredParameter]
        public string ApiKey { get; set; }

        [RequiredParameter]
        public Guid LogId { get; set; }

        public string Application { get; set; }

        public ElmahIoTarget()
        {
        }

        public ElmahIoTarget(IElmahioAPI client)
        {
            _client = client;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (_client == null)
            {
                _client = ElmahioAPI.Create(ApiKey);
            }

            var title = Layout != null && Layout.ToString() != "'${longdate}|${level:uppercase=true}|${logger}|${message}'"
                ? Layout.Render(logEvent)
                : logEvent.FormattedMessage;

            var message = new CreateMessage
            {
                Title = title,
                Severity = LevelToSeverity(logEvent.Level),
                DateTime = logEvent.TimeStamp.ToUniversalTime(),
                Detail = logEvent.Exception?.ToString(),
                Data = PropertiesToData(logEvent.Properties),
                Source = logEvent.LoggerName,
                User = Identity(logEvent),
                Hostname = MachineName(logEvent),
                Application = Application,
                Url = AspNetRequestUrl(logEvent),
            };

            _client.Messages.CreateAndNotify(LogId, message);
        }
        
        /// <summary>
        /// Try to look up current url. If we are not in an HTTP context, this will just return null;
        /// </summary>
        private string AspNetRequestUrl(LogEventInfo logEvent)
        {
            Layout renderer = "${aspnet-request-url:IncludeQueryString=true}";
            var url = renderer.Render(logEvent);
            return string.IsNullOrWhiteSpace(url) ? null : url;
        }

        /// <summary>
        /// Try to look up the current user identity. If we are not in an HTTP context, this will just return null;
        /// </summary>
        private string Identity(LogEventInfo logEvent)
        {
            Layout aspNetRenderer = "${aspnet-user-identity}";
            var user = aspNetRenderer.Render(logEvent);
            return user;
        }

        private string MachineName(LogEventInfo logEvent)
        {
            return new MachineNameLayoutRenderer().Render(logEvent);
        }

        private List<Item> PropertiesToData(IDictionary<object, object> properties)
        {
            return properties.Keys.Select(key => new Item{Key = key.ToString(), Value = properties[key].ToString()}).ToList();
        }

        private string LevelToSeverity(LogLevel level)
        {
            if (level == LogLevel.Debug) return Severity.Debug.ToString();
            if (level == LogLevel.Error) return Severity.Error.ToString();
            if (level == LogLevel.Fatal) return Severity.Fatal.ToString();
            if (level == LogLevel.Trace) return Severity.Verbose.ToString();
            if (level == LogLevel.Warn) return Severity.Warning.ToString();
            return Severity.Information.ToString();
        }
    }
}