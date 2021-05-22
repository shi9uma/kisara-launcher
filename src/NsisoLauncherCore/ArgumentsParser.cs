﻿using NsisoLauncherCore.Modules;
using NsisoLauncherCore.Net.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NsisoLauncherCore
{
    public class ArgumentsParser
    {
        public event EventHandler<Log> ArgumentsParserLog;

        private LaunchHandler handler;

        public ArgumentsParser(LaunchHandler handler)
        {
            this.handler = handler;
        }

        public string Parse(Modules.LaunchSetting setting)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            #region JVM头参数部分
            StringBuilder jvmHead = new StringBuilder();

            #region 处理JavaAgent
            if (!string.IsNullOrWhiteSpace(setting.JavaAgent))
            {
                jvmHead.Append("-javaagent:");
                jvmHead.Append(setting.JavaAgent.Trim());
                jvmHead.Append(' ');
            }
            #endregion

            #region 处理JVM启动头参数  
            if (setting.GCEnabled)
            {
                switch (setting.GCType)
                {
                    case GCType.G1GC:
                        jvmHead.Append("-XX:+UseG1GC");
                        break;
                    case GCType.SerialGC:
                        jvmHead.Append("-XX:+UseSerialGC");
                        break;
                    case GCType.ParallelGC:
                        jvmHead.Append("-XX:+UseParallelGC");
                        break;
                    case GCType.CMSGC:
                        jvmHead.Append("-XX:+UseConcMarkSweepGC");
                        break;
                    case GCType.NULL:
                        break;
                    default:
                        break;
                }
                jvmHead.Append(' ');
            }
            if (!string.IsNullOrWhiteSpace(setting.GCArgument))
            {
                jvmHead.Append(setting.GCArgument).Append(' ');
            }
            if (setting.MinMemory != 0)
            {
                jvmHead.Append(string.Format("-Xms{0}m ", setting.MinMemory));
            }
            if (setting.MaxMemory != 0)
            {
                jvmHead.Append(string.Format("-Xmx{0}m ", setting.MaxMemory));
            }
            if (!string.IsNullOrWhiteSpace(setting.AdvencedJvmArguments))
            {
                jvmHead.Append(setting.AdvencedJvmArguments).Append(' ');
            }
            #endregion

            #region 处理游戏JVM参数
            Dictionary<string, string> jvmArgDic = new Dictionary<string, string>()
            {
                {"${natives_directory}",string.Format("\"{0}{1}\"",handler.GetGameVersionRootDir(setting.Version), @"\$natives") },
                {"${launcher_name}","NsisoLauncher5" },
                {"${launcher_version}", Assembly.GetExecutingAssembly().GetName().Version.ToString() },
                {"${classpath}", GetClassPaths(setting.Version.Libraries,setting.Version) },
            };

            jvmHead.Append(ReplaceByDic(setting.Version.JvmArguments, jvmArgDic)?.Trim());
            jvmHead.Append(' ');
            jvmHead.Append("-Dfml.ignoreInvalidMinecraftCertificates=true -Dfml.ignorePatchDiscrepancies=true ");
            #endregion

            #endregion

            #region 处理游戏参数
            string assetsPath = string.Format("\"{0}\\assets\"", handler.GameRootPath);
            //if (setting.LaunchUser.SelectedProfile.Legacy.HasValue)
            //{
            //    legacy = setting.LaunchUser.SelectedProfile.Legacy.Value ? "Legacy" : "Mojang";
            //}
            string gameDir = string.Format("\"{0}\"", handler.GetGameVersionRootDir(setting.Version));
            Dictionary<string, string> gameArgDic = new Dictionary<string, string>()
            {
                {"${auth_player_name}",string.Format("\"{0}\"", setting.LaunchUser.LaunchPlayerName) },
                {"${auth_session}",setting.LaunchUser.LaunchAccessToken },
                {"${version_name}",string.Format("\"{0}\"", setting.Version.Id) },
                {"${game_directory}",gameDir },
                {"${game_assets}",assetsPath },
                {"${assets_root}",assetsPath },
                {"${assets_index_name}",setting.Version.Assets },
                {"${auth_uuid}",setting.LaunchUser.LaunchUuid },
                {"${auth_access_token}",setting.LaunchUser.LaunchAccessToken },
                {"${user_properties}",ToList(setting.LaunchUser.Properties) },
                {"${user_type}",setting.LaunchUser.UserType },
                {"${version_type}", string.IsNullOrWhiteSpace(setting.VersionType) ? "NsisoLauncher5":string.Format("\"{0}\"",setting.VersionType) }
            };
            StringBuilder otherGamearg = new StringBuilder();
            if (setting.WindowSize != null)
            {
                if (setting.WindowSize.FullScreen)
                {
                    otherGamearg.Append(" --fullscreen");
                }
                else
                {
                    if (setting.WindowSize.Width > 0)
                    {
                        otherGamearg.AppendFormat(" --width {0}", setting.WindowSize.Width);
                    }
                    if (setting.WindowSize.Height > 0)
                    {
                        otherGamearg.AppendFormat(" --height {0}", setting.WindowSize.Height);
                    }
                }
            }
            if (setting.LaunchToServer != null)
            {
                ServerInfo server = new ServerInfo(setting.LaunchToServer);
                SendDebugLog("正在获取服务器信息(SRV)");
                server.StartGetServerInfo();
                if (server.State == ServerInfo.StateType.GOOD)
                {
                    SendDebugLog("服务器信息返回正常");
                    otherGamearg.AppendFormat(" --server {0}", server.ServerAddress);
                    otherGamearg.AppendFormat(" --port {0}", server.ServerPort);
                }
                else
                {
                    SendDebugLog("服务器未正常响应，尝试使用原始地址");
                    if (!string.IsNullOrWhiteSpace(setting.LaunchToServer.Address))
                    {
                        otherGamearg.AppendFormat(" --server {0}", setting.LaunchToServer.Address);
                    }
                    if (setting.LaunchToServer.Port > 0)
                    {
                        otherGamearg.AppendFormat(" --port {0}", setting.LaunchToServer.Port);
                    }
                }
            }
            if (!string.IsNullOrWhiteSpace(setting.AdvencedGameArguments))
            {
                otherGamearg.Append(' ' + setting.AdvencedGameArguments);
            }
            if (setting.GameProxy != null)
            {
                if (!string.IsNullOrWhiteSpace(setting.GameProxy.ProxyHost))
                {
                    otherGamearg.AppendFormat(" --proxyHost {0}", setting.GameProxy.ProxyHost);
                }
                if (setting.GameProxy.ProxyPort != 0)
                {
                    otherGamearg.AppendFormat(" --proxyPort {0}", setting.GameProxy.ProxyHost);
                }
                if (!string.IsNullOrWhiteSpace(setting.GameProxy.ProxyUsername))
                {
                    otherGamearg.AppendFormat(" --proxyUser {0}", setting.GameProxy.ProxyUsername);
                }
                if (!string.IsNullOrWhiteSpace(setting.GameProxy.ProxyPassword))
                {
                    otherGamearg.AppendFormat(" --proxyPass {0}", setting.GameProxy.ProxyPassword);
                }
            }
            string gameArg = (ReplaceByDic(setting.Version.MinecraftArguments, gameArgDic) + otherGamearg.ToString())?.Trim();

            #endregion

            stopwatch.Stop();

            string allArg = jvmHead.ToString() + setting.Version.MainClass + ' ' + gameArg;
            SendDebugLog(string.Format("完成启动参数转换,用时:{0}ms", stopwatch.ElapsedMilliseconds));
            SendDebugLog(string.Format("启动参数:{0}", allArg));

            return allArg.Trim();
        }

        private static string ToList(List<UserData.Property> properties)
        {
            if (properties == null)
            {
                return "{}";
            }
            var sb = new StringBuilder();
            foreach (var item in properties)
            {
                sb.AppendFormat("\"{0}\":[\"{1}\"],", item.Name, item.Value);
            }
            var totalSb = new StringBuilder();
            totalSb.Append('{').Append(sb.ToString().TrimEnd(',').Trim()).Append('}');
            return totalSb.ToString();
        }

        private string GetClassPaths(List<Modules.Library> libs, Modules.Version ver)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append('\"');

            foreach (var item in libs)
            {
                string libPath = handler.GetLibraryPath(item);
                stringBuilder.AppendFormat("{0};", libPath);
            }
            stringBuilder.Append(handler.GetJarPath(ver));
            stringBuilder.Append('\"');
            return stringBuilder.ToString().Trim();
        }

        private static string ReplaceByDic(string str, Dictionary<string, string> dic)
        {
            if (str == null)
            {
                return null;
            }
            return dic.Aggregate(str, (a, b) => a.Replace(b.Key, b.Value));
        }

        private void SendDebugLog(string str)
        {
            ArgumentsParserLog?.Invoke(this, new Log(LogLevel.DEBUG, str));
        }
    }
}