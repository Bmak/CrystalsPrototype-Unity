using System;
using System.Security.Cryptography;
using System.Text;

public class HashUtil
{
	public static string GetMd5Sum(string str)
	{
		byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes((str));
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] result = md5.ComputeHash(toEncodeAsBytes);

		StringBuilder sb = new StringBuilder();
		for (int i=0;i<result.Length;i++)
		{
			sb.Append(result[i].ToString("X2"));
		}

		return sb.ToString();
	}

}

