using BlaisePascal.GestoreUdienze.Domain.Entities;
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
                CodiceProfessore TEXT PRIMARY KEY,
                Nome TEXT,
                Cognome TEXT
            );
            ";
            command.ExecuteNonQuery();
        }

        public static Professore LeggiProfessore(string codiceProfessore)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT * FROM Professori WHERE CodiceProfessore = @codiceProfessore
            ";
            command.Parameters.AddWithValue("@codiceProfessore", codiceProfessore);

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Professore
                {
                    CodiceProfessore = reader.GetString(0),
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
                (CodiceProfessore, Nome, Cognome)
                VALUES
                (@codiceProfessore, @nome, @cognome)
                ";

                command.Parameters.AddWithValue("@codiceProfessore", p.CodiceProfessore ?? string.Empty);
                command.Parameters.AddWithValue("@nome", p.Nome ?? string.Empty);
                command.Parameters.AddWithValue("@cognome", p.Cognome ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}