using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PilotLife.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAircraftFileData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AircraftCfgRaw",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgAtcAirline",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgAtcId",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgAtcModel",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgAtcType",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgCategory",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgEditable",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgGeneralAtcModel",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgGeneralAtcType",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgIcaoAirline",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgModel",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgPanel",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgPerformance",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgSound",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgTexture",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgTitle",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgUiManufacturer",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgUiType",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CfgUiVariation",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestContentId",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestContentType",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestCreator",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestJsonRaw",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestManufacturer",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestMinimumGameVersion",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestPackageVersion",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestTitle",
                table: "aircraft_requests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManifestTotalPackageSize",
                table: "aircraft_requests",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AircraftCfgRaw",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgAtcAirline",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgAtcId",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgAtcModel",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgAtcType",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgCategory",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgEditable",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgGeneralAtcModel",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgGeneralAtcType",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgIcaoAirline",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgModel",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgPanel",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgPerformance",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgSound",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgTexture",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgTitle",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgUiManufacturer",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgUiType",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "CfgUiVariation",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestContentId",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestContentType",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestCreator",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestJsonRaw",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestManufacturer",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestMinimumGameVersion",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestPackageVersion",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestTitle",
                table: "aircraft_requests");

            migrationBuilder.DropColumn(
                name: "ManifestTotalPackageSize",
                table: "aircraft_requests");
        }
    }
}
