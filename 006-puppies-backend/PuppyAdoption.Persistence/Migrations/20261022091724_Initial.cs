using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PuppyAdoption.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "puppy_adoption");

            migrationBuilder.CreateTable(
                name: "Shelters",
                schema: "puppy_adoption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shelters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Puppies",
                schema: "puppy_adoption",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Breed = table.Column<string>(type: "text", nullable: false),
                    DateOfBirth = table.Column<LocalDate>(type: "date", nullable: false),
                    DateArrived = table.Column<LocalDate>(type: "date", nullable: false),
                    DateAdopted = table.Column<LocalDate>(type: "date", nullable: true),
                    AdoptedBy = table.Column<string>(type: "text", nullable: true),
                    ShelterId = table.Column<int>(type: "integer", nullable: false),
                    Image = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puppies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Puppies_Shelters_ShelterId",
                        column: x => x.ShelterId,
                        principalSchema: "puppy_adoption",
                        principalTable: "Shelters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puppies_DateAdopted",
                schema: "puppy_adoption",
                table: "Puppies",
                column: "DateAdopted");

            migrationBuilder.CreateIndex(
                name: "IX_Puppies_ShelterId",
                schema: "puppy_adoption",
                table: "Puppies",
                column: "ShelterId");

            migrationBuilder.CreateIndex(
                name: "IX_Shelters_Name",
                schema: "puppy_adoption",
                table: "Shelters",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Puppies",
                schema: "puppy_adoption");

            migrationBuilder.DropTable(
                name: "Shelters",
                schema: "puppy_adoption");
        }
    }
}
