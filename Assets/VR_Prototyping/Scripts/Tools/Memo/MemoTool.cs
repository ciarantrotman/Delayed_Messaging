using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace VR_Prototyping.Scripts.Tools.Memo
{
    public class MemoTool : BaseTool
    {        
        [BoxGroup("Memo Tool Settings")] [Required] public GameObject memoPrefab;
        [BoxGroup("Memo Tool Settings")] public List<string> microphones;
        [BoxGroup("Memo Tool Settings")] [Range(.01f, .1f)] public float triggerDistance = .05f;

        internal MemoText MemoText { get; set; }
        
        private readonly List<AudioSource> audioSources = new List<AudioSource>();
        private MemoNode memoNode;
        private string microphone;
        private int index;
        private float time;

        private AudioClip memo;
        
        protected override void Initialise()
        {
            SetupMicrophone();
        }

        private void SetupMicrophone()
        {
            foreach (string device in Microphone.devices)
            {
                if (device != null)
                {
                    microphones.Add(device);
                }
            }

            microphone = microphones[0];
        }

        public void ChangeMicrophone(int i)
        {
            if (i <= microphones.Count - 1)
            {
                microphone = microphones[i];
            }
        }

        protected override void ToolUpdate()
        {
            
        }
        
        protected override void ToolStart()
        {
            time = Time.time;
            memo = Microphone.Start(microphone, true, 30, 44100);
        }

        protected override void ToolStay()
        {
            
        }

        protected override void ToolEnd()
        {
            NewMemo();
            Microphone.End(microphone);
            memoNode.ReleaseMemo(memo, Time.time - time);
            memoNode = null;
        }

        protected override void ToolActivate()
        {
            if (microphone == null) return;
            MemoText.SetText(microphone);
        }

        private void NewMemo()
        {
            GameObject node = Instantiate(memoPrefab);
            memoNode = node.GetComponent<MemoNode>();
            memoNode.Initialise(index, this);
            audioSources.Add(memoNode.AudioSource);
            index++;
        }

        public void PlayAudio(int memoIndex)
        {
            int i = 0;
            foreach (AudioSource audioSource in audioSources)
            {
                if (memoIndex == i)
                {
                    audioSource.Play();
                }
                else
                {
                    audioSource.Stop();
                }
                i++;
            }
        }
        public void PauseAudio()
        {
            foreach (AudioSource audioSource in audioSources)
            {
                audioSource.Stop();
            }
        }
    }
}