using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Socket : MonoBehaviour
{
    private TcpClient _tcpClient;

    private System.Net.Sockets.Socket _socket;

    private NetworkStream _stream;

    private BinaryReader _textReader;

    private BinaryReader _voiceReader;

    private BinaryReader _voiceSizeReader;

    private BinaryWriter _writer;

    [SerializeField]
    private TMP_InputField _serverIP;

    [SerializeField]
    private TMP_InputField _serverPort;

    [SerializeField]
    private GameObject _errorFrame;

    [SerializeField]
    private GameObject _connectionFrame;

    [SerializeField]
    private Button _connectionButton;

    public static Socket Instance;

    public NetworkStream Stream { get { return _stream; } }

    public TcpClient TcpClient { get { return _tcpClient; } }

    public BinaryReader TextReader { get { return _textReader; } }
    public BinaryReader VoiceReader { get { return _voiceReader; } }
    public BinaryReader VoiceSizeReader { get { return _voiceSizeReader; } }

    public BinaryWriter Writer { get { return _writer; } }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void ConnectButtonStatus(bool buttonActive)
    {
        if (_connectionButton != null)
        {
            if (buttonActive)
                _connectionButton.interactable = true;
            else
                _connectionButton.interactable = false;
        }
    }   

    public void ConnectSocket()
    {
        ConnectButtonStatus(false);

        var hostName = _serverIP.text;

        int port = 0;

        if (!string.IsNullOrEmpty(_serverPort.text))
            port = int.Parse(_serverPort.text);

        Debug.Log("HOST: " +  hostName + "\t PORT: " +  port);

        try
        {
            System.Text.Encoding encoding = System.Text.Encoding.Default;
            bool leaveOpen = true;


            //_tcpClient = new TcpClient();
            //_tcpClient.Connect(hostName, port);
            //_stream = _tcpClient.GetStream();

            //_textReader = new BinaryReader(_stream, encoding, leaveOpen);
            //_voiceReader = new BinaryReader(_stream, encoding, leaveOpen);
            //_voiceSizeReader = new BinaryReader(_stream, encoding, leaveOpen);
            //_writer = new BinaryWriter(_stream, encoding, leaveOpen);

            //------------------------------------------------------------

            _socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                Blocking = true
            };
            _socket.Connect(hostName, port);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 0);
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 0);
            _stream = new NetworkStream(_socket, FileAccess.ReadWrite, true);
            _textReader = new BinaryReader(_stream, encoding, leaveOpen);
            _voiceReader = new BinaryReader(_stream, encoding, leaveOpen);
            _voiceSizeReader = new BinaryReader(_stream, encoding, leaveOpen);
            _writer = new BinaryWriter(_stream, encoding, leaveOpen);

            ConnectButtonStatus(true);

            if (_connectionFrame != null)
                _connectionFrame.SetActive(true);

            Debug.Log("Connected!");
        }
        catch (Exception e)
        {
            if (_errorFrame != null)
                _errorFrame.SetActive(true);

            ConnectButtonStatus(true);

            Debug.LogError("Error connect to socket: " + e.Message);
            throw;
        }
    }

    public void CloseAndDisposeSocket()
    {
        // Очищаем соединение
        _writer.Dispose();
        _textReader.Dispose();
        _voiceReader.Dispose();
        _voiceSizeReader.Dispose();
        _stream.Dispose();

        _socket.Dispose();
        //_tcpClient.Dispose();

        // Закрываем соединение
        _writer.Close();
        _textReader.Close();
        _voiceReader.Close();
        _voiceSizeReader.Close();
        _stream.Close();

        _socket.Close();
        //_tcpClient.Close();
    }

    private void OnDestroy()
    {
        CloseAndDisposeSocket();
    }
}
