using System;
using System.Data;
using System.Security.Cryptography;
using Mono.Data.Sqlite;
using UnityEngine;

public static class AdminSecurityService
{
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Pbkdf2Iterations = 100000;
    private const string DefaultTestingPin = "1234";

    public static string CreateSaltedHash(string pin, out string base64Salt)
    {
        if (string.IsNullOrEmpty(pin))
        {
            throw new ArgumentException("PIN cannot be null or empty.", nameof(pin));
        }

        byte[] saltBytes = new byte[SaltSizeBytes];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        byte[] hashBytes = ComputePbkdf2Hash(pin, saltBytes, Pbkdf2Iterations, HashSizeBytes);
        base64Salt = Convert.ToBase64String(saltBytes);
        return Convert.ToBase64String(hashBytes);
    }

    public static bool VerifyPin(string candidatePin, string storedHashBase64, string storedSaltBase64)
    {
        if (string.IsNullOrEmpty(candidatePin) ||
            string.IsNullOrEmpty(storedHashBase64) ||
            string.IsNullOrEmpty(storedSaltBase64))
        {
            return false;
        }

        byte[] saltBytes;
        byte[] expectedHashBytes;

        try
        {
            saltBytes = Convert.FromBase64String(storedSaltBase64);
            expectedHashBytes = Convert.FromBase64String(storedHashBase64);
        }
        catch (FormatException)
        {
            return false;
        }

        byte[] actualHashBytes = ComputePbkdf2Hash(
            candidatePin,
            saltBytes,
            Pbkdf2Iterations,
            expectedHashBytes.Length);

        return FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    private static byte[] ComputePbkdf2Hash(string pin, byte[] salt, int iterations, int outputBytes)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(pin, salt, iterations, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(outputBytes);
        }
    }

    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }

        return diff == 0;
    }

    public static void EnsureDefaultPinConfigured(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath))
        {
            return;
        }

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS AdminConfig (" +
                    "id INTEGER PRIMARY KEY, " +
                    "pin_hash TEXT NOT NULL, " +
                    "pin_salt TEXT NOT NULL);";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT COUNT(1) FROM AdminConfig WHERE id = 1;";
                long exists = Convert.ToInt64(command.ExecuteScalar());
                if (exists > 0)
                {
                    return;
                }
            }

            string hash = CreateSaltedHash(DefaultTestingPin, out string salt);
            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText =
                    "INSERT INTO AdminConfig (id, pin_hash, pin_salt) VALUES (1, @hash, @salt);";
                insertCmd.Parameters.Add(new SqliteParameter("@hash", hash));
                insertCmd.Parameters.Add(new SqliteParameter("@salt", salt));
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    public static bool VerifyPinAgainstDatabase(string dbPath, string candidatePin)
    {
        if (string.IsNullOrEmpty(dbPath) || string.IsNullOrEmpty(candidatePin))
        {
            return false;
        }

        EnsureDefaultPinConfigured(dbPath);

        string storedHash = null;
        string storedSalt = null;

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT pin_hash, pin_salt FROM AdminConfig WHERE id = 1;";
                using (IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        storedHash = reader.GetString(0);
                        storedSalt = reader.GetString(1);
                    }
                }
            }
        }

        return VerifyPin(candidatePin, storedHash, storedSalt);
    }

    public static void SetPinInDatabase(string dbPath, string newPin)
    {
        if (string.IsNullOrEmpty(dbPath))
        {
            return;
        }

        EnsureDefaultPinConfigured(dbPath);
        string hash = CreateSaltedHash(newPin, out string salt);

        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "UPDATE AdminConfig SET pin_hash = @hash, pin_salt = @salt WHERE id = 1;";
                command.Parameters.Add(new SqliteParameter("@hash", hash));
                command.Parameters.Add(new SqliteParameter("@salt", salt));
                command.ExecuteNonQuery();
            }
        }
        Debug.Log("Updated admin PIN hash in database.");
    }
}