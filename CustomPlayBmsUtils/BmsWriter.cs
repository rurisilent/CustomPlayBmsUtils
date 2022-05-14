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

            }
        }
    }
}
