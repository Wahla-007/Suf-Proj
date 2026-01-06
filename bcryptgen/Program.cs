using System;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;

class Program
{
    static string GeneratePassword(int length = 14)
    {
        const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lower = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string symbols = "!@#$%&*()-_=+[]{}<>?";
        string all = upper + lower + digits + symbols;

        var pw = new char[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            // ensure at least one of each required type
            pw[0] = upper[GetRandomInt(rng, upper.Length)];
            pw[1] = lower[GetRandomInt(rng, lower.Length)];
            pw[2] = digits[GetRandomInt(rng, digits.Length)];
            pw[3] = symbols[GetRandomInt(rng, symbols.Length)];
            for (int i = 4; i < length; i++) pw[i] = all[GetRandomInt(rng, all.Length)];
        }
        // shuffle
        var rnd = new Random();
        for (int i = pw.Length - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            var tmp = pw[i]; pw[i] = pw[j]; pw[j] = tmp;
        }
        return new string(pw);
    }

    static int GetRandomInt(RandomNumberGenerator rng, int max)
    {
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var val = Math.Abs(BitConverter.ToInt32(bytes, 0));
        return val % max;
    }

    static void Main(string[] args)
    {
        if (args.Length >= 1 && args[0] == "gen-fixed")
        {
            var admin = BCrypt.Net.BCrypt.HashPassword("Admin@123");
            var teacher = BCrypt.Net.BCrypt.HashPassword("Teacher@123");
            System.IO.File.WriteAllText("hashes.txt", $"ADMIN:{admin}\nTEACHER:{teacher}");
            Console.WriteLine("Done writing to hashes.txt");
            return;
        }
        if (args.Length >= 3 && args[0] == "verify")
        {
            var password = args[1];
            var h = args[2];
            Console.WriteLine("VERIFY => password: '***' , hash starts: " + (h?.Length > 20 ? h.Substring(0, 20) : h));
            Console.WriteLine("Result: " + BCrypt.Net.BCrypt.Verify(password, h));
            return;
        }

        if (args.Length >= 3 && args[0] == "apply")
        {
            var email = args[1];
            var h = args[2];
            Console.WriteLine($"Applying hash for {email} (hash length {h?.Length})");
            var connString = "Server=(localdb)\\MSSQLLocalDB;Database=mess;Trusted_Connection=True;TrustServerCertificate=True;";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE AspNetUsers SET PasswordHash = @h, IsPasswordChanged = 0 WHERE Email = @e; SELECT @@ROWCOUNT;";
                    cmd.Parameters.Add(new SqlParameter("@h", (object)h ?? DBNull.Value));
                    cmd.Parameters.Add(new SqlParameter("@e", email));
                    var rows = cmd.ExecuteScalar();
                    Console.WriteLine("Rows affected: " + rows);
                }
            }
            return;
        }

        if (args.Length >= 1 && args[0] == "fix-defaults")
        {
            var connString = "Server=(localdb)\\MSSQLLocalDB;Database=mess;Trusted_Connection=True;TrustServerCertificate=True;";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    // Drop existing default constraint on WeeklyMenu.CreatedAt if exists
                    cmd.CommandText = @"DECLARE @df nvarchar(128); SELECT @df = df.name FROM sys.default_constraints df JOIN sys.columns c ON df.parent_object_id = c.object_id AND df.parent_column_id = c.column_id WHERE OBJECT_NAME(df.parent_object_id) = 'WeeklyMenu' AND c.name = 'CreatedAt'; IF @df IS NOT NULL EXEC('ALTER TABLE WeeklyMenu DROP CONSTRAINT ' + @df);";
                    cmd.ExecuteNonQuery();
                    // Add clean default
                    cmd.CommandText = "ALTER TABLE WeeklyMenu ADD CONSTRAINT DF_WeeklyMenu_CreatedAt DEFAULT GETDATE() FOR CreatedAt;";
                    try { cmd.ExecuteNonQuery(); Console.WriteLine("WeeklyMenu CreatedAt default fixed."); } catch (Exception ex) { Console.WriteLine("Warning: " + ex.Message); }

                    // MonthlyBill GeneratedOn default
                    cmd.CommandText = @"DECLARE @df2 nvarchar(128); SELECT @df2 = df.name FROM sys.default_constraints df JOIN sys.columns c ON df.parent_object_id = c.object_id AND df.parent_column_id = c.column_id WHERE OBJECT_NAME(df.parent_object_id) = 'MonthlyBill' AND c.name = 'GeneratedOn'; IF @df2 IS NOT NULL EXEC('ALTER TABLE MonthlyBill DROP CONSTRAINT ' + @df2);";
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "ALTER TABLE MonthlyBill ADD CONSTRAINT DF_MonthlyBill_GeneratedOn DEFAULT GETDATE() FOR GeneratedOn;";
                    try { cmd.ExecuteNonQuery(); Console.WriteLine("MonthlyBill GeneratedOn default fixed."); } catch (Exception ex) { Console.WriteLine("Warning: " + ex.Message); }
                }
            }
            return;
        }

        if (args.Length >= 6 && args[0] == "insert-menu")
        {
            var weekStart = args[1];
            var breakfast = Decimal.Parse(args[2]);
            var lunch = Decimal.Parse(args[3]);
            var dinner = Decimal.Parse(args[4]);
            var createdBy = args[5];

            var connString = "Server=(localdb)\\MSSQLLocalDB;Database=mess;Trusted_Connection=True;TrustServerCertificate=True;";
            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO WeeklyMenu (WeekStartDate, BreakfastRate, LunchRate, DinnerRate, CreatedById) OUTPUT INSERTED.ID, INSERTED.CreatedAt VALUES (@d, @b, @l, @di, @c);";
                    cmd.Parameters.Add(new SqlParameter("@d", weekStart));
                    cmd.Parameters.Add(new SqlParameter("@b", breakfast));
                    cmd.Parameters.Add(new SqlParameter("@l", lunch));
                    cmd.Parameters.Add(new SqlParameter("@di", dinner));
                    cmd.Parameters.Add(new SqlParameter("@c", createdBy));
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Inserted Id={reader.GetInt32(0)}, CreatedAt={reader.GetDateTime(1)}");
                        }
                    }
                }
            }
            return;
        }

        var temp = GeneratePassword(14);
        var hash = BCrypt.Net.BCrypt.HashPassword(temp);
        Console.WriteLine("TEMP_PASSWORD=" + temp);
        Console.WriteLine("BCRYPT_HASH=" + hash);
    }
}
