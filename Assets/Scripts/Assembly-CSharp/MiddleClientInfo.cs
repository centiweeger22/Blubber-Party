public class MiddleClientInfo
{
	public int ticket = -1;

	public int proxyId = -1;

	public Channel channel;

	public Preferences preferences;

	public PlayerEntity proxy;

	public float idleTime;

	public MiddleClientInfo()
	{
		preferences = new Preferences();
	}
}
