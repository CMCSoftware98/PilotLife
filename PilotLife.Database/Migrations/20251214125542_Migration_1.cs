using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class Migration_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aircraft",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    atc_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    atc_model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    category = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    engine_type = table.Column<int>(type: "integer", nullable: false),
                    engine_type_str = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    number_of_engines = table.Column<int>(type: "integer", nullable: false),
                    max_gross_weight_lbs = table.Column<double>(type: "double precision", nullable: false),
                    empty_weight_lbs = table.Column<double>(type: "double precision", nullable: false),
                    cruise_speed_kts = table.Column<double>(type: "double precision", nullable: false),
                    simulator_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    is_approved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    aircraft_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    atc_type = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    atc_model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    category = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    engine_type = table.Column<int>(type: "integer", nullable: false),
                    engine_type_str = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    number_of_engines = table.Column<int>(type: "integer", nullable: false),
                    max_gross_weight_lbs = table.Column<double>(type: "double precision", nullable: false),
                    empty_weight_lbs = table.Column<double>(type: "double precision", nullable: false),
                    cruise_speed_kts = table.Column<double>(type: "double precision", nullable: false),
                    simulator_version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    review_notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    reviewed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_aircraft_requests_users_requested_by_user_id",
                        column: x => x.requested_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aircraft_requests_users_reviewed_by_user_id",
                        column: x => x.reviewed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_title",
                table: "aircraft",
                column: "title",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_requests_aircraft_title",
                table: "aircraft_requests",
                column: "aircraft_title");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_requests_requested_by_user_id",
                table: "aircraft_requests",
                column: "requested_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_requests_reviewed_by_user_id",
                table: "aircraft_requests",
                column: "reviewed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_requests_status",
                table: "aircraft_requests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft");

            migrationBuilder.DropTable(
                name: "aircraft_requests");
        }
    }
}
