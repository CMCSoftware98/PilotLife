using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddReputationAndSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    offers_starter_loan = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    starter_loan_max_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 250000m),
                    starter_loan_interest_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false, defaultValue: 0.015m),
                    base_interest_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false, defaultValue: 0.02m),
                    max_interest_rate = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false, defaultValue: 0.08m),
                    min_credit_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 500),
                    max_loan_to_net_worth_ratio = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 3.0m),
                    min_down_payment_percent = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 0.05m),
                    max_term_months = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    days_to_default = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    late_payment_fee_percent = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: false, defaultValue: 0.05m),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banks", x => x.id);
                    table.ForeignKey(
                        name: "FK_banks_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerWorldId = table.Column<Guid>(type: "uuid", nullable: false),
                    SkillType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CurrentXp = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    LastUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player_skills", x => x.id);
                    table.ForeignKey(
                        name: "FK_player_skills_player_worlds_PlayerWorldId",
                        column: x => x.PlayerWorldId,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reputation_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerWorldId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PointChange = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    ResultingScore = table.Column<decimal>(type: "numeric(10,4)", precision: 10, scale: 4, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RelatedJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedFlightId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reputation_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_reputation_events_jobs_RelatedJobId",
                        column: x => x.RelatedJobId,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_reputation_events_player_worlds_PlayerWorldId",
                        column: x => x.PlayerWorldId,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reputation_events_tracked_flights_RelatedFlightId",
                        column: x => x.RelatedFlightId,
                        principalTable: "tracked_flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "loans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    principal_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_rate_per_month = table.Column<decimal>(type: "numeric(8,6)", precision: 8, scale: 6, nullable: false),
                    term_months = table.Column<int>(type: "integer", nullable: false),
                    monthly_payment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    total_repayment_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    remaining_principal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    accrued_interest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    total_paid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    payments_made = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    payments_remaining = table.Column<int>(type: "integer", nullable: false),
                    late_payment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    missed_payment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    next_payment_due = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    disbursed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    paid_off_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    defaulted_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    collateral_aircraft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purpose = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loans", x => x.id);
                    table.ForeignKey(
                        name: "FK_loans_banks_bank_id",
                        column: x => x.bank_id,
                        principalTable: "banks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_loans_owned_aircraft_collateral_aircraft_id",
                        column: x => x.collateral_aircraft_id,
                        principalTable: "owned_aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_loans_player_worlds_player_world_id",
                        column: x => x.player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_loans_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "skill_xp_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerSkillId = table.Column<Guid>(type: "uuid", nullable: false),
                    XpGained = table.Column<int>(type: "integer", nullable: false),
                    ResultingXp = table.Column<int>(type: "integer", nullable: false),
                    ResultingLevel = table.Column<int>(type: "integer", nullable: false),
                    CausedLevelUp = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RelatedFlightId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedJobId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skill_xp_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_skill_xp_events_jobs_RelatedJobId",
                        column: x => x.RelatedJobId,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_skill_xp_events_player_skills_PlayerSkillId",
                        column: x => x.PlayerSkillId,
                        principalTable: "player_skills",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_skill_xp_events_tracked_flights_RelatedFlightId",
                        column: x => x.RelatedFlightId,
                        principalTable: "tracked_flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "credit_score_events",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    score_before = table.Column<int>(type: "integer", nullable: false),
                    score_after = table.Column<int>(type: "integer", nullable: false),
                    score_change = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    related_loan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_job_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credit_score_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_credit_score_events_jobs_related_job_id",
                        column: x => x.related_job_id,
                        principalTable: "jobs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_credit_score_events_loans_related_loan_id",
                        column: x => x.related_loan_id,
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_credit_score_events_player_worlds_player_world_id",
                        column: x => x.player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_credit_score_events_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "loan_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_number = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    principal_portion = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    interest_portion = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    late_fee = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    remaining_balance_after = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    due_date = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    paid_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_late = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loan_payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_loan_payments_loans_loan_id",
                        column: x => x.loan_id,
                        principalTable: "loans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_loan_payments_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_banks_world_id",
                table: "banks",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_banks_world_id_is_active",
                table: "banks",
                columns: new[] { "world_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_credit_score_events_event_type",
                table: "credit_score_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_credit_score_events_player_world_id",
                table: "credit_score_events",
                column: "player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_score_events_player_world_id_created_at",
                table: "credit_score_events",
                columns: new[] { "player_world_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_credit_score_events_related_job_id",
                table: "credit_score_events",
                column: "related_job_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_score_events_related_loan_id",
                table: "credit_score_events",
                column: "related_loan_id");

            migrationBuilder.CreateIndex(
                name: "IX_credit_score_events_world_id",
                table: "credit_score_events",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_loan_payments_loan_id",
                table: "loan_payments",
                column: "loan_id");

            migrationBuilder.CreateIndex(
                name: "IX_loan_payments_loan_id_payment_number",
                table: "loan_payments",
                columns: new[] { "loan_id", "payment_number" });

            migrationBuilder.CreateIndex(
                name: "IX_loan_payments_paid_at",
                table: "loan_payments",
                column: "paid_at");

            migrationBuilder.CreateIndex(
                name: "IX_loan_payments_world_id",
                table: "loan_payments",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_bank_id",
                table: "loans",
                column: "bank_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_collateral_aircraft_id",
                table: "loans",
                column: "collateral_aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_loan_type",
                table: "loans",
                column: "loan_type");

            migrationBuilder.CreateIndex(
                name: "IX_loans_next_payment_due",
                table: "loans",
                column: "next_payment_due");

            migrationBuilder.CreateIndex(
                name: "IX_loans_player_world_id",
                table: "loans",
                column: "player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_player_world_id_status",
                table: "loans",
                columns: new[] { "player_world_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_loans_status",
                table: "loans",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_loans_world_id",
                table: "loans",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_loans_world_id_status",
                table: "loans",
                columns: new[] { "world_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_player_skills_PlayerWorldId",
                table: "player_skills",
                column: "PlayerWorldId");

            migrationBuilder.CreateIndex(
                name: "IX_player_skills_PlayerWorldId_SkillType",
                table: "player_skills",
                columns: new[] { "PlayerWorldId", "SkillType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_skills_SkillType",
                table: "player_skills",
                column: "SkillType");

            migrationBuilder.CreateIndex(
                name: "IX_reputation_events_EventType",
                table: "reputation_events",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_reputation_events_OccurredAt",
                table: "reputation_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_reputation_events_PlayerWorldId",
                table: "reputation_events",
                column: "PlayerWorldId");

            migrationBuilder.CreateIndex(
                name: "IX_reputation_events_PlayerWorldId_OccurredAt",
                table: "reputation_events",
                columns: new[] { "PlayerWorldId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_reputation_events_RelatedFlightId",
                table: "reputation_events",
                column: "RelatedFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_reputation_events_RelatedJobId",
                table: "reputation_events",
                column: "RelatedJobId");

            migrationBuilder.CreateIndex(
                name: "IX_skill_xp_events_OccurredAt",
                table: "skill_xp_events",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_skill_xp_events_PlayerSkillId",
                table: "skill_xp_events",
                column: "PlayerSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_skill_xp_events_PlayerSkillId_OccurredAt",
                table: "skill_xp_events",
                columns: new[] { "PlayerSkillId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_skill_xp_events_RelatedFlightId",
                table: "skill_xp_events",
                column: "RelatedFlightId");

            migrationBuilder.CreateIndex(
                name: "IX_skill_xp_events_RelatedJobId",
                table: "skill_xp_events",
                column: "RelatedJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credit_score_events");

            migrationBuilder.DropTable(
                name: "loan_payments");

            migrationBuilder.DropTable(
                name: "reputation_events");

            migrationBuilder.DropTable(
                name: "skill_xp_events");

            migrationBuilder.DropTable(
                name: "loans");

            migrationBuilder.DropTable(
                name: "player_skills");

            migrationBuilder.DropTable(
                name: "banks");
        }
    }
}
