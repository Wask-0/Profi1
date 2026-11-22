using System;
using System.IO;
using UnityEngine;

namespace Labyrinthino.Data
{
    /// <summary>
    /// Reads and writes <see cref="SaveData"/> as JSON. Keeps a backup copy so a corrupted
    /// or half-written file never destroys the player progress.
    /// </summary>
    public sealed class SaveService
    {
        private readonly string _path;
        private readonly string _backupPath;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="path">Absolute path of the main save file.</param>
        public SaveService(string path)
        {
            _path = path;
            _backupPath = path + ".bak";
            Data = new SaveData();
        }

        /// <summary>Currently loaded save data.</summary>
        public SaveData Data { get; private set; }

        /// <summary>
        /// Loads the save file; falls back to the backup and finally to defaults.
        /// </summary>
        public void Load()
        {
            Data = TryRead(_path) ?? TryRead(_backupPath) ?? new SaveData();
        }

        /// <summary>
        /// Writes the save data and refreshes the backup copy.
        /// </summary>
        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(Data);
                string temporaryPath = _path + ".tmp";
                File.WriteAllText(temporaryPath, json);
                if (File.Exists(_path))
                {
                    File.Copy(_path, _backupPath, true);
                }

                File.Copy(temporaryPath, _path, true);
                File.Delete(temporaryPath);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Save failed: " + exception.Message);
            }
        }

        /// <summary>
        /// Returns the progress record of a mini-game, creating it when missing.
        /// </summary>
        /// <param name="id">Mini-game whose progress is requested.</param>
        /// <returns>The mutable progress record.</returns>
        public MiniGameProgress GetProgress(MiniGameId id)
        {
            foreach (MiniGameProgress progress in Data.Games)
            {
                if (progress.GameId == (int)id)
                {
                    return progress;
                }
            }

            MiniGameProgress created = new MiniGameProgress { GameId = (int)id };
            Data.Games.Add(created);
            return created;
        }

        /// <summary>
        /// Removes all progress but keeps the audio settings.
        /// </summary>
        public void ResetProgress()
        {
            Data.Games.Clear();
            Save();
        }

        private static SaveData TryRead(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }

                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
