using StreamCore;
using StreamCore.Chat;
using StreamCore.SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using SongRequestManager;
using StreamCore.Utils;
using System.IO.Compression;
// Feature requests: Add Reason for being banned to banlist

namespace SongRequestManager
{
    public partial class RequestBot : MonoBehaviour
    {

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
            {
                string newFilePath = Path.Combine(target.FullName, file.Name);
                    try
                    {
                        file.CopyTo(newFilePath);
                    }
                    catch (Exception)
                    {
                    }
            }
        }

        public static string Backup(ParseState state)
        {
            string errormsg = Backup();
            if (errormsg=="") state.msg("SRManager files backed up.");
            return errormsg;
        } 
    public static string Backup()
        {
            DateTime Now = DateTime.Now;
            string BackupName = Path.Combine(RequestBotConfig.Instance.backuppath, $"SRMBACKUP-{Now.ToString("yyyy-MM-dd-HHmm")}.zip");

            Plugin.Log($"Backing up {Plugin.DataPath}");
            try
            {
                if (!Directory.Exists(RequestBotConfig.Instance.backuppath))
                    Directory.CreateDirectory(RequestBotConfig.Instance.backuppath);

                ZipFile.CreateFromDirectory(Plugin.DataPath, BackupName, System.IO.Compression.CompressionLevel.Fastest, true);
                RequestBotConfig.Instance.LastBackup = DateTime.Now.ToString();
                RequestBotConfig.Instance.Save();

                Plugin.Log($"Backup success writing {BackupName}");
                return success;
            }
        catch       
            {

            }
            Plugin.Log($"Backup failed writing {BackupName}");
            return $"Failed to backup to {BackupName}";
        }

        public class StringNormalization
        {
            public static HashSet<string> BeatsaverBadWords = new HashSet<string>();

            public void ReplaceSymbols(StringBuilder text, char[] mask)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c < 128) text[i] = mask[c];
                }
            }

            public string RemoveSymbols(ref string text, char[] mask)
            {
                var o = new StringBuilder(text.Length);

                foreach (var c in text)
                {
                    if (mask[c] != ' ') o.Append(c);
                }
                return o.ToString();
            }

            // This function takes a user search string, and fixes it for beatsaber.
            public string NormalizeBeatSaverString(string text)
            {
                var words = Split(text);
                StringBuilder result = new StringBuilder();
                foreach (var word in words)
                {
                    if (word.Length < 3) continue;
                    if (BeatsaverBadWords.Contains(word.ToLower())) continue;
                    result.Append(word);
                    result.Append(' ');
                }

                //RequestBot.Instance.QueueChatMessage($"Search string: {result.ToString()}");


                if (result.Length == 0) return "qwesartysasasdsdaa";
                return result.ToString().Trim();
            }

            public string[] Split(string text)
            {
                var sb = new StringBuilder(text);
                ReplaceSymbols(sb, _SymbolsMap);
                string[] result = sb.ToString().ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
               
                return result;
            }

            public char[] _SymbolsMap = new char[128];
            public char[] _SymbolsNoDash = new char[128];


            public StringNormalization()
            {
                for (char i = (char)0; i < 128; i++)
                {
                    _SymbolsMap[i] = i;
                    _SymbolsNoDash[i] = i;
                }

                foreach (var c in new char[] { '@', '*', '+', ':', '-', '<', '~', '>', '(', ')', '[', ']', '/', '\\', '.', ',' }) if (c < 128) _SymbolsMap[c] = ' ';
                foreach (var c in new char[] { '@', '*', '+', ':',  '<', '~', '>', '(', ')', '[', ']', '/', '\\', '.', ',' }) if (c < 128) _SymbolsNoDash[c] = ' ';

                // Incomplete list of words that BeatSaver.com filters out for no good reason.
                foreach (var word in new string[] { "and","the", "this", "from", "will", "when", "with", "what", "who", "why", "how", "was" })
                {
                    BeatsaverBadWords.Add(word.ToLower());
                }

            }
        }

        public static StringNormalization normalize = new StringNormalization();


    }
}