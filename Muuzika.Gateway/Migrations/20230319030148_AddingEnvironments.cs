using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Muuzika.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddingEnvironments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_rooms_code",
                table: "rooms");

            migrationBuilder.AddColumn<int>(
                name: "environment_id",
                table: "servers",
                type: "integer",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "environments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    room_code_length = table.Column<int>(type: "integer", nullable: false),
                    nickname_min_length = table.Column<int>(type: "integer", nullable: false),
                    nickname_max_length = table.Column<int>(type: "integer", nullable: false),
                    min_number_of_rounds = table.Column<int>(type: "integer", nullable: false),
                    max_number_of_rounds = table.Column<int>(type: "integer", nullable: false),
                    default_number_of_rounds = table.Column<int>(type: "integer", nullable: false),
                    max_number_of_players = table.Column<int>(type: "integer", nullable: false),
                    min_round_duration = table.Column<int>(type: "integer", nullable: false),
                    max_round_duration = table.Column<int>(type: "integer", nullable: false),
                    default_round_duration = table.Column<int>(type: "integer", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: true),
                    updated_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_environments", x => x.id);
                    table.ForeignKey(
                        name: "fk_environments_authenticatables_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "authenticatables",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_environments_authenticatables_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "authenticatables",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_servers_environment_id",
                table: "servers",
                column: "environment_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_code",
                table: "rooms",
                column: "code",
                unique: true,
                filter: "not finished");

            migrationBuilder.CreateIndex(
                name: "ix_environments_created_by_id",
                table: "environments",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_environments_name",
                table: "environments",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_environments_updated_by_id",
                table: "environments",
                column: "updated_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_servers_environments_environment_id",
                table: "servers",
                column: "environment_id",
                principalTable: "environments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_servers_environments_environment_id",
                table: "servers");

            migrationBuilder.DropTable(
                name: "environments");

            migrationBuilder.DropIndex(
                name: "ix_servers_environment_id",
                table: "servers");

            migrationBuilder.DropIndex(
                name: "ix_rooms_code",
                table: "rooms");

            migrationBuilder.DropColumn(
                name: "environment_id",
                table: "servers");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_code",
                table: "rooms",
                column: "code",
                unique: true,
                filter: "[Finished] = 0");
        }
    }
}
