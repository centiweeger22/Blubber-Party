using System.Collections.Generic;

public class MiddleInfo
{
	public Channel channel;

	public float idleTime;

	public List<ClientInfo> clients;

	public MiddleInfo()
	{
		clients = new List<ClientInfo>();
	}

	public ClientInfo GetClientByTicket(int ticket, out int index)
	{
		int count = clients.Count;
		for (int i = 0; i < count; i++)
		{
			ClientInfo clientInfo = clients[i];
			if (clientInfo.ticket == ticket)
			{
				index = i;
				return clientInfo;
			}
		}
		index = -1;
		return null;
	}

	public ClientInfo GetClientByProxyId(int proxyId, out int index)
	{
		int count = clients.Count;
		for (int i = 0; i < count; i++)
		{
			ClientInfo clientInfo = clients[i];
			if (clientInfo.proxy.id == (uint)proxyId)
			{
				index = i;
				return clientInfo;
			}
		}
		index = -1;
		return null;
	}

	public int DestroyClientByTicket(int ticket)
	{
		int count = clients.Count;
		for (int i = 0; i < count; i++)
		{
			ClientInfo clientInfo = clients[i];
			if (clientInfo.ticket == ticket)
			{
				clients.RemoveAt(i);
				return (int)clientInfo.proxy.id;
			}
		}
		return -1;
	}
}
