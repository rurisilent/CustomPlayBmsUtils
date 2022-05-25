using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace CustomPlayBmsUtils
{
    public class BmsWriter
    {
        private BmsHeaderInfo _info;
        private BmsData _data;
        private BmsWavList _wavList;
        private IBmsWriterOutput _output;

        private Dictionary<int, string> _bpmHashSheet;

        public BmsWriter(BmsHeaderInfo info)
        {
            _info = info;
        }

        public BmsWriter(BmsHeaderInfo info, BmsWavList wavList)
        {
            _info = info;
            _wavList = wavList;
        }

        public BmsWriter(BmsHeaderInfo info, BmsWavList wavList, BmsData data)
        {
            _info = info;
            _wavList = wavList;
            _data = data;
        }

        public BmsWriter(BmsHeaderInfo info, BmsWavList wavList, BmsData data, IBmsWriterOutput output)
        {
            _info = info;
            _wavList = wavList;
            _data = data;
            _output = output;
        }

        public void Write(string path)
        {
            StringBuilder bmsFile = new StringBuilder();

            //write header 1st
            WriteHeader(bmsFile);
            WriteData(bmsFile);

            if (!File.Exists(path))
            {
                using (FileStream stream = File.Create(path))
                {
                    stream.Close();
                }
            }
            File.WriteAllText(path, bmsFile.ToString());
        }

        private void WriteHeader(StringBuilder sb)
        {
            sb.AppendLine("");
            sb.AppendLine(Const.INFO_HEADER);
            sb.AppendLine("");
            sb.AppendLine($"#PLAYER {_info.Speed}");
            sb.AppendLine($"#GENRE {_info.Scene}");
            sb.AppendLine($"#TITLE {_info.Title}");
            sb.AppendLine($"#ARTIST {_info.Artist}");
            sb.AppendLine($"#BPM {_info.BPM}");
            sb.AppendLine($"#PLAYLEVEL {_info.Level}");
            sb.AppendLine($"#RANK {_info.Difficulty}");
            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine("#LNTYPE 1");

            if (_wavList != null)
            {
                sb.AppendLine("");
                foreach (var wav in _wavList)
                {
                    sb.AppendLine($"#WAV{wav.Key} {wav.Value}");
                }
            }

            if (_data != null)
            {
                int bmsCounter = 1;
                _bpmHashSheet = new Dictionary<int, string>();
                if (_data.DataBpmList != null && _data.DataBpmList.Count > 0)
                {
                    //if not integer bpm, put it in datalist
                    foreach (var bpm in _data.DataBpmList)
                    {
                        //this is a float bpm
                        if (Math.Abs(bpm.BPM - Math.Round(bpm.BPM)) > 0.001f || bpm.BPM > 255)
                        {
                            _bpmHashSheet.Add(_data.DataBpmList.IndexOf(bpm), BmsUtils.DecToHex(bmsCounter));
                            sb.AppendLine($"#BPM{BmsUtils.DecToHex(bmsCounter)} {bpm.BPM}");
                            bmsCounter++;
                        }
                    }
                }
            }

            sb.AppendLine("");
            sb.AppendLine("");
        }

        private void WriteData(StringBuilder sb)
        {
            sb.AppendLine(Const.DATA_HEADER);
            sb.AppendLine("");
            sb.AppendLine("");

            if (_data != null)
            {
                _output?.Log($"{_data.DataBlockList.Count} : {_data.DataBpmList.Count} : {_data.SectionList.Count}");

                if (_data.DataBlockList.Count <= 0 && _data.DataBpmList.Count <= 0 && _data.SectionList.Count <= 0) return;

                _data.DataBlockList.Sort(new BmsBlockComparer());

                int ptSec = 0;
                bool written = false;
                StringBuilder ret = new StringBuilder();

                int ptDataSection = 0;
                int ptDataBpm = 0;
                int ptDataBlock = 0;

                int limit = 0;
                if (_data.DataBlockList.Count > 0)
                    limit = _data.DataBlockList[_data.DataBlockList.Count - 1].Section;
                if (_data.DataBpmList.Count > 0)
                    limit = Math.Max(limit, _data.DataBpmList[_data.DataBpmList.Count - 1].Section);
                if (_data.SectionList.Count > 0)
                    limit = Math.Max(limit, _data.SectionList[_data.SectionList.Count - 1].ID);

                while (ptSec <= limit)
                {
                    written = false;

                    //write section
                    if (ptDataSection < _data.SectionList.Count && _data.SectionList[ptDataSection].ID <= ptSec)
                    {
                        sb.AppendLine($"#{BmsUtils.IntToSecID(ptSec)}{Const.TRACK_BEAT}:{_data.SectionList[ptDataSection].Scale}");
                        _output?.Log($"#{BmsUtils.IntToSecID(ptSec)}{Const.TRACK_BEAT}:{_data.SectionList[ptDataSection].Scale}");
                        written = true;
                        ptDataSection++;
                    }

                    //write bpm
                    List<BmsDataBpm> tempBpm = new List<BmsDataBpm>();
                    List<BmsDataBpm> tempBpmFloat = new List<BmsDataBpm>();
                    while (ptDataBpm < _data.DataBpmList.Count && _data.DataBpmList[ptDataBpm].Section <= ptSec)
                    {
                        if (_bpmHashSheet.ContainsKey(ptDataBpm))
                        {
                            var tmp = _data.DataBpmList[ptDataBpm];
                            tmp.BPM = ptDataBpm;
                            tempBpmFloat.Add(tmp);
                        }
                        else
                        {
                            tempBpm.Add(_data.DataBpmList[ptDataBpm]);
                        }
                        ptDataBpm++;
                    }

                    if (tempBpm.Count > 0)
                    {
                        var finDeno = BmsUtils.LCMTimestamp(tempBpm);
                        tempBpm.Sort(new BmsTimestampComparer());
                        ret.Clear();
                        ret.Append($"#{BmsUtils.IntToSecID(ptSec)}{Const.TRACK_BPM}:");
                        int pt = 0;
                        for (int i = 0; i < finDeno; i++)
                        {
                            if (pt < tempBpm.Count && tempBpm[pt] == new BmsTimestamp(ptSec, i, finDeno))
                            {
                                ret.Append(BmsUtils.DecToHex((int)Math.Round(tempBpm[pt].BPM)));
                                pt++;
                            }
                            else
                            {
                                ret.Append("00");
                            }
                        }
                        sb.AppendLine(ret.ToString());
                        _output?.Log(ret.ToString());
                        written = true;
                    }

                    if (tempBpmFloat.Count > 0)
                    {
                        var finDeno = BmsUtils.LCMTimestamp(tempBpmFloat);
                        tempBpmFloat.Sort(new BmsTimestampComparer());
                        ret.Clear();
                        ret.Append($"#{BmsUtils.IntToSecID(ptSec)}{Const.TRACK_BPMFLOAT}:");
                        int pt = 0;
                        for (int i = 0; i < finDeno; i++)
                        {
                            if (pt < tempBpmFloat.Count && tempBpmFloat[pt] == new BmsTimestamp(ptSec, i, finDeno))
                            {
                                ret.Append(_bpmHashSheet[(int)tempBpmFloat[pt].BPM]);
                                pt++;
                            }
                            else
                            {
                                ret.Append("00");
                            }
                        }
                        sb.AppendLine(ret.ToString());
                        _output?.Log(ret.ToString());
                        written = true;
                    }

                    //write note data
                    List<BmsDataBlock> tempBlock = new List<BmsDataBlock>();
                    string trackId = "";
                    while (ptDataBlock < _data.DataBlockList.Count && _data.DataBlockList[ptDataBlock].Section <= ptSec)
                    {
                        if (trackId == "")
                        {
                            trackId = _data.DataBlockList[ptDataBlock].Track;
                            tempBlock.Add(_data.DataBlockList[ptDataBlock]);
                        }
                        else
                        {
                            if (trackId == _data.DataBlockList[ptDataBlock].Track)
                            {
                                tempBlock.Add(_data.DataBlockList[ptDataBlock]);
                            }
                            else
                            {
                                if (tempBlock.Count > 0)
                                {
                                    var finDeno = BmsUtils.LCMTimestamp(tempBlock);
                                    tempBlock.Sort(new BmsTimestampComparer());
                                    ret.Clear();
                                    ret.Append($"#{BmsUtils.IntToSecID(ptSec)}{trackId}:");
                                    int pt = 0;
                                    for (int i = 0; i < finDeno; i++)
                                    {
                                        if (pt < tempBlock.Count && tempBlock[pt] == new BmsTimestamp(ptSec, i, finDeno))
                                        {
                                            ret.Append(tempBlock[pt].Code);
                                            pt++;
                                        }
                                        else
                                        {
                                            ret.Append("00");
                                        }
                                    }
                                    sb.AppendLine(ret.ToString());
                                    _output?.Log(ret.ToString());
                                    written = true;
                                }

                                trackId = _data.DataBlockList[ptDataBlock].Track;
                                tempBlock = new List<BmsDataBlock>();
                                tempBlock.Add(_data.DataBlockList[ptDataBlock]);
                            }
                        }
                        ptDataBlock++;
                    }

                    if (tempBlock.Count > 0)
                    {
                        var finDeno = BmsUtils.LCMTimestamp(tempBlock);
                        tempBlock.Sort(new BmsTimestampComparer());
                        ret.Clear();
                        ret.Append($"#{BmsUtils.IntToSecID(ptSec)}{trackId}:");
                        int pt = 0;
                        for (int i = 0; i < finDeno; i++)
                        {
                            if (pt < tempBlock.Count && tempBlock[pt] == new BmsTimestamp(ptSec, i, finDeno))
                            {
                                ret.Append(tempBlock[pt].Code);
                                pt++;
                            }
                            else
                            {
                                ret.Append("00");
                            }
                        }
                        sb.AppendLine(ret.ToString());
                        _output?.Log(ret.ToString());
                        written = true;
                    }

                    ptSec++;
                    if (written)
                    {
                        sb.AppendLine("");
                    }
                }
            }
        }
    }
}
