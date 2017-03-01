﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameClientManager : StateMachine, IObserver
{
	//public MahjongView mview;
	private int waitTime = 0;
	private int MaxWaitTime = 12;
	private Player _savepl;
	private static GameClientManager _instance;
	public static GameClientManager Get()
	{
		return _instance;
	}

	private MahjongMain mahjong;
	public MahjongMain LogicMain
	{
		get { return mahjong; }
	}

	private EVoiceType _systemVoiceType = EVoiceType.W_B;
	public EVoiceType SystemVoiceType
	{
		get{ return _systemVoiceType; }
		set{ _systemVoiceType = value; }
	}


	void OnEnable() 
	{
		//if(isClient)
		EventManager.Instance.AddObserver(this);
	}
	void OnDisable() 
	{
		//if(isClient)
		if(EventManager.Instance)
			EventManager.Instance.RemoveObserver(this);
	}


	void Awake() {
		_instance = this;
		mahjong = new MahjongMain();
		//EventManager.Instance.AddObserver(this);
	}

	void Start() {
		Debug.Log ("GameClientManager.Start()");
		//EventManager.Instance.AddObserver(this);
		//if(isLocalPlayer) {
		//if (isClient) {
		//mahjong = new MahjongMain();
		//if (isServer) {
		startGame ();
		//}
	}

	public void startGame() {
		Debug.Log ("GameClientManager.startGame()");
		ChangeState<GameStartState> ();
		waitTime = MaxWaitTime;
		StartCoroutine (checkState ());
	}

	IEnumerator checkState() {
		bool _send = false;
		while (_currentState != null) {
			yield return new WaitForSeconds (1);
			//Debug.Log ("checkState(_currentState=" + _currentState+")");
			if (_currentState is LoopState_AskHandleTsumoHai
				//|| _currentState is LoopState_AskSelectSuteHai 
				|| _currentState is LoopState_AskHandleSuteHai
			) {
				//MahjongMain owner = (_currentState as GameStateBase).logicOwner;
				MahjongMain owner = mahjong;
				Player pl = owner.ActivePlayer;
				if (_savepl != pl) {
					_savepl = pl;
					waitTime = MaxWaitTime;
					_send = false;
				}
				//Debug.Log ("!!!!!!!!!!!!!!!");
				// 等待 MaxWaitTime秒 回應
				if (waitTime > 0) {
					waitTime--;
					if (!_send && waitTime <= 10 ) {
						//Debug.Log (_currentState+"!!!" + waitTime);
						EventManager.Instance.RpcSendEvent (UIEventType.Display_Countdown_Panel, waitTime);
						_send = true;
					}
				} else {
					//waitTime = MaxWaitTime;
					//_send = false;
					_savepl = null;
					handleNoResponse ();
				}
			}
		}
	}

	//10秒內沒有回應處理
	void handleNoResponse() {
		_currentState.Handle ();
		ChangeState<LoopState_ToNextLoop> ();
	}

	void OnDestroy()
	{
		ResManager.ClearMahjongPaiPool();
	}

	void OnApplicationQuit()
	{
		ResManager.ClearMahjongPaiPool();
	}


	public void Restart()
	{
		SceneManager.LoadScene( SceneManager.GetActiveScene().name );
	}

	public void Speak( ECvType content )
	{
		string path = AudioConfig.GetCVPath (SystemVoiceType, content);
		//Debug.Log ("GameClientManager.Speak("+path+")");
		AudioManager.Get().PlaySFX(path);
	}

	public void OnHandleEvent(UIEventType evtID, object[] args) 
	{
		Debug.Log ("GameClientManager.OnHandleEvent("+evtID+")");
		if( CurrentState is GameStateBase )
			(CurrentState as GameStateBase).OnHandleEvent(evtID, args);
	}

}

