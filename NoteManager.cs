using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Melanchall.DryWetMidi.Smf;
using System;

namespace dsitnick {
    namespace Note {

        public class NoteManager : MonoBehaviour {

            public string Path;

            [Range (50, 200)]
            public float BPM = 120;

            public NotePlayer[] Players;
            public int TrackIndex;

            private void Start () {
                List<NoteTrack> tracks = ReadMidiFile (Path);
                //Set specific tracks based on array index in inspector
                for (int i = 0; i < Players.Length; i++) {
                    if (Players[i] != null)
                        Players[i].PlayTrack (tracks[i], BPM);
                }
            }

            public static List<NoteTrack> ReadMidiFile (string path) {
                MidiFile file;
                try {
                    file = MidiFile.Read (path);
                } catch (Exception e) {
                    Debug.LogError (e.ToString ());
                    return null;
                }

                TicksPerQuarterNoteTimeDivision div = file.TimeDivision as TicksPerQuarterNoteTimeDivision;
                if (div == null) {
                    Debug.LogError ("Bad time division format for " + path);
                    return null;
                }

                List<NoteTrack> result = new List<NoteTrack> ();

                foreach (TrackChunk chunk in file.GetTrackChunks ()) {
                    List<NoteValue> notes = new List<NoteValue> ();
                    Dictionary<byte, long> active = new Dictionary<byte, long> ();
                    long currentTime = 0;

                    foreach (MidiEvent e in chunk.Events) {
                        currentTime += e.DeltaTime;

                        NoteOnEvent on = e as NoteOnEvent;
                        NoteOffEvent off = e as NoteOffEvent;

                        if (on != null) {
                            byte n = on.NoteNumber;
                            if (active.ContainsKey (n)) {
                                notes.Add (new NoteValue (n, active[n], currentTime, div.TicksPerQuarterNote));
                                active.Remove (n);
                            }
                            active.Add (n, currentTime);
                        }
                        if (off != null) {
                            byte n = off.NoteNumber;
                            if (active.ContainsKey (n)) {
                                notes.Add (new NoteValue (n, active[n], currentTime, div.TicksPerQuarterNote));
                                active.Remove (n);
                            }
                        }
                    }

                    result.Add (new NoteTrack (notes));
                }

                return result;
            }

        }

        public struct NoteTrack {
            public NoteValue[] Notes;

            public NoteTrack (List<NoteValue> n) {
                Notes = n.ToArray ();
            }
        }

        public struct NoteValue {

            public byte note;
            public double startTime, endTime;

            public NoteValue (byte note, double startTime, double endTime) {
                this.note = note;
                this.startTime = startTime;
                this.endTime = endTime;
            }

            public NoteValue (byte note, long startTime, long endTime, short perQuarter) {
                this.note = note;
                this.startTime = quarters (startTime, perQuarter);
                this.endTime = quarters (endTime, perQuarter);
            }

            private static double quarters (long samples, short perQuarter) {
                return (double)samples / perQuarter;
            }
        }
    }
}