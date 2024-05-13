using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using TMPro;
using System.Runtime.InteropServices.ComTypes;
using System.Net;
using UnityEngine.Events;
using System.Text;
using System.Runtime.InteropServices;

public class StreamListener : MonoBehaviour
{
    [SerializeField]
    private NativeGalleryCustom _nativeGalleryCustom;

    [SerializeField]
    private GameObject _photoSelectPanel;

    [SerializeField]
    private GameObject _photoResultPanel;

    private Socket _socket;

    //private Task<string> _textResponse;

    //private Task<byte[]> _voiceResponse;

    [HideInInspector]
    public UnityEvent<string> TextReceivedEvent;

    [HideInInspector]
    public UnityEvent<byte[]> VoiceReceivedEvent;

    [SerializeField]
    private TMP_Text _debug1;

    [SerializeField]
    private TMP_Text _debug2;

    [SerializeField]
    private Answer _answer;

    private void Awake()
    {
        TextReceivedEvent = new UnityEvent<string>();
        VoiceReceivedEvent = new UnityEvent<byte[]>();
    }

    private void Start()
    {
        _nativeGalleryCustom.ByteImage.AddListener(OnActivityResult);
        
        _socket = FindObjectOfType<Socket>();
    }

    private void OnDestroy()
    {
        TextReceivedEvent.RemoveAllListeners();
        VoiceReceivedEvent.RemoveAllListeners();
    }

