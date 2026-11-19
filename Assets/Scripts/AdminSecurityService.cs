using System;
using System.Data;
using System.Security.Cryptography;
using Mono.Data.Sqlite;
using UnityEngine;

/// <summary>
/// Handles salted PIN hashing, constant-time verification, and parameterized SQLite persistence
/// for the admin cheat terminal.
/// </summary>
public static class AdminSecurityService
{
    // 16-byte (128-bit) random salt and 32-byte (256-bit) derived key for PBKDF2-HMAC-SHA256
    private const int SaltSizeBytes = 16;
    private const int HashSizeBytes = 32;
    private const int Pbkdf2Iterations = 100000;
    private const string DefaultTestingPin = "1234";

    /// <summary>
    /// Generates a fresh cryptographic salt and returns a Base64-encoded PBKDF2 hash for the given PIN.
    /// </summary>
    public static string CreateSaltedHash(string pin, out string base64Salt)
    {
        if (string.IsNullOrEmpty(pin))
        {
            throw new ArgumentException("PIN cannot be null or empty.", nameof(pin));
        }

        // Generate a unique random salt per PIN creation so identical PINs produce different hashes
        byte[] saltBytes = new byte[SaltSizeBytes];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }

        byte[] hashBytes = ComputePbkdf2Hash(pin, saltBytes, Pbkdf2Iterations, HashSizeBytes);
        base64Salt = Convert.ToBase64String(saltBytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// /// Recomputes the candidate PIN's hash using the stored salt and compares it in constant time.
    /// </summary>
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

        // Reject malformed Base64 strings cleanly instead of throwing an unhandled FormatException
        try
        {
            saltBytes = Convert.FromBase64String(storedSaltBase64);
            expectedHashBytes = Convert.FromBase64String(storedHashBase64);
        }
        catch (FormatException)
        {
            return false;
        }

        // Derive using the stored hash length in case a custom or legacy hash size was stored
        byte[] actualHashBytes = ComputePbkdf2Hash(
            candidatePin,
            saltBytes,
            Pbkdf2Iterations,
            expectedHashBytes.Length);

        return FixedTimeEquals(actualHashBytes, expectedHashBytes);
    }

    /// <summary>
    /// Derives a cryptographic key from the PIN and salt using PBKDF2 with HMAC-SHA256.
    /// </summary>
    private static byte[] ComputePbkdf2Hash(string pin, byte[] salt, int iterations, int outputBytes)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(pin, salt, iterations, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(outputBytes);
        }
    }

    /// <summary>
    /// Compares two byte arrays without short-circuiting to reduce timing side-channel risk.
    /// </summary>
    private static bool FixedTimeEquals(byte[] a, byte[] b)
    {
        if (a == null || b == null || a.Length != b.Length)
        {
            return false;
        }

        // Accumulate XOR differences across all bytes before checking for zero
        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }

        return diff == 0;
    }

    /// <summary>
    /// Ensures the AdminConfig table exists and seeds a salted hash for the default testing PIN if missing.
    /// </summary>
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
                // Store only the hash and salt—never the plaintext PIN
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS AdminConfig (" +
                    "id INTEGER PRIMARY KEY, " +
                    "pin_hash TEXT NOT NULL, " +
                    "pin_salt TEXT NOT NULL);";
                command.ExecuteNonQuery();

                // Do not overwrite an existing admin PIN configuration once initialized
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
                // Use parameterized SQLite values to prevent SQL injection
                insertCmd.CommandText =
                    "INSERT INTO AdminConfig (id, pin_hash, pin_salt) VALUES (1, @hash, @salt);";
                insertCmd.Parameters.Add(new SqliteParameter("@hash", hash));
                insertCmd.Parameters.Add(new SqliteParameter("@salt", salt));
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// Loads the stored hash and salt from SQLite and verifies the candidate PIN.
    /// </summary>
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

    /// <summary>
    /// Replaces the stored PIN hash and salt in SQLite with a newly salted hash for <paramref name="newPin"/>.
    /// </summary>
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
                // Use bound parameters so newPin is never concatenated into SQL
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