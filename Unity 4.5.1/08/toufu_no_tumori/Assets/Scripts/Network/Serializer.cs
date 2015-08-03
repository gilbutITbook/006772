
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

public class Serializer
{
	
	private MemoryStream 	m_buffer = null;
	
	private int				m_offset = 0;

	
	private Endianness		m_endianness;
	

	// 엔디언.
	public enum Endianness
	{
		BigEndian = 0,		// 빅 엔디언.
	    LittleEndian,		// 리틀 엔디언.
	}
	
	public Serializer()
	{
		// 시리얼라이즈용 버퍼를 만듭니다.
		m_buffer = new MemoryStream();

		// 엔디언을 판정합니다.
		int val = 1;
		byte[] conv = BitConverter.GetBytes(val);
		m_endianness = (conv[0] == 1)? Endianness.LittleEndian : Endianness.BigEndian;
	}
	
	
	public byte[] GetSerializedData()
	{	
		return m_buffer.ToArray();	
	}

	
	
	public void Clear()
	{
		byte[] buffer = m_buffer.GetBuffer();
		Array.Clear(buffer, 0, buffer.Length);
		
		m_buffer.Position = 0;
		m_buffer.SetLength(0);
		m_offset = 0;
	}

	//
	// 디시리얼라이즈할 데이터를 버퍼에 설정합니다.
	//
	public bool SetDeserializedData(byte[] data)
	{
		// 설정할 버퍼를 클리어합니다.
		Clear();

		try {
			// 디시리얼라이즈할 데이터를 설정합니다.
			m_buffer.Write(data, 0, data.Length);
		}
		catch {
			return false;
		}
		
		return 	true;
	}

	
	//
	// bool형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(bool element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(bool));
	}
	
	//
	// char형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(char element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(char));
	}
	
	//
	// float형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(float element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(float));
	}
	
	//
	// double형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(double element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(double));
	}	
		
	//
	// short형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(short element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(short));
	}	
	
	//
	// ushort형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(ushort element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(ushort));
	}		
	
	//
	// int형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(int element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(int));
	}	
	
	//
	// uint형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(uint element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(uint));
	}		
	
	//
	// long형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(long element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(long));
	}	
	
	//
	// ulong형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(ulong element)
	{
		byte[] data = BitConverter.GetBytes(element);
		
		return WriteBuffer(data, sizeof(ulong));
	}
	
	//
	// byte[]형 데이터를 시리얼라이즈 합니다.
	//
	protected bool Serialize(byte[] element, int length)
	{
		// byte열은 데이터 덩어리로서 설정하므로 엔디언 변환을 하지 않기 위해
		// 여기서 원래대로 돌아가게 합니다.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(element);	
		}

		return WriteBuffer(element, length);
	}

	//
	// string형 데이터를 시리얼라이즈합니다.
	//
	protected bool Serialize(string element, int length)
	{
		byte[] data = new byte[length];

		byte[] buffer = System.Text.Encoding.UTF8.GetBytes(element);
		int size = Math.Min(buffer.Length, data.Length);
		Buffer.BlockCopy(buffer, 0, data, 0, size);

		// byte열은 데이터의 덩어리로서 설정하므로 엔디언 변환을 하지 않기 위해
		// 여기서 원래대로 돌아가게 합니다.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(data);	
		}

		return WriteBuffer(data, data.Length);
	}
	
	//
	// 데이터를 bool형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref bool element)
	{
		int size = sizeof(bool);
		byte[] data = new byte[size];

		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToBoolean(data, 0);
			return true;
		}
		
		return false;
	}
	
	//
	// 데이터를 char형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref char element)
	{
		int size = sizeof(char);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToChar(data, 0);
			return true;
		}
		
		return false;
	}
	
	
	//
	// 데이터를 float형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref float element)
	{
		int size = sizeof(float);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToSingle(data, 0);
			return true;
		}
		
		return false;
	}
	
	//
	// 데이터를 double형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref double element)
	{
		int size = sizeof(double);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToDouble(data, 0);
			return true;
		}
		
		return false;
	}	
	
	//
	// 데이터를 short형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref short element)
	{
		int size = sizeof(short);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			//읽어낸 값을 변환합니다.
			element = BitConverter.ToInt16(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// 데이터를 ushort형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref ushort element)
	{
		int size = sizeof(ushort);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToUInt16(data, 0);
			return true;
		}
		
		return false;
	}
	
	//
	// 데이터를 int형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref int element)
	{
		int size = sizeof(int);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToInt32(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// 데이터를 uint형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref uint element)
	{
		int size = sizeof(uint);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToUInt32(data, 0);
			return true;
		}
		
		return false;
	}
		
	//
	// 데이터를 long형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref long element)
	{
		int size = sizeof(long);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToInt64(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// 데이터를 ulong형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref ulong element)
	{
		int size = sizeof(ulong);
		byte[] data = new byte[size];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
			// 읽어낸 값을 변환합니다.
			element = BitConverter.ToUInt64(data, 0);
			return true;
		}
		
		return false;
	}

	//
	// byte[]형 데이터로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref byte[] element, int length)
	{

		// 
		bool ret = ReadBuffer(ref element, length);

		// byte열은 데이터 덩어리로서 저장되므로 엔디언 변환을 하지 않기 위해서
		// 버퍼를 여기서 원래로 되돌립니다.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(element);	
		}

		if (ret == true) {
			return true;
		}
		
		return false;
	}

	//
	// string형으로 디시리얼라이즈합니다.
	//
	protected bool Deserialize(ref string element, int length)
	{
		byte[] data = new byte[length];
		
		// 
		bool ret = ReadBuffer(ref data, data.Length);
		if (ret == true) {
		// byte열은 데이터 덩어리로서 저장되므로 엔디언 변환을 하지 않기 위해서
		// 버퍼를 여기서 원래로 되돌립니다.
			if (m_endianness == Endianness.LittleEndian) {
				Array.Reverse(data);	
			}
			string str = System.Text.Encoding.UTF8.GetString(data);
			element = str.Trim('\0');

			return true;
		}
		
		return false;
	}
	
	protected bool ReadBuffer(ref byte[] data, int size)
	{
		// 현재 오프셋에서부터 데이터를 읽어냅니다.
		try {
			m_buffer.Position = m_offset;
			m_buffer.Read(data, 0, size);
			m_offset += size;
		}
		catch {
			return false;
		}
	
		// 읽어낸 값을 호스트 바이트 오더로 변환합니다.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(data);	
		}	
		
		return true;
	}
	
	protected bool WriteBuffer(byte[] data, int size)
	{
		// 써넣을 값을 네트워크 바이트 오더로 변환합니다.
		if (m_endianness == Endianness.LittleEndian) {
			Array.Reverse(data);	
		}
	
		// 현재 오프셋에서부터 데이터를 써넣습니다.
		try {
			m_buffer.Position = m_offset;		
			m_buffer.Write(data, 0, size);	
			m_offset += size;
		}
		catch {
			return false;
		}
		
		return true;
	}
	
	public Endianness GetEndianness()
	{
		return m_endianness;	
	}
		
	public long GetDataSize()
	{
		return m_buffer.Length;	
	}
}

