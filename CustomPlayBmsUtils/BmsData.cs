using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CustomPlayBmsUtils
{
    public class BmsData
    {
        public List<BmsDataBpm> DataBpmList;
        public List<BmsDataBlock> DataBlockList;
    }

    public class BmsHeaderInfo
    {
        public int Speed;
        public string Scene;
        public string Title;
        public string Artist;
        public float BPM;
        public string Level;
        public int Difficulty;
    }

    public class BmsTimestamp
    {
        public int Section;
        public int Numerator;
        public int Denominator;
    }

    public class BmsDataBpm : BmsTimestamp
    {
        public float BPM;
    }

    public class BmsDataBlock : BmsTimestamp
    {
        public string Code;
        public string Track;
    }

    public class BmsWavList : IEnumerable<KeyValuePair<string, string>>
    {
        List<string> _code = new List<string>();
        List<string> _caption = new List<string>();
        int _size = 0;

        public int Count
        { 
            get { return _size; }
        }

        public KeyValuePair<string, string> this[int i]
        { 
            get
            {
                if (i < _size)
                    return new KeyValuePair<string, string>(_code[i], _caption[i]);
                else
                    throw new IndexOutOfRangeException();
            }
        }

        public void Add(string code, string caption)
        {
            _size++;
            _code.Add(code);
            _caption.Add(caption);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return new BmsWavListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    class BmsWavListEnumerator : IEnumerator<KeyValuePair<string, string>>
    {
        BmsWavList _bwl;
        int _current;

        public BmsWavListEnumerator(BmsWavList data)
        {
            _current = -1;
            _bwl = data;
        }

        public KeyValuePair<string, string> Current
        {
            get
            {
                if (_current < _bwl.Count) return _bwl[_current];
                else throw new IndexOutOfRangeException();
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }

        public void Dispose()
        {
            
        }

        public bool MoveNext()
        {
            _current++;
            if (_current < _bwl.Count)
                return true;
            else
                return false;
        }

        public void Reset()
        {
            _current = -1;
        }
    }
}
