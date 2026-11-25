using System;
using System.IO;
using System.Text.Json;
using BattleCity.Models;

namespace BattleCity
{
    // Статический класс для сохранения и загрузки состояния игры
    public static class GameSerializer
    {
        private const string SavePath = "savegame.json";

        // Настройки сериализации JSON
        private static readonly JsonSerializerOptions Options =
            new JsonSerializerOptions
            {
                // Включает форматирование JSON с отступами
                WriteIndented = true
            };

        // Метод сохранения текущего состояния игры
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

        // Метод загрузки сохранённой игры
        public static Game Load()
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

        // Проверяет, существует ли файл сохранения
        public static bool SaveExists()
        {
            return File.Exists(SavePath);
        }

        // Метод удаления сохранённой игры
        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
        }
    }
}