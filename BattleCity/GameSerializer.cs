using System;
using System.IO;
using System.Text.Json;
using BattleCity.Models;

namespace BattleCity
{
    // Сохраняет и загружает состояние игры в формате JSON.
    public static class GameSerializer
    {
        private const string SavePath = "savegame.json";

        // Параметры сериализации JSON.
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                // Записывать JSON с отступами для удобства чтения.
                WriteIndented = true
            };

        // Сохранить текущее состояние игры в файл.
        public static bool Save(Game game)
        {
            try
            {
                string json = JsonSerializer.Serialize(game, Options);

                File.WriteAllText(SavePath, json);

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Загрузить состояние игры из файла сохранения.
        public static Game? Load()
        {
            try
            {
                if (!File.Exists(SavePath))
                    return null;

                string json = File.ReadAllText(SavePath);

                return JsonSerializer.Deserialize<Game>(
                    json, Options);
            }
            catch
            {
                return null;
            }
        }

        // Проверить, существует ли файл сохранения.
        public static bool SaveExists()
        {
            return File.Exists(SavePath);
        }

        // Удалить файл сохранения, если он существует.
        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
    }
}
