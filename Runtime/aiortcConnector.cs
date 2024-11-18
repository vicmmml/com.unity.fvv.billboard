using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;


public class aiortcConnector : MonoBehaviour
{
    [SerializeField] public RawImage receiveVideo;
    [SerializeField] public string m_ServerIp;
    [SerializeField] private string m_ServerPort;
    [SerializeField] private Camera Camera;
    [SerializeField] private AudioSource receivedAudio;
    [SerializeField] private Transform viewPlane;
    [SerializeField] private int radio_fvv = 2500; //mm
    //[SerializeField] private bool VR_version = false;


    private RTCDataChannel channel;
    private Texture prev_text;
    public UnityEvent OnConnect;
    public UnityEvent OnDisconnect;
    private Vector3 init_cam_pos = new Vector3();
    private Vector3 init_cam_rot = new Vector3();
    private bool first_track = true;



    private enum Side
    {
        Local,
        Remote
    }

    private class SignalingMsg
    {
        public string type;
        public string sdp;
        public RTCSessionDescription ToDesc()
        {
            return new RTCSessionDescription
            {
                type = type == "offer" ? RTCSdpType.Offer : RTCSdpType.Answer,
                sdp = sdp
            };
        }
    }

    public RTCPeerConnection pc;
    public MediaStream receiveVideoStream;
    private MediaStream receiveDepthStream;
    

    void Start()
    {
        init_cam_pos = Camera.transform.position;
        init_cam_rot = Camera.transform.localEulerAngles;
        // WebRTC.Initialize();
        StartCoroutine(WebRTC.Update());
        receivedAudio =  GetComponent<AudioSource>();
        
    }


    public void Connect()
    {
        // Peer Connection and data channel creation
        pc = new RTCPeerConnection();
        receiveVideoStream = new MediaStream();
        receiveDepthStream = new MediaStream();
        channel = pc.CreateDataChannel("cam_movement");
        prev_text = receiveVideo.texture;
        pc.AddTransceiver(TrackKind.Video);
        pc.AddTransceiver(TrackKind.Video); //Depth transceiver
        pc.AddTransceiver(TrackKind.Audio);

        //When the ice candidate is created, aiortc signaling starts
        pc.OnIceCandidate = cand =>
        {
            pc.OnIceCandidate = null;
            var msg = new SignalingMsg
            {
                type = pc.LocalDescription.type.ToString().ToLower(),
                sdp = pc.LocalDescription.sdp
            };

            StartCoroutine(aiortcSignaling(msg));
        };
        pc.OnIceGatheringStateChange = state =>
        {
            Debug.Log($"OnIceGatheringStateChange > state: {state}");
        };
        pc.OnConnectionStateChange = state =>
        {
            Debug.Log($"OnConnectionStateChange > state: {state}");
        };

        // When video/audio track is added to the MediaStream, if it's a video track, the video texture will be added to the
        // Billboard in the scene; if it's an audio track, it will be added to the audiosource of the scene and played

        receiveVideoStream.OnAddTrack = e => {
                Debug.Log("Added Track to MediaStream");


                if(e.Track is AudioStreamTrack audio)
                {
                    receivedAudio.SetTrack(audio);
                    receivedAudio.loop = true;
                    receivedAudio.Play();
                    Debug.Log("AudioStreamTrack received");
                } 

                else if (e.Track is VideoStreamTrack video)
                {
                    Debug.Log("VideoStreamTrack received");
                    video.OnVideoReceived += tex =>
                    {
                        receiveVideo.texture = tex;
                        Debug.Log("Video track received");                  
                    };
                }
                        
        };


        receiveDepthStream.OnAddTrack = e => {
            Debug.Log("Added Track to Video MediaStream");
            if (e.Track is VideoStreamTrack video){
                video.OnVideoReceived += tex =>
                {

                };
            }
        };

        // Adds tracks of video/audio to the MediaStream when they are received
        pc.OnTrack = e =>
        {
            Debug.Log($"OnTrack");

            //Debug comments
            if (e.Track.Kind == TrackKind.Video)
            {
                Debug.Log($"OnTrackVideo");
                if(first_track) 
                { 
                    receiveVideoStream.AddTrack(e.Track);
                    first_track = false;
                }
                else receiveDepthStream.AddTrack(e.Track);
            }
            else if (e.Track.Kind == TrackKind.Audio)
            {
                Debug.Log($"OnTrackAudio");
                receiveVideoStream.AddTrack(e.Track);
            }

        };

        Debug.Log("Debug Connect");
        OnConnect.Invoke();
        StartCoroutine(CreateDesc(RTCSdpType.Offer));
    }

    
    //Creates SDP Offer or Answer
    private IEnumerator CreateDesc(RTCSdpType type)
    {
        Debug.Log("Debug CreateDesc");
        var op = type == RTCSdpType.Offer ? pc.CreateOffer() : pc.CreateAnswer();
        yield return op;

        if (op.IsError)
        {
            Debug.LogError($"Create {type} Error: {op.Error.message}");
            yield break;
        }

        StartCoroutine(SetDesc(Side.Local, op.Desc));
    }


