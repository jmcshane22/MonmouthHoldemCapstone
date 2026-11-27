using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MH.Capstone.Migrations.Postgres.Migrations.Cache
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnimalApiCache",
                columns: table => new
                {
                    Url = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    QueryParams = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CachedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CachedResponse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnimalApiCache", x => new { x.Url, x.QueryParams });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnimalApiCache");
        }
    }
}
