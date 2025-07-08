using System;
using System.Net;
using UnityEngine;  // For WWW

public class IPOperation
{
    public static IPAddress resolvedIp;

    public static bool IsLocal(IPAddress address)
    {
        byte[] addressBytes = address.GetAddressBytes();
        byte b = addressBytes[0];
        byte b2 = addressBytes[1];
        if (b == 192 && b2 == 168)
        {
            return true;
        }
        switch (b)
        {
            case 10:
                return true;
            case 172:
                if (b2 >= 16 && b2 <= 31)
                {
                    return true;
                }
                break;
        }
        byte b3 = addressBytes[2];
        byte b4 = addressBytes[3];
        if (b == 127 && b2 == 0 && b3 == 0 && b4 >= 1 && b4 <= 8)
        {
            return true;
        }
        return false;
    }

    public static IPAddress GlobalIPAddress()
    {
        try
        {
            using (WWW www = new WWW("http://checkip.dyndns.org"))
            {
                while (!www.isDone) { } // Blocking call; risky but matches your sync style

                if (!string.IsNullOrEmpty(www.error))
                {
                    return IPAddress.Any;
                }

                string result = www.text;
                string text = "Current IP Address: ";
                int num = result.IndexOf(text);
                num += text.Length;
                int num2 = result.IndexOf("</body>", num);
                string ip = result.Substring(num, num2 - num);
                return IPAddress.Parse(ip);
            }
        }
        catch
        {
            return IPAddress.Any;
        }
    }

    public static IPAddress ResolveDomainName(string domain)
    {
        IPHostEntry hostEntry = Dns.GetHostEntry(domain);
        if (hostEntry.AddressList.Length != 0)
        {
            IPAddress iPAddress = hostEntry.AddressList[0];
            byte[] addressBytes = iPAddress.GetAddressBytes();
            if (addressBytes[0] + addressBytes[1] + addressBytes[2] + addressBytes[3] > 0)
            {
                resolvedIp = iPAddress;
            }
            return resolvedIp;
        }
        return resolvedIp;
    }
}
