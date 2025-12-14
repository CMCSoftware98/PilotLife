using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftMarketplace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "aircraft_dealers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    airport_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    dealer_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    min_inventory = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    max_inventory = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    inventory_refresh_days = table.Column<int>(type: "integer", nullable: false, defaultValue: 7),
                    price_multiplier = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 1.0m),
                    offers_financing = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    financing_down_payment_percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    financing_interest_rate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    min_condition = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    max_condition = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    min_hours = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_hours = table.Column<int>(type: "integer", nullable: false, defaultValue: 10000),
                    reputation_score = table.Column<double>(type: "double precision", nullable: false, defaultValue: 3.0),
                    total_sales = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    last_inventory_refresh = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_dealers", x => x.id);
                    table.ForeignKey(
                        name: "FK_aircraft_dealers_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dealer_inventory",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dealer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    aircraft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registration = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    condition = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    total_flight_minutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_cycles = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    base_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    list_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    is_new = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    has_warranty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    warranty_months = table.Column<int>(type: "integer", nullable: true),
                    avionics_package = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    included_modifications = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    listed_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    is_sold = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    sold_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dealer_inventory", x => x.id);
                    table.ForeignKey(
                        name: "FK_dealer_inventory_aircraft_aircraft_id",
                        column: x => x.aircraft_id,
                        principalTable: "aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dealer_inventory_aircraft_dealers_dealer_id",
                        column: x => x.dealer_id,
                        principalTable: "aircraft_dealers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dealer_inventory_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "aircraft_purchases",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_world_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owned_aircraft_id = table.Column<Guid>(type: "uuid", nullable: false),
                    dealer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    dealer_inventory_id = table.Column<Guid>(type: "uuid", nullable: true),
                    seller_player_world_id = table.Column<Guid>(type: "uuid", nullable: true),
                    purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    down_payment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    trade_in_value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    trade_in_aircraft_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_financed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    loan_id = table.Column<Guid>(type: "uuid", nullable: true),
                    financing_interest_rate = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    financing_term_months = table.Column<int>(type: "integer", nullable: true),
                    monthly_payment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    purchase_location_icao = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    purchased_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    condition_at_purchase = table.Column<int>(type: "integer", nullable: false),
                    flight_minutes_at_purchase = table.Column<int>(type: "integer", nullable: false),
                    included_warranty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    warranty_months = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aircraft_purchases", x => x.id);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_aircraft_dealers_dealer_id",
                        column: x => x.dealer_id,
                        principalTable: "aircraft_dealers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_dealer_inventory_dealer_inventory_id",
                        column: x => x.dealer_inventory_id,
                        principalTable: "dealer_inventory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_owned_aircraft_owned_aircraft_id",
                        column: x => x.owned_aircraft_id,
                        principalTable: "owned_aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_owned_aircraft_trade_in_aircraft_id",
                        column: x => x.trade_in_aircraft_id,
                        principalTable: "owned_aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_player_worlds_player_world_id",
                        column: x => x.player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_player_worlds_seller_player_world_id",
                        column: x => x.seller_player_world_id,
                        principalTable: "player_worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_aircraft_purchases_worlds_world_id",
                        column: x => x.world_id,
                        principalTable: "worlds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_dealers_airport_icao",
                table: "aircraft_dealers",
                column: "airport_icao");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_dealers_dealer_type",
                table: "aircraft_dealers",
                column: "dealer_type");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_dealers_world_id",
                table: "aircraft_dealers",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_dealers_world_id_airport_icao",
                table: "aircraft_dealers",
                columns: new[] { "world_id", "airport_icao" });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_dealer_id",
                table: "aircraft_purchases",
                column: "dealer_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_dealer_inventory_id",
                table: "aircraft_purchases",
                column: "dealer_inventory_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_owned_aircraft_id",
                table: "aircraft_purchases",
                column: "owned_aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_player_world_id",
                table: "aircraft_purchases",
                column: "player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_player_world_id_purchased_at",
                table: "aircraft_purchases",
                columns: new[] { "player_world_id", "purchased_at" });

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_purchased_at",
                table: "aircraft_purchases",
                column: "purchased_at");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_seller_player_world_id",
                table: "aircraft_purchases",
                column: "seller_player_world_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_trade_in_aircraft_id",
                table: "aircraft_purchases",
                column: "trade_in_aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_aircraft_purchases_world_id",
                table: "aircraft_purchases",
                column: "world_id");

            migrationBuilder.CreateIndex(
                name: "IX_dealer_inventory_aircraft_id",
                table: "dealer_inventory",
                column: "aircraft_id");

            migrationBuilder.CreateIndex(
                name: "IX_dealer_inventory_dealer_id",
                table: "dealer_inventory",
                column: "dealer_id");

            migrationBuilder.CreateIndex(
                name: "IX_dealer_inventory_dealer_id_is_sold",
                table: "dealer_inventory",
                columns: new[] { "dealer_id", "is_sold" });

            migrationBuilder.CreateIndex(
                name: "IX_dealer_inventory_is_sold",
                table: "dealer_inventory",
                column: "is_sold");

            migrationBuilder.CreateIndex(
                name: "IX_dealer_inventory_world_id",
                table: "dealer_inventory",
                column: "world_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aircraft_purchases");

            migrationBuilder.DropTable(
                name: "dealer_inventory");

            migrationBuilder.DropTable(
                name: "aircraft_dealers");
        }
    }
}
