using BlaisePascal.GestoreUdienze.Domain.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Infrastructure.Database.Data
{
    public static class OrarioTurniRepository
    {
        private static string connectionString = "Data Source=gestoreudienze.db";

        public static void CreaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS OrarioTurni (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Orario INTEGER,
                NomeProfessore TEXT,
                CognomeProfessore TEXT,
                FOREIGN KEY (NomeProfessore, CognomeProfessore) REFERENCES Professori(Nome, Cognome)
            );
            ";
            command.ExecuteNonQuery();
        }

        public static OrarioTurni? LeggiOrarioTurni(int id)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT * FROM OrarioTurni WHERE Id = @id
            ";
            command.Parameters.AddWithValue("@id", id);

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new OrarioTurni
                {
                    Id = reader.GetInt32(0),
                    Orario = reader.GetInt32(1),
                    NomeProfessore = reader.GetString(2),
                    CognomeProfessore = reader.GetString(3)
                };
            }
            return null;
        }

        public static void SvuotaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM OrarioTurni;";
            command.ExecuteNonQuery();
        }

        public static void SalvaOrarioTurni(List<OrarioTurni> orariTurni)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            foreach (var o in orariTurni)
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO OrarioTurni
                (Orario, NomeProfessore, CognomeProfessore)
                VALUES
                (@orario, @nomeProf, @cognomeProf)
                ";

                command.Parameters.AddWithValue("@orario", o.Orario);
                command.Parameters.AddWithValue("@nomeProf", o.NomeProfessore ?? string.Empty);
                command.Parameters.AddWithValue("@cognomeProf", o.CognomeProfessore ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}