using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealtorApi.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class _20261101144300_MakePropertyImageUrlNullable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "ImageUrl",
            table: "properties",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(500)",
            oldMaxLength: 500);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // This rollback can fail if properties with a null ImageUrl already exist.
        // No default is introduced because that would fabricate image data.
        migrationBuilder.AlterColumn<string>(
            name: "ImageUrl",
            table: "properties",
            type: "character varying(500)",
            maxLength: 500,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(500)",
            oldMaxLength: 500,
            oldNullable: true);
    }
}
