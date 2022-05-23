using System;
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
        /// <returns>BMS 时间戳（Section = 0：精确结果，Section = 1：模糊结果）</returns>
        public static BmsTimestamp TimestampToFraction(float lTime, float rTime, float nTime)
        {
            const float THRESHOLD = 0.0001f;
            const float DENO_LIMIT = 192;

            float target = (nTime - lTime) / (rTime - lTime);

            int deno = 1;
            int nume = 0;
            int direction = 1;
            int section = 1;

            float ret = 0;

            while (deno <= DENO_LIMIT)
            {
                ret = (float)nume / deno;
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
                            } while (nume % (tempDeno - 1) != 0 && nume <= deno);

                            if (nume > deno) nume = deno;
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
