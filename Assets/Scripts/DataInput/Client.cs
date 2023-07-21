using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;

public class Client
{
    public static ConcurrentDictionary<int, Client> Clients = new ConcurrentDictionary<int, Client>();

    public delegate void OnReceiveHandler(string message);

    public event OnReceiveHandler OnReceive;

    private bool active;
    private TcpClient sock;
    private Thread readThread, writeThread;
    private readonly ConcurrentQueue<object> messageQueue = new ConcurrentQueue<object>();

    public Client(string addr, int port, OnReceiveHandler callback)
    {
        if (active) return;
        active = true;

        try
        {
            sock = new TcpClient(addr, port);
        }
        catch (Exception ex)
        {
            Debug.Log($"Failed connection {addr}:{port} because {ex}");
            return;
        }

        OnReceive = callback;

        var stream = sock.GetStream();
        readThread = new Thread(() => Listen(stream)) { IsBackground = true };
        writeThread = new Thread(() => Send(stream)) { IsBackground = true };
        readThread.Start();
        writeThread.Start();

        Clients[Clients.Count] = this;
        Debug.Log($"Started client @ {addr}:{port}");
    }

    public void Stop()
    {
        if (!active) return;
        active = false;
        readThread = null;
        writeThread = null;
        sock.Close();
        sock = null;
    }

    private void Listen(Stream stream)
    {
        var sizeBytes = new byte[4];

        while (active)
            try
            {
                var i = 0;
                while (i < sizeBytes.Length) i += stream.Read(sizeBytes, i, sizeBytes.Length - i);
                Array.Reverse(sizeBytes);
                var size = BitConverter.ToInt32(sizeBytes, 0);
                i = 0;
                var buffer = new byte[size];
                while (i < size) i += stream.Read(buffer, i, size - i);
                var message = Encoding.UTF8.GetString(buffer);
                OnReceive?.Invoke(message);
            }
            catch
            {
                break;
            }
    }

    public void SendMessage(in object message)
    {
        messageQueue.Enqueue(new
        {
            timestamp = $"{DateTime.UtcNow:O}",
            msg = message
        });
    }

    private void Send(Stream stream)
    {
        while (true)
        {
            while (messageQueue.Count == 0) Thread.Sleep(100);

            messageQueue.TryPeek(out var obj);
            var str = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(str);
            var len = BitConverter.GetBytes(bytes.Length);

            try
            {
                stream.Write(len, 0, len.Length);
                stream.Write(bytes, 0, bytes.Length);
            }
            catch
            {
                break;
            }

            messageQueue.TryDequeue(out _);
        }
    }
}