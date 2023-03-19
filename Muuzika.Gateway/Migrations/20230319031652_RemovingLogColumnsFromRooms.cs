using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muuzika.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class RemovingLogColumnsFromRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_rooms_authenticatables_created_by_id",
                table: "rooms");

            migrationBuilder.DropForeignKey(
                name: "fk_rooms_authenticatables_updated_by_id",
                table: "rooms");

            migrationBuilder.DropIndex(
                name: "ix_rooms_created_by_id",
                table: "rooms");

            migrationBuilder.DropIndex(
                name: "ix_rooms_updated_by_id",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "created_by_id",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "updated_by_id",
                table: "rooms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "created_by_id",
                table: "rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "updated_by_id",
                table: "rooms",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_rooms_created_by_id",
                table: "rooms",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_updated_by_id",
                table: "rooms",
                column: "updated_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_rooms_authenticatables_created_by_id",
                table: "rooms",
                column: "created_by_id",
                principalTable: "authenticatables",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_rooms_authenticatables_updated_by_id",
                table: "rooms",
                column: "updated_by_id",
                principalTable: "authenticatables",
                principalColumn: "id");
        }
    }
}
