using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CustomPlayBmsUtils
{
    public class BmsData
    {
        public List<BmsSection> SectionList;
        public List<BmsDataBpm> DataBpmList;
        public List<BmsDataBlock> DataBlockList;

        public BmsData()
        {
            SectionList = new List<BmsSection>();
            DataBpmList = new List<BmsDataBpm>();
            DataBlockList = new List<BmsDataBlock>();
        }
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

    public class BmsSection
    {
        public int ID;
        public float Scale;
    }

    public class BmsTimestamp : IComparable<BmsTimestamp>
    {
        public int Section;
        public int Numerator;
        public int Denominator;

        public BmsTimestamp()
        {
            Section = 0;
            Numerator = 0;
            Denominator = 1;
        }

        public BmsTimestamp(int sec, int nume, int deno)
        {
            Section = sec;
            Numerator = nume;
            Denominator = deno;
        }

        public int CompareTo(BmsTimestamp obj)
        {
            if (Section > obj.Section) return 1;
            else if (Section < obj.Section) return -1;
            else
            {
                var xFloat = (float)Numerator / Denominator;
                var yFloat = (float)obj.Numerator / obj.Denominator;

                if (xFloat > yFloat)
                {
                    return 1;
                }
                else if (xFloat < yFloat) return -1;
                else return 0;
            }
        }

        public void Simplify()
        {
            int gcd = Gcd(Denominator, Numerator);
            Denominator /= gcd;
            Numerator /= gcd;
        }

        private static int Gcd(int a, int b)
        {
            if (a <= 0 || b <= 0) return 0;
            if (a % b == 0) return b;
            else return Gcd(b, a % b);
        }

        public static BmsTimestamp operator +(BmsTimestamp x, BmsTimestamp y)
        {
            int xDeno = x.Denominator;
            int xNume = x.Numerator + x.Denominator * x.Section;
            int yDeno = y.Denominator;
            int yNume = y.Numerator + y.Denominator * y.Section;

            var tmpDeno = xDeno;
            xDeno *= yDeno;
            xNume *= yDeno;
            yDeno *= tmpDeno;
            yNume *= tmpDeno;

            var retNume = xNume + yNume;

            var gcd = Gcd(xDeno, retNume);
            xDeno /= gcd;
            retNume /= gcd;

            return new BmsTimestamp()
            {
                Section = retNume / xDeno,
                Denominator = xDeno,
                Numerator = retNume % xDeno
            };
        }

        public static bool operator >(BmsTimestamp x, BmsTimestamp y)
        {
            return x.CompareTo(y) > 0;
        }

        public static bool operator <(BmsTimestamp x, BmsTimestamp y)
        {
            return x.CompareTo(y) < 0;
        }

        public static bool operator >=(BmsTimestamp x, BmsTimestamp y)
        {
            return x.CompareTo(y) >= 0;
        }

        public static bool operator <=(BmsTimestamp x, BmsTimestamp y)
        {
            return x.CompareTo(y) <= 0;
        }

        public static bool operator ==(BmsTimestamp x, BmsTimestamp y)
        {
            return x.CompareTo(y) == 0;
        }

        public static bool operator !=(BmsTimestamp x, BmsTimestamp y)
        {
            return x.CompareTo(y) == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType()) return false;

            return CompareTo((BmsTimestamp)obj) == 0;
        }

        public override int GetHashCode()
        {
            return Section * 10000 + Denominator * 100 + Numerator * 1;
        }
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

    public class BmsTimestampComparer : IComparer<BmsTimestamp>
    {
        public int Compare(BmsTimestamp x, BmsTimestamp y)
        {
            if (x.Section > y.Section) return 1;
            else if (x.Section < y.Section) return -1;
            else
            {
                var xFloat = (float)x.Numerator / x.Denominator;
                var yFloat = (float)y.Numerator / y.Denominator;

                if (xFloat > yFloat)
                {
                    return 1;
                }
                else if (xFloat < yFloat) return -1;
                else return 0;
            }
        }
    }
}
