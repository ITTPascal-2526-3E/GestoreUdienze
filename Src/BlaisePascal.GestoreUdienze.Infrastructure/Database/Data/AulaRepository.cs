using BlaisePascal.GestoreUdienze.Domain.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Infrastructure.Database.Data
{
    public static class AulaRepository
    {
        private static string connectionString = "Data Source=gestoreudienze.db";

        public static void CreaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Aule (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT,
                Ala TEXT,
                Piano INTEGER
            );
            ";
            command.ExecuteNonQuery();
        }

        public static Aula? LeggiAula(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT * FROM Aule WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", id);

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Aula
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1)
                };
            }
            return null;
        }

        public static void SvuotaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Aule;";
            command.ExecuteNonQuery();
        }

        public static void SalvaAule(List<Aula> aule)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            foreach (var a in aule)
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO Aule
                (Nome)
                VALUES
                (@nome)
                ";

                command.Parameters.AddWithValue("@nome", a.Nome ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}