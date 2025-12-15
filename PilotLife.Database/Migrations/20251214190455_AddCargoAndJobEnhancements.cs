using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCargoAndJobEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "weight",
                table: "jobs");

            migrationBuilder.AlterColumn<string>(
                name: "required_aircraft_type",
                table: "jobs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "cargo_type",
                table: "jobs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "accepted_at",
                table: "jobs",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "actual_payout",
                table: "jobs",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "arrival_icao",
                table: "jobs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "assigned_to_player_world_id",
                table: "jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "base_payout",
                table: "jobs",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "bonus_payout",
                table: "jobs",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cargo_type_id",
                table: "jobs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at",
                table: "jobs",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "departure_icao",
                table: "jobs",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "jobs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "distance_category",
                table: "jobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "failure_reason",
                table: "jobs",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_failed",
                table: "jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "min_crew_count",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "passenger_class",
                table: "jobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "passenger_count",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "required_certification",
                table: "jobs",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requires_special_certification",
                table: "jobs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "risk_level",
                table: "jobs",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "route_difficulty",
                table: "jobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "started_at",
                table: "jobs",
                type: "timestamptz",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "jobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "title",
                table: "jobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "jobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "urgency",
                table: "jobs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "volume_cu_ft",
                table: "jobs",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "weight_lbs",
                table: "jobs",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "world_id",
                table: "jobs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "cargo_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subcategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    base_rate_per_lb = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    min_weight_lbs = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    max_weight_lbs = table.Column<int>(type: "integer", nullable: false, defaultValue: 5000),
                    density_factor = table.Column<decimal>(type: "numeric(6,4)", precision: 6, scale: 4, nullable: false, defaultValue: 0.1m),
                    requires_special_handling = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    special_handling_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_temperature_sensitive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_time_critical = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_illegal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    illegal_risk_level = table.Column<int>(type: "integer", nullable: true),
                    payout_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo_types", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_assigned_to_player_world_id",
                table: "jobs",
                column: "assigned_to_player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_cargo_type_id",
                table: "jobs",
                column: "cargo_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_status",
                table: "jobs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_world_id",
                table: "jobs",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_jobs_world_id_departure_icao_status",
                table: "jobs",
                columns: new[] { "world_id", "departure_icao", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_jobs_world_id_status",
                table: "jobs",
                columns: new[] { "world_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_cargo_types_category",
                table: "cargo_types",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_types_category_subcategory",
                table: "cargo_types",
                columns: new[] { "category", "subcategory" });

            migrationBuilder.CreateIndex(
                name: "IX_cargo_types_is_active",
                table: "cargo_types",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_types_subcategory",
                table: "cargo_types",
                column: "subcategory");

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_cargo_types_cargo_type_id",
                table: "jobs",
                column: "cargo_type_id",
                principalTable: "cargo_types",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_player_worlds_assigned_to_player_world_id",
                table: "jobs",
                column: "assigned_to_player_world_id",
                principalTable: "player_worlds",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_jobs_worlds_world_id",
                table: "jobs",
                column: "world_id",
                principalTable: "worlds",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_jobs_cargo_types_cargo_type_id",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_jobs_player_worlds_assigned_to_player_world_id",
                table: "jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_jobs_worlds_world_id",
                table: "jobs");

            migrationBuilder.DropTable(
                name: "cargo_types");

            migrationBuilder.DropIndex(
                name: "IX_jobs_assigned_to_player_world_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_cargo_type_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_status",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_world_id",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_world_id_departure_icao_status",
                table: "jobs");

            migrationBuilder.DropIndex(
                name: "IX_jobs_world_id_status",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "actual_payout",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "arrival_icao",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "assigned_to_player_world_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "base_payout",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "bonus_payout",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "cargo_type_id",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "departure_icao",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "description",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "distance_category",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "failure_reason",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "is_failed",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "min_crew_count",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "passenger_class",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "passenger_count",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "required_certification",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "requires_special_certification",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "risk_level",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "route_difficulty",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "started_at",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "status",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "title",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "type",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "urgency",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "volume_cu_ft",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "weight_lbs",
                table: "jobs");

            migrationBuilder.DropColumn(
                name: "world_id",
                table: "jobs");

            migrationBuilder.AlterColumn<string>(
                name: "required_aircraft_type",
                table: "jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "cargo_type",
                table: "jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "weight",
                table: "jobs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
