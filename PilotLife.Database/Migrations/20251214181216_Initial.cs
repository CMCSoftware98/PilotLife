using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aircraft",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft", x => x.id);
                });

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
                name: "roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_system_role = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_global = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "worlds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    difficulty = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    starting_capital = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    job_payout_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    aircraft_price_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    maintenance_cost_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    license_cost_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    loan_interest_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    detection_risk_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    fine_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    job_expiry_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    credit_recovery_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    worker_salary_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    max_players = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_worlds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    experience_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    email_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    newsletter_subscribed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    current_airport_id = table.Column<int>(type: "integer", nullable: true),
                    home_airport_id = table.Column<int>(type: "integer", nullable: true),
                    balance = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    total_flight_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_airports_current_airport_id",
                        column: x => x.current_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_users_airports_home_airport_id",
                        column: x => x.home_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_granted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.role_id, x.permission });
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    departure_airport_id = table.Column<int>(type: "integer", nullable: false),
                    arrival_airport_id = table.Column<int>(type: "integer", nullable: false),
                    cargo_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false),
                    payout = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    distance_nm = table.Column<double>(type: "double precision", nullable: false),
                    estimated_flight_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    required_aircraft_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    assigned_to_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "player_worlds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    credit_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 650),
                    total_flight_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_flights = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_jobs_completed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_earnings = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_spent = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    reputation_score = table.Column<decimal>(type: "numeric(3,1)", precision: 3, scale: 1, nullable: false, defaultValue: 3.0m),
                    on_time_deliveries = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    late_deliveries = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    failed_deliveries = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    violation_points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_violation_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    last_active_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    current_airport_id = table.Column<int>(type: "integer", nullable: true),
                    home_airport_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_worlds", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_worlds_airports_current_airport_id",
                        column: x => x.current_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_player_worlds_airports_home_airport_id",
                        column: x => x.home_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_player_worlds_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_player_worlds_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    replaced_by_token = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tracked_flights",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    aircraft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    aircraft_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    aircraft_icao_type = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    departure_airport_id = table.Column<int>(type: "integer", nullable: true),
                    departure_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    departure_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    arrival_airport_id = table.Column<int>(type: "integer", nullable: true),
                    arrival_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    arrival_time = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    current_latitude = table.Column<double>(type: "double precision", nullable: true),
                    current_longitude = table.Column<double>(type: "double precision", nullable: true),
                    current_altitude = table.Column<double>(type: "double precision", nullable: true),
                    current_heading = table.Column<double>(type: "double precision", nullable: true),
                    current_ground_speed = table.Column<double>(type: "double precision", nullable: true),
                    last_position_update = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    flight_time_minutes = table.Column<int>(type: "integer", nullable: false),
                    distance_nm = table.Column<double>(type: "double precision", nullable: false),
                    max_altitude = table.Column<double>(type: "double precision", nullable: false),
                    max_ground_speed = table.Column<double>(type: "double precision", nullable: false),
                    hard_landing_count = table.Column<int>(type: "integer", nullable: false),
                    overspeed_count = table.Column<int>(type: "integer", nullable: false),
                    stall_warning_count = table.Column<int>(type: "integer", nullable: false),
                    landing_rate = table.Column<double>(type: "double precision", nullable: true),
                    fuel_used_gallons = table.Column<double>(type: "double precision", nullable: true),
                    start_fuel_gallons = table.Column<double>(type: "double precision", nullable: true),
                    end_fuel_gallons = table.Column<double>(type: "double precision", nullable: true),
                    payload_weight_lbs = table.Column<double>(type: "double precision", nullable: true),
                    total_weight_lbs = table.Column<double>(type: "double precision", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    connector_session_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracked_flights", x => x.id);
                    table.ForeignKey(
                        name: "FK_tracked_flights_aircraft_aircraft_id",
                        column: x => x.aircraft_id,
                        principalTable: "aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tracked_flights_airports_arrival_airport_id",
                        column: x => x.arrival_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tracked_flights_airports_departure_airport_id",
                        column: x => x.departure_airport_id,
                        principalTable: "airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tracked_flights_users_UserId1",
                        column: x => x.UserId1,
                        principalTable: "users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracked_flights_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: true),
                    granted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    granted_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_granted_by_user_id",
                        column: x => x.granted_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "world_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_payout_multiplier_override = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    aircraft_price_multiplier_override = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    maintenance_cost_multiplier_override = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    allow_new_players = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    allow_illegal_cargo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    enable_auctions = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    enable_ai_crews = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    enable_aircraft_rental = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    max_aircraft_per_player = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_loans_per_player = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    max_workers_per_player = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    max_active_jobs_per_player = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    require_approval_to_join = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    enable_chat = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    enable_reporting = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_modified_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_world_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_world_settings_users_last_modified_by_user_id",
                        column: x => x.last_modified_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_world_settings_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_financials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracked_flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_revenue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    on_time_bonus = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    landing_bonus = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fuel_efficiency_bonus = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    other_bonuses = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    fuel_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    landing_fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    handling_fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    navigation_fees = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    maintenance_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    insurance_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    crew_cost = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    late_penalty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    damage_penalty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    incident_penalty = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_financials", x => x.id);
                    table.ForeignKey(
                        name: "FK_flight_financials_tracked_flights_tracked_flight_id",
                        column: x => x.tracked_flight_id,
                        principalTable: "tracked_flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "flight_jobs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracked_flight_id = table.Column<Guid>(type: "uuid", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_failed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    failure_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actual_payout = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    xp_earned = table.Column<int>(type: "integer", nullable: false),
                    reputation_change = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_flight_jobs", x => x.id);
                    table.ForeignKey(
                        name: "FK_flight_jobs_jobs_job_id",
                        column: x => x.job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_flight_jobs_tracked_flights_tracked_flight_id",
                        column: x => x.tracked_flight_id,
                        principalTable: "tracked_flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "IX_flight_financials_tracked_flight_id",
                table: "flight_financials",
                column: "tracked_flight_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flight_jobs_job_id",
                table: "flight_jobs",
                column: "job_id");

            migrationBuilder.CreateIndex(
                name: "IX_flight_jobs_tracked_flight_id",
                table: "flight_jobs",
                column: "tracked_flight_id");

            migrationBuilder.CreateIndex(
                name: "IX_flight_jobs_tracked_flight_id_job_id",
                table: "flight_jobs",
                columns: new[] { "tracked_flight_id", "job_id" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_player_worlds_current_airport_id",
                table: "player_worlds",
                column: "current_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_worlds_home_airport_id",
                table: "player_worlds",
                column: "home_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_worlds_user_id",
                table: "player_worlds",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_player_worlds_user_id_world_id",
                table: "player_worlds",
                columns: new[] { "user_id", "world_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_worlds_world_id",
                table: "player_worlds",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_aircraft_id",
                table: "tracked_flights",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_arrival_airport_id",
                table: "tracked_flights",
                column: "arrival_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_connector_session_id",
                table: "tracked_flights",
                column: "connector_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_departure_airport_id",
                table: "tracked_flights",
                column: "departure_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_state",
                table: "tracked_flights",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_user_id",
                table: "tracked_flights",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_user_id_state",
                table: "tracked_flights",
                columns: new[] { "user_id", "state" });

            migrationBuilder.CreateIndex(
                name: "IX_tracked_flights_UserId1",
                table: "tracked_flights",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_granted_by_user_id",
                table: "user_roles",
                column: "granted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id_role_id_world_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id", "world_id" });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_world_id",
                table: "user_roles",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_current_airport_id",
                table: "users",
                column: "current_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_home_airport_id",
                table: "users",
                column: "home_airport_id");

            migrationBuilder.CreateIndex(
                name: "IX_world_settings_last_modified_by_user_id",
                table: "world_settings",
                column: "last_modified_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_world_settings_world_id",
                table: "world_settings",
                column: "world_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_worlds_slug",
                table: "worlds",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_requests");

            migrationBuilder.DropTable(
                name: "flight_financials");

            migrationBuilder.DropTable(
                name: "flight_jobs");

            migrationBuilder.DropTable(
                name: "player_worlds");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "world_settings");

            migrationBuilder.DropTable(
                name: "jobs");

            migrationBuilder.DropTable(
                name: "tracked_flights");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "worlds");

            migrationBuilder.DropTable(
                name: "aircraft");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "airports");
        }
    }
}
