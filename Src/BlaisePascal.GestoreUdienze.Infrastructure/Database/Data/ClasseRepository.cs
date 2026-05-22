using BlaisePascal.GestoreUdienze.Domain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Infrastructure.Database.Data
{
    public static class ClasseRepository
    {
        private static string connectionString = "Data Source=gestoreudienze.db";

        public static void CreaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Classi (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT,
                CodiceProfessore TEXT,
                FOREIGN KEY (CodiceProfessore) REFERENCES Professori(CodiceProfessore)
            );
            ";
            command.ExecuteNonQuery();
        }

        public static Classe LeggiClasse(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT * FROM Classi WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", id);

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Classe
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    CodiceProfessore = reader.GetString(2)
                };
            }
            return null;
        }

        public static void SvuotaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Classi;";
            command.ExecuteNonQuery();
        }

        public static void SalvaClassi(List<Classe> classi)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            foreach (var c in classi)
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO Classi
                (Nome, CodiceProfessore)
                VALUES
                (@nome, @codiceProfessore)
                ";

                command.Parameters.AddWithValue("@nome", c.Nome ?? string.Empty);
                command.Parameters.AddWithValue("@codiceProfessore", c.CodiceProfessore ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}