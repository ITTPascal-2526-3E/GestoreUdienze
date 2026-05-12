using BlaisePascal.GestoreUdienze.Domain.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Infrastructure.Database.Data
{
    public static class ProfessoreRepository
    {
        private static string connectionString = "Data Source=gestoreudienze.db";

        public static void CreaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Professori (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Nome TEXT,
                Cognome TEXT,
                UNIQUE(Nome, Cognome)
            );
            ";
            command.ExecuteNonQuery();
        }

        public static Professore LeggiProfessore(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT * FROM Professori WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", id);

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Professore
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Cognome = reader.GetString(2)
                };
            }
            return null;
        }

        public static void SvuotaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Professori;";
            command.ExecuteNonQuery();
        }

        public static void SalvaProfessori(List<Professore> professori)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            foreach (var p in professori)
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO Professori
                (Nome, Cognome)
                VALUES
                (@nome, @cognome)
                ";

                command.Parameters.AddWithValue("@nome", p.Nome ?? string.Empty);
                command.Parameters.AddWithValue("@cognome", p.Cognome ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}