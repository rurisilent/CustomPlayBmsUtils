using System;
using System.Collections.Generic;
using System.Text;

namespace CustomPlayBmsUtils
{
    public static class BmsUtils
    {
        public static int HexToDec(string hexCode)
        {
            const string hexSheet = "0123456789ABCDEF";
            return hexSheet.IndexOf(hexCode[0]) * 16 + hexSheet.IndexOf(hexCode[1]);
        }

        public static string DecToHex(int n)
        {
            const string hexSheet = "0123456789ABCDEF";
            List<char> temp = new List<char>();

            while (n % 16 != n)
            {
                temp.Add(hexSheet[n % 16]);
                n /= 16;
            }

            temp.Add(hexSheet[n % 16]);
            if (temp.Count == 1) temp.Add('0');

            StringBuilder ret = new StringBuilder();
            int count = temp.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                ret.Append(temp[i]);
            }

            return ret.ToString();
        }

        public static string IntToSecID(int n)
        {
            if (n < 10) return $"00{n}";
            else if (n < 100) return $"0{n}";
            else return $"{n}";
        }

        public static int LCMTimestamp(List<BmsDataBpm> timestamps)
        {
            int count = timestamps.Count;
            int finDeno = 1;
            int gcd;
            for (int i = 0; i < count; i++)
            {
                gcd = GCD(finDeno, timestamps[i].Denominator);
                finDeno = timestamps[i].Denominator * finDeno / gcd;
            }

            for (int i = 0; i < count; i++)
            {
                timestamps[i].Numerator *= finDeno / timestamps[i].Denominator;
                timestamps[i].Denominator = finDeno;
            }

            return finDeno;
        }

        public static int LCMTimestamp(List<BmsDataBlock> timestamps)
        {
            int count = timestamps.Count;
            int finDeno = 1;
            int gcd;
            for (int i = 0; i < count; i++)
            {
                gcd = GCD(finDeno, timestamps[i].Denominator);
                finDeno = timestamps[i].Denominator * finDeno / gcd;
            }

            for (int i = 0; i < count; i++)
            {
                timestamps[i].Numerator *= finDeno / timestamps[i].Denominator;
                timestamps[i].Denominator = finDeno;
            }

            return finDeno;
        }

        private static int GCD(int a, int b)
        {
            if (b > a) return GCD(b, a);
            if (a % b == 0) return b;
            else return GCD(b, a % b);
        }

        public static string AppendTrackToType(string type, string track)
        {
            string actualTrack;

            if (track.Contains("road")) actualTrack = "road";
            else if (track.Contains("air")) actualTrack = "air";
            else actualTrack = "null";

            if (type == "small_1" ||
            type == "mid_1" ||
            type == "mid_2" ||
            type == "large_1" ||
            type == "large_2" ||
            type == "long" ||
            type == "ghost" ||
            type == "assault" ||
            type == "hammer" ||
            type == "gear" ||
            type == "boss_ra_far_atk_1" ||
            type == "boss_ra_far_atk_2" ||
            type == "boss_ra_far_atk_3" ||
            type == "boss_ra_gear" ||
            type == "special_note" ||
            type == "special_hp" ||
            type == "long_short" ||
            type == "special_pigeon")
            {

                return $"{type}_{actualTrack}";
            }
            else if (type == "double" ||
                type == "punch")
            {
                return $"{type}_ra";
            }
            else if (
                type == "boss_ra" ||
                type == "boss_ra_out" ||
                type == "boss_ra_atk_1" ||
                type == "boss_ra_atk_1_wait" ||
                type == "boss_ra_punch" ||
                type == "boss_ra_punch_out" ||
                type == "boss_ra_far_atk_1_start" ||
                type == "boss_ra_far_atk_2_start" ||
                type == "boss_ra_far_atk_1_end" ||
                type == "boss_ra_far_atk_2_end" ||
                type == "boss_ra_far_atk_1_to_2" ||
                type == "boss_ra_far_atk_2_to_1" ||
                type == "effect_timing" ||
                type == "preset_beatbpm" ||
                type == "vfx_changescene")
            {
                return type;
            }
            else if (
                type == "vfx_visibility_show" ||
                type == "vfx_visibility_hide" ||
                type == "vfx_bossvisibility_show" ||
                type == "vfx_bossvisibility_hide"
                )
            {
                return type.Replace("_show", "").Replace("_hide", "");
            }
            else
                return type;
        }

        /// <summary>
        /// 时间戳转换为分数
        /// </summary>
        /// <param name="lTime">左边界</param>
        /// <param name="rTime">右边界</param>
        /// <param name="nTime">时间</param>
        /// <returns>BMS 时间戳</returns>
        public static BmsTimestamp TimestampToFraction(float lTime, float rTime, float nTime)
        {
            const float THRESHOLD = 0.0001f;
            const float DENO_LIMIT = 384;

            float target = (nTime - lTime) / (rTime - lTime);

            int deno = 1;
            int nume = 0;
            int direction = 1;
            int section = 0;

            while (deno < DENO_LIMIT)
            {
                float ret = (float)nume / deno;
                if (Math.Abs(target - ret) <= THRESHOLD)
                {
                    section = 0;
                    break;
                }
                else
                {
                    if (direction > 0) //right iterating
                    {
                        if (ret > target)
                        {
                            var tempDeno = deno + 1;
                            deno *= tempDeno;
                            nume *= tempDeno;

                            do
                            {
                                nume++;
                            } while (nume % (tempDeno - 1) != 0);

                            //if (nume > deno) nume = deno;
                            tempDeno--;
                            deno /= tempDeno;
                            nume /= tempDeno;
                            direction *= -1;
                            nume += direction;
                        }
                        else
                        {
                            nume += direction;
                        }
                    }
                    else //left iterating
                    {
                        if (ret < target)
                        {
                            var tempDeno = deno + 1;
                            deno *= tempDeno;
                            nume *= tempDeno;

                            do
                            {
                                nume--;
                            } while (nume % (tempDeno - 1) != 0 && nume >= 0);

                            if (nume < 0) nume = 0;
                            tempDeno--;
                            deno /= tempDeno;
                            nume /= tempDeno;
                            direction *= -1;
                            nume += direction;
                        }
                        else
                        {
                            nume += direction;
                        }
                    }
                    
                }
            }

            return new BmsTimestamp()
            {
                Denominator = deno,
                Numerator = nume,
                Section = section
            };
        }
    }
}