    // Метод, вызываемый при получении результата выбора фотографии
    public void OnActivityResult(byte[] data)
    {
        try
        {
            _socket.CloseAndDisposeSocket();
            _socket.ConnectSocket();

            _photoSelectPanel.SetActive(false);
            _photoResultPanel.SetActive(true);

            //SendPhotoAsync(data);
            SendPhoto(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling activity result: " + e.Message);
        }
    }
    #region Non Async Methods
    private void ReadText()
    {
        try
        {
            // Чтение текстовых данных
            byte[] buffer = new byte[1024];
            int bytesRead = _socket.TextReader.Read(buffer, 0, buffer.Length);
            string response = System.Text.Encoding.Default.GetString(buffer, 0, buffer.Length);

            //byte[] textBuffer = new byte[1024];
            //textBuffer = _socket.Reader.ReadBytes(textBuffer.Length);
            //string response = System.Text.Encoding.Default.GetString(textBuffer, 0, textBuffer.Length);

            TextReceivedEvent?.Invoke(response);
            //_answer.SetRecievedText(response);

            response = null;
        }
        catch (Exception e)
        {
            Debug.LogError("Error recieve text: " + e.Message);
        }
    }

    private IEnumerator ReadVoiceCoroutine()
    {
        byte[] sizeBuffer = new byte[4];
        int fileSize = 0;

        // Ожидание получения размера аудио файла
        while (fileSize == 0)
        {
            sizeBuffer = _socket.VoiceSizeReader.ReadBytes(sizeBuffer.Length);
            fileSize = BitConverter.ToInt32(sizeBuffer, 0);
            //Debug.Log("Filesize " + fileSize);

            // Отправка ответа в случае, если размер файла не был получен
            if (fileSize <= 0)
            {
                byte[] byteAnswer = BitConverter.GetBytes(0);
                _socket.Writer.Write(byteAnswer);
                sizeBuffer = new byte[4];
            }
            else
            {
                byte[] byteAnswer = BitConverter.GetBytes(1);
                _socket.Writer.Write(byteAnswer);
                break;
            }

            yield return new WaitForSeconds(0.01f);
        }

        // Получение аудио данных
        byte[] audioData = new byte[fileSize];
        int bytesRead = 0;
        while (bytesRead < fileSize)
        {
            int bytesReceived = _socket.VoiceReader.Read(audioData, bytesRead, fileSize - bytesRead);
            bytesRead += bytesReceived;
            yield return null; // Подождать один кадр перед следующей итерацией
        }

        Debug.Log("Voice size " + fileSize);
        if (_debug2 != null)
            _debug2.text = "\nVoice size " + fileSize.ToString();

        // Отправка полученных аудио данных событию
        VoiceReceivedEvent.Invoke(audioData);

        // Освобождение ресурсов
        audioData = null;
        sizeBuffer = null;
    }

    private void ReadVoice()
    {
        try
        {
            // Принятие размера аудио файла
            //byte[] sizeBuffer = new byte[4];
            //int read_buf = _socket.Reader.Read(sizeBuffer, 0, sizeBuffer.Length);

            //string bytes_arra_str = BitConverter.ToString(sizeBuffer);
            //Debug.Log("Bytes array " + bytes_arra_str);

            //int fileSize = BitConverter.ToInt32(sizeBuffer, 0);

            //-----------------------------------------------

            //byte[] sizeBuffer = new byte[4];
            //sizeBuffer = _socket.VoiceSizeReader.ReadBytes(sizeBuffer.Length);

            //string bytes_arra_str = BitConverter.ToString(sizeBuffer);
            //Debug.Log("Bytes array " + bytes_arra_str);
            //if (_debug2 != null)    
                //_debug2.text = "Bytes array " + bytes_arra_str;

            //int fileSize = BitConverter.ToInt32(sizeBuffer, 0);

            //-----------------------------------------------
            
            byte[] sizeBuffer = new byte[4];
            int fileSize = 0;
            while (fileSize == 0)
            {
                sizeBuffer = _socket.VoiceSizeReader.ReadBytes(sizeBuffer.Length);
                fileSize = BitConverter.ToInt32(sizeBuffer, 0);
                if (fileSize == 0)
                {
                    byte[] byteAnswer = BitConverter.GetBytes(0);
                    _socket.Writer.Write(byteAnswer);
                    sizeBuffer = new byte[4];
                }
                else
                {
                    byte[] byteAnswer = BitConverter.GetBytes(1);
                    _socket.Writer.Write(byteAnswer);
                    break;
                }
            }


            Debug.Log("Voice size " + fileSize);
            _debug2.text += "\nVoice size " + fileSize.ToString();

            // Принятие аудио данных
            byte[] audioData = new byte[fileSize];
            int bytesRead = 0;
            while (bytesRead < fileSize)
            {
                int bytesReceived = _socket.VoiceReader.Read(audioData, bytesRead, fileSize - bytesRead);
                bytesRead += bytesReceived;
            }

            VoiceReceivedEvent.Invoke(audioData);
            //_answer.GetAudioClipByByte(audioData);

            audioData = null;
            sizeBuffer = null;
        }
        catch (Exception e)
        {
            Debug.LogError("Error recieve voice: " + e.Message);
            //_debug1.text += e.Message;
        }
    }

    private void SendPhoto(byte[] photoData)
    {
        try
        {
            // Отправляем фотографию на сервер асинхронно
            _socket.Writer.Write(photoData.Length);
            _socket.Writer.Write(photoData, 0, photoData.Length);

            ReadText();
            //ReadVoice();
            StartCoroutine(ReadVoiceCoroutine());

            //_ = ReadTextAsync();
            //_ = ReadVoiceAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending photo: " + e.Message);
            //if (_debug1 != null)
                //_debug1.text = e.Message;
        }
    }
    #endregion

    #region Async Methods
    private async Task<string> ReadTextAsync()
    {
        try
        {
            // Чтение текстовых данных
            byte[] buffer = new byte[1024];
            int bytesRead = await _socket.Stream.ReadAsync(buffer, 0, buffer.Length);
            string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);

            TextReceivedEvent?.Invoke(response);

            return response;
        }
        catch (Exception e)
        {
            Debug.LogError("Error recieve text: " + e.Message);
            return null;
        }
    }

    private async Task<byte[]> ReadVoiceAsync()
    {
        try
        {
            // Чтение аудио данных
            byte[] audioBuffer = new byte[10000000];
            int audioBytesRead = await _socket.Stream.ReadAsync(audioBuffer, 0, audioBuffer.Length);
            byte[] audioBytes = new byte[audioBytesRead];
            Array.Copy(audioBuffer, audioBytes, audioBytesRead);

            // Декодирование Base64
            byte[] decodedAudioData = Convert.FromBase64String(System.Text.Encoding.UTF8.GetString(audioBytes));

            Debug.Log("Decoded Audio Data Length " + decodedAudioData.Length);

            VoiceReceivedEvent?.Invoke(decodedAudioData);

            return decodedAudioData;

        }
        catch (Exception e)
        {
            Debug.LogError("Error recieve voice: " + e.Message);
            return null;
        }
    }

    private async void SendPhotoAsync(byte[] photoData)
    {
        try
        {
            // Отправляем фотографию на сервер асинхронно
            await _socket.Stream.WriteAsync(photoData, 0, photoData.Length);

            //_ = ReadTextAsync();
            //_ = ReadVoiceAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending photo: " + e.Message);
            if (_debug1 != null)
                _debug1.text = e.Message;
        }
    }
    #endregion
}