    private IEnumerator SetDesc(Side side, RTCSessionDescription desc)
    {
        var op = side == Side.Local ? pc.SetLocalDescription(ref desc) : pc.SetRemoteDescription(ref desc);
        yield return op;

        if (op.IsError)
        {
            Debug.Log($"Set {desc.type} Error: {op.Error.message}");
            yield break;
        }

        if (side == Side.Local)
        {
            // aiortc not support Tricle ICE.
        }
        else if (desc.type == RTCSdpType.Offer)
        {
            yield return StartCoroutine(CreateDesc(RTCSdpType.Answer));
        }
    }

    private IEnumerator aiortcSignaling(SignalingMsg msg)
    {
        string ServerURL = "http://" + m_ServerIp + ":" + m_ServerPort;
        var jsonStr = JsonUtility.ToJson(msg);
        using var req = new UnityWebRequest($"{ServerURL}/{msg.type}", "POST");
        var bodyRaw = Encoding.UTF8.GetBytes(jsonStr);
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        var resMsg = JsonUtility.FromJson<SignalingMsg>(req.downloadHandler.text);

        yield return StartCoroutine(SetDesc(Side.Remote, resMsg.ToDesc()));
    }

    // When Disconnect button is pressed 
    public void Disconnect(){
        if (pc != null)
        {
            channel.Close();
            pc.Close();
            receiveVideoStream = null;
            receiveDepthStream = null;
            receiveVideo.texture = prev_text;
            OnDisconnect.Invoke();
            first_track = true;
        }

    }

    // Sends Unity Camera position, rotation, field of view and plane position via Datachannel
    public void SendMsg(string msg)
    {
        if (channel != null)
        {
            Debug.Log($"Sending message '{msg}'");
            channel.Send(msg);            
        }
    }

    // Resets Unity Camera position to initial one
    public void ResetCam() 
    {
        Debug.Log("Reset cam pressed, changing position from " + Camera.transform.position.ToString() + " to " + init_cam_pos.ToString());
        Camera.transform.position = init_cam_pos;
        Camera.transform.localEulerAngles = init_cam_rot;
    }


    // If the user is moving around the Unity Scene, the new camera position, rotation, fov are updated and sent to FVV in a combined json string 
    void Update()
    {

        if (pc == null)
            return;

        if (Camera.GetComponent<FlyCamera>().IsMoving)
        {
            Camera.GetComponent<FlyCamera>().IsMoving = false;
            var camTransform = Camera.transform;
            var camPos = camTransform.localPosition.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var camRot = camTransform.localRotation.eulerAngles.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var fov = Camera.fieldOfView.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var PlanePos = viewPlane.localPosition.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var radio = radio_fvv;
            //var focalLength = m_Camera.focalLength.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture);

            //var msg = $@"{{""position"": {camPos}, ""rotation"": {camRot}, ""fov"": {fov}}}";  //,"focalLength":[{focalLength}]
            var msg = $@"{{""position"": {camPos}, ""rotation"": {camRot}, ""fov"": {fov}, ""Plane_position"": {PlanePos}, ""radius"": {radio}}}";
            // var msg = $@"{{""position"": {camPos}, ""rotation"": {camRot}, ""fov"": {fov}, ""Plane_position"": {PlanePos}}}";
            msg = msg.Replace('(', '[');
            msg = msg.Replace(')', ']');
            
            SendMsg(msg);
        }
        
    }

    
}
