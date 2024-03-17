﻿using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace NsisoLauncherCore.Util.Checker
{
    [Obsolete("use sha256 instead")]
    public class MD5Checker : IChecker
    {
        public string CheckSum { get; set; }
        public string FilePath { get; set; }

        public bool CheckFilePass()
        {
            if (string.IsNullOrWhiteSpace(CheckSum))
            {
                throw new ArgumentException("检验器缺少校验值");
            }

            return string.Equals(CheckSum, GetFileChecksum(), StringComparison.OrdinalIgnoreCase);
        }

        public Task<bool> CheckFilePassAsync()
        {
            return Task.Run(() => CheckFilePass());
        }

        public string GetFileChecksum()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                throw new ArgumentException("检验器校验目标文件路径为空");
            }
            using (FileStream file = new FileStream(FilePath, FileMode.Open))
            {
                using (MD5 md5 = new MD5CryptoServiceProvider())//创建SHA1对象
                {
                    byte[] md5Bytes = md5.ComputeHash(file);//Hash运算
                    string result = BitConverter.ToString(md5Bytes);//将运算结果转为string类型
                    result = result.Replace("-", "");
                    return result;
                }
            }
        }
    }
}
