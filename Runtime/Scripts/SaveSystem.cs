using System.IO;
using UnityEngine;
using System.Collections.Generic;
using ThornDuck.SaveSystem.Encryption;

namespace ThornDuck.SaveSystem
{
    /// <summary>
    /// Provides slot-based save system for storing data into files. 
    /// </summary>
    /// <seealso cref="EncryptionUtility"/>
    /// <author>Murilo M. Grosso</author>
    public static class SaveSystem
    {
        private const string NAME = "save";
        private const string EXTENSION = "save";
        private static readonly string SaveFolderPath = Application.persistentDataPath;
        
        /// <summary>
        /// Gets the absolute file path for a specific save slot.
        /// </summary>
        /// <param name="slot">The slot index to locate.</param>
        /// <returns>The full save file path for the given slot.</returns>
        public static string GetSlotFilePath(int slot)
            => Path.Combine(SaveFolderPath, $"{NAME}{slot}.{EXTENSION}");

        /// <summary>
        /// Saves the provided data object into the specified file slot.
        /// </summary>
        /// <typeparam name="T">The serializable type of the data.</typeparam>
        /// <param name="data">The instance holding the data to save.</param>
        /// <param name="slot">The save slot index.</param>
        public static void SaveDataInSlot<T>(T data, int slot) where T : new()
        {
            try
            {
                string path = GetSlotFilePath(slot);
                string dataJson = JsonUtility.ToJson(data);
                dataJson = EncryptionUtility.EncryptString(dataJson);

                File.WriteAllText(path, dataJson);
                if (Debug.isDebugBuild)
                    Debug.Log($"[SAVE SYSTEM] Data saved in slot {slot}!");
            }
            catch (System.Exception e)
            {
                if (Debug.isDebugBuild)
                    Debug.LogError($"[SAVE SYSTEM] Failed to save data to slot {slot}:\n{e}");
            }
        }

        /// <summary>
        /// Loads a data object from a specific slot.
        /// </summary>
        /// <typeparam name="T">The type to deserialize into.</typeparam>
        /// <param name="slot">The slot index to load from.</param>
        /// <returns>The loaded data, or default(T) on failure.</returns>
        public static T LoadDataFromSlot<T>(int slot) where T : new()
        {
            try
            {
                string path = GetSlotFilePath(slot);
                string dataJson = File.ReadAllText(path);
                dataJson = EncryptionUtility.DecryptString(dataJson);

                T data = JsonUtility.FromJson<T>(dataJson);
                if (Debug.isDebugBuild)
                    Debug.Log($"[SAVE SYSTEM] Data loaded from slot {slot}!");
                return data;
            }
            catch (System.Exception e)
            {
                if (Debug.isDebugBuild)
                    Debug.LogWarning($"[SAVE SYSTEM] Failed to load data from slot {slot}:\n{e}");
                return default;
            }
        }

        /// <summary>
        /// Deletes the save file associated with the given slot, if it exists.
        /// </summary>
        /// <param name="slot">The slot index to delete.</param>
        public static void DeleteSlot(int slot)
        {
            string path = GetSlotFilePath(slot);

            if (!File.Exists(path))
                return;

            try
            {
                File.Delete(path);
                if (Debug.isDebugBuild)
                    Debug.Log($"[SAVE SYSTEM] Slot {slot} deleted!");
            }
            catch (System.Exception e)
            {
                if (Debug.isDebugBuild)
                    Debug.LogError($"[SAVE SYSTEM] Failed to delete slot {slot}:\n{e}");
            }
        }

        /// <summary>
        /// Copies the contents of one save slot to another.
        /// </summary>
        /// <param name="fromSlot">Source slot index.</param>
        /// <param name="toSlot">Destination slot index.</param>
        /// <param name="overwrite">Whether to overwrite if the destination already exists.</param>
        public static void CopySlot(int fromSlot, int toSlot, bool overwrite = true)
        {
            string fromPath = GetSlotFilePath(fromSlot);
            string toPath = GetSlotFilePath(toSlot);

            try
            {
                if (File.Exists(toPath) && !overwrite)
                {
                    if (Debug.isDebugBuild)
                        Debug.LogWarning($"[SAVE SYSTEM] Cannot overwrite slot {toSlot} with {fromSlot}!");
                    return;
                }

                File.Copy(fromPath, toPath, overwrite);
                if (Debug.isDebugBuild)
                    Debug.Log($"[SAVE SYSTEM] Slot {fromSlot} copied to slot {toSlot}!");
            }
            catch (System.Exception e)
            {
                if (Debug.isDebugBuild)
                    Debug.LogError($"[SAVE SYSTEM] Failed to copy slot {fromSlot} to {toSlot}:\n{e}");
            }
        }

        /// <summary>
        /// Checks if a save file exists for the given slot.
        /// </summary>
        /// <param name="slot">The slot index to check.</param>
        /// <returns>True if the slot exists, otherwise false.</returns>
        public static bool DoesSlotExist(int slot)
        {
            string path = GetSlotFilePath(slot);
            return File.Exists(path);
        }

        /// <summary>
        /// Finds and deletes all slots.
        /// </summary>
        public static void DeleteAllSlots()
        {
            List<int> slots = FindAllSavedSlots();
            
            foreach (int slot in slots)
                DeleteSlot(slot);
        }

        /// <summary>
        /// Finds all slots.
        /// </summary>
        /// <returns>List of all slot indexes.</returns>
        public static List<int> FindAllSavedSlots()
        {
            DirectoryInfo dir = new DirectoryInfo(SaveFolderPath);
            FileInfo[] files = dir.GetFiles($"{NAME}*.{EXTENSION}");
            List<int> slots = new();

            foreach (FileInfo f in files)
            {
                string slotString = f
                    .ToString()
                    .Split(NAME)[1]
                    .Split(".")[0];
                try
                {
                    int slot = int.Parse(slotString);
                    slots.Add(slot);
                }
                catch{}
            }

            return slots;
        }
    }
}