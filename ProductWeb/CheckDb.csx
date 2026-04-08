using System;
using Npgsql;

var connString = "Host=localhost;Port=5432;Database=ProductCatalogDB;Username=sulaimansaleh;Trust Server Certificate=true;Include Error Detail=true";
using var conn = new NpgsqlConnection(connString);
conn.Open();

using var cmd = new NpgsqlCommand("SELECT \"Id\", \"Username\", \"Role\" FROM \"Users\"", conn);
using var reader = cmd.ExecuteReader();
while (reader.Read()) {
    Console.WriteLine($"{reader[0]} - {reader[1]} - '{reader[2]}'");
}
