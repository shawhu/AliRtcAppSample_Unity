﻿using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using ali_unity_rtc;
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
using UnityEngine.Android;
#endif

public class Home : MonoBehaviour
{
    public Text info;

	Button mControlButton = null;
	bool mJoined = false;
	IAliRtcEngine mRtcEngine = null;
    private ArrayList permissionList = new ArrayList(); 


    void Start ()
	{
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        permissionList.Add(Permission.Microphone);
        permissionList.Add(Permission.Camera);
        permissionList.Add(Permission.ExternalStorageRead);
        permissionList.Add(Permission.ExternalStorageWrite);
#endif

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        Screen.fullScreen = false;
        Application.wantsToQuit += OnApplicationGoingToQuit;
#endif
        mControlButton = GameObject.Find("ControlButton").GetComponent<Button>();
        //mControlButton.onClick.AddListener(OnControlButtonClicked);

        GameObject go = GameObject.Find("LocalVideoCube");
        VideoDisplaySurface surface = go.AddComponent<VideoDisplaySurface>();

        go = GameObject.Find("RemoteVideoCube");
        surface = go.AddComponent<VideoDisplaySurface>();

        //create alirtc engine
        string extra = "";
        mRtcEngine = IAliRtcEngine.GetEngine (extra);

        //set callback
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        IAliRtcEngine.OnJoinChannelNotify = onJoinChannelNotify;
        IAliRtcEngine.OnPublishNotify = onPublishNotify;
        IAliRtcEngine.OnSubscribeNotify = onSubscribeNotify;
        IAliRtcEngine.OnRemoteUserOnLineNotify = onRemoteUserOnLineNotify;
        IAliRtcEngine.OnRemoteUserOffLineNotify = onRemoteUserOffLineNotify;
        IAliRtcEngine.OnRemoteTrackAvailableNotify = onRemoteTrackAvailableNotify;
        IAliRtcEngine.OnSubscribeChangedNotify = onSubscribeChangedNotify;
        IAliRtcEngine.OnLeaveChannelResultNotify = onLeaveChannelResultNotify;


#else
        mRtcEngine.OnJoinChannelNotify = onJoinChannelNotify;
		mRtcEngine.OnPublishNotify = onPublishNotify;
		mRtcEngine.OnSubscribeNotify = onSubscribeNotify;
		mRtcEngine.OnRemoteUserOnLineNotify = onRemoteUserOnLineNotify;
		mRtcEngine.OnRemoteUserOffLineNotify = onRemoteUserOffLineNotify;
		mRtcEngine.OnRemoteTrackAvailableNotify = onRemoteTrackAvailableNotify;
		mRtcEngine.OnSubscribeChangedNotify = onSubscribeChangedNotify;
		mRtcEngine.OnLeaveChannelResultNotify = onLeaveChannelResultNotify;
		
#endif
        //set auto publish and subscribe
        mRtcEngine.SetAutoPublish(true, true);

    }

	void Update ()
	{
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        CheckPermission();
#endif

    }

    bool OnApplicationGoingToQuit()
    {
        Debug.Log("OnApplicationGoingQuit");


        IAliRtcEngine.Destroy();

        Debug.Log("OnApplicationGoingQuit End");
        return true;
    }

    private void CheckPermission()
    {
#if (UNITY_2018_3_OR_NEWER && UNITY_ANDROID)
        foreach (string permission in permissionList)
        {
            if (Permission.HasUserAuthorizedPermission(permission))
            {

            }
            else
            {
                Permission.RequestUserPermission(permission);
            }
        }
#endif
    }

