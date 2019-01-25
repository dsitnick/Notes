using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace dsitnick {
    namespace Note {
        public class NotePlayer : MonoBehaviour {

            public AudioClip Clip;

            private AudioSource[] sources;
            private double[] readyTimes;
            private double startTime;
            private NoteValue[] values;
            private int currentNote;
            private float bpm;

            private double BPM_SCALE { get { return bpm / 60d; } }
            private double TIME { get { return AudioSettings.dspTime; } }

            private void Awake () {
                sources = GetComponents<AudioSource> ();
                readyTimes = new double[sources.Length];

                foreach (AudioSource src in sources) { src.clip = Clip; }
            }

            public void PlayTrack (NoteTrack track, float bpm) {
                this.bpm = bpm;
                startTime = TIME;
                values = track.Notes;
                currentNote = 0;
            }

            private void Update () {
                for (int i = 0; i < sources.Length; i++) {
                    if (TIME > readyTimes[i]) {
                        playNote (i);
                    }
                }
            }

            private const int C3 = 60;
            private void playNote(int srcIndex) {
                NoteValue v = values[currentNote];

                sources[srcIndex].pitch = getPitchMultiplier (v.note, C3);
                sources[srcIndex].PlayScheduled (v.startTime / BPM_SCALE + startTime);
                sources[srcIndex].SetScheduledEndTime (v.endTime / BPM_SCALE + startTime);
                readyTimes[srcIndex] = v.endTime / BPM_SCALE + startTime;
                Debug.Log (srcIndex + ": " + readyTimes[srcIndex] + ": " + v.note);

                currentNote++;
            }

            private static float getPitchMultiplier(int note, int root) {
                return Mathf.Pow (2, (note - root) / 12f);
            }

        }
    }
}