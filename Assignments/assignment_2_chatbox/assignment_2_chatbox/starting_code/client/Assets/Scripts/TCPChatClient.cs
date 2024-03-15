using shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/**
 * Assignment 2 - Starting project.
 * 
 * @author J.C. Wichman
 */
public class TCPChatClient : MonoBehaviour
{
    [SerializeField] private PanelWrapper _panelWrapper = null;
    [SerializeField] private string _hostname = "localhost";
    [SerializeField] private int _port = 55555;

    private TcpClient _client;
    private NetworkStream _stream;

    void Start()
    {
        _panelWrapper.OnChatTextEntered += onTextEntered;
        connectToServer();

        _stream = _client.GetStream();
        
    }

    private void connectToServer()
    {
        try
        {
			_client = new TcpClient();
            _client.Connect(_hostname, _port);
            _panelWrapper.ClearOutput();
            _panelWrapper.AddOutput("Connected to server.");
        }
        catch (Exception e)
        {
            _panelWrapper.AddOutput("Could not connect to server:");
            _panelWrapper.AddOutput(e.Message);
        }
    }

    private void Update()
    {
	    try
	    {
		    if(_stream.DataAvailable)
				readStream();
	    }
	    catch (Exception e)
	    {
		    Console.WriteLine(e);
	    }
    }

    private void onTextEntered(string pInput)
    {
        if (string.IsNullOrEmpty(pInput)) 
	        return;

        _panelWrapper.ClearInput();

		try 
        {
			//echo client - send one, expect one (hint: that is not how a chat works ...)
			byte[] outBytes = Encoding.UTF8.GetBytes(pInput);
			StreamUtil.Write(_client.GetStream(), outBytes);

			byte[] inBytes = StreamUtil.Read(_client.GetStream());
            string inString = Encoding.UTF8.GetString(inBytes);
            _panelWrapper.AddOutput(inString);
		} 
        catch (Exception e) 
        {
            _panelWrapper.AddOutput(e.Message);
			//for quicker testing, we reconnect if something goes wrong.
			_client.Close();
			connectToServer();
		}
    }

    private void readStream()
    {
	    byte[] receivedData = StreamUtil.Read(_stream);
	    string textRepresentation = System.Text.Encoding.UTF8.GetString(receivedData, 0, receivedData.Length);
	    _panelWrapper.AddOutput(textRepresentation);
    }

}

