using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Muuzika.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "authenticatables",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<string>(type: "text", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: true),
                    updated_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_authenticatables", x => x.id);
                    table.ForeignKey(
                        name: "fk_authenticatables_authenticatables_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "authenticatables",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_authenticatables_authenticatables_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "authenticatables",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servers", x => x.id);
                    table.ForeignKey(
                        name: "fk_servers_authenticatables_id",
                        column: x => x.id,
                        principalTable: "authenticatables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    last_token_invalidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_users_authenticatables_id",
                        column: x => x.id,
                        principalTable: "authenticatables",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "text", nullable: false),
                    pending = table.Column<bool>(type: "boolean", nullable: false),
                    finished = table.Column<bool>(type: "boolean", nullable: false),
                    server_id = table.Column<int>(type: "integer", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: true),
                    updated_by_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                    table.ForeignKey(
                        name: "fk_rooms_authenticatables_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "authenticatables",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_rooms_authenticatables_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "authenticatables",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_rooms_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_authenticatables_created_by_id",
                table: "authenticatables",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_authenticatables_updated_by_id",
                table: "authenticatables",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_code",
                table: "rooms",
                column: "code",
                unique: true,
                filter: "not finished");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_created_by_id",
                table: "rooms",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_server_id",
                table: "rooms",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "ix_rooms_updated_by_id",
                table: "rooms",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_servers_name",
                table: "servers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "servers");

            migrationBuilder.DropTable(
                name: "authenticatables");
        }
    }
}
