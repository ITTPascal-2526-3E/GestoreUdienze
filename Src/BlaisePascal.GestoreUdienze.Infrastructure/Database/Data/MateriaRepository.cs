using BlaisePascal.GestoreUdienze.Domain.Entities;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace BlaisePascal.GestoreUdienze.Infrastructure.Database.Data
{
    public static class MateriaRepository
    {
        private static string connectionString = "Data Source=gestoreudienze.db";

        public static void CreaTabella()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            CREATE TABLE IF NOT EXISTS Materie (
                CodiceMateria TEXT PRIMARY KEY,
                NomeMateria TEXT,
                CodiceProfessore TEXT,
                FOREIGN KEY (CodiceProfessore) REFERENCES Professori(CodiceProfessore)
            );
            ";
            command.ExecuteNonQuery();
        }

        public static Materia LeggiMateria(string codiceMateria)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
            SELECT * FROM Materie WHERE CodiceMateria = @codiceMateria
            ";
            command.Parameters.AddWithValue("@codiceMateria", codiceMateria);

            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Materia
                {
                    CodiceMateria = reader.GetString(0),
                    NomeMateria = reader.GetString(1),
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
            command.CommandText = "DELETE FROM Materie;";
            command.ExecuteNonQuery();
        }

        public static void SalvaMaterie(List<Materia> materie)
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            foreach (var m in materie)
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO Materie
                (CodiceMateria, NomeMateria, CodiceProfessore)
                VALUES
                (@codiceMateria, @nomeMateria, @codiceProfessore)
                ";

                command.Parameters.AddWithValue("@codiceMateria", m.CodiceMateria ?? string.Empty);
                command.Parameters.AddWithValue("@nomeMateria", m.NomeMateria ?? string.Empty);
                command.Parameters.AddWithValue("@codiceProfessore", m.CodiceProfessore ?? string.Empty);

                command.ExecuteNonQuery();
            }
        }
    }
}