    public void OnControlButtonClicked()
	{
        Debug.Log("clicked");
        if(mJoined)
		{
            //stop local preview
            mRtcEngine.StopPreview();
            GameObject go = GameObject.Find("LocalVideoCube");
            VideoDisplaySurface surface = go.GetComponent<VideoDisplaySurface>();
            surface.SetEnable(false);

            go = GameObject.Find("RemoteVideoCube");
            if (!ReferenceEquals(go, null))
            {
                surface = go.GetComponent<VideoDisplaySurface>();
                surface.SetEnable(false);
            }

            mRtcEngine.LeaveChannel();
        }
		else
		{
            //set local preview surface
            GameObject go = GameObject.Find("LocalVideoCube");
            VideoDisplaySurface surface = go.GetComponent<VideoDisplaySurface>();
            surface.SetUserId("");
            surface.SetVideoTrack(AliRTCVideoTrack.VIDEO_TRACK_CAMERA);
            surface.SetEnable(true);

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            mRtcEngine.SetLocalViewConfig(true);
            mRtcEngine.ConfigExternalVideoRendering(true);
#endif
            //start local preview
            mRtcEngine.StartPreview();
            // authInfo 从业务 appserver 获取
            // ckwxs2so
            // 24f2feb9cd976671d4271ba0db6597f6
            //join channel


            AliRTCAuthInfo ai = new AliRTCAuthInfo();
            ai.appId = "ckwxs2so";
            ai.userId = "myuserid";
            ai.channel = "myfirstchannel";
            ai.nonce = "AK-a04b4307-64d4-4c9e-be8f-13b42b3d4222";
            ai.timestamp = 1584946092;
            ai.token = "70f16e8615b510c3bd0ccfda19e7da448ff5b9d18e6443321b648d592ce6ce98";
            ai.gslbArray = "['https://rgslb.rtc.aliyuncs.com']";
            
            mRtcEngine.JoinChannel(ai, "shawhu2000");

        }

		mJoined = !mJoined;
		mControlButton.GetComponentInChildren<Text>(true).text = mJoined == false ? "Join" : "Leave";
	}

	private void onJoinChannelNotify (int errorCode)
	{
        if (errorCode == 0)
        {
            Debug.Log("加入频道成功");
            info.text = "加入频道成功";
        }
        else
        {
            Debug.Log("加入频道失败, errorcode:"+errorCode);
            info.text = "加入频道失败, errorcode:" + errorCode;
        }
    }

	private void onPublishNotify(int errorCode)
	{
        Debug.Log("AliRTCUnityDemo onPublishNotify errorCode = " + errorCode);
        if (errorCode == 0)
        {
            Debug.Log("推流成功");
        }
        else
        {
            Debug.Log("推流失败");
        }
    }

	private void onSubscribeNotify (string userId, int videoTrack, int audioTrack)
	{
	}

	private void onRemoteUserOnLineNotify (string userId)
	{

	}

	private void onRemoteUserOffLineNotify (string userId)
	{
        GameObject go = GameObject.Find("RemoteVideoCube");
        if (!ReferenceEquals(go, null))
        {
            VideoDisplaySurface surface = go.GetComponent<VideoDisplaySurface>();
            surface.SetEnable(false);
        }
    }

	private void onRemoteTrackAvailableNotify (string userId, int audioTrack, int videoTrack)
	{
	}

	private void onSubscribeChangedNotify (string userId, int audioTrack, int videoTrack)
	{

        if (videoTrack == (int)AliRTCVideoTrack.VIDEO_TRACK_CAMERA)
        {
            GameObject go = GameObject.Find("RemoteVideoCube");
            if (!ReferenceEquals(go, null))
            {
                VideoDisplaySurface surface = go.GetComponent<VideoDisplaySurface>();
                surface.SetUserId(userId);
                surface.SetVideoTrack(AliRTCVideoTrack.VIDEO_TRACK_CAMERA);
                surface.SetEnable(true);
            }

        }
        else if (videoTrack == (int)AliRTCVideoTrack.VIDEO_TRACK_NONE)
        {
            GameObject go = GameObject.Find("RemoteVideoCube");
            if (!ReferenceEquals(go, null))
            {
                VideoDisplaySurface surface = go.GetComponent<VideoDisplaySurface>();
                surface.SetEnable(false);
            }

        }
    }

	private void onLeaveChannelResultNotify(int result)
	{
		Debug.Log("AliRTCUnityDemo onLeaveChannelResultNotify result: " + result);
	}

}
