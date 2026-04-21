using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyRentalManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentManagerAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ManagerId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                """
                DECLARE @FallbackManagerId INT;
                SELECT TOP(1) @FallbackManagerId = [Id]
                FROM [Users]
                WHERE [Role] IN ('Manager', 'Owner')
                ORDER BY CASE WHEN [Role] = 'Manager' THEN 0 ELSE 1 END, [Id];
                
                IF @FallbackManagerId IS NULL
                    SELECT TOP(1) @FallbackManagerId = [Id]
                    FROM [Users]
                    ORDER BY [Id];

                UPDATE [Appointments]
                SET [ManagerId] = @FallbackManagerId
                WHERE [ManagerId] IS NULL;
                """); // FIXED: Book appointment with manager

            migrationBuilder.AlterColumn<int>(
                name: "ManagerId",
                table: "Appointments",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ManagerId",
                table: "Appointments",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Users_ManagerId",
                table: "Appointments",
                column: "ManagerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Users_ManagerId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ManagerId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Appointments");
        }
    }
}
