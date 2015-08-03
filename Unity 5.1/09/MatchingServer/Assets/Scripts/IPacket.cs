using System.Collections;
using System.IO; 


public interface IPacket<T>
{
	// 
	PacketId 	GetPacketId();

	//	
	T 			GetPacket();

	//
	byte[] 		GetData();
}
