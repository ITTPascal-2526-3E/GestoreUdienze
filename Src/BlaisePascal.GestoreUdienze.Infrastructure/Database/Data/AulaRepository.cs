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
                NomeProfessore1 TEXT,
                CognomeProfessore1 TEXT,
                NomeProfessore2 TEXT,
                CognomeProfessore2 TEXT,
                FOREIGN KEY (NomeProfessore1, CognomeProfessore1) REFERENCES Professori(Nome, Cognome),
                FOREIGN KEY (NomeProfessore2, CognomeProfessore2) REFERENCES Professori(Nome, Cognome)
            );
            ";
            command.ExecuteNonQuery();
        }

        public static Aula LeggiAula(int id)
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
                    Nome = reader.GetString(1),
                    NomeProfessore1 = reader.GetString(2),
                    CognomeProfessore1 = reader.GetString(3),
                    NomeProfessore2 = reader.GetString(4),
                    CognomeProfessore2 = reader.GetString(5)
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
                (Nome, NomeProfessore1, CognomeProfessore1, NomeProfessore2, CognomeProfessore2)
                VALUES
                (@nome, @nomeProf1, @cognomeProf1, @nomeProf2, @cognomeProf2)
                ";

                command.Parameters.AddWithValue("@nome", a.Nome ?? string.Empty);
                command.Parameters.AddWithValue("@nomeProf1", a.NomeProfessore1 ?? string.Empty);
                command.Parameters.AddWithValue("@cognomeProf1", a.CognomeProfessore1 ?? string.Empty);
                command.Parameters.AddWithValue("@nomeProf2", a.NomeProfessore2 ?? string.Empty);
                command.Parameters.AddWithValue("@cognomeProf2", a.CognomeProfessore2 ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}