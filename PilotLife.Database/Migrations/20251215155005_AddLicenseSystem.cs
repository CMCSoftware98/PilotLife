using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "license_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    base_exam_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    exam_duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    passing_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 70),
                    required_aircraft_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    required_aircraft_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    validity_game_days = table.Column<int>(type: "integer", nullable: true),
                    base_renewal_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    prerequisite_licenses_json = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_exams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    scheduled_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    time_limit_minutes = table.Column<int>(type: "integer", nullable: false),
                    required_aircraft_category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    required_aircraft_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    departure_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    route_json = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    assigned_altitude_ft = table.Column<int>(type: "integer", nullable: true),
                    score = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    passing_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 70),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    examiner_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    attempt_number = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    fee_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    eligible_for_retake_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    flight_time_minutes = table.Column<int>(type: "integer", nullable: true),
                    distance_flown_nm = table.Column<double>(type: "double precision", nullable: true),
                    aircraft_used = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_exams", x => x.id);
                    table.ForeignKey(
                        name: "FK_license_exams_license_types_license_type_id",
                        column: x => x.license_type_id,
                        principalTable: "license_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_license_exams_player_worlds_player_world_id",
                        column: x => x.player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_license_exams_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_checkpoints",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: false),
                    longitude = table.Column<double>(type: "double precision", nullable: false),
                    required_altitude_ft = table.Column<int>(type: "integer", nullable: true),
                    radius_nm = table.Column<double>(type: "double precision", nullable: false, defaultValue: 1.0),
                    was_reached = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    reached_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    altitude_at_reach = table.Column<int>(type: "integer", nullable: true),
                    speed_at_reach_kts = table.Column<int>(type: "integer", nullable: true),
                    points_awarded = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_points = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_checkpoints", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_checkpoints_license_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "license_exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_landings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    airport_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    vertical_speed_fpm = table.Column<float>(type: "real", nullable: false),
                    centerline_deviation_ft = table.Column<float>(type: "real", nullable: false),
                    touchdown_zone_distance_ft = table.Column<float>(type: "real", nullable: false),
                    ground_speed_kts = table.Column<float>(type: "real", nullable: true),
                    pitch_deg = table.Column<float>(type: "real", nullable: true),
                    bank_deg = table.Column<float>(type: "real", nullable: true),
                    gear_down = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    runway_used = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    points_awarded = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_points = table.Column<int>(type: "integer", nullable: false),
                    landed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_landings", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_landings_license_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "license_exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_maneuvers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    maneuver_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    max_points = table.Column<int>(type: "integer", nullable: false),
                    points_awarded = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    result = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NotAttempted"),
                    altitude_tolerance_ft = table.Column<int>(type: "integer", nullable: true),
                    heading_tolerance_deg = table.Column<int>(type: "integer", nullable: true),
                    speed_tolerance_kts = table.Column<int>(type: "integer", nullable: true),
                    altitude_deviation_ft = table.Column<int>(type: "integer", nullable: true),
                    heading_deviation_deg = table.Column<int>(type: "integer", nullable: true),
                    speed_deviation_kts = table.Column<int>(type: "integer", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_maneuvers", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_maneuvers_license_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "license_exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_violations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    occurred_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    value = table.Column<float>(type: "real", nullable: false),
                    threshold = table.Column<float>(type: "real", nullable: false),
                    points_deducted = table.Column<int>(type: "integer", nullable: false),
                    caused_failure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    latitude_at_violation = table.Column<double>(type: "double precision", nullable: false),
                    longitude_at_violation = table.Column<double>(type: "double precision", nullable: false),
                    altitude_at_violation = table.Column<int>(type: "integer", nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_violations", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_violations_license_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "license_exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_licenses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    license_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    earned_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    last_renewed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    renewal_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    revocation_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    exam_score = table.Column<int>(type: "integer", nullable: false),
                    exam_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    total_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    passed_exam_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_licenses", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_licenses_license_exams_passed_exam_id",
                        column: x => x.passed_exam_id,
                        principalTable: "license_exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_licenses_license_types_license_type_id",
                        column: x => x.license_type_id,
                        principalTable: "license_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_licenses_player_worlds_player_world_id",
                        column: x => x.player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_checkpoints_exam_id",
                table: "exam_checkpoints",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_checkpoints_exam_id_order",
                table: "exam_checkpoints",
                columns: new[] { "exam_id", "order" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_landings_exam_id",
                table: "exam_landings",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_landings_exam_id_order",
                table: "exam_landings",
                columns: new[] { "exam_id", "order" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_maneuvers_exam_id",
                table: "exam_maneuvers",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_maneuvers_exam_id_order",
                table: "exam_maneuvers",
                columns: new[] { "exam_id", "order" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_violations_exam_id",
                table: "exam_violations",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_violations_exam_id_occurred_at",
                table: "exam_violations",
                columns: new[] { "exam_id", "occurred_at" });

            migrationBuilder.CreateIndex(
                name: "IX_exam_violations_type",
                table: "exam_violations",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_license_exams_license_type_id",
                table: "license_exams",
                column: "license_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_license_exams_player_world_id",
                table: "license_exams",
                column: "player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_license_exams_player_world_id_license_type_id_status",
                table: "license_exams",
                columns: new[] { "player_world_id", "license_type_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_license_exams_player_world_id_status",
                table: "license_exams",
                columns: new[] { "player_world_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_license_exams_status",
                table: "license_exams",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_license_exams_world_id",
                table: "license_exams",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_license_types_category",
                table: "license_types",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_license_types_code",
                table: "license_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_license_types_display_order",
                table: "license_types",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "IX_license_types_is_active",
                table: "license_types",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_user_licenses_expires_at",
                table: "user_licenses",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_licenses_is_valid",
                table: "user_licenses",
                column: "is_valid");

            migrationBuilder.CreateIndex(
                name: "IX_user_licenses_license_type_id",
                table: "user_licenses",
                column: "license_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_licenses_passed_exam_id",
                table: "user_licenses",
                column: "passed_exam_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_licenses_player_world_id",
                table: "user_licenses",
                column: "player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_licenses_player_world_id_license_type_id",
                table: "user_licenses",
                columns: new[] { "player_world_id", "license_type_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_checkpoints");

            migrationBuilder.DropTable(
                name: "exam_landings");

            migrationBuilder.DropTable(
                name: "exam_maneuvers");

            migrationBuilder.DropTable(
                name: "exam_violations");

            migrationBuilder.DropTable(
                name: "user_licenses");

            migrationBuilder.DropTable(
                name: "license_exams");

            migrationBuilder.DropTable(
                name: "license_types");
        }
    }
}
