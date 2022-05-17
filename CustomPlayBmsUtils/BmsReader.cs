using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CustomPlayBmsUtils
{
    public class BmsReader
    {
        private BmsHeaderInfo _info = new BmsHeaderInfo();
        private BmsData _data = new BmsData();

        public BmsHeaderInfo Info
        {
            get { return _info; }
        }

        public BmsData Data
        {
            get { return _data; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path">BMS file path</param>
        public BmsReader(string bmsData)
        {
            ReadBms(bmsData);
        }

        private void ReadBms(string bmsData)
        {
            string[] bmsLine = bmsData.Split( new string[] { "\r\n" }, StringSplitOptions.None);

            int pt = 0; //a pointer to read all the files
            int limit = bmsLine.Length;

            Dictionary<string, float> floatBpmList = new Dictionary<string, float>();

            //find header & read until maindata appears
            if (!FindLine(bmsLine, Const.INFO_HEADER, ref pt)) throw new BmsFileNotValidException();
            while (pt < limit)
            {
                if (bmsLine[pt].Contains("#PLAYER ")) int.TryParse(bmsLine[pt].Replace("#PLAYER ", ""), out _info.Speed);
                if (bmsLine[pt].Contains("#GENRE ")) _info.Scene = bmsLine[pt].Replace("#GENRE ", "");
                if (bmsLine[pt].Contains("#TITLE ")) _info.Title = bmsLine[pt].Replace("#TITLE ", "");
                if (bmsLine[pt].Contains("#ARTIST ")) _info.Artist = bmsLine[pt].Replace("#ARTIST ", "");
                if (bmsLine[pt].Contains("#BPM ")) float.TryParse(bmsLine[pt].Replace("#BPM ", ""), out _info.BPM);
                if (bmsLine[pt].Contains("#PLAYLEVEL ")) _info.Level = bmsLine[pt].Replace("#PLAYLEVEL ", "");
                if (bmsLine[pt].Contains("#RANK ")) int.TryParse(bmsLine[pt].Replace("#RANK ", ""), out _info.Difficulty);

                if (bmsLine[pt].Contains("#BPM"))
                {
                    string[] ln = bmsLine[pt].Split(' ');
                    floatBpmList.Add(ln[0].Replace("#BPM", ""), float.Parse(ln[1]));
                }

                if (bmsLine[pt].Contains(Const.DATA_HEADER)) break;
                pt++;
            }

            if (pt >= limit) throw new BmsFileNotValidException();

            //read maindata
            while (pt < limit)
            {
                if (bmsLine[pt].Contains("#"))
                {
                    string[] ln = bmsLine[pt].Split(':');
                    var ctrlCode = ln[0];
                    var noteList = ln[1];

                    int sectionId = ctrlCode[3] - '0' + (ctrlCode[2] - '0') * 10 + (ctrlCode[1] - '0') * 100;
                    string track = ctrlCode[4].ToString() + ctrlCode[5].ToString();

                    if (track == Const.TRACK_BEAT)
                    {
                        _data.SectionList.Add(new BmsSection() { ID = sectionId, Scale = float.Parse(noteList) });
                    }
                    else if (track == Const.TRACK_BPM)
                    {
                        int deno = noteList.Length / 2;
                        int nume = 0;
                        while (nume < deno)
                        {
                            string code = noteList[nume * 2].ToString() + noteList[nume * 2 + 1].ToString();
                            int bpm = BmsUtils.HexToDec(code);
                            _data.DataBpmList.Add(new BmsDataBpm() { Section = sectionId, Denominator = deno, Numerator = nume, BPM = bpm });
                            nume++;
                        }
                    }
                    else if (track == Const.TRACK_BPMFLOAT)
                    {
                        int deno = noteList.Length / 2;
                        int nume = 0;
                        while (nume < deno)
                        {
                            string code = noteList[nume * 2].ToString() + noteList[nume * 2 + 1].ToString();
                            floatBpmList.TryGetValue(code, out var bpm);
                            _data.DataBpmList.Add(new BmsDataBpm() { Section = sectionId, Denominator = deno, Numerator = nume, BPM = bpm });
                            nume++;
                        }
                    }
                    else
                    {
                        int deno = noteList.Length / 2;
                        int nume = 0;
                        while (nume < deno)
                        {
                            string code = noteList[nume * 2].ToString() + noteList[nume * 2 + 1].ToString();
                            _data.DataBlockList.Add(new BmsDataBlock { Section = sectionId, Denominator = deno, Numerator = nume, Code = code, Track = track });
                            nume++;
                        }
                    }
                }

                pt++;
            }

            //finish reading
        }

        private bool FindLine(string[] data, string target, ref int index)
        {
            var limit = data.Length;
            while (index < limit)
            {
                if (data[index].Contains(target)) return true;

                index++;
            }
            return false;
        }
    }
}
