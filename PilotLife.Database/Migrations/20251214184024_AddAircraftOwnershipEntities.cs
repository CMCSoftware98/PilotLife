using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftOwnershipEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "owned_aircraft",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    nickname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    condition = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    total_flight_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_cycles = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    hours_since_last_inspection = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_location_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    last_moved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_in_maintenance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_in_use = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_listed_for_rental = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_listed_for_sale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    purchased_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    was_purchased_new = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    has_warranty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    warranty_expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    has_insurance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    insurance_expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    insurance_premium = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_owned_aircraft", x => x.id);
                    table.ForeignKey(
                        name: "FK_owned_aircraft_aircraft_aircraft_id",
                        column: x => x.aircraft_id,
                        principalTable: "aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_owned_aircraft_player_worlds_player_world_id",
                        column: x => x.player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_owned_aircraft_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_components",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owned_aircraft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    component_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    serial_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    manufacturer = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    condition = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    operating_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    cycles = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    time_since_overhaul = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    time_between_overhaul = table.Column<int>(type: "integer", nullable: true),
                    is_life_limited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    life_limit_minutes = table.Column<int>(type: "integer", nullable: true),
                    life_limit_cycles = table.Column<int>(type: "integer", nullable: true),
                    installed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    aircraft_hours_at_install = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_serviceable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_components", x => x.id);
                    table.ForeignKey(
                        name: "FK_aircraft_components_owned_aircraft_owned_aircraft_id",
                        column: x => x.owned_aircraft_id,
                        principalTable: "owned_aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aircraft_components_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_modifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owned_aircraft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    modification_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    installed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    installation_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    installed_at_airport = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_removable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    removal_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    cargo_capacity_change_lbs = table.Column<double>(type: "double precision", nullable: false),
                    passenger_capacity_change = table.Column<int>(type: "integer", nullable: false),
                    range_change_nm = table.Column<double>(type: "double precision", nullable: false),
                    cruise_speed_change_kts = table.Column<double>(type: "double precision", nullable: false),
                    fuel_consumption_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    takeoff_distance_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    landing_distance_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    empty_weight_change_lbs = table.Column<double>(type: "double precision", nullable: false),
                    max_gross_weight_change_lbs = table.Column<double>(type: "double precision", nullable: false),
                    enables_water_operations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enables_snow_operations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enables_stol_operations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enables_ifr_operations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enables_night_operations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enables_known_icing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    additional_maintenance_cost_per_hour = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    special_inspection_interval_minutes = table.Column<int>(type: "integer", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_modifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_aircraft_modifications_owned_aircraft_owned_aircraft_id",
                        column: x => x.owned_aircraft_id,
                        principalTable: "owned_aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aircraft_modifications_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "maintenance_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owned_aircraft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_component_id = table.Column<Guid>(type: "uuid", nullable: true),
                    maintenance_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    estimated_duration_hours = table.Column<int>(type: "integer", nullable: false),
                    actual_duration_hours = table.Column<int>(type: "integer", nullable: true),
                    aircraft_flight_minutes_at_service = table.Column<int>(type: "integer", nullable: false),
                    aircraft_cycles_at_service = table.Column<int>(type: "integer", nullable: false),
                    performed_at_airport = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    facility_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    labor_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    parts_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    covered_by_warranty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    covered_by_insurance = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    condition_improvement = table.Column<int>(type: "integer", nullable: false),
                    resulting_condition = table.Column<int>(type: "integer", nullable: true),
                    parts_replaced = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    squawks_found = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    deferred_items = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    airworthiness_directive_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    service_bulletin_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mechanic_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    mechanic_license = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    inspector_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    next_service_due_minutes = table.Column<int>(type: "integer", nullable: true),
                    next_service_due_date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_maintenance_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_maintenance_logs_aircraft_components_aircraft_component_id",
                        column: x => x.aircraft_component_id,
                        principalTable: "aircraft_components",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_maintenance_logs_owned_aircraft_owned_aircraft_id",
                        column: x => x.owned_aircraft_id,
                        principalTable: "owned_aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_maintenance_logs_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_components_owned_aircraft_id",
                table: "aircraft_components",
                column: "owned_aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_components_owned_aircraft_id_component_type",
                table: "aircraft_components",
                columns: new[] { "owned_aircraft_id", "component_type" });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_components_world_id",
                table: "aircraft_components",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_modifications_owned_aircraft_id",
                table: "aircraft_modifications",
                column: "owned_aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_modifications_owned_aircraft_id_modification_type",
                table: "aircraft_modifications",
                columns: new[] { "owned_aircraft_id", "modification_type" });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_modifications_world_id",
                table: "aircraft_modifications",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_logs_aircraft_component_id",
                table: "maintenance_logs",
                column: "aircraft_component_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_logs_maintenance_type",
                table: "maintenance_logs",
                column: "maintenance_type");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_logs_owned_aircraft_id",
                table: "maintenance_logs",
                column: "owned_aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_logs_owned_aircraft_id_started_at",
                table: "maintenance_logs",
                columns: new[] { "owned_aircraft_id", "started_at" });

            migrationBuilder.CreateIndex(
                name: "IX_maintenance_logs_world_id",
                table: "maintenance_logs",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_owned_aircraft_aircraft_id",
                table: "owned_aircraft",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_owned_aircraft_current_location_icao",
                table: "owned_aircraft",
                column: "current_location_icao");

            migrationBuilder.CreateIndex(
                name: "IX_owned_aircraft_player_world_id",
                table: "owned_aircraft",
                column: "player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_owned_aircraft_world_id",
                table: "owned_aircraft",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_owned_aircraft_world_id_player_world_id",
                table: "owned_aircraft",
                columns: new[] { "world_id", "player_world_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_modifications");

            migrationBuilder.DropTable(
                name: "maintenance_logs");

            migrationBuilder.DropTable(
                name: "aircraft_components");

            migrationBuilder.DropTable(
                name: "owned_aircraft");
        }
    }
}
