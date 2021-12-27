using System;
using System.IO;
using System.Text;

namespace WpfApp2
{

	public class XTEA
	{
		private uint[] IV = null;

		public string Encrypt(string data, string key, uint Rounds, uint[] iv = null)
		{
			var dataBytes = Encoding.Unicode.GetBytes(data);
			var keyBytes = Encoding.Unicode.GetBytes(key);
			if (iv != null)
			{
				IV = new uint[iv.Length];
				IV[0] = iv[0];
				IV[1] = iv[1];
			}
			else
				IV = null;

			var keyBuffer = CreateKey(keyBytes);
			var blockBuffer = new uint[2];
			var result = new byte[NextMultipleOf8(dataBytes.Length + 4)];
			var lengthBuffer = BitConverter.GetBytes(dataBytes.Length);
			Array.Copy(lengthBuffer, result, lengthBuffer.Length);
			Array.Copy(dataBytes, 0, result, lengthBuffer.Length, dataBytes.Length);
			using (var stream = new MemoryStream(result))
			{
				using (var writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < result.Length; i += 8)
					{
						blockBuffer[0] = BitConverter.ToUInt32(result, i);
						blockBuffer[1] = BitConverter.ToUInt32(result, i + 4);
						Encrypt(Rounds, blockBuffer, keyBuffer);
						writer.Write(blockBuffer[0]);
						writer.Write(blockBuffer[1]);
					}
				}
			}

			return Convert.ToBase64String(result);
		}

		public string Decrypt(string data, string key, uint Rounds, uint[] iv = null)
		{
			var dataBytes = Convert.FromBase64String(data);
			var keyBytes = Encoding.Unicode.GetBytes(key);

			if (iv != null)
			{
				IV = new uint[iv.Length];
				IV[0] = iv[0];
				IV[1] = iv[1];
			}
			else
				IV = null;

			if (dataBytes.Length % 8 != 0) throw new ArgumentException("Encrypted data length must be a multiple of 8 bytes.");
			var keyBuffer = CreateKey(keyBytes);
			var blockBuffer = new uint[2];
			var buffer = new byte[dataBytes.Length];
			Array.Copy(dataBytes, buffer, dataBytes.Length);
			using (var stream = new MemoryStream(buffer))
			{
				using (var writer = new BinaryWriter(stream))
				{
					for (int i = 0; i < buffer.Length; i += 8)
					{
						blockBuffer[0] = BitConverter.ToUInt32(buffer, i);
						blockBuffer[1] = BitConverter.ToUInt32(buffer, i + 4);
						Decrypt(Rounds, blockBuffer, keyBuffer);
						writer.Write(blockBuffer[0]);
						writer.Write(blockBuffer[1]);
					}
				}
			}
			var length = BitConverter.ToUInt32(buffer, 0);
			if (length > buffer.Length - 4) throw new ArgumentException("Invalid encrypted data");
			var result = new byte[length];
			Array.Copy(buffer, 4, result, 0, length);
			return Encoding.Unicode.GetString(result);
		}

		private int NextMultipleOf8(int length)
		{
			return (length + 7) / 8 * 8;
		}

		private uint[] CreateKey(byte[] key)
		{
			var hash = new byte[16];
			for (int i = 0; i < key.Length; i++)
			{
				hash[i % 16] = (byte)((31 * hash[i % 16]) ^ key[i]);
			}
			for (int i = key.Length; i < hash.Length; i++)
			{
				hash[i] = (byte)(17 * i ^ key[i % key.Length]);
			}
			return new[] {
				BitConverter.ToUInt32(hash, 0), BitConverter.ToUInt32(hash, 4),
				BitConverter.ToUInt32(hash, 8), BitConverter.ToUInt32(hash, 12)
			};
		}

		private void Encrypt(uint rounds, uint[] v, uint[] key)
		{
			uint v0 = v[0], v1 = v[1], sum = 0, delta = 0x9E3779B9;

			if(IV != null)
            {
				v0 ^= IV[0];
				v1 ^= IV[1];
            }

			for (uint i = 0; i < rounds; i++)
			{
				v0 += (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
				sum += delta;
				v1 += (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
			}

			v[0] = v0;
			v[1] = v1;

			if (IV != null)
			{
				IV[0] = v0;
				IV[1] = v1;
			}
		}

		private void Decrypt(uint rounds, uint[] v, uint[] key)
		{
			uint v0 = v[0], v1 = v[1], delta = 0x9E3779B9, sum = delta * rounds;

			

			for (uint i = 0; i < rounds; i++)
			{
				v1 -= (((v0 << 4) ^ (v0 >> 5)) + v0) ^ (sum + key[(sum >> 11) & 3]);
				sum -= delta;
				v0 -= (((v1 << 4) ^ (v1 >> 5)) + v1) ^ (sum + key[sum & 3]);
			}

			if (IV != null)
			{
				v0 ^= IV[0];
				v1 ^= IV[1];

				IV[0] = v[0];
				IV[1] = v[1];
			}

			v[0] = v0;
			v[1] = v1;
		}
	}
}
