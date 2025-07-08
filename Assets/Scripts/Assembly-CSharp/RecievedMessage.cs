using System;
using System.Net;

public class RecievedMessage
{
	public byte[] buffer;

	public int bufferLength;

	public Settings.EMessageType type;

	public EndPoint origin;

	public Channel owner;

	public RecievedMessage(byte[] _buffer, int _bufferLength, Settings.EMessageType _type, EndPoint _origin = null, Channel _owner = null)
	{
		buffer = new byte[_bufferLength];
		Buffer.BlockCopy(_buffer, 0, buffer, 0, _bufferLength);
		bufferLength = _bufferLength;
		type = _type;
		origin = _origin;
		owner = _owner;
	}
}
