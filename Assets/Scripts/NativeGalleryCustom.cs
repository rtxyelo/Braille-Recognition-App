using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;

public class NativeGalleryCustom : MonoBehaviour
{
    [SerializeField]
    private RawImage _rawImage;

    [SerializeField]
    private SpriteShapeController _spriteShapeController;

    [SerializeField]
    private Shader _shader;

    private Texture2D _imageTexture;

    private byte[] _imageBuffer;

    [HideInInspector]
    public UnityEvent<byte[]> ByteImage;
    public byte[] ImageBuffer { get { return _imageBuffer; } }

    [SerializeField]
    private StreamListener _streamListener;

    [SerializeField]
    private Texture2D _initTexture;

    [SerializeField]
    private TMP_Text _debug;

    private void Awake()
    {
        ByteImage = new UnityEvent<byte[]>();

        _spriteShapeController.spriteShape.fillTexture = _initTexture;
    }

    private void OnDestroy()
    {
        ByteImage.RemoveAllListeners();
    }

    public void SendBufferedImage()
    {
        bool bufferReady = ImageToBytesArr();

        if (bufferReady)
            ByteImage.Invoke(_imageBuffer);
            //_streamListener.OnActivityResult(_imageBuffer);
    }

    private bool ImageToBytesArr()
    {
        if (_imageTexture != null)
        {
            try
            {
                ClearBuffer();
                _imageBuffer = _imageTexture.EncodeToJPG();
                Debug.Log("Image buffer lenght " +  _imageBuffer.Length);
                _debug.text = "Image buffer lenght " + _imageBuffer.Length;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError("Error converting image to byte array: " + e.Message);
                if (_debug != null)
                    _debug.text = e.Message;
                return false;
            }
        }
        return false;
    }

    private void ClearTexture()
    {
        _imageTexture = null;
    }

    private void ClearBuffer()
    {
        _imageBuffer = null;
    }

    public void PickImage(int maxSize)
    {
        ClearTexture();

        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                _imageTexture = NativeGallery.LoadImageAtPath(path, maxSize, false);
                Debug.Log("Texture is readable " + _imageTexture.isReadable);
                if (_imageTexture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }

                if (_rawImage != null)
                {
                    _rawImage.color = new Color(255, 255, 255);
                    _rawImage.texture = _imageTexture;
                }

                _spriteShapeController.spriteShape.fillTexture = _imageTexture;
            }
        });

        Debug.Log("Permission result: " + permission);
        _debug.text = "Permission result: " + permission;
    }
}
