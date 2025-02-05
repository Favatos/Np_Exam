using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace TcpLib;

public static class TcpClientExstensions
{
    public static async Task SendInt32Async(this TcpClient tcpClient, int @int)
    {
        await tcpClient.GetStream().WriteAsync(BitConverter.GetBytes(@int));
    }
    public static async Task SendBytesAsync(this TcpClient tcpClient, byte[] bytes)
    {
         await tcpClient.SendInt32Async(bytes.Length);
        await tcpClient.GetStream().WriteAsync(bytes);
    }
    public static async Task<int> ReceiveInt32Async(this TcpClient tcpClient)
    {
        byte[] buffer = new byte[sizeof(int)];
        await tcpClient.GetStream().ReadExactlyAsync(buffer);
        return BitConverter.ToInt32(buffer);
    }
    public static async Task<byte[]> ReceiveBytesAsync(this TcpClient tcpClient)
    {
        byte[] buffer = new byte[(await tcpClient.ReceiveInt32Async())];
        await tcpClient.GetStream().ReadExactlyAsync(buffer);
        return buffer;
    }

    public static async Task SendInt64Async(this TcpClient tcpClient, long @long)
    {
        await tcpClient.GetStream().WriteAsync(BitConverter.GetBytes(@long));
    }
    public static async Task<long> ReceiveInt64Async(this TcpClient tcpClient)
    {
        byte[] buffer = new byte[sizeof(long)];
        await tcpClient.GetStream().ReadExactlyAsync(buffer);
        return BitConverter.ToInt64(buffer);
    }

    public static async Task SendStringAsync(this TcpClient tcpClient, string @string)
    {
        await tcpClient.SendBytesAsync(Encoding.UTF8.GetBytes(@string));
    }
    public static async Task<string> ReceiveStringAsync(this TcpClient tcpClient)
    {
        return Encoding.UTF8.GetString(await tcpClient.ReceiveBytesAsync());
    }

    public static async Task SendJsonAsync<T>(this TcpClient tcpClient, T t)
    {
        await tcpClient.SendStringAsync(JsonSerializer.Serialize(t));
    }
    public static async Task<T> ReceiveJsonAsync<T>(this TcpClient tcpClient)
    {
        string json = await tcpClient.ReceiveStringAsync();
        return JsonSerializer.Deserialize<T>(json) ?? throw new NullReferenceException();
    }

    public static async Task SendFileAsync(this TcpClient tcpClient, Stream stream)
    {
        await tcpClient.SendInt64Async(stream.Length);
        await stream.CopyToAsync(tcpClient.GetStream());
    }
    public static async Task ReceiveFileAsync(this TcpClient tcpClient, Stream stream)
    {
        long length = await tcpClient.ReceiveInt64Async();
        byte[] buffer = new byte[1024];
        for (long i = 0; i < length; )
        {
            int read = await tcpClient.GetStream().ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, length - i));
            await stream.WriteAsync(buffer, 0, read);
            i += read;
        }
    }

    public static async Task SendBinaryAsync(this TcpClient tcpClient, Action<BinaryWriter> write)
    {
        MemoryStream memory = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memory);
        write(writer);

        await tcpClient.SendBytesAsync(memory.ToArray());
    }
    public static async Task<BinaryReader> ReceiveBinaryAsync(this TcpClient tcpClient)
    {
        MemoryStream memory = new(await tcpClient.ReceiveBytesAsync());
       return new BinaryReader(memory);
    }
}
