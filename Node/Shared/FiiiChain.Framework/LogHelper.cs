﻿// Copyright (c) 2018 FiiiLab Technology Ltd
// Distributed under the MIT software license, see the accompanying
// file LICENSE or or http://www.opensource.org/licenses/mit-license.php.
using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FiiiChain.Framework
{
    public class LogHelper
    {
        static ILog log;
        static LogHelper()
        {
            ILoggerRepository repository = LogManager.CreateRepository("NETCoreRepository");
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
#if DEBUG
            log = LogManager.GetLogger(repository.Name, "ConsoleLog");
#else
            log = LogManager.GetLogger(repository.Name, "");
#endif
        }

        public static void Debug(string message)
        {
            log.Debug(message);
        }

        public static void Info(string message)
        {
            log.Info(message);
        }

        public static void Warn(string message)
        {
            log.Warn(message);
        }

        public static void Error(string message, Exception ex = null)
        {
            if(ex != null)
            {
                log.Error(message, ex);
            }
            else
            {
                log.Error(message);
            }
        }

        public static void Fatal(string message, Exception ex = null)
        {
            if (ex != null)
            {
                log.Fatal(message, ex);
            }
            else
            {
                log.Fatal(message);
            }
        }
    }
}
