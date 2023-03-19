using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Muuzika.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class CreateCreateRoomFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
CREATE FUNCTION create_room(server_id INTEGER, length INTEGER, max_attempts INTEGER)
RETURNS TEXT AS $$
DECLARE
    generated_code TEXT;
    attempts INTEGER := 0;
BEGIN
    WHILE attempts < max_attempts LOOP
        generated_code := (SELECT lpad(floor(random() * (10^length)::numeric)::text, length, '0'));
        IF NOT EXISTS (SELECT 1 FROM rooms WHERE code = generated_code) THEN
            INSERT INTO rooms (code, pending, finished, server_id, created_at, updated_at) VALUES (generated_code, true, false, server_id, NOW(), NOW());
            RETURN generated_code;
        END IF;
        attempts := attempts + 1;
    END LOOP;
    RAISE EXCEPTION 'Maximum attempts reached to generate a unique code';
END
$$ LANGUAGE plpgsql
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION create_room");
        }
    }
}
