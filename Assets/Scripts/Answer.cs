using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Answer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _text;

    [SerializeField]
    private AudioSource _audioSource;

    private StreamListener _streamListener;

    private AudioClip _audioClip;

    [SerializeField]
    private TMP_Text _debug;

    [SerializeField]
    private TMP_FontAsset _fontAsset;

    private void Start()
    {
        _streamListener = FindObjectOfType<StreamListener>();

        _streamListener.TextReceivedEvent.AddListener(SetRecievedText);
        _streamListener.VoiceReceivedEvent.AddListener(GetAudioClipByByte);
    }

    public void SetRecievedText(string text)
    {
        try
        {
            _text.text = text;
            _text.font = _fontAsset;
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to recieve text." + e.Message);
            //if (_debug != null)
                //_debug.text = e.Message;
        }
    }

    public void GetAudioClipByByte(byte[] buffer)
    {
        try
        {
            _audioClip = WavUtility.ToAudioClip(buffer);
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to create AudioClip from audio bytes." + e.Message);
            //if (_debug != null)
                //_debug.text = e.Message + "  1";
        }
    }

    public void PlayAudioFromBytes()
    {
        try
        {
            _audioSource.clip = _audioClip;
            _audioSource.Play();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Failed to create AudioClip from audio bytes." + e.Message);
            //if (_debug != null)
                //_debug.text = e.Message + "  2";
        }
    }

    public void ClearResults()
    {
        if (_text != null)
            _text.text = "Text..";
        _text.gameObject.SetActive(false);
        _audioSource.clip = null;
    }
}