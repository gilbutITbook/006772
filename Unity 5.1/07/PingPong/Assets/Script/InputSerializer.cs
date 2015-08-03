using UnityEngine;
using System.Collections;




public class MouseSerializer : Serializer
{

	public bool Serialize(MouseData packet)
	{
		// 각 요소를 차례로 시리얼라이즈합니다.
		bool ret = true;
		ret &= Serialize(packet.frame);	
		ret &= Serialize(packet.mouseButtonLeft);
		ret &= Serialize(packet.mouseButtonRight);
		ret &= Serialize(packet.mousePositionX);
		ret &= Serialize(packet.mousePositionY);
		ret &= Serialize(packet.mousePositionZ);
		
		return ret;
	}

	public bool Deserialize(byte[] data, ref MouseData serialized)
	{
		// 데이터의 요소별로 디시리얼라이즈합니다.
		// 디시리얼라이즈할 데이터를 설정합니다.
		bool ret = SetDeserializedData(data);
		if (ret == false) {
			return false;
		}
		
		// 데이터의 요소별로 디시리얼라이즈합니다.
		ret &= Deserialize(ref serialized.frame);
		ret &= Deserialize(ref serialized.mouseButtonLeft);
		ret &= Deserialize(ref serialized.mouseButtonRight);
		ret &= Deserialize(ref serialized.mousePositionX);
		ret &= Deserialize(ref serialized.mousePositionY);
		ret &= Deserialize(ref serialized.mousePositionZ);
		
		return ret;
	}
}



public class InputSerializer : Serializer
{
	public bool Serialize(InputData data)
	{
		// 기존 데이터를 클리어합니다.
		Clear();
		
		// 각 요소를 차례로 시리얼라이즈합니다.
		bool ret = true;
		ret &= Serialize(data.count);	
		ret &= Serialize(data.flag);

		MouseSerializer mouse = new MouseSerializer();
		
		for (int i = 0; i < data.datum.Length; ++i) {
			mouse.Clear();
			bool ans = mouse.Serialize(data.datum[i]);
			if (ans == false) {
				return false;
			}
			
			byte[] buffer = mouse.GetSerializedData();
			ret &= Serialize(buffer, buffer.Length);
		}
		
		return ret;
	}

	public bool Deserialize(byte[] data, ref InputData serialized)
	{
		// 디시리얼라이즈할 데이터를 설정합니다.
		bool ret = SetDeserializedData(data);
		if (ret == false) {
			return false;
		}
		
		// 데이터 요소별로 디시리얼라이즈합니다.
		ret &= Deserialize(ref serialized.count);
		ret &= Deserialize(ref serialized.flag);

		// 디시리얼라이즈 후의 버퍼 크기를 구합니다.
		MouseSerializer mouse = new MouseSerializer();
		MouseData md = new MouseData();
		mouse.Serialize(md);
		byte[] buf= mouse.GetSerializedData();
		int size = buf.Length;
		
		serialized.datum = new MouseData[serialized.count];
		for (int i = 0; i < serialized.count; ++i) {
			serialized.datum[i] = new MouseData();
		}
		
		for (int i = 0; i < serialized.count; ++i) {
			byte[] buffer = new byte[size];
			
			// mouseData의11프레임분의 데이터를 추출합니다.
			bool ans = Deserialize(ref buffer, size);
			if (ans == false) {
				return false;
			}

			ret &= mouse.Deserialize(buffer, ref md);
			if (ret == false) {
				return false;
			}
			
			serialized.datum[i] = md;
		}
		
		return ret;
	}
}
