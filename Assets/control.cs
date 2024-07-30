using System.Collections;
using System.Collections.Generic;
using BCI2000RemoteNET;
using UnityEngine;

public class control : MonoBehaviour
{
    UnityBCI2000 bci;

    public Color on_c;
    public Color off_c;

    public float on_time = 1;
    public float off_time = 1;

    public int trials = 100;

    AudioSource sound();
    void Awake() {
	bci = GameObject.Find("BCI2000").GetComponent<UnityBCI2000>();
	bci.OnIdle(bc => {
		bc.AddEvent("light_unity", 1);
		bc.AddEvent("sound_unity", 1);
		bc.AddParameter("Application:Unity", "on_time", on_time.ToString(), "0");
		bc.AddParameter("Application:Unity", "off_time", off_time.ToString(), "0");
		bc.AddParameter("Application:Unity", "trials", trials.ToString(), "1");
		});
    }

    void Start()
    {
	StartCoroutine(Loop());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PollSystemState(BCI2000Remote.SystemState state) {
	while (bci.Control.GetSystemState() != state) {
	    yield return null;
	}
    }

    IEnumerator Loop() {
	while (true) {
	    yield return PollSystemState(BCI2000Remote.SystemState.ParamsModified);
	    on_time = float.Parse(bci.Control.GetParameter("on_time"));
	    off_time = float.Parse(bci.Control.GetParameter("off_time"));
	    trials = int.Parse(bci.Control.GetParameter("trials"));
	    yield return PollSystemState(BCI2000Remote.SystemState.Running);
	    yield return Light(true, trials);
	}
    }

    IEnumerator Light(bool on, int trials) {
	yield return new WaitForSeconds(on ? on_time : off_time);
	bci.Control.SetEvent("light_unity", on ? 1u : 0u);
	this.GetComponent<Renderer>().material.SetColor("_Color", on ? on_c : off_c);
	bci.Control.SetEvent("sound", on ? 1u : 0u);
	if (on) {
	    this.GetComponent<AudioSource>.Play();
	} else {
	    this.GetComponent<AudioSource>.Stop();
	}
	StartCoroutine(Light(!on, trials - (on ? 1 : 0)));
    }
}

