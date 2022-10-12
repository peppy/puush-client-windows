﻿using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace osu_common.Helpers
{
    public static class CryptoHelper
    {
        private static MD5 md5Hasher = MD5.Create();
        private static UTF8Encoding utf8Encoding = new UTF8Encoding();
        private static NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;

        public static string GetMd5(String path)
        {
            try
            {
                using (Stream s = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    byte[] data = md5Hasher.ComputeHash(s);

                    StringBuilder sBuilder = new StringBuilder();

                    for (int i = 0; i < data.Length; i++)
                        sBuilder.Append(data[i].ToString("x2"));
                    
                    return sBuilder.ToString();
                }
            }
            catch (Exception e)
            {
                return "fail" + e;
            }
        }

        public static string GetMd5String(String instr)
        {
            return GetMd5String(utf8Encoding.GetBytes(instr));
        }

        public static string GetMd5String(byte[] data)
        {
            try
            {
                // Convert the input string to a byte array and compute the hash.
                data = md5Hasher.ComputeHash(data);
            }
            catch (Exception)
            {
                md5Hasher = MD5.Create();
                try
                {
                    data = md5Hasher.ComputeHash(data);
                }
                catch (Exception)
                {
                    return "fail";
                }
            }

            char[] str = new char[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
                data[i].ToString("x2", nfi).CopyTo(0, str, i * 2, 2);

            return new String(str);
        }

        public static byte[] GetMd5ByteArrayString(String instr)
        {
            byte[] data;

            try
            {
                // Convert the input string to a byte array and compute the hash.
                data = md5Hasher.ComputeHash(utf8Encoding.GetBytes(instr));
            }
            catch (Exception)
            {
                md5Hasher = MD5.Create();
                try
                {
                    data = md5Hasher.ComputeHash(utf8Encoding.GetBytes(instr));
                }
                catch (Exception)
                {
                    return null;
                }
            }
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            //StringBuilder sBuilder = new StringBuilder();

            return data;

        }
    }
}