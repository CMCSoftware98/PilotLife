using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAirportsAndJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "balance",
                table: "users",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "current_airport_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "home_airport_id",
                table: "users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_flight_minutes",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "airports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ident = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    iata_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    elevation_ft = table.Column<int>(type: "integer", nullable: true),
                    country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    municipality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_airports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    departure_airport_id = table.Column<int>(type: "integer", nullable: false),
                    arrival_airport_id = table.Column<int>(type: "integer", nullable: false),
                    cargo_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false),
                    payout = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    distance_nm = table.Column<double>(type: "double precision", nullable: false),
                    estimated_flight_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    required_aircraft_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_jobs_airports_arrival_airport_id",
                        column: x => x.arrival_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jobs_airports_departure_airport_id",
                        column: x => x.departure_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_jobs_users_assigned_to_user_id",
                        column: x => x.assigned_to_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_current_airport_id",
                table: "users",
                column: "current_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_home_airport_id",
                table: "users",
                column: "home_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_airports_iata_code",
                table: "airports",
                column: "iata_code");

            migrationBuilder.CreateIndex(
                name: "IX_airports_ident",
                table: "airports",
                column: "ident",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_airports_name",
                table: "airports",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_arrival_airport_id",
                table: "jobs",
                column: "arrival_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_assigned_to_user_id",
                table: "jobs",
                column: "assigned_to_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_departure_airport_id",
                table: "jobs",
                column: "departure_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_is_completed",
                table: "jobs",
                column: "is_completed");

            migrationBuilder.AddForeignKey(
                name: "FK_users_airports_current_airport_id",
                table: "users",
                column: "current_airport_id",
                principalTable: "airports",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_users_airports_home_airport_id",
                table: "users",
                column: "home_airport_id",
                principalTable: "airports",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_airports_current_airport_id",
                table: "users");

            migrationBuilder.DropForeignKey(
                name: "FK_users_airports_home_airport_id",
                table: "users");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "airports");

            migrationBuilder.DropIndex(
                name: "IX_users_current_airport_id",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_home_airport_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "balance",
                table: "users");

            migrationBuilder.DropColumn(
                name: "current_airport_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "home_airport_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "total_flight_minutes",
                table: "users");
        }
    }
}
