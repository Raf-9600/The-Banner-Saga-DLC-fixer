using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace The_Banner_Saga_DLC_fixer
{
    internal class Program
    {
        static readonly string tut_heroes = "\"tut_heroes\": \"1\",";
        static readonly string tryggviunlocked = "\"tryggviunlocked\": \"1\",";
        static readonly string tryggvidied = "\"tryggvidied\": \"1\",";

        static readonly string varl_added = "\"varl_added\": \"1\",";
        static readonly List<string> dlc3List = new() {
            "\"unlock_505_exclusive\": \"1\",",
            "\"unlock_hyperrpg_a\": \"1\",",
            "\"unlock_ks_banner_bearer\": \"1\",",
            "\"unlock_ks_banner_folk\": \"1\",",
            "\"unlock_ks_community_reward\": \"1\",",
            "\"unlock_ks_dredge_loot\": \"1\",",
            "\"unlock_ks_human_loot\": \"1\",",
            "\"unlock_ks_sweetener\": \"1\",",
            "\"unlock_ks_varl_loot\": \"1\",",
            "\"unlock_kivi\": \"1\",",
            "\"unlock_petrie_clan_ring\": \"1\",",
            "\"unlock_saga3_deluxe\": \"1\",",
            "\"unlock_sculptors_tools\": \"1\",",
            "\"unlock_shadow_walker\": \"1\",",
            "\"unlock_shield_cleaver\": \"1\","
        };

        static void Main(string[] args)
        {
            List<string> pthSaga1 = GetInstallPathSaga1();

            if (pthSaga1.Count > 0)
                foreach (var saga1JsonZ in pthSaga1)
                {
                    var hashString = GetMD5HashLIte(saga1JsonZ);

                    if (hashString.Equals("efee3d74465a636add20c9e650782d72", StringComparison.OrdinalIgnoreCase) || hashString.Equals("5dd2a2073920f377d33c8d4fbbbcaa85", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            File.WriteAllBytes(saga1JsonZ, Properties.Resources.saga1_json_z);
                            Console.WriteLine("WriteFile " + saga1JsonZ);
                        }
                        catch (Exception) { }
                    }
                }


            string saga1path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBannerSaga", "Local Store", "save", "saga1");

            foreach (string dirNum in Directory.EnumerateDirectories(saga1path))
            {
                if (!int.TryParse(Path.GetFileName(dirNum.TrimEnd(Path.DirectorySeparatorChar)), out _))
                    continue;

                AddStr(Path.Combine(dirNum, "sav_skogr.save.json"), tryggviunlocked, tryggvidied);
                AddStr(Path.Combine(dirNum, "resume.save.json"), tryggviunlocked, tryggvidied);
                AddStr(Path.Combine(dirNum, "sav_chapter2.save.json"), tryggviunlocked, tryggvidied);
            }


            string saga3path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TheBannerSaga3", "Local Store", "save", "saga3");

            foreach (string dirNum in Directory.EnumerateDirectories(saga3path))
            {
                if (!int.TryParse(Path.GetFileName(dirNum.TrimEnd(Path.DirectorySeparatorChar)), out _))
                    continue;


                foreach (string jsonFile in Directory.EnumerateFiles(dirNum))
                {
                    if (Path.GetExtension(jsonFile) != ".json")
                        continue;

                    AddStr3(jsonFile, new List<string>(dlc3List));
                }
            }
        }

        static string GetMD5HashLIte(string filename)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data;
            using (Stream stream = System.IO.File.OpenRead(filename))
                data = md5Hasher.ComputeHash(stream);
            return BitConverter.ToString(data).Replace("-", string.Empty);
        }

        static List<string> GetInstallPathSaga1()
        {
            List<string> result = new(2);

            using (RegistryKey? registryHKLM64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                if (registryHKLM64 != null)
                {
                    RegistryKey? hklm64UninstSteamApp = registryHKLM64.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 237990", false);
                    if (hklm64UninstSteamApp != null)
                    {
                        string? instLoc = (string?)hklm64UninstSteamApp?.GetValue("InstallLocation");
                        if (!string.IsNullOrEmpty(instLoc))
                        {
                            string saga1JsonZ = Path.Combine(instLoc, "assets", "saga1", "saga1.json.z");
                            if (File.Exists(saga1JsonZ))
                                result.Add(saga1JsonZ);
                        }
                    }
                }


            using (RegistryKey? registryHKLM32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                if (registryHKLM32 != null)
                {
                    RegistryKey? hklm32GOG = registryHKLM32.CreateSubKey("SOFTWARE\\GOG.com\\Games\\1207660483", false);
                    if (hklm32GOG != null)
                    {
                        string? instPath = (string?)hklm32GOG?.GetValue("PATH");
                        if (!string.IsNullOrEmpty(instPath))
                        {
                            string saga1JsonZ = Path.Combine(instPath, "assets", "saga1", "saga1.json.z");
                            if (File.Exists(saga1JsonZ))
                                result.Add(saga1JsonZ);
                        }
                    }
                }

            return result;
        }

        static void AddStr3(string path, List<string> addList)
        {
            Console.WriteLine();
            if (!File.Exists(path))
            {
                Console.WriteLine("NotFind " + path);
                return;
            }
            else Console.WriteLine("Open " + path);

            string[] array = File.ReadAllText(path).Split('\n');
            List<string> result = new(array.Length + addList.Count);
            bool add = false;

            foreach (string str in array)
            {
                string strTrim = str.Trim();

                if (addList.Contains(strTrim))
                {
                    addList.Remove(strTrim);
                    Console.WriteLine("\tExis " + strTrim);
                }

                if (strTrim == varl_added && addList.Count > 0)
                {
                    add = true;
                    foreach(string item in addList)
                    {
                        result.Add("\t" + item);
                        Console.WriteLine("\tAdd " + item);
                    }
                }

                result.Add(str);
            }

            if (add)
            {
                Console.WriteLine("Save and Close");
                File.WriteAllText(path, string.Join("\n", result.ToArray()));
            }
            else Console.WriteLine("Close");
        }



        static void AddStr(string path, string addStr, string notStr)
        {
            Console.WriteLine();
            if (!File.Exists(path)) 
            {
                Console.WriteLine("NotFind " + path);
                return;
            }
            else Console.WriteLine("Open " + path);

            string[] array = File.ReadAllText(path).Split('\n');
            List<string> result = new(array.Length +1);
            bool? exis = null;
            string addStrAdd = "    " + addStr;

            foreach (string str in array)
            {
                string strTrim = str.Trim();

                if (strTrim == notStr)
                {
                    exis = true;
                    Console.WriteLine("\tExis " + notStr);
                }

                if (strTrim == addStr)
                {
                    exis = true;
                    Console.WriteLine("\tExis " + addStr);
                }

                if (exis == null && strTrim == tut_heroes)
                {
                    exis = false;
                    result.Add(addStrAdd);
                    Console.WriteLine("\tAdd " + addStr);
                }

                result.Add(str);
            }

            if (exis == false) 
            {
                Console.WriteLine("Save and Close");
                File.WriteAllText(path, string.Join("\n", result.ToArray())); 
            }
            else Console.WriteLine("Close");
        }

    }
}
