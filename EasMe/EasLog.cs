﻿
using EasMe.Exceptions;
using EasMe.Extensions;
using EasMe.InternalUtils;
using EasMe.Models.LogModels;
using log4net;
using System.Reflection;
using System.Runtime.CompilerServices;
namespace EasMe
{

    /*
     Things to keep note;
    1. This class is not thread safe.
    2. This logger will work on multiple threads
    3. For .NET Web projects it is better to initialize HttpContext in the constructor. 
        This will print out the request information. 
        Which contains request url etc. so you dont really need to put method name in log
     */

    //private static readonly EasLog logger = IEasLog.CreateLogger(typeof(AdminManager).Namespace + "." + typeof(AdminManager).Name);
    /// <summary>
    /// Simple static json and console logger with useful options.
    /// </summary>
    public sealed class EasLog
    {
        //internal static EasLogConfiguration Configuration { get; set; } = IEasLog.Config;
        private static int? _OverSizeExt = 0;

        internal static bool _IsCreated = false;
        internal string _LogSource;
        private static readonly object _fileLock = new object();
        internal EasLog(string source)
        {
            _LogSource = source;
        }
        internal EasLog()
        {
            _LogSource = "Sys";
        }
        private static string GetExactLogPath()
        {
            return Path.Combine(EasLogFactory.Config.LogFolderPath, EasLogFactory.Config.LogFileName + DateTime.Now.ToString(EasLogFactory.Config.DateFormatString) + EasLogFactory.Config.LogFileExtension);
        }
        private static string GetExactLogPath(Severity severity)
        {
            if (EasLogFactory.Config.SeperateLogLevelToFolder)
            {
				return Path.Combine(EasLogFactory.Config.LogFolderPath, severity.ToString(), EasLogFactory.Config.LogFileName + DateTime.Now.ToString(EasLogFactory.Config.DateFormatString) + EasLogFactory.Config.LogFileExtension);
				
            }
			return Path.Combine(EasLogFactory.Config.LogFolderPath, EasLogFactory.Config.LogFileName + DateTime.Now.ToString(EasLogFactory.Config.DateFormatString) + EasLogFactory.Config.LogFileExtension);
        }
        public void Log(Severity severity, params object[] param)
        {
            WriteLog(severity, null, param);
        }
 
        public void Info(params object[] param)
        {
            WriteLog(Severity.INFO, null, param);
        }

        public void Error(params object[] param)
        {
            WriteLog(Severity.ERROR, null, param);
        }

        public void Warn(params object[] param)
        {
            WriteLog(Severity.WARN, null, param);
        }

        public void Exception(Exception ex)
        {
            WriteLog(Severity.EXCEPTION, ex);
        }

        public void Exception(Exception ex, params object[] param)
        {
            WriteLog(Severity.EXCEPTION, ex, param);
        }

        public void Fatal(params object[] param)
        {
            WriteLog(Severity.FATAL, null, param);
        }
        public void Fatal(Exception ex, params object[] param)
        {
            WriteLog(Severity.FATAL, ex, param);

        }

        public void Debug(params object[] param)
        {
            WriteLog(Severity.DEBUG, null, param);
        }
        public void Debug(Exception ex, params object[] param)
        {
            WriteLog(Severity.DEBUG, ex, param);
        }

        public void Trace(params object[] param)
        {
            WriteLog(Severity.TRACE, null, param);
        }
        private void WriteLog(Severity severity, Exception? exception = null, params object[] param)
        {
            Task.Run(() =>
            {
                if (EasLogFactory.Config.DontLog) return;
                if (!severity.IsLoggable()) return;
                var log = "";
                var paramToLog = param.ToLogString();
                if (EasLogFactory.Config.IsLogJson)
                {
                    var model = EasLogHelper.LogModelCreate(severity, _LogSource, paramToLog, exception);
                    log = model.JsonSerialize();
                }
                else
                {
                    var dateStr = DateTime.Now.ToString(EasLogFactory.Config.DateFormatString);
                    log = $"[{dateStr}] [{severity}] " + paramToLog;
                    if (exception != null) log = log + " Exception:" + exception.Message;
                }
                try
                {
                    if (EasLogFactory.Config.ConsoleAppender) EasLogConsole.Log(severity, log);
                    if (EasLogFactory.Config.StackLogCount > 0)
                    {
                        if (EasLogFactory.Config.StackLogCount >= _stackLogs.Count)
                        {
                            WriteLines(_stackLogs);
                            _stackLogs.Clear();
                        }
                        else
                        {
                            lock (_stackLogs)
                            {
                                _stackLogs.Add(log);
                            }
                        }
                    }
                    else
                    {
                        WriteToFile(severity, log);
                    }
                }
                catch (Exception ex)
                {
                    lock (Errors)
                    {
                        Errors.Add(ex);
                    }
                }
            });
            void WriteToFile(Severity severity, string log)
            {
                var logFilePath = GetExactLogPath(severity);
                var folderPath = Path.GetDirectoryName(logFilePath);
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
                lock (_fileLock)
                {
                    File.AppendAllText(logFilePath, log + "\n");
                }
            }
        }

        private void WriteLines(List<string> logs)
        {
            if (!Directory.Exists(EasLogFactory.Config.LogFolderPath)) Directory.CreateDirectory(EasLogFactory.Config.LogFolderPath);
            lock (_fileLock)
            {
                File.WriteAllLines(GetExactLogPath(), logs);
            }
        }

        private static List<string> _stackLogs = new();
        public static List<Exception> Errors { get; set; } = new();

        /// <summary>
        /// This method must be called before application exit. If 
        /// </summary>
        public void Flush()
        {
            if (EasLogFactory.Config.StackLogCount > 0)
                WriteLines(_stackLogs);
        }

    }

}

